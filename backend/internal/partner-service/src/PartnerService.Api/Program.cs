using PartnerService.Api.Middleware;
using PartnerService.Api.Options;
using PartnerService.Application.Constants;
using PartnerService.Application.Interfaces;
using PartnerService.Infrastructure.Integrations;
using PartnerService.Infrastructure.Options;
using PartnerService.Infrastructure.Persistence;
using PartnerService.Infrastructure.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
var httpClientResilienceOptions = builder.Configuration.GetHttpClientResilienceOptions();
builder.Services.Configure<InternalAuthOptions>(builder.Configuration.GetSection(InternalAuthOptions.SectionName));
builder.Services.Configure<FileServiceOptions>(builder.Configuration.GetSection(FileServiceOptions.SectionName));
builder.Services.Configure<PaymentServiceOptions>(builder.Configuration.GetSection(PaymentServiceOptions.SectionName));
builder.Services.Configure<BookingServiceOptions>(builder.Configuration.GetSection(BookingServiceOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("DbConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    var herokuDatabaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (string.IsNullOrWhiteSpace(herokuDatabaseUrl))
    {
        throw new NullReferenceException("No Database URL was provided.");
    }

    connectionString = HerokuHelper.BuildConnectionString(herokuDatabaseUrl);
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("app-cors", policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            return;
        }

        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});

var jwtPublicKey = builder.Configuration["Jwt:PublicKey"];
if (string.IsNullOrWhiteSpace(jwtPublicKey))
{
    throw new InvalidOperationException("Configuration value 'Jwt:PublicKey' is required.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var normalizedPublicKey = jwtPublicKey.Replace("\\n", "\n").Trim();

RSAParameters rsaPublicKeyParameters;
using (var rsa = RSA.Create())
{
    rsa.ImportFromPem(normalizedPublicKey);
    rsaPublicKeyParameters = rsa.ExportParameters(false);
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(rsaPublicKeyParameters),
            ValidateIssuer = !string.IsNullOrWhiteSpace(jwtIssuer),
            ValidIssuer = jwtIssuer,
            ValidateAudience = !string.IsNullOrWhiteSpace(jwtAudience),
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("partners:view", policy =>
        policy.RequireClaim("permissions", PermissionConstants.PartnerView));

    options.AddPolicy("partners:create", policy =>
        policy.RequireClaim("permissions", PermissionConstants.PartnerCreate));

    options.AddPolicy("partners:update", policy =>
        policy.RequireClaim("permissions", PermissionConstants.PartnerUpdate));

    options.AddPolicy("partners:delete", policy =>
        policy.RequireClaim("permissions", PermissionConstants.PartnerDelete));
});

builder.Services.AddScoped<IPartnerService, PartnerService.Infrastructure.Services.PartnerService>();
builder.Services.AddHttpClient<IFileStorageClient, FileStorageClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<FileServiceOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        throw new InvalidOperationException("FileService:BaseUrl configuration is required.");
    }

    if (string.IsNullOrWhiteSpace(options.InternalApiKey))
    {
        throw new InvalidOperationException("FileService:InternalApiKey configuration is required.");
    }

    client.BaseAddress = new Uri(NormalizeBaseUrl(options.BaseUrl));
    client.Timeout = Timeout.InfiniteTimeSpan;
})
.AddConfiguredResilience(httpClientResilienceOptions);
builder.Services.AddHttpClient<IPartnerPaymentClient, PartnerPaymentClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<PaymentServiceOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        throw new InvalidOperationException("PaymentService:BaseUrl configuration is required.");
    }

    if (string.IsNullOrWhiteSpace(options.InternalApiKey))
    {
        throw new InvalidOperationException("PaymentService:InternalApiKey configuration is required.");
    }

    client.BaseAddress = new Uri(NormalizeBaseUrl(options.BaseUrl));
    client.Timeout = Timeout.InfiniteTimeSpan;
})
.AddConfiguredResilience(httpClientResilienceOptions);
builder.Services.AddHttpClient<IPartnerBookingClient, PartnerBookingClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<BookingServiceOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        throw new InvalidOperationException("BookingService:BaseUrl configuration is required.");
    }

    if (string.IsNullOrWhiteSpace(options.InternalApiKey))
    {
        throw new InvalidOperationException("BookingService:InternalApiKey configuration is required.");
    }

    client.BaseAddress = new Uri(NormalizeBaseUrl(options.BaseUrl));
    client.Timeout = Timeout.InfiniteTimeSpan;
})
.AddConfiguredResilience(httpClientResilienceOptions);

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors("app-cors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.Run();

static string NormalizeBaseUrl(string url)
{
    return url.Trim().TrimEnd('/');
}
