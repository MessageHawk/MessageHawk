# MessageHawk

Message tracker for ESB-style integrations: ingest log steps through a **RabbitMQ** queue (sharded by interchange id for strict per-interchange ordering), persist to **SQL Server** via **EF Core**, and browse timelines in a **React** SPA hosted by **ASP.NET Core**. **Entra ID** is wired for the API (optional local development authentication is available).

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) (for the SPA)
- Docker (optional, for RabbitMQ and SQL Server)
- SQL Server or LocalDB (if not using Docker SQL)

## Quick start (Docker dependencies)

```powershell
docker compose up -d
```

Use a connection string pointing at the container, for example:

`Server=localhost,1433;Database=MessageHawk;User Id=sa;Password=MessageHawk_Dev_123;TrustServerCertificate=True;MultipleActiveResultSets=true`

Set `ConnectionStrings:DefaultConnection` in `src/MessageHawk.Api/appsettings.Development.json` and `src/MessageHawk.Worker/appsettings.Development.json`, or use user secrets / environment variables.

RabbitMQ management UI: http://localhost:15672 (guest / guest).

## Database

The API uses **EF Core** with **SQL Server** (default) or **PostgreSQL**. Set `Database:Provider` to `SqlServer` or `PostgreSQL` and configure `ConnectionStrings:DefaultConnection` accordingly. PostgreSQL migrations: add `--context MessageHawkDbContextNpgsql` to `dotnet-ef` commands; SQL Server uses `--context MessageHawkDbContextSqlServer`.

Apply migrations (or run the API once in Development — it calls `Migrate()` automatically):

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update `
  --project src/MessageHawk.Infrastructure/MessageHawk.Infrastructure.csproj `
  --startup-project src/MessageHawk.Api/MessageHawk.Api.csproj `
  --context MessageHawkDbContextSqlServer
```

## Run the backend

Terminal 1 — API (HTTPS profile matches the Vite proxy):

```powershell
dotnet run --project src/MessageHawk.Api/MessageHawk.Api.csproj --launch-profile https
```

Terminal 2 — worker (consumes all shard queues locally when `Worker:AssignedShardIndex` is null; use `RabbitMq:ShardCount: 1` in Development to match the API):

```powershell
dotnet run --project src/MessageHawk.Worker/MessageHawk.Worker.csproj
```

- API / Swagger: `https://localhost:7079` (HTTP `http://localhost:5206`)
- Health: `GET /health`

### Development authentication

`src/MessageHawk.Api/appsettings.Development.json` sets `Authentication:DisableJwt` to `true`, which registers a **development-only** authentication handler so you can call the API without Entra tokens. **Do not use this in production.**

For Entra ID, set `Authentication:DisableJwt` to `false` and fill in the `AzureAd` section (tenant, API client id, audience). The SPA will need `@azure/msal-browser` (or similar) to acquire tokens and send `Authorization: Bearer …` on API calls.

## Run the SPA

```powershell
cd client/messagehawk-web
npm install
npm run dev
```

Vite proxies `/api` and `/health` to `https://localhost:7079`. For a production-style single host, `npm run build` writes static files to `src/MessageHawk.Api/wwwroot`; then serve via `dotnet run` on the API.

## Ingest a log step

`POST /api/v1/interchanges/{interchangeId}/steps` with a JSON body (camelCase):

```json
{
  "stepId": "b0f0c0d0-e0f0-4000-a000-000000000001",
  "messageTypeCode": "DEMO",
  "messageTypeDisplayName": "Demo type",
  "sequenceNumber": 1,
  "sender": "APP-A",
  "receiver": "APP-B",
  "status": "Received",
  "occurredAt": "2026-05-02T12:00:00Z",
  "contentType": "text/plain",
  "bodyBase64": "SGVsbG8sIE1lc3NhZ2VIYXdr",
  "indexedProperties": { "orderId": "12345" }
}
```

Sequence numbers must increase by 1 per interchange after the first step. The worker enforces ordering together with **one consumer per shard** (and matching `RabbitMq:ShardCount` everywhere).

## Sharding and scale-out

- **Publisher** (API) routes messages with `hash(interchangeId) mod RabbitMq:ShardCount`.
- **Workers**: leave `Worker:AssignedShardIndex` unset to consume every shard in one process (good for dev). For Kubernetes-style scaling, run one pod per shard and set `Worker:AssignedShardIndex` to `0..ShardCount-1` with a matching `RabbitMq:ShardCount` across the cluster.

## Solution layout

| Project | Role |
|--------|------|
| `MessageHawk.Api` | HTTP API, SPA static files, Swagger |
| `MessageHawk.Worker` | RabbitMQ consumers → persist steps |
| `MessageHawk.Domain` | Entities |
| `MessageHawk.Application` | Contracts, ingest DTOs, sharding helper |
| `MessageHawk.Infrastructure` | EF Core (SQL Server), RabbitMQ |
| `client/messagehawk-web` | React + Vite |

## License

MIT. See [LICENSE](LICENSE).
