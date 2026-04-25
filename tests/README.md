# Tests

Top-level folder for project test artifacts referenced by the thesis chapter
*4. Experimental Evaluation*. Each subfolder corresponds to one section of
that chapter.

| Folder | Thesis section | Tool | Purpose |
|---|---|---|---|
| [performance/](performance/) | 4.1 Performance testing | k6 | Load tests of key backend scenarios — login, catalog, car details, price preview, booking creation, ticket queue. |

> Future sections (4.2–4.4) will add their own subfolders here:
> `event-driven/`, `rbac/`, `observability/`.

## Prerequisites

The test suite assumes the project's docker-compose stack is **running locally**:

```bash
docker compose up -d
```

The API gateway is exposed at `http://localhost:9186` (see `docker-compose.yml`,
service `api-gateway`). The performance scripts default to that URL.

Demo users are seeded by migration `V11__seed_default_demo_users.sql` in
`backend/shared/identity-service` and used as test credentials.

## See also

- [performance/README.md](performance/README.md) — how to run the k6 suite
  and interpret the results.
