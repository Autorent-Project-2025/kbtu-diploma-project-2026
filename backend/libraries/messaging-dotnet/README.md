# AutoRent.Messaging

## Назначение
`messaging-dotnet` - общая .NET-библиотека для event-driven интеграций через RabbitMQ.

Она централизует:
- `RabbitMqOptions` с секцией конфигурации `RabbitMq`;
- topology exchange/queue/routing key;
- envelope интеграционного сообщения;
- общий publisher;
- контракты событий между сервисами.

Проект: `src/AutoRent.Messaging/AutoRent.Messaging.csproj`

## Что входит в библиотеку
### RabbitMQ инфраструктура
- `RabbitMq/RabbitMqOptions.cs` - настройки подключения и exchange.
- `RabbitMq/RabbitMqTopology.cs` - имена exchange, очередей и routing key, плюс helper-методы для declare/bind.
- `RabbitMq/IntegrationMessage.cs` - общий envelope вида `EventId + RoutingKey + OccurredAtUtc + Payload`.
- `RabbitMq/IRabbitMqPublisher.cs` и `RabbitMq/RabbitMqPublisher.cs` - публикация событий в topic exchange.

### Контракты событий
- `Contracts/TicketEmailEvents.cs` - события email-уведомлений по approve/reject тикетов.
- `Contracts/PartnerCarEvents.cs` - событие provisioning партнерской машины.
- `Contracts/BookingPaymentEvents.cs` - события booking -> payment.

## Текущая topology
Exchange:
- `autorent.events`

Queues:
- `email-service.notifications`
- `payment-service.booking-payments`
- `car-service.partner-car-provision`

Routing keys:
- `ticket.email.client-approved`
- `ticket.email.client-rejected`
- `ticket.email.partner-approved`
- `ticket.email.partner-rejected`
- `ticket.email.partner-car-approved`
- `ticket.email.partner-car-rejected`
- `ticket.partner-car.provision-requested`
- `booking.payment.confirmed`
- `booking.payment.canceled`
- `booking.payment.completed`

## Где используется
Сейчас библиотека подключена в:
- `ticket-service` - публикация approve/reject workflow событий;
- `booking-service` - публикация payment-событий из outbox;
- `payment-service` - потребление booking payment событий;
- `car-service` - потребление partner-car provisioning события.

`email-service` использует ту же topology, но сам пакет не подключает, потому что он написан на Node.js.

## Подключение в .NET-сервис
Добавьте `ProjectReference` на библиотеку:

```xml
<ItemGroup>
  <ProjectReference Include="..\..\..\..\libraries\messaging-dotnet\src\AutoRent.Messaging\AutoRent.Messaging.csproj" />
</ItemGroup>
```

Зарегистрируйте настройки и publisher:

```csharp
builder.Services
    .AddOptions<RabbitMqOptions>()
    .BindConfiguration(RabbitMqOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
```

Минимальная конфигурация:

```json
{
  "RabbitMq": {
    "HostName": "rabbitmq",
    "Port": 5672,
    "UserName": "autorent",
    "Password": "autorent",
    "VirtualHost": "/",
    "ExchangeName": "autorent.events"
  }
}
```

Пример публикации:

```csharp
await publisher.PublishAsync(
    eventId: $"booking:{bookingId}:confirmed",
    routingKey: RabbitMqTopology.RoutingKeys.BookingPaymentConfirmed,
    payload: new BookingPaymentConfirmed(
        BookingId: bookingId,
        UserId: userId,
        PartnerUserId: partnerUserId,
        PartnerCarId: partnerCarId,
        PriceHour: priceHour,
        TotalPrice: totalPrice),
    cancellationToken);
```

## Ограничения
- Библиотека не содержит consumers и не запускает background processing сама.
- Библиотека не решает idempotency, inbox/outbox и retry-политику на уровне бизнес-процессов.
- Изменения в контрактах событий считаются межсервисным контрактным изменением и должны делаться осознанно.
