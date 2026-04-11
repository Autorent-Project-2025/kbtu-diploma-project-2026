using AutoRent.Messaging.RabbitMq;
using ChatService.Application.Conversations.Commands.MarkAsRead;
using ChatService.Application.Conversations.Commands.SendMessage;
using ChatService.Application.Conversations.Queries.GetConversation;
using ChatService.Application.Conversations.Queries.GetConversationByContext;
using ChatService.Application.Conversations.Queries.GetMessages;
using ChatService.Application.Interfaces;
using ChatService.Infrastructure.Integrations;
using ChatService.Infrastructure.Notifications;
using ChatService.Infrastructure.Options;
using ChatService.Infrastructure.Persistence;
using ChatService.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ChatService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString, string databaseName,
        IConfiguration configuration)
    {
        var dbContext = new MongoDbContext(connectionString, databaseName);
        services.AddSingleton(dbContext);

        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();

        services.AddScoped<SendMessageCommandHandler>();
        services.AddScoped<MarkAsReadCommandHandler>();
        services.AddScoped<GetMessagesQueryHandler>();
        services.AddScoped<GetConversationQueryHandler>();
        services.AddScoped<GetConversationByContextQueryHandler>();

        // RabbitMQ + Notifications
        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
        services.AddScoped<IChatNotificationPublisher, ChatNotificationPublisher>();

        // File service client
        services.Configure<FileServiceOptions>(configuration.GetSection(FileServiceOptions.SectionName));
        services.AddHttpClient<IFileServiceClient, FileServiceClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<FileServiceOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                var baseUrl = options.BaseUrl.TrimEnd('/');
                if (!baseUrl.EndsWith('/')) baseUrl += '/';
                client.BaseAddress = new Uri(baseUrl);
            }
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
