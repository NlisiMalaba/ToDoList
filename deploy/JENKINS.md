# Jenkins pipeline: single branch, three environments

## Overview
- One long-lived branch: `main`.
- One Jenkins pipeline (see `Jenkinsfile`).
- Build & publish **once** on merge to `main`.
- Promote the **same image tag** through:
  - Staging → Testing → Production
  - using manual approval gates.

## Required Jenkins setup

### Credentials
- **Docker registry credentials**
  - ID: `docker-registry-creds` (or override `DOCKER_CREDS_ID` in job env).
  - Permissions: push/pull `DOCKER_REGISTRY/DOCKER_IMAGE`.
- **SSH deploy credentials**
  - ID: `ssh-deploy` (or override `SSH_CREDS_ID`).
  - Type: “SSH Username with private key”.
  - Access: SSH to each environment VM.

### Global/job environment variables
At minimum set these in the Jenkins job:
- `DOCKER_REGISTRY`: e.g. `myregistry.local` or `registry.hub.docker.com/myorg`
- `DOCKER_IMAGE`: e.g. `todolist-api`
- `STAGING_HOST`, `TESTING_HOST`, `PROD_HOST`: hostnames/IPs of the VMs.
- `REMOTE_APP_PATH`: path on each VM where the repo lives (e.g. `/opt/ToDoList`).
- `REMOTE_ENV_STAGING`, `REMOTE_ENV_TESTING`, `REMOTE_ENV_PRODUCTION`:
  - paths to the env files (`/secure/todolist/*.env`).

## Pipeline stages (high level)
1. **Checkout** (main only).
2. **Build & Test**
   - `dotnet restore`, `dotnet build`, `dotnet test`.
3. **Build & Push Image**
   - Tag: short git SHA (e.g. `IMAGE_TAG=abcdef1`).
   - Push: `${DOCKER_REGISTRY}/${DOCKER_IMAGE}:${IMAGE_TAG}`.
4. **Deploy to Staging**
   - Runs `docker compose ... -f docker-compose.yml -f docker-compose.staging.yml up -d` on the staging VM.
5. **Approve: Promote to Testing**
   - Jenkins `input` step; can be restricted to QA group.
6. **Deploy to Testing**
   - Same `IMAGE_TAG`, different host/env file and compose override.
7. **Approve: Promote to Production**
   - Jenkins `input` step; can be restricted to ops/business approvers.
8. **Deploy to Production**
   - Same `IMAGE_TAG`, different host/env file and compose override.

## Artifact pinning (handling long approvals)
- The pipeline computes `IMAGE_TAG` from the git commit (`shortSha`) and pushes that tag.
- The **exact tag** is then reused for Staging, Testing, and Production.
- Approvals gate **promotion of that pinned tag**, not “whatever is latest on main”.
- If approvals take weeks and the tag is now outdated or known-bad, simply:
  - do not approve it; let newer builds run.
  - approve a newer `IMAGE_TAG` when ready.

## Secrets handling
- No registry passwords or SSH keys are stored in git.
- Jenkins credentials store holds:
  - registry creds (`docker-registry-creds`).
  - SSH keys (`ssh-deploy`).
- Each VM stores its env file (per environment):
  - `/secure/todolist/staging.env`
  - `/secure/todolist/testing.env`
  - `/secure/todolist/production.env`
- These env files contain:
  - DB credentials
  - edge/basic-auth passwords (for non-prod)
  - ports and other per-environment details.

## Database migrations
- Staging/Testing:
  - `Database:ApplyMigrationsOnStartup` is `true` in `appsettings.Staging.json` and `appsettings.Testing.json`.
  - When a new image is deployed, the API applies pending EF Core migrations automatically on startup.
- Production:
  - `Database:ApplyMigrationsOnStartup` is `false` in `appsettings.Production.json`.
  - Recommended:
    1. Take a DB backup/snapshot.
    2. Run migrations explicitly (one-off job/command) against the production DB.
    3. Deploy the new image tag via the Jenkins Production stage.
  - Alternative (temporary):
    - For a specific deployment, set `Database__ApplyMigrationsOnStartup=true` via the production env file, deploy, verify, then revert to `false`.

See also:
- `deploy/BRANCHING.md` for branching strategy.
- `deploy/PROMOTION.md` for image promotion details.

