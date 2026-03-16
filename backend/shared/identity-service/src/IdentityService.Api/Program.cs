using IdentityService.Api.Middleware;
using IdentityService.Api.Observability;
using IdentityService.Application.Constants;
using IdentityService.Application.Commands.ActivateUser;
using IdentityService.Application.Commands.ActivateUserByAdmin;
using IdentityService.Application.Commands.AssignParentRoleToRole;
using IdentityService.Application.Commands.AssignPermissionToRole;
using IdentityService.Application.Commands.AssignRoleToUser;
using IdentityService.Application.Commands.CreatePermission;
using IdentityService.Application.Commands.CreateRole;
using IdentityService.Application.Commands.CreateUser;
using IdentityService.Application.Commands.DeactivateUser;
using IdentityService.Application.Commands.DeleteUser;
using IdentityService.Application.Commands.LoginUser;
using IdentityService.Application.Commands.ProvisionUser;
using IdentityService.Application.Commands.RefreshToken;
using IdentityService.Application.Commands.RemoveParentRoleFromRole;
using IdentityService.Application.Commands.RemovePermissionFromRole;
using IdentityService.Application.Commands.RemoveRoleFromUser;
using IdentityService.Application.Commands.UpdateUser;
using IdentityService.Application.Queries.GetActivationTokenStatus;
using IdentityService.Application.Queries.GetPermissions;
using IdentityService.Application.Queries.GetRoles;
using IdentityService.Application.Queries.GetUserById;
using IdentityService.Application.Queries.GetUsers;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

static Uri BuildOtlpTracesEndpoint(string endpoint)
{
    var uri = new Uri(endpoint, UriKind.Absolute);
    if (uri.AbsolutePath.EndsWith("/v1/traces", StringComparison.OrdinalIgnoreCase))
    {
        return uri;
    }

    var builder = new UriBuilder(uri);
    var normalizedPath = builder.Path.TrimEnd('/');
    builder.Path = string.IsNullOrEmpty(normalizedPath)
        ? "/v1/traces"
        : $"{normalizedPath}/v1/traces";

    return builder.Uri;
}

builder.Logging.Configure(options =>
{
    options.ActivityTrackingOptions =
        ActivityTrackingOptions.SpanId |
        ActivityTrackingOptions.TraceId |
        ActivityTrackingOptions.ParentId;
});
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ ";
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<ObservabilityMetrics>();
builder.Services.AddSingleton<ObservabilityLogWriter>();

builder.Services.AddInfrastructure(builder.Configuration);

var otlpEndpoint = builder.Configuration["OpenTelemetry:Endpoint"]
    ?? Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
if (!string.IsNullOrWhiteSpace(otlpEndpoint))
{
    builder.Services
        .AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService("identity-service")
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = builder.Environment.EnvironmentName
            }))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation(options => options.RecordException = true)
            .AddSource("AutoRent.IdentityService")
            .AddOtlpExporter(options =>
            {
                options.Endpoint = BuildOtlpTracesEndpoint(otlpEndpoint);
                options.Protocol = OtlpExportProtocol.HttpProtobuf;
            }));
}

builder.Services.AddScoped<ActivateUserCommandHandler>();
builder.Services.AddScoped<ActivateUserByAdminCommandHandler>();
builder.Services.AddScoped<LoginUserCommandHandler>();
builder.Services.AddScoped<RefreshTokenCommandHandler>();
builder.Services.AddScoped<CreateUserCommandHandler>();
builder.Services.AddScoped<UpdateUserCommandHandler>();
builder.Services.AddScoped<DeactivateUserCommandHandler>();
builder.Services.AddScoped<DeleteUserCommandHandler>();
builder.Services.AddScoped<ProvisionUserCommandHandler>();
builder.Services.AddScoped<CreateRoleCommandHandler>();
builder.Services.AddScoped<AssignParentRoleToRoleCommandHandler>();
builder.Services.AddScoped<AssignRoleToUserCommandHandler>();
builder.Services.AddScoped<RemoveRoleFromUserCommandHandler>();
builder.Services.AddScoped<CreatePermissionCommandHandler>();
builder.Services.AddScoped<AssignPermissionToRoleCommandHandler>();
builder.Services.AddScoped<RemoveParentRoleFromRoleCommandHandler>();
builder.Services.AddScoped<RemovePermissionFromRoleCommandHandler>();
builder.Services.AddScoped<GetUsersQueryHandler>();
builder.Services.AddScoped<GetUserByIdQueryHandler>();
builder.Services.AddScoped<GetRolesQueryHandler>();
builder.Services.AddScoped<GetPermissionsQueryHandler>();
builder.Services.AddScoped<GetActivationTokenStatusQueryHandler>();

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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("roles:create", policy =>
        policy.RequireClaim("permissions", PermissionConstants.RoleCreate));

    options.AddPolicy("roles:assign-permission", policy =>
        policy.RequireClaim("permissions", PermissionConstants.RoleAssignPermission));

    options.AddPolicy("roles:view", policy =>
        policy.RequireClaim("permissions", PermissionConstants.RoleView));

    options.AddPolicy("permissions:create", policy =>
        policy.RequireClaim("permissions", PermissionConstants.PermissionCreate));

    options.AddPolicy("permissions:view", policy =>
        policy.RequireClaim("permissions", PermissionConstants.PermissionView));

    options.AddPolicy("users:create", policy =>
        policy.RequireClaim("permissions", PermissionConstants.UserCreate));

    options.AddPolicy("users:view", policy =>
        policy.RequireClaim("permissions", PermissionConstants.UserView));

    options.AddPolicy("users:update", policy =>
        policy.RequireClaim("permissions", PermissionConstants.UserUpdate));

    options.AddPolicy("users:assign-role", policy =>
        policy.RequireClaim("permissions", PermissionConstants.UserAssignRole));

    options.AddPolicy("users:remove-role", policy =>
        policy.RequireClaim("permissions", PermissionConstants.UserRemoveRole));

    options.AddPolicy("users:deactivate", policy =>
        policy.RequireClaim("permissions", PermissionConstants.UserDeactivate));

    options.AddPolicy("users:activate", policy =>
        policy.RequireClaim("permissions", PermissionConstants.UserActivate));

    options.AddPolicy("users:delete", policy =>
        policy.RequireClaim("permissions", PermissionConstants.UserDelete));
});

var app = builder.Build();

app.UseMiddleware<RequestObservabilityMiddleware>();
app.UseMiddleware<ApiExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors("app-cors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));
app.MapGet(
    "/metrics",
    (ObservabilityMetrics metrics) => Results.Text(metrics.RenderPrometheus(), "text/plain; version=0.0.4; charset=utf-8"));

app.Run();
