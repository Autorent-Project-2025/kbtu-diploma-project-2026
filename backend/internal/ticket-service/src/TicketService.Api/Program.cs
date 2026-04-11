using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Security.Cryptography;
using TicketService.Api.Middleware;
using TicketService.Application.Commands.ApproveTicket;
using TicketService.Application.Commands.CreateTicket;
using TicketService.Application.Commands.IssueTicketFine;
using TicketService.Application.Commands.RejectTicket;
using TicketService.Application.AccessRequests.Commands.ApproveAccessRequest;
using TicketService.Application.AccessRequests.Commands.CreateAccessRequest;
using TicketService.Application.AccessRequests.Commands.RejectAccessRequest;
using TicketService.Application.AccessRequests.Commands.RevokeAccessRequest;
using TicketService.Application.AccessRequests.Queries.GetAccessRequestById;
using TicketService.Application.AccessRequests.Queries.GetAccessRequestForComplaint;
using TicketService.Application.AccessRequests.Queries.GetAccessRequests;
using TicketService.Application.AccessRequests.Queries.GetBookingReview;
using TicketService.Application.Complaints.Commands.AddManagerNote;
using TicketService.Application.Complaints.Commands.CreateComplaint;
using TicketService.Application.Complaints.Commands.RejectComplaint;
using TicketService.Application.Complaints.Commands.RequestInfo;
using TicketService.Application.Complaints.Commands.RespondToInfoRequest;
using TicketService.Application.Complaints.Commands.ResolveComplaint;
using TicketService.Application.Complaints.Commands.TakeComplaint;
using TicketService.Application.Complaints.Queries.GetAllComplaints;
using TicketService.Application.Complaints.Queries.GetComplaintById;
using TicketService.Application.Complaints.Queries.GetMyComplaintById;
using TicketService.Application.Complaints.Queries.GetMyComplaints;
using TicketService.Application.Constants;
using TicketService.Application.Queries.GetAllTickets;
using TicketService.Application.Queries.GetPendingTickets;
using TicketService.Application.Queries.GetTicketById;
using TicketService.Infrastructure;
using TicketService.Infrastructure.Observability;

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
builder.Services.AddInfrastructure(builder.Configuration);

var otlpEndpoint = builder.Configuration["OpenTelemetry:Endpoint"]
    ?? Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
if (!string.IsNullOrWhiteSpace(otlpEndpoint))
{
    builder.Services
        .AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService("ticket-service")
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = builder.Environment.EnvironmentName
            }))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation(options => options.RecordException = true)
            .AddSource("AutoRent.TicketService")
            .AddHttpClientInstrumentation(options => options.RecordException = true)
            .AddOtlpExporter(options =>
            {
                options.Endpoint = BuildOtlpTracesEndpoint(otlpEndpoint);
                options.Protocol = OtlpExportProtocol.HttpProtobuf;
            }));
}

builder.Services.AddScoped<CreateTicketCommandHandler>();
builder.Services.AddScoped<GetPendingTicketsQueryHandler>();
builder.Services.AddScoped<GetAllTicketsQueryHandler>();
builder.Services.AddScoped<GetTicketByIdQueryHandler>();
builder.Services.AddScoped<ApproveTicketCommandHandler>();
builder.Services.AddScoped<IssueTicketFineCommandHandler>();
builder.Services.AddScoped<RejectTicketCommandHandler>();

builder.Services.AddScoped<CreateComplaintCommandHandler>();
builder.Services.AddScoped<TakeComplaintCommandHandler>();
builder.Services.AddScoped<RequestInfoCommandHandler>();
builder.Services.AddScoped<RespondToInfoRequestCommandHandler>();
builder.Services.AddScoped<AddManagerNoteCommandHandler>();
builder.Services.AddScoped<ResolveComplaintCommandHandler>();
builder.Services.AddScoped<RejectComplaintCommandHandler>();
builder.Services.AddScoped<GetMyComplaintsQueryHandler>();
builder.Services.AddScoped<GetMyComplaintByIdQueryHandler>();
builder.Services.AddScoped<GetAllComplaintsQueryHandler>();
builder.Services.AddScoped<GetComplaintByIdQueryHandler>();

builder.Services.AddScoped<CreateAccessRequestCommandHandler>();
builder.Services.AddScoped<ApproveAccessRequestCommandHandler>();
builder.Services.AddScoped<RejectAccessRequestCommandHandler>();
builder.Services.AddScoped<RevokeAccessRequestCommandHandler>();
builder.Services.AddScoped<GetAccessRequestsQueryHandler>();
builder.Services.AddScoped<GetAccessRequestByIdQueryHandler>();
builder.Services.AddScoped<GetAccessRequestForComplaintQueryHandler>();
builder.Services.AddScoped<GetBookingReviewQueryHandler>();

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

RSAParameters publicRsaParameters;
using (var rsa = RSA.Create())
{
    rsa.ImportFromPem(normalizedPublicKey);
    publicRsaParameters = rsa.ExportParameters(false);
}

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
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("tickets:view", policy =>
        policy.RequireClaim("permissions", PermissionConstants.TicketView));

    options.AddPolicy("tickets:approve", policy =>
        policy.RequireClaim("permissions", PermissionConstants.TicketApprove));

    options.AddPolicy("tickets:reject", policy =>
        policy.RequireClaim("permissions", PermissionConstants.TicketReject));

    options.AddPolicy("tickets:view-all", policy =>
        policy.RequireClaim("permissions", PermissionConstants.TicketViewAll));

    options.AddPolicy("complaints:view", policy =>
        policy.RequireClaim("permissions", PermissionConstants.ComplaintView));

    options.AddPolicy("complaints:review", policy =>
        policy.RequireClaim("permissions", PermissionConstants.ComplaintReview));

    options.AddPolicy("complaints:resolve", policy =>
        policy.RequireClaim("permissions", PermissionConstants.ComplaintResolve));

    options.AddPolicy("access-requests:review", policy =>
        policy.RequireClaim("permissions", PermissionConstants.AccessRequestReview));
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
