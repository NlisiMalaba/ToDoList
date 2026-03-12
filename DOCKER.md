# Docker build and run

## Quick start

```bash
docker-compose up --build
```

API: http://localhost:8081  
Swagger: http://localhost:8081/swagger

---

## If build fails: "no such host" for mcr.microsoft.com

The Dockerfile uses **Microsoft Container Registry** (mcr.microsoft.com). If Docker cannot resolve that host, try:

1. **Test from your machine**  
   In PowerShell: `ping mcr.microsoft.com`  
   If this fails, the problem is network/DNS on your side, not Docker.

2. **Set DNS in Docker Desktop**  
   - Docker Desktop → **Settings** → **Docker Engine**  
   - Add or adjust DNS in the JSON, e.g.:
   ```json
   "dns": ["8.8.8.8", "1.1.1.1"]
   ```
   - **Apply & Restart**

3. **Corporate proxy**  
   If you use an HTTP/HTTPS proxy:  
   Docker Desktop → **Settings** → **Resources** → **Proxies** → enable and set proxy URL.

4. **Pull the image manually**  
   Run: `docker pull mcr.microsoft.com/dotnet/sdk:8.0`  
   If it succeeds, run `docker-compose up --build` again.

5. **VPN / different network**  
   Try from another network (e.g. mobile hotspot) to see if your usual network blocks MCR.

---

## If you cannot use MCR at all

Build and run **without** building the API inside Docker:

1. **Install .NET 8 SDK** and run MySQL (e.g. via `docker-compose up db` only).
2. **Publish locally:**
   ```bash
   cd src/ToDoList.Api
   dotnet publish -c Release -o ../../publish
   ```
3. **Run the app:**
   ```bash
   cd ../../publish
   dotnet ToDoList.Api.dll
   ```
   Set `ConnectionStrings__Default` to point at your MySQL (e.g. `Server=localhost;Port=3306;...`).

---

## Three-environment deployment (Docker Compose on VMs)

This repo supports **Staging**, **Testing**, and **Production** as three isolated Compose “projects” (stacks).

### Files
- Base compose (local dev defaults): `[docker-compose.yml](docker-compose.yml)`
- Environment overrides:
  - `[docker-compose.staging.yml](docker-compose.staging.yml)`
  - `[docker-compose.testing.yml](docker-compose.testing.yml)`
  - `[docker-compose.production.yml](docker-compose.production.yml)`
- Env file templates (copy to secure location on each VM; do not commit real secrets):
  - `[deploy/.env.staging.example](deploy/.env.staging.example)`
  - `[deploy/.env.testing.example](deploy/.env.testing.example)`
  - `[deploy/.env.production.example](deploy/.env.production.example)`

### Recommended layout on each VM
- Create a secure directory, e.g.:
  - `/secure/todolist/`
- Copy the appropriate env file there:
  - `/secure/todolist/staging.env`
  - `/secure/todolist/testing.env`
  - `/secure/todolist/production.env`

### Deploy commands (per environment)
Staging:

```bash
docker compose -p todolist-staging --env-file /secure/todolist/staging.env -f docker-compose.yml -f docker-compose.staging.yml up -d
```

Testing:

```bash
docker compose -p todolist-testing --env-file /secure/todolist/testing.env -f docker-compose.yml -f docker-compose.testing.yml up -d
```

Production:

```bash
docker compose -p todolist-prod --env-file /secure/todolist/production.env -f docker-compose.yml -f docker-compose.production.yml up -d
```

### Operational notes
- **Edge (reverse proxy)**: each environment override adds a small edge proxy (Caddy) that publishes the HTTP port. The API container itself is not published directly.
- **Do not expose MySQL ports publicly**. Restrict DB access to the Docker network / localhost (prefer removing DB port publishing entirely on shared hosts).
- **Use distinct credentials per environment** (already modeled in the env templates).
- **Promote the same image tag** across environments; only env files change.
- **Promotion workflow**: see `[deploy/PROMOTION.md](deploy/PROMOTION.md)` for build-once / promote-same-tag guidance.

### Lock down staging/testing
- Staging and testing edge proxies are protected with **basic auth** by default. Generate password hashes on the VM:

```bash
docker run --rm caddy:2 caddy hash-password --plaintext "your-password"
```

- Populate the `*_BASIC_AUTH_PASSWORD_HASH` values in the env file.
