# Docker build and run

## Quick start

```bash
docker-compose up --build
```

API: http://localhost:8080  
Swagger: http://localhost:8080/swagger

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
