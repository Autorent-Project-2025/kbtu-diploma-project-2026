# Деплой AutoRent бесплатно: Oracle Cloud + Vercel

| Что | Где | Стоимость |
|---|---|---|
| Бэкенд: 15 сервисов, БД, RabbitMQ, Redis, Ollama | 1 VM Oracle Cloud Always Free (ARM, 4 CPU / 24 GB RAM) | $0 |
| HTTPS для API | Caddy + Let's Encrypt на домене `api.<ip>.sslip.io` | $0 |
| 3 фронтенда | Vercel Hobby, 3 проекта | $0 |

Render (free) не подходит: 512 MB RAM на сервис, сервисы засыпают, нет private services, только одна Postgres на 30 дней, Ollama не запустится.

---

## 1. Сервер в Oracle Cloud

1. Зарегистрироваться на <https://www.oracle.com/cloud/free/>. Карта нужна только для верификации, с Always Free ничего не списывается.
2. **Compute → Instances → Create instance**:
   - Image: **Ubuntu 24.04** (не Oracle Linux).
   - Shape: **Ampere → VM.Standard.A1.Flex**, **4 OCPU, 24 GB RAM**.
   - Boot volume: **100 GB** (образы .NET SDK + torch + модели Ollama занимают много места).
   - SSH key: загрузить свой `~/.ssh/id_ed25519.pub` или скачать сгенерированный.
   - Если пишет *Out of capacity*: попробовать другой Availability Domain или повторить позже. Это нормально для free-тира.
3. **Открыть порты в облачном файрволе**: Instance → Subnet → Security List → *Add Ingress Rules*:
   - Source `0.0.0.0/0`, TCP, порт `80`
   - Source `0.0.0.0/0`, TCP, порт `443`
4. Записать **Public IP** инстанса.

## 2. Запуск бэкенда

```bash
ssh ubuntu@<PUBLIC_IP>
git clone https://github.com/Autorent-Project-2025/kbtu-diploma-project-2026.git autorent
cd autorent
./deploy/deploy.sh
```

Что делает скрипт:

- ставит Docker и открывает 80/443 в iptables (на образах Oracle они закрыты);
- создаёт `.env` с `API_DOMAIN=api.<ip-через-дефисы>.sslip.io`;
- **генерирует новую JWT-пару и новые internal API-ключи**: те, что лежат в `.env.example`, публичные, и с ними любой может выпустить себе админский токен. Секреты сохраняются в `deploy/secrets/` (в git не попадают);
- поднимает всё через `docker-compose.yml` + `docker-compose.prod.yml` без фронтов и observability. Наружу открыт только Caddy на 80/443.

Первая сборка на ARM занимает 20–40 минут, плюс загрузка моделей Ollama. Проверить:

```bash
curl https://api.<ip-через-дефисы>.sslip.io/healthz
docker compose -f docker-compose.yml -f docker-compose.prod.yml ps
```

Если ссылки на фронты отличаются от `https://autorent-{external,internal,superadmin}.vercel.app`, поправьте `*_FRONTEND_URL` в `.env` и перезапустите `./deploy/deploy.sh`.

SMTP (письма активации): заполните `SMTP_USER` / `SMTP_PASS` в `backend/shared/email-service/.env` (например, Gmail App Password).

## 3. Фронтенды на Vercel

Для каждого из трёх приложений: **Add New → Project → Import** репозитория:

| Project name | Root Directory | Env `VITE_API_URL` |
|---|---|---|
| `autorent-external` | `frontend/external` | `https://api.<ip>.sslip.io` |
| `autorent-internal` | `frontend/internal` | `https://api.<ip>.sslip.io` |
| `autorent-superadmin` | `frontend/superadmin` | `https://api.<ip>.sslip.io` |

Framework определится как Vite. `vercel.json` в каждой папке уже настраивает сборку и SPA-роутинг. Опция *Include files outside the root directory* должна быть включена (по умолчанию так и есть), потому что приложения импортируют `frontend/shared`.

## 4. Обновление

```bash
cd ~/autorent && ./deploy/deploy.sh --pull
```

Фронты на Vercel обновляются сами при пуше в `main`.

## Полезное

```bash
C="docker compose -f docker-compose.yml -f docker-compose.prod.yml"
$C logs -f api-gateway          # логи gateway
$C restart booking-service      # перезапуск сервиса
$C --profile observability up -d   # включить Grafana/Prometheus/Loki (Grafana только через SSH-туннель)
```
