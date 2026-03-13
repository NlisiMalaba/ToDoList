# Multi-Environment CI/CD Pipeline

This document describes the multi-environment deployment pipeline for ToDoList.

## Overview

| Environment | URL | Port | Database | Promotion |
|-------------|-----|------|----------|-----------|
| Staging | http://10.50.30.126:8084 | 8084 | app_staging_db | Auto after build/test |
| Testing | http://10.50.30.126:8082 | 8082 | app_testing_db | 2 QA approvals |
| Production | http://10.50.30.126:8083 | 8083 | app_production_db | QA gate + 3 team lead approvals |

## Pipeline Flow

```
main branch push → Checkout → Build → Test → Build Docker Image (once)
    → Deploy Staging (auto)
    → [2 QA Approvals] → Deploy Testing
    → [QA Decision: Proceed / Reject]
    → [3 Team Lead Approvals] → Deploy Production
```

## Jenkins Setup

### 1. Pipeline Configuration

- **Pipeline script from SCM**: Point to `Jenkinsfile.deploy-multi`
- **Branch**: `main`
- **Trigger**: Poll SCM (e.g. `H/2 * * * *`) or webhook

### 2. Credentials

Create SSH credential with ID `ssh-deploy-10-50-30-126`:
- Username: `adminfeedback` (or your deploy user)
- Private key: Key that can SSH to `10.50.30.126`

### 3. Approval Groups

Configure Jenkins users/groups for input steps:

| Stage | Submitter | Purpose |
|-------|-----------|---------|
| Testing Promotion | `qa-team` | Comma-separated list of QA usernames |
| Production Promotion | `team-leads` | Comma-separated list of team lead usernames |

Example: In Jenkins → Manage Jenkins → Security → Matrix Authorization, create roles `qa-team` and `team-leads`, or use exact usernames: `user1,user2,user3`.

### 4. Environment Files on Server

Create env files on the deployment server at `/secure/todolist/env/` (or set `DEPLOY_ENV_DIR`):

```bash
# On 10.50.30.126
sudo mkdir -p /secure/todolist/env
sudo cp config/.env.staging.example /secure/todolist/env/.env.staging
sudo cp config/.env.testing.example /secure/todolist/env/.env.testing
sudo cp config/.env.production.example /secure/todolist/env/.env.production
# Edit each file with real credentials - never commit secrets
```

### 5. Prerequisites

**Jenkins agent:**
- Docker installed (for building image)
- .NET SDK (for build/test)
- SSH/SCP access to deploy host

**Deploy server (10.50.30.126):**
- Docker
- MySQL instances (or containers) for app_staging_db, app_testing_db, app_production_db
- Env files at `DEPLOY_ENV_DIR`

**MySQL on host:** If MySQL runs on the host (not in Docker), use `Server=host.docker.internal` in `ConnectionStrings__Default`. The pipeline adds `--add-host=host.docker.internal:host-gateway` so containers can reach the host.

## Environment Variables

Each env file controls:

| Variable | Staging/Testing | Production |
|----------|-----------------|------------|
| APP_ENV | staging/testing | production |
| DB_NAME | app_staging_db / app_testing_db | app_production_db |
| BANK_API_URL | https://sandbox.bank-api.com | https://api.bank.com |
| PAYMENTS_ENABLED | false | true |

## Same Image Guarantee

The pipeline builds the Docker image **once** and reuses it for all environments. No rebuild between Staging, Testing, or Production.

## Security Notes

- Never commit `.env.staging`, `.env.testing`, or `.env.production` (only `.example` templates)
- Use Jenkins credentials for DB passwords when possible
- Restrict `DEPLOY_ENV_DIR` permissions: `chmod 600` on env files
