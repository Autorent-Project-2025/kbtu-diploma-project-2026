using AutoRent.Messaging.RabbitMq;
using Microsoft.EntityFrameworkCore;
using PaymentService.Api.Middleware;
using PaymentService.Api.Messaging;
using PaymentService.Api.Options;
using PaymentService.Application.Interfaces;
using PaymentService.Infrastructure.Options;
using PaymentService.Infrastructure.Persistence;
using PaymentService.Infrastructure.Services;
using PaymentService.Infrastructure.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOptions<InternalAuthOptions>()
    .Bind(builder.Configuration.GetSection(InternalAuthOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.ApiKey), "Internal auth API key is required.")
    .ValidateOnStart();
builder.Services.AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection(RabbitMqOptions.SectionName))
    .Validate(options =>
        !string.IsNullOrWhiteSpace(options.HostName) &&
        options.Port > 0 &&
        !string.IsNullOrWhiteSpace(options.UserName) &&
        !string.IsNullOrWhiteSpace(options.Password),
        "RabbitMQ configuration is invalid.")
    .ValidateOnStart();

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

builder.Services.AddOptions<PaymentOptions>()
    .Bind(builder.Configuration.GetSection(PaymentOptions.SectionName))
    .Validate(options =>
        !string.IsNullOrWhiteSpace(options.Currency) &&
        options.Currency.Trim().Length == 3 &&
        options.PlatformCommissionRate >= 0m &&
        options.PlatformCommissionRate <= 1m,
        "Payment configuration is invalid.")
    .ValidateOnStart();

builder.Services.AddScoped<IPaymentLedgerService, PaymentLedgerService>();
builder.Services.AddScoped<IMockPaymentService, MockPaymentService>();
builder.Services.AddHostedService<BookingPaymentConsumer>();

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseHttpsRedirection();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

app.Run();
