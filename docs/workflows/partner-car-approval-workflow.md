# Partner Car Approval Workflow

This document describes the current partner car moderation workflow based on the implemented frontend and backend code.

## Source Files

The workflow was checked against these files:

- `frontend/external/src/router/index.ts`
- `frontend/external/src/views/PartnerCarsView.vue`
- `frontend/external/src/api/tickets.ts`
- `frontend/internal/src/views/ManagerTicketsView.vue`
- `frontend/internal/src/api/tickets.ts`
- `backend/internal/ticket-service/src/TicketService.Api/Controllers/TicketsController.cs`
- `backend/internal/ticket-service/src/TicketService.Application/Commands/CreateTicket/CreateTicketCommandHandler.cs`
- `backend/internal/ticket-service/src/TicketService.Application/Commands/ApproveTicket/ApproveTicketCommandHandler.cs`
- `backend/internal/ticket-service/src/TicketService.Application/Commands/RejectTicket/RejectTicketCommandHandler.cs`
- `backend/internal/ticket-service/src/TicketService.Domain/Entities/Ticket.cs`
- `backend/internal/ticket-service/src/TicketService.Domain/Entities/TicketData.cs`
- `backend/internal/ticket-service/src/TicketService.Infrastructure/Integrations/FileStorageClient.cs`
- `backend/internal/ticket-service/src/TicketService.Infrastructure/Integrations/ImageStorageClient.cs`
- `backend/internal/ticket-service/src/TicketService.Infrastructure/Events/TicketEventPublisher.cs`
- `backend/internal/ticket-service/src/TicketService.Infrastructure/Events/TicketWorkflowOutboxDispatcher.cs`
- `backend/libraries/messaging-dotnet/src/AutoRent.Messaging/Contracts/PartnerCarEvents.cs`
- `backend/libraries/messaging-dotnet/src/AutoRent.Messaging/RabbitMq/RabbitMqTopology.cs`
- `backend/external/car-service/src/CarService.Api/Messaging/PartnerCarProvisionConsumer.cs`
- `backend/external/car-service/src/CarService.Infrastructure/Services/PartnerCarService.cs`
- `backend/shared/image-service/README.md`
- `backend/shared/email-service/src/rabbitmq/consumer.ts`

## High-Level Flow

```mermaid
flowchart LR
    Partner[Partner] --> ExternalFrontend[/partner/cars]
    ExternalFrontend --> Gateway[API gateway /tickets]
    Gateway --> TicketService[ticket-service]
    TicketService --> FileService[file-service: ownership PDF]
    TicketService --> ImageService[image-service: car photos]
    TicketService --> TicketDb[(ticket-db)]

    Manager[Manager / supermanager / admin] --> InternalFrontend[internal frontend /tickets]
    InternalFrontend --> TicketService
    TicketService --> Outbox[(ticket workflow outbox)]
    Outbox --> RabbitMQ[RabbitMQ]
    RabbitMQ --> CarService[car-service]
    CarService --> CarDb[(car-db)]
    CarService --> Catalog[public car catalog]
    Outbox --> EmailEvent[RabbitMQ email event]
    EmailEvent --> EmailService[email-service]
```

## Submission

Partner sends a car to moderation from the external frontend route:

```text
/partner/cars
```

This route is protected on the frontend with `requiresAuth: true` and `actorType: "partner"`. The form submits a multipart request to:

```http
POST /tickets
```

The request contains:

- `ticketType = PartnerCar`
- `email` from the current auth store
- `carBrand`
- `carModel`
- `carYear`
- `licensePlate`
- optional `transmission`
- optional `fuelType`
- optional `seats`
- optional `doors`
- optional `bodyType`
- optional `horsepower`
- `selectedTags`
- `ownershipDocumentFile`
- one or more `carImageFiles`
- one `carImageTypes` value for each uploaded image

The allowed image type labels used by the frontend and backend are:

- `front`
- `back`
- `side`
- `interior`
- `general`

The frontend requires at least one car photo and limits the upload to 12 photos. The backend also requires at least one image and validates that every uploaded image has a matching image type.

## Ticket Creation

The created ticket type is:

```text
TicketType.PartnerCar
```

The ticket is created in status:

```text
TicketStatus.Pending
```

For partner car tickets, `ticket-service` does not trust only the submitted form data. It resolves the current partner context from the `Authorization` header through `partner-service`. If the current user is not a partner, ticket creation is rejected.

## Document And Photo Storage

Two different storage services are used:

| Data | Storage service | How it is stored in ticket data |
| --- | --- | --- |
| Ownership document PDF | `file-service` | `OwnershipDocumentFileName` |
| Car photos | `image-service` | `CarImages[]` with `ImageId`, `ImageUrl`, and `ImageType` |

The ownership document must be a PDF. `ticket-service` uploads it through `IFileStorageClient` to the internal file-service endpoint:

```http
POST /api/internal/files/upload
```

Car photos are uploaded through `IImageStorageClient` to:

```http
POST /api/images
```

