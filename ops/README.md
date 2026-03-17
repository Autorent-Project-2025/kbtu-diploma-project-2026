# Ops

## Назначение
`ops` содержит operational-конфигурацию проекта:
- observability-конфиги;
- provisioning для Grafana;
- конфиги Prometheus, Loki, Tempo, Promtail и OpenTelemetry Collector.

Это не код сервисов и не application-level бизнес-логика. Здесь лежит всё, что нужно для эксплуатации, диагностики и локального наблюдения за системой.

## Структура
| Путь | Назначение |
|---|---|
| `ops/observability/prometheus` | scrape-конфиг для Prometheus |
| `ops/observability/loki` | конфиг Loki |
| `ops/observability/tempo` | конфиг Tempo |
| `ops/observability/promtail` | сбор логов из Docker volumes |
| `ops/observability/otel-collector` | OpenTelemetry Collector pipeline |
| `ops/observability/grafana` | datasource provisioning и dashboard-ы Grafana |

## Запуск
Observability-стек теперь входит в обычный запуск compose и не требует отдельного profile:

```bash
docker compose up -d --build
```

Основные UI и endpoints:
- Grafana: `http://localhost:3000`
- Prometheus: `http://localhost:9090`
- Loki API: `http://localhost:3100/loki/api/v1/query`
- Tempo ready: `http://localhost:3200/ready`

## Связанный compose
Конфиги из `ops/observability` монтируются в сервисы `prometheus`, `grafana`, `loki`, `tempo`, `promtail` и `otel-collector` из корневого `docker-compose.yml`.

Подробности по составу observability-конфигов описаны в `ops/observability/README.md`.
