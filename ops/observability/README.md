# Observability

## Назначение
`ops/observability` содержит конфигурацию локального observability-стека AutoRent.

Стек включает:
- `Prometheus` для метрик;
- `Grafana` для дашбордов;
- `Loki` для логов;
- `Promtail` для доставки логов;
- `Tempo` для traces;
- `OpenTelemetry Collector` для приема и маршрутизации telemetry.

## Структура каталогов
| Путь | Назначение |
|---|---|
| `grafana/provisioning` | автоподключение datasource-ов и dashboard provisioning |
| `grafana/dashboards` | JSON dashboard-ы Grafana |
| `prometheus/prometheus.yml` | scrape targets и правила опроса |
| `loki/config.yml` | хранилище и API-конфиг Loki |
| `tempo/tempo.yml` | конфиг Tempo |
| `promtail/promtail.yml` | сбор JSON-логов из service volumes |
| `otel-collector/config.yml` | pipeline для traces/metrics/logs |

## Как запускается
Отдельный compose profile больше не нужен. Observability-сервисы стартуют вместе с обычным:

```bash
docker compose up -d --build
```

Остановка и очистка:

```bash
docker compose down -v --remove-orphans
```

## Порты по умолчанию
- Grafana: `3000`
- Prometheus: `9090`
- Loki: `3100`
- Tempo: `3200`

## Что уже подключено
Метрики в текущем compose собираются Prometheus для:
- `api-gateway`
- `ticket-service`
- `identity-service`

JSON-логи через Promtail собираются для:
- `api-gateway`
- `ticket-service`
- `identity-service`
- `car-service`
- `booking-service`
- `email-service`
- `ai-search-service`

Promtail читает соответствующие Docker volumes:
- `gateway_logs`
- `ticket_logs`
- `identity_logs`
- `car_logs`
- `booking_logs`
- `email_logs`
- `ai_search_logs`

Grafana получает datasource provisioning для Prometheus, Loki и Tempo, а dashboard-ы лежат в `grafana/dashboards`.

## Практическое замечание
Если UI недоступен после `docker compose up -d --build`, сначала проверь:

```bash
docker compose ps
docker compose logs --tail 50 grafana
docker compose logs --tail 50 prometheus
```
