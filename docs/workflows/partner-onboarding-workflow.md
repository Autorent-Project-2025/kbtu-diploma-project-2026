# Partner Onboarding Workflow

This document describes the current partner onboarding workflow based on the implemented frontend and backend code.

## Source Files

The workflow was checked against these files:

- `frontend/external/src/router/index.ts`
- `frontend/external/src/views/PartnerApplyView.vue`
- `frontend/external/src/api/tickets.ts`
- `backend/external/reverse-proxy-service/src/index.ts`
- `backend/internal/ticket-service/src/TicketService.Api/Controllers/TicketsController.cs`
- `backend/internal/ticket-service/src/TicketService.Api/Contracts/Tickets/CreateTicketRequest.cs`
- `backend/internal/ticket-service/src/TicketService.Application/Commands/CreateTicket/CreateTicketCommandHandler.cs`
- `backend/internal/ticket-service/src/TicketService.Application/Commands/ApproveTicket/ApproveTicketCommandHandler.cs`
- `backend/internal/ticket-service/src/TicketService.Application/Commands/RejectTicket/RejectTicketCommandHandler.cs`
- `backend/internal/ticket-service/src/TicketService.Infrastructure/Events/TicketEventPublisher.cs`
- `backend/internal/ticket-service/src/TicketService.Infrastructure/Events/TicketWorkflowOutboxDispatcher.cs`
- `backend/internal/ticket-service/src/TicketService.Infrastructure/Integrations/FileStorageClient.cs`
- `backend/shared/identity-service/src/IdentityService.Api/Controllers/InternalUsersController.cs`
- `backend/shared/identity-service/src/IdentityService.Application/Commands/ProvisionUser/ProvisionUserCommandHandler.cs`
- `backend/internal/partner-service/src/PartnerService.Api/Controllers/InternalPartnersController.cs`
- `backend/internal/partner-service/src/PartnerService.Infrastructure/Services/PartnerService.cs`
- `backend/shared/email-service/src/rabbitmq/consumer.ts`

## High-Level Flow

```mermaid
flowchart LR
    Applicant[Partner applicant] --> Frontend[/partner/apply]
    Frontend --> Gateway[API gateway /tickets]
    Gateway --> TicketService[ticket-service]
    TicketService --> FileService[file-service]
    TicketService --> TicketDb[(ticket-db)]

    Manager[Manager or supermanager] --> InternalFrontend[internal frontend /tickets]
    InternalFrontend --> TicketService
    TicketService --> Outbox[(ticket workflow outbox)]
    Outbox --> IdentityService[identity-service]
    Outbox --> PartnerService[partner-service]
    Outbox --> RabbitMQ[RabbitMQ]
    RabbitMQ --> EmailService[email-service]
```

## Application Submission

The applicant opens the external frontend route:

```text
/partner/apply
```

This is a frontend route, not a backend endpoint. The form submits a multipart request to:

```http
POST /tickets
```

The API gateway proxies `/tickets` to `ticket-service`. The request contains:

- `ticketType = Partner`
- `firstName`
- `lastName`
- `email`
- `phoneNumber`
- `identityDocumentFile`

The current partner application form does not send company name, separate contact email, contract file, driver license, or car ownership documents. Backend DTOs have optional `companyName` and `contactEmail` fields, but the implemented external form does not use them. In the ticket domain, missing company/contact data is normalized with fallbacks: company name becomes the applicant full name, and contact email becomes the submitted email.

## Document Storage

The uploaded partner identity document must be a PDF. `ticket-service` validates it and uploads it through `IFileStorageClient`.

The actual upload call goes to internal file-service:

```http
POST /api/internal/files/upload
X-Internal-Api-Key: <file service key>
```

`file-service` returns a stored `fileName`. That file name is stored in the ticket data as `IdentityDocumentFileName`. The file itself is stored by `file-service`: Google Cloud Storage is the default backend, while local `/uploads` storage is used when `USE_WEB_STORAGE=false`.

## Ticket Creation

The created ticket type is:

```text
TicketType.Partner
```

The ticket is created in status:

```text
TicketStatus.Pending
```

The pending ticket is stored in `ticket-db` and appears in the internal manager tickets queue.

## Review Permissions

Review is permission-based, not hardcoded to a single role.

