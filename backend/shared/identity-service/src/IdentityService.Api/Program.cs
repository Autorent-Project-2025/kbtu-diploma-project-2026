using IdentityService.Api.Middleware;
using IdentityService.Application.Commands.AssignPermissionToRole;
using IdentityService.Application.Commands.AssignRoleToUser;
using IdentityService.Application.Commands.CreatePermission;
using IdentityService.Application.Commands.CreateRole;
using IdentityService.Application.Commands.LoginUser;
using IdentityService.Application.Commands.RefreshToken;
using IdentityService.Application.Commands.RegisterUser;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<RegisterUserCommandHandler>();
builder.Services.AddScoped<LoginUserCommandHandler>();
builder.Services.AddScoped<RefreshTokenCommandHandler>();
builder.Services.AddScoped<CreateRoleCommandHandler>();
builder.Services.AddScoped<AssignRoleToUserCommandHandler>();
builder.Services.AddScoped<CreatePermissionCommandHandler>();
builder.Services.AddScoped<AssignPermissionToRoleCommandHandler>();

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

var jwtPrivateKey = builder.Configuration["Jwt:PrivateKey"];
var jwtPublicKey = builder.Configuration["Jwt:PublicKey"];
var publicKeyPem = !string.IsNullOrWhiteSpace(jwtPublicKey)
    ? RsaKeyMaterial.NormalizePem(jwtPublicKey)
    : RsaKeyMaterial.DerivePublicKeyPem(
        jwtPrivateKey ?? throw new InvalidOperationException("Configuration value 'Jwt:PrivateKey' is required."));

var publicRsaParameters = RsaKeyMaterial.ReadPublicKeyParameters(publicKeyPem);
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(publicRsaParameters),
            ValidateIssuer = !string.IsNullOrWhiteSpace(jwtIssuer),
            ValidIssuer = jwtIssuer,
            ValidateAudience = !string.IsNullOrWhiteSpace(jwtAudience),
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors("app-cors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.Run();
