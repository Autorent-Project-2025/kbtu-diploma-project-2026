# Changelog — AutoRent

## [Март 2026] — Улучшения UX и новые фичи

---

### 🔴 403 страница (external)

**Проблема:** при 403 ответе от бэкенда пользователь видел пустой экран или чужое содержимое.

**Что сделано:**
- Добавлена страница `ForbiddenView.vue` — тёмный экран с иконкой замка, кнопками «Назад» и «На главную»
- В `axios.ts` добавлен перехватчик: любой 403 ответ автоматически редиректит на `/403`
- Добавлен маршрут `/403` в `router/index.ts`

**Файлы:**
- `frontend/external/src/views/ForbiddenView.vue` — новый
- `frontend/external/src/api/axios.ts` — обновлён
- `frontend/external/src/router/index.ts` — обновлён

---

### 👋 Онбординг-баннер в ProfileView (external)

**Проблема:** новый пользователь мог не знать, что для бронирования нужно загрузить документы.

**Что сделано:**
- Баннер появляется если у пользователя не загружены удостоверение личности или водительские права
- Кнопка «Заполнить» открывает форму редактирования профиля прямо на странице
- Баннер можно закрыть — решение запоминается в `localStorage` (`profile_onboarding_dismissed`)

**Файлы:**
- `frontend/external/src/views/ProfileView.vue` — обновлён

---

### 🚗 Empty states с CTA (external)

**Проблема:** пустые секции в ProfileView показывали только текст, без подсказки что делать дальше.

**Что сделано:**
- Секция «История броней»: при отсутствии броней показывается иконка 🚗, описание и кнопка **«Выбрать автомобиль →»** ведущая на `/cars`
- Секция «Мои отзывы»: при отсутствии отзывов показывается иконка ⭐, подсказка и ссылка **«Перейти к автомобилям»**

**Файлы:**
- `frontend/external/src/views/ProfileView.vue` — обновлён

---

### ❌ Партнёр отменяет бронирование (backend + external)

**Проблема:** партнёр не мог отменить бронирование на своей машине — существующий эндпоинт проверял только `UserId` клиента.

**Что сделано:**

*Backend:*
- Добавлен метод `CancelBookingByPartner(int id, Guid partnerUserId)` в `IBookingService` и реализацию в `BookingService`
- Поиск брони идёт по `PartnerUserId`, отмена блокируется для статусов `Completed` и `Canceled`
- Добавлен эндпоинт `POST /bookings/{id}/partner-cancel` в `BookingController`

*Frontend:*
- В `partners.ts` добавлена функция `cancelPartnerBooking(bookingId)`
- В `PartnerBookingsView.vue` добавлена колонка с кнопкой **«Отменить»** — видна только для статусов `pending` и `confirmed`
- Кнопка блокируется на время запроса, результат отображается через toast

**Файлы:**
- `backend/.../BookingService.Application/Interfaces/IBookingService.cs` — обновлён
- `backend/.../BookingService.Infrastructure/Services/BookingService.cs` — обновлён
- `backend/.../BookingService.Api/Controllers/BookingController.cs` — обновлён
- `frontend/external/src/api/partners.ts` — обновлён
- `frontend/external/src/views/PartnerBookingsView.vue` — обновлён

---

### 🔔 Toast-уведомления в internal панели

**Проблема:** при одобрении или отказе заявки страница молча обновлялась — менеджер не получал никакого подтверждения.

**Что сделано:**
- Перенесена вся toast-система из `external` в `internal` (`useToast`, `ToastItem`, `ToastContainer`, типы)
- `App.vue` обновлён — добавлен `<ToastContainer />` поверх всего контента
- В `ManagerTicketsView.vue` все `errorMessage`/`successMessage` заменены на вызовы `toastSuccess()` / `toastError()`
  - ✅ Одобрение заявки → зелёный попап «✓ Заявка одобрена»
  - ❌ Отказ заявки → красный попап «✕ Заявка отклонена»
  - Ошибки валидации и сетевые ошибки также отображаются через toast

**Файлы:**
- `frontend/internal/src/App.vue` — обновлён
- `frontend/internal/src/views/ManagerTicketsView.vue` — обновлён
- `frontend/internal/src/composables/useToast.ts` — новый
- `frontend/internal/src/types/Toast.ts` — новый
- `frontend/internal/src/components/ToastContainer.vue` — новый
- `frontend/internal/src/components/ToastItem.vue` — новый