The backend approve endpoint requires:

```text
Ticket.Approve
```

The backend reject endpoint requires:

```text
Ticket.Reject
```

In the current migrations, `manager`, `supermanager`, and `admin` can have these permissions. `supermanager` also has broader ticket visibility through `Ticket.ViewAll`. Therefore, partner onboarding can be reviewed by a manager or supermanager if the JWT contains the required ticket permissions.

## Approve Workflow

When a partner ticket is approved:

1. `ticket-service` marks the ticket as `Approved`.
2. `ticket-service` writes a `ticket.approved` workflow message to its outbox.
3. `TicketWorkflowOutboxDispatcher` processes the outbox message.
4. The dispatcher provisions an identity user in `identity-service`.
5. The dispatcher provisions a partner profile in `partner-service`.
6. The dispatcher publishes a partner approved email event to RabbitMQ.
7. `email-service` consumes the event and sends the approval/set-password email.

The identity provisioning call is:

```http
POST /internal/users/provision
```

It creates an identity user with:

- `subjectType = user`
- `actorType = partner`
- default role `user`
- activation token for password setup

There is no separate `partner` role assigned during provisioning. Partner behavior is represented through `actor_type = partner` and the partner profile.

The partner profile provisioning call is:

```http
POST /internal/partners/provision
```

It creates a partner profile with:

- owner first name
- owner last name
- owner identity document file name
- registration date
- partnership end date, one year after registration
- related identity user id
- phone number
- provision request key for idempotency

`ContractFileName` is passed as `null` in the current partner approval workflow.

## Reject Workflow

When a partner ticket is rejected:

1. `ticket-service` validates that the manager supplied a decision reason.
2. The ticket status becomes `Rejected`.
3. The rejection reason, reviewing manager id, and review timestamp are stored in ticket data.
4. `ticket-service` writes a `ticket.rejected` workflow message to its outbox.
5. `TicketWorkflowOutboxDispatcher` publishes a partner rejected email event to RabbitMQ.
6. `email-service` consumes the event and sends the rejection email.

Reject does not create a user in `identity-service` and does not create a partner profile in `partner-service`.

## Direct Answers

1. **Как partner подаёт заявку: через `/partner/apply`?**
   Да, applicant открывает frontend route `/partner/apply`. Но backend request отправляется не на `/partner/apply`, а на `POST /tickets` с `ticketType=Partner`.

2. **Какие данные/документы partner загружает?**
   В текущем external frontend: owner first name, owner last name, owner email, phone number, owner identity document PDF. Другие partner documents в этой форме не загружаются.

3. **Где сохраняются документы: file-service?**
   Да. `ticket-service` отправляет PDF в `file-service` через internal endpoint `/api/internal/files/upload`, а в ticket хранит возвращённый `fileName`.

4. **Какой ticket type создаётся?**
   Создаётся `TicketType.Partner`.

5. **Кто рассматривает заявку: manager/supermanager?**
   Рассмотрение завязано на permissions. Approve требует `Ticket.Approve`, reject требует `Ticket.Reject`. Эти permissions есть у manager, supermanager и admin в текущих миграциях/ролях.

6. **Что происходит при approve?**
   Ticket становится `Approved`, затем outbox workflow создаёт identity user, создаёт partner profile и отправляет approval email через RabbitMQ/email-service.

7. **Какие сервисы вызываются после approve?**
   После approve вызываются `identity-service`, `partner-service`, RabbitMQ и `email-service`. `file-service` используется раньше, на этапе создания ticket, а не после approve.

8. **Создаётся ли user в identity-service?**
   Да. Создаётся user с `subjectType=user`, `actorType=partner`, default role `user`, временным паролем и activation token для установки пароля.

9. **Создаётся ли partner profile в partner-service?**
   Да. После identity provisioning создаётся partner profile в `partner-service`, связанный с identity user через `RelatedUserId`.

10. **Отправляется ли email notification?**
   Да. При approve публикуется RabbitMQ event `ticket.email.partner-approved`, который обрабатывает `email-service`. При reject публикуется `ticket.email.partner-rejected`.

11. **Что происходит при reject?**
   Ticket становится `Rejected`, сохраняется decision reason, user/profile не создаются, и отправляется rejection email notification.
