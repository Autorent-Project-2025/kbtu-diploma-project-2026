using BookingService.Api.Middleware;
using BookingService.Api.Options;
using BookingService.Application.Constants;
using BookingService.Application.Interfaces;
using BookingService.Application.Interfaces.Integrations;
using BookingService.Infrastructure.Integrations;
using BookingService.Infrastructure.Options;
using BookingService.Infrastructure.Persistence;
using BookingService.Infrastructure.Services;
using BookingService.Infrastructure.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<InternalAuthOptions>(builder.Configuration.GetSection(InternalAuthOptions.SectionName));
builder.Services.Configure<CarServiceOptions>(builder.Configuration.GetSection(CarServiceOptions.SectionName));
builder.Services.Configure<PaymentServiceOptions>(builder.Configuration.GetSection(PaymentServiceOptions.SectionName));
builder.Services.AddOptions<PaymentSyncOutboxOptions>()
    .Bind(builder.Configuration.GetSection(PaymentSyncOutboxOptions.SectionName))
    .Validate(options =>
        options.BatchSize > 0 &&
        options.BatchSize <= 200 &&
        options.PollIntervalSeconds > 0 &&
        options.LockTimeoutSeconds > 0 &&
        options.InitialRetryDelaySeconds > 0 &&
        options.MaxRetryDelaySeconds >= options.InitialRetryDelaySeconds,
        "Payment sync outbox configuration is invalid.");
builder.Services.AddOptions<PendingBookingExpirationOptions>()
    .Bind(builder.Configuration.GetSection(PendingBookingExpirationOptions.SectionName))
    .Validate(options =>
        options.TtlMinutes > 0 &&
        options.TtlMinutes <= 1440 &&
        options.PollIntervalSeconds > 0 &&
        options.PollIntervalSeconds <= 3600 &&
        options.BatchSize > 0 &&
        options.BatchSize <= 500,
        "Pending booking expiration configuration is invalid.");

var connectionString = builder.Configuration.GetConnectionString("DbConnection");
if (string.IsNullOrEmpty(connectionString))
{
    var herokuDatabaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (string.IsNullOrEmpty(herokuDatabaseUrl)) throw new NullReferenceException("No Database URL was provided");

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
        options.RequireHttpsMetadata = true;
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
    options.AddPolicy("bookings:create", policy =>
        policy.RequireClaim("permissions", PermissionConstants.BookingCreate));
});
builder.Services.AddHttpClient<IPartnerCarReadClient, PartnerCarReadClient>((serviceProvider, client) =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<CarServiceOptions>>()
        .Value;

    if (string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        throw new InvalidOperationException("Configuration value 'CarService:BaseUrl' is required.");
    }

    client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
});
builder.Services.AddHttpClient<IPaymentSyncClient, PaymentSyncClient>((serviceProvider, client) =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<PaymentServiceOptions>>()
        .Value;

    if (string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        throw new InvalidOperationException("Configuration value 'PaymentService:BaseUrl' is required.");
    }

    client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
});
builder.Services.AddScoped<IBookingService, BookingService.Infrastructure.Services.BookingService>();
builder.Services.AddHostedService<PaymentSyncOutboxDispatcher>();
builder.Services.AddHostedService<PendingBookingExpirationDispatcher>();

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors("app-cors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
