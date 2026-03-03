# AutoRent

## О проекте
AutoRent - микросервисная платформа каршеринга.

В репозитории находятся:
- 9 backend-сервисов (external/internal/shared);
- 3 frontend-приложения (external/internal/superadmin);
- общая оркестрация через `docker-compose.yml` в корне.

Диаграмма архитектуры: `docs/project-architecture.puml`.

## Быстрый старт
1. Заполните нужные `.env` файлы по `.env.example`.
2. Из корня проекта выполните:

```bash
docker compose up --build
```

Основные порты по умолчанию:
- API Gateway: `9186`
- External Frontend: `5173`
- Internal Frontend: `5174`
- Superadmin Frontend: `5175`
- Identity Service: `1244`
- Car Service: `1298`
- Booking Service: `1821`
- Ticket Service: `1248`
- Image Service: `9181`
- Email Service: `9182`

## Сервисы
| Сервис | Путь | Назначение |
|---|---|---|
| Identity Service | `backend/shared/identity-service` | Auth, users, roles, permissions, JWKS |
| Car Service | `backend/external/car-service` | Каталог автомобилей |
| Booking Service | `backend/external/booking-service` | Бронирования |
| Ticket Service | `backend/internal/ticket-service` | Заявки на регистрацию/верификацию |
| Image Service | `backend/shared/image-service` | Загрузка/удаление изображений |
| Email Service | `backend/shared/email-service` | SMTP-уведомления |
| API Gateway | `backend/external/reverse-proxy-service` | Проксирование `/identity`, `/cars`, `/bookings`, `/tickets`, `/internal` |
| Client Service | `backend/external/client-service` | Не реализован (заготовка) |
| Partner Service | `backend/internal/partner-service` | Не реализован (заготовка) |
| External Frontend | `frontend/external` | Пользовательский UI |
| Internal Frontend | `frontend/internal` | UI менеджера |
| Superadmin Frontend | `frontend/superadmin` | UI супер-админа |

## Модель прав (permissions)
Права передаются в JWT в claim `permissions`.

### Backend-права по сервисам
| Сервис | Необходимые права |
|---|---|
| Identity Service | `User.View`, `User.Create`, `User.Update`, `User.AssignRole`, `User.RemoveRole`, `User.Activate`, `User.Deactivate`, `User.Delete`, `Role.View`, `Role.Create`, `Role.AssignPermission`, `Permission.View`, `Permission.Create` |
| Car Service | `Car.Create`, `Car.Update`, `Car.Delete`, `Car.Image.Create` |
| Booking Service | `Booking.Create` (для создания), остальные пользовательские операции требуют валидный JWT |
| Ticket Service | `Ticket.View`, `Ticket.Approve`, `Ticket.Reject` |
| Image Service | Не требуются (в текущей реализации) |
| Email Service | Не требуются (в текущей реализации) |
| API Gateway | Не требуются (в текущей реализации) |

### Frontend-права
- External Frontend: просмотр каталога и создание тикета без прав; бронирование требует JWT, создание брони - `Booking.Create`.
- Internal Frontend: доступ к списку заявок - `Ticket.View`; approve/reject - `Ticket.Approve`/`Ticket.Reject`.
- Superadmin Frontend: доступ к разделу пользователей - `User.View`; дальнейшее управление зависит от соответствующих `User.*` и `Role.View`.

## Документация по сервисам
Подробная документация находится в `README.md` каждого сервиса.
