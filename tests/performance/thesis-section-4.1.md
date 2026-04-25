# 4.1 Performance Testing

> Drop-in section for the thesis. After running `bash k6/run-all.sh`, replace
> the placeholder table below with the contents of `results/summary.md`.

## Goal

Validate that the backend's user-facing scenarios meet response-time and error-rate
budgets under representative concurrent load. The system under test is the full
docker-compose stack; clients reach it through the API gateway
(`reverse-proxy-service`, exposed on `http://localhost:9186`).

## Methodology

The load tests were implemented with **k6**. Six scenarios were measured, each
mapped to one critical user journey:

| # | Scenario | Endpoint (via API gateway) | Why it's measured |
|---|---|---|---|
| 1 | Login | `POST /identity/auth/login` | Auth latency directly affects time-to-first-interaction. |
| 2 | Car catalog loading | `GET /cars/partner-cars` | Primary read path on the home / search page. |
| 3 | Car details loading | `GET /cars/partner-cars/{id}` | Triggered by every car click; latency-sensitive. |
| 4 | Price preview | `GET /bookings/price-preview` | Runs on every date-range change in the booking flow. |
| 5 | Booking creation | `POST /bookings` | Conversion-critical write path; depends on availability checks. |
| 6 | Ticket queue loading | `GET /tickets/pending` | Manager workspace screen; first request after login. |

Each scenario uses the same load shape: ramp-up to the target VU count, hold
for one minute, ramp-down. Virtual users follow the spec from the chapter
plan — 50 VUs for read scenarios and login, 20 VUs for write/auth-required
scenarios.

The metrics extracted from k6 for each scenario are:

- **Average response time** — `http_req_duration.avg`
- **P95 response time** — `http_req_duration.p(95)`
- **Error rate** — `http_req_failed.rate`
- **Requests per second** — `http_reqs.rate`

A scenario is marked **Passed** when its p95 stays under the per-scenario
budget *and* its error rate stays under the per-scenario error budget. Both
budgets are defined in [tests/performance/k6/lib/config.js](k6/lib/config.js).

## Test environment

- Backend: full `docker compose up` stack (12+ services, PostgreSQL, RabbitMQ).
- Gateway: `http://localhost:9186`.
- Hardware: *<fill in: CPU model, RAM, disk, OS>*
- k6 version: *<fill in: `k6 version`>*

## Results

> Replace the row values below with those produced by
> `tests/performance/results/summary.md` after running `k6/run-all.sh`.

| Test scenario | Users | Avg response time | P95 response time | Error rate | Result |
|---|---|---|---|---|---|
| Login | 50 | … ms | … ms | … % | Passed |
| Catalog loading | 50 | … ms | … ms | … % | Passed |
| Car details | 50 | … ms | … ms | … % | Passed |
| Price preview | 50 | … ms | … ms | … % | Passed |
| Booking creation | 20 | … ms | … ms | … % | Passed |
| Ticket queue | 20 | … ms | … ms | … % | Passed |

## Discussion

*<short paragraph after results are in: which scenarios stayed comfortably
under budget, which were close to the limit, and what hot path (DB query,
external integration call, gateway hop) drove the highest latencies.>*
