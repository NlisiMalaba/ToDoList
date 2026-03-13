# Image promotion workflow (Staging → Testing → Production)

## Principle
- **Build once**: create a single immutable container image tag.
- **Promote the same tag** through Staging → Testing → Production.
- **Config differs per environment** via env files and `ASPNETCORE_ENVIRONMENT`; **code does not**.

## Tagging strategy
Use a unique tag per build, for example:
- `IMAGE_TAG=1.3.0`
- `IMAGE_TAG=1.3.0+<short_sha>` (preferred)

## Build & publish (CI)
1. Build and test:
   - `dotnet test`
2. Build image:
   - `docker build -t <registry>/todolist-api:${IMAGE_TAG} .`
3. Push image:
   - `docker push <registry>/todolist-api:${IMAGE_TAG}`

## Deploy to Staging (Dev-only)
On the Staging VM:

```bash
export DOCKER_REGISTRY=<registry>
export IMAGE_TAG=<tag>
docker compose -p todolist-staging --env-file /secure/todolist/staging.env -f docker-compose.yml -f docker-compose.staging.yml pull
docker compose -p todolist-staging --env-file /secure/todolist/staging.env -f docker-compose.yml -f docker-compose.staging.yml up -d
```

### Smoke checks (minimum)
- `GET /health` returns 200
- `GET /swagger` reachable only behind staging access controls
- Create/update/list one todo end-to-end

## Promote to Testing (QA)
After Staging is green, deploy **the same** `IMAGE_TAG` to the Testing VM:

```bash
export DOCKER_REGISTRY=<registry>
export IMAGE_TAG=<tag>
docker compose -p todolist-testing --env-file /secure/todolist/testing.env -f docker-compose.yml -f docker-compose.testing.yml pull
docker compose -p todolist-testing --env-file /secure/todolist/testing.env -f docker-compose.yml -f docker-compose.testing.yml up -d
```

### QA checks
- Regression test suite
- Performance baseline (basic)
- Validate error responses and logs don’t leak sensitive details

## Promote to Production (Clients)
After QA sign-off, deploy **the same** `IMAGE_TAG` to the Production VM:

```bash
export DOCKER_REGISTRY=<registry>
export IMAGE_TAG=<tag>
docker compose -p todolist-prod --env-file /secure/todolist/production.env -f docker-compose.yml -f docker-compose.production.yml pull
docker compose -p todolist-prod --env-file /secure/todolist/production.env -f docker-compose.yml -f docker-compose.production.yml up -d
```

## Database migrations (Production)
Production defaults to:
- `Database:ApplyMigrationsOnStartup=false` (see `appsettings.Production.json`)

Recommended production flow:
1. Take a DB backup / snapshot.
2. Run migrations explicitly (from a controlled runner) before switching traffic:
   - Option A: a one-off container/job that runs `dotnet ef database update`
   - Option B: temporarily set `Database__ApplyMigrationsOnStartup=true` for the deployment, then set it back to `false`

## Rollback
Rollback is simply redeploying the previous known-good `IMAGE_TAG`:
- `IMAGE_TAG=<previous_tag>` then `docker compose ... up -d`

If a migration was applied, rollback may require a DB restore unless migrations are reversible.

