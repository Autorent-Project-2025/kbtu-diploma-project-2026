using CarService.Application.Constants;
using CarService.Api.Options;
using CarService.Api.Middleware;
using CarService.Application.Interfaces;
using CarService.Application.Interfaces.Integrations;
using CarService.Infrastructure.Integrations;
using CarService.Infrastructure.Options;
using CarService.Infrastructure.Persistance;
using CarService.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<PartnerServiceOptions>(builder.Configuration.GetSection(PartnerServiceOptions.SectionName));
builder.Services.Configure<BookingServiceOptions>(builder.Configuration.GetSection(BookingServiceOptions.SectionName));
builder.Services.Configure<ImageServiceOptions>(builder.Configuration.GetSection(ImageServiceOptions.SectionName));
builder.Services.Configure<InternalAuthOptions>(builder.Configuration.GetSection(InternalAuthOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("DbConnection");

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
    options.AddPolicy("car-models:create", policy =>
        policy.RequireClaim("permissions", PermissionConstants.CarModelCreate));

    options.AddPolicy("car-models:update", policy =>
        policy.RequireClaim("permissions", PermissionConstants.CarModelUpdate));

    options.AddPolicy("car-models:delete", policy =>
        policy.RequireClaim("permissions", PermissionConstants.CarModelDelete));

    options.AddPolicy("partner-cars:create", policy =>
        policy.RequireClaim("permissions", PermissionConstants.PartnerCarCreate));

    options.AddPolicy("partner-cars:update", policy =>
        policy.RequireClaim("permissions", PermissionConstants.PartnerCarUpdate));

    options.AddPolicy("partner-cars:delete", policy =>
        policy.RequireClaim("permissions", PermissionConstants.PartnerCarDelete));

    options.AddPolicy("partner-cars:view-own", policy =>
        policy.RequireClaim("permissions", PermissionConstants.PartnerCarViewOwn));

    options.AddPolicy("car-comments:create", policy =>
        policy.RequireClaim("permissions", PermissionConstants.CarCommentCreate));

    options.AddPolicy("car-comments:update", policy =>
        policy.RequireClaim("permissions", PermissionConstants.CarCommentUpdate));

    options.AddPolicy("car-comments:delete", policy =>
        policy.RequireClaim("permissions", PermissionConstants.CarCommentDelete));

    options.AddPolicy("car-images:create", policy =>
        policy.RequireClaim("permissions", PermissionConstants.CarImageCreate));

    options.AddPolicy("car-images:update", policy =>
        policy.RequireClaim("permissions", PermissionConstants.CarImageUpdate));

    options.AddPolicy("car-images:delete", policy =>
        policy.RequireClaim("permissions", PermissionConstants.CarImageDelete));
});

builder.Services.AddScoped<ICarModelService, CarModelService>();
builder.Services.AddScoped<IPartnerCarService, PartnerCarService>();
builder.Services.AddScoped<ICarCommentService, CarCommentService>();
builder.Services.AddScoped<ICarImageService, CarImageService>();
builder.Services.AddScoped<ICarFeatureService, CarFeatureService>();
builder.Services.AddScoped<CarCatalogResolver>();

builder.Services.AddHttpClient<IPartnerContextClient, PartnerContextClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<PartnerServiceOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        throw new InvalidOperationException("PartnerService:BaseUrl configuration is required.");
    }

    client.BaseAddress = new Uri(NormalizeBaseUrl(options.BaseUrl));
});

builder.Services.AddHttpClient<IBookingReadClient, BookingReadClient>((serviceProvider, client) =>
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
});

builder.Services.AddHttpClient<IImageStorageClient, ImageStorageClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ImageServiceOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        throw new InvalidOperationException("ImageService:BaseUrl configuration is required.");
    }

    client.BaseAddress = new Uri(NormalizeBaseUrl(options.BaseUrl));
});

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
