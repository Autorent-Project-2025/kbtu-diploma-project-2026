using System.Security.Cryptography;
using ChatService.Api.Hubs;
using ChatService.Api.Middleware;
using ChatService.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ── MongoDB ────────────────────────────────────────────────
var mongoConn = builder.Configuration["MongoDB:ConnectionString"]
    ?? "mongodb://localhost:27017";
var mongoDb = builder.Configuration["MongoDB:DatabaseName"]
    ?? "chat_db";

builder.Services.AddInfrastructure(mongoConn, mongoDb, builder.Configuration);

// ── JWT Authentication ─────────────────────────────────────
var jwtPublicKey = builder.Configuration["Jwt:PublicKey"];
if (string.IsNullOrWhiteSpace(jwtPublicKey))
{
    throw new InvalidOperationException("Configuration value 'Jwt:PublicKey' is required.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var normalizedPublicKey = jwtPublicKey.Replace("\\n", "\n").Trim();

RSAParameters publicRsaParameters;
using (var rsa = RSA.Create())
{
    rsa.ImportFromPem(normalizedPublicKey);
    publicRsaParameters = rsa.ExportParameters(false);
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(publicRsaParameters),
            ValidateIssuer = !string.IsNullOrEmpty(jwtIssuer),
            ValidIssuer = jwtIssuer,
            ValidateAudience = !string.IsNullOrEmpty(jwtAudience),
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/conversation"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ── SignalR ────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── Controllers ────────────────────────────────────────────
builder.Services.AddControllers();

// ── CORS ───────────────────────────────────────────────────
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins);
        else
            policy.AllowAnyOrigin();

        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ── Middleware pipeline ────────────────────────────────────
app.UseMiddleware<ApiExceptionMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ConversationHub>("/hubs/conversation");

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.Run();
