using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TicketService.Application.Interfaces;
using TicketService.Infrastructure.Events;
using TicketService.Infrastructure.Integrations;
using TicketService.Infrastructure.Options;
using TicketService.Infrastructure.Observability;
using TicketService.Infrastructure.Persistence;
using TicketService.Infrastructure.Persistence.Repositories;
using TicketService.Infrastructure.Utils;

namespace TicketService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<ObservabilityMetrics>();
        services.AddSingleton<ObservabilityLogWriter>();
        services.AddTransient<ObservabilityHttpClientHandler>();

        services.Configure<IdentityServiceOptions>(configuration.GetSection(IdentityServiceOptions.SectionName));
        services.Configure<EmailServiceOptions>(configuration.GetSection(EmailServiceOptions.SectionName));
        services.Configure<ClientServiceOptions>(configuration.GetSection(ClientServiceOptions.SectionName));
        services.Configure<PartnerServiceOptions>(configuration.GetSection(PartnerServiceOptions.SectionName));
        services.Configure<FileServiceOptions>(configuration.GetSection(FileServiceOptions.SectionName));
        services.Configure<ImageServiceOptions>(configuration.GetSection(ImageServiceOptions.SectionName));
        services.Configure<CarServiceOptions>(configuration.GetSection(CarServiceOptions.SectionName));
        services.Configure<ActivationOptions>(configuration.GetSection(ActivationOptions.SectionName));

        var connectionString = configuration.GetConnectionString("DbConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var herokuDatabaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (!string.IsNullOrWhiteSpace(herokuDatabaseUrl))
            {
                connectionString = HerokuHelper.BuildConnectionString(herokuDatabaseUrl);
            }
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:DbConnection is required.");
        }

        services.AddDbContext<TicketDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ITicketUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<TicketDbContext>());
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ITicketEventPublisher, TicketEventPublisher>();

        services.AddHttpClient<IIdentityProvisioningClient, IdentityProvisioningClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<IdentityServiceOptions>>().Value;
            if (string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                throw new InvalidOperationException("IdentityService:BaseUrl configuration is required.");
            }

            client.BaseAddress = new Uri(NormalizeBaseUrl(options.BaseUrl));
        })
        .AddHttpMessageHandler<ObservabilityHttpClientHandler>();

        services.AddHttpClient<IEmailNotificationClient, EmailNotificationClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<EmailServiceOptions>>().Value;
            if (string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                throw new InvalidOperationException("EmailService:BaseUrl configuration is required.");
            }

            client.BaseAddress = new Uri(NormalizeBaseUrl(options.BaseUrl));
        })
        .AddHttpMessageHandler<ObservabilityHttpClientHandler>();

        services.AddHttpClient<IClientProvisioningClient, ClientProvisioningClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<ClientServiceOptions>>().Value;
            if (string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                throw new InvalidOperationException("ClientService:BaseUrl configuration is required.");
            }

            client.BaseAddress = new Uri(NormalizeBaseUrl(options.BaseUrl));
        })
        .AddHttpMessageHandler<ObservabilityHttpClientHandler>();

        services.AddHttpClient<IPartnerProvisioningClient, PartnerProvisioningClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<PartnerServiceOptions>>().Value;
            if (string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                throw new InvalidOperationException("PartnerService:BaseUrl configuration is required.");
            }

            client.BaseAddress = new Uri(NormalizeBaseUrl(options.BaseUrl));
        })
        .AddHttpMessageHandler<ObservabilityHttpClientHandler>();

        services.AddHttpClient<IPartnerContextClient, PartnerContextClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<PartnerServiceOptions>>().Value;
            if (string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                throw new InvalidOperationException("PartnerService:BaseUrl configuration is required.");
            }

            client.BaseAddress = new Uri(NormalizeBaseUrl(options.BaseUrl));
        })
        .AddHttpMessageHandler<ObservabilityHttpClientHandler>();

        services.AddHttpClient<IPartnerCarProvisioningClient, PartnerCarProvisioningClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<CarServiceOptions>>().Value;
            if (string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                throw new InvalidOperationException("CarService:BaseUrl configuration is required.");
            }

            client.BaseAddress = new Uri(NormalizeBaseUrl(options.BaseUrl));
        })
        .AddHttpMessageHandler<ObservabilityHttpClientHandler>();

        services.AddHttpClient<IFileStorageClient, FileStorageClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<FileServiceOptions>>().Value;
            if (string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                throw new InvalidOperationException("FileService:BaseUrl configuration is required.");
            }

            client.BaseAddress = new Uri(NormalizeBaseUrl(options.BaseUrl));
        })
        .AddHttpMessageHandler<ObservabilityHttpClientHandler>();

        services.AddHttpClient<IImageStorageClient, ImageStorageClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<ImageServiceOptions>>().Value;
            if (string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                throw new InvalidOperationException("ImageService:BaseUrl configuration is required.");
            }

            client.BaseAddress = new Uri(NormalizeBaseUrl(options.BaseUrl));
        })
        .AddHttpMessageHandler<ObservabilityHttpClientHandler>();

        return services;
    }

    private static string NormalizeBaseUrl(string url)
    {
        return url.Trim().TrimEnd('/');
    }
}