The image upload uses the partner JWT and requires the `Image.Create` permission. `image-service` validates the file, converts it to WebP, and returns `imageId` plus `imageUrl`. In local mode it stores files under its local upload/public storage; in web storage mode it uses Google Cloud Storage.

## Review

Partner car tickets appear in the internal manager ticket queue. Review actions are exposed through `ticket-service`:

```http
POST /tickets/{id}/approve
POST /tickets/{id}/reject
```

Backend permissions:

- approve requires `Ticket.Approve`
- reject requires `Ticket.Reject`

In the current role setup, `manager` has `Ticket.View`, `Ticket.Approve`, and `Ticket.Reject`; `supermanager` has these permissions plus `Ticket.ViewAll`; `admin` and `superadmin` can also have these permissions through role permission migrations.

The internal frontend allows the reviewer to correct the main car fields before the final decision. The current UI sends corrected `carBrand`, `carModel`, `carYear`, and `licensePlate`. The backend review DTO also supports additional fields such as `transmission`, `fuelType`, `seats`, `doors`, `bodyType`, `horsepower`, and `confirmedTags`.

## Approval Flow

When the reviewer approves the ticket:

1. `ticket-service` changes ticket status from `Pending` to `Approved`.
2. `ticket-service` stores reviewer id and review timestamp.
3. `TicketEventPublisher` creates a workflow outbox message.
4. `TicketWorkflowOutboxDispatcher` publishes a RabbitMQ event for car provisioning.
5. `car-service` asynchronously consumes the event and creates the approved partner car in `car-db`.
6. `car-service` creates or updates catalog model data when needed, saves partner car images, and sets the partner car status to `Available`.
7. `car-service` publishes a car search upsert event for indexing.
8. In the ticket workflow, `ticket-service` also publishes a partner car approved email event after the provisioning event has been published to RabbitMQ. It does not synchronously wait for `car-service` to finish persistence.
9. `email-service` consumes the email event and sends notification to the partner.

The main provisioning event published by `ticket-service` is:

```text
PartnerCarProvisionRequested
```

RabbitMQ routing key:

```text
ticket.partner-car.provision-requested
```

RabbitMQ queue used by the consumer:

```text
car-service.partner-car-provision
```

The event payload contains:

- `ticketId`
- `provisionRequestKey`
- `relatedUserId`
- car brand/model/year/license plate
- technical characteristics
- semantic tags
- ownership document file name
- uploaded image ids, URLs, and image types

`ProvisionRequestKey` is built as:

```text
ticket:{ticketId}:partner-car
```

`car-service` uses it as an idempotency key, so repeated delivery of the same event does not create duplicate partner cars.

## Catalog Visibility

The car appears in the public catalog only after `car-service` successfully consumes the provisioning event and persists the partner car.

The created `PartnerCar` is saved with:

```text
PartnerCarStatus.Available
IsActive = true
```

Public catalog availability is based on active partner cars with `Status = Available`. Therefore, the car is not visible in the catalog at ticket creation time; it becomes visible after approval and successful `car-service` provisioning.

## Rejection Flow

When the reviewer rejects the ticket:

1. `ticket-service` requires a decision reason.
2. Ticket status changes from `Pending` to `Rejected`.
3. Reviewer id, review time, and rejection reason are stored in the ticket data.
4. No `PartnerCarProvisionRequested` event is published.
5. `car-service` is not called and no car is created in `car-db`.
6. `ticket-service` publishes a partner car rejection email event.
7. `email-service` sends the rejection notification to the partner.

RabbitMQ routing key for rejection email:

```text
ticket.email.partner-car-rejected
```

## Direct Answers

1. Partner sends a car to moderation from `/partner/cars`. The frontend sends `POST /tickets` with `ticketType=PartnerCar`.
2. Required car data: brand, model, year, license plate, ownership PDF, and at least one photo. Optional data: transmission, fuel type, seats, doors, body type, horsepower, and semantic tags.
3. Yes. Car photos are uploaded, and at least one photo is required.
4. Car photos are saved through `image-service`. The ownership document PDF is saved through `file-service`.
5. The created ticket type is `TicketType.PartnerCar`.
6. Partner car tickets are approved or rejected by internal users with `Ticket.Approve` or `Ticket.Reject`, mainly manager, supermanager, admin, and superadmin roles.
7. After approve, the ticket becomes `Approved`, an outbox workflow publishes a provisioning event, `car-service` asynchronously creates the partner car, and an approval email event is sent.
8. The main event is `PartnerCarProvisionRequested` with routing key `ticket.partner-car.provision-requested`.
9. `car-service` consumes this event through `PartnerCarProvisionConsumer`.
10. The car appears in the catalog after `car-service` persists it with `PartnerCarStatus.Available` and `IsActive = true`.
11. Yes. Approval sends `ticket.email.partner-car-approved`; rejection sends `ticket.email.partner-car-rejected`; both are consumed by `email-service`.
12. On reject, the ticket becomes `Rejected`, the reason is stored, no car is provisioned, and a rejection email is sent.
