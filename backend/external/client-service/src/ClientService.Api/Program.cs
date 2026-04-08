using ClientService.Api.Middleware;
using ClientService.Api.Options;
using ClientService.Application.Constants;
using ClientService.Application.Interfaces;
using ClientService.Application.Interfaces.Integrations;
using ClientService.Infrastructure.Integrations;
using ClientService.Infrastructure.Options;
using ClientService.Infrastructure.Persistence;
using ClientService.Infrastructure.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<InternalAuthOptions>(builder.Configuration.GetSection(InternalAuthOptions.SectionName));
builder.Services.Configure<ImageServiceOptions>(builder.Configuration.GetSection(ImageServiceOptions.SectionName));

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
    options.AddPolicy("clients:view", policy =>
        policy.RequireClaim("permissions", PermissionConstants.ClientView));

    options.AddPolicy("clients:create", policy =>
        policy.RequireClaim("permissions", PermissionConstants.ClientCreate));

    options.AddPolicy("clients:update", policy =>
        policy.RequireClaim("permissions", PermissionConstants.ClientUpdate));

    options.AddPolicy("clients:delete", policy =>
        policy.RequireClaim("permissions", PermissionConstants.ClientDelete));
});

builder.Services.AddHttpClient<IImageStorageClient, ImageStorageClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ImageServiceOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        throw new InvalidOperationException("ImageService:BaseUrl configuration is required.");
    }

    client.BaseAddress = new Uri(NormalizeBaseUrl(options.BaseUrl));
    client.Timeout = Timeout.InfiniteTimeSpan;
});

builder.Services.AddScoped<IClientService, ClientService.Infrastructure.Services.ClientService>();

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
