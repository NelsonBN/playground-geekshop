# GeekShop

Backend API for a shop, built with .NET 11 and ASP.NET Core Minimal APIs. It
currently manages products and catalogs, stored in SQL Server through EF Core.

## Business Features

- **Product management**: add, view, list, update and delete products.
- **Catalog management**: create, view, list, update and delete catalogs, and
  add or remove the products they contain. A catalog shows its products and a
  product shows the catalogs it belongs to. Products that belong to a catalog
  cannot be deleted.
- **Change notifications**: every change to a product or a catalog is announced
  to other systems, so they can react to it.

## Integration events

Each successful write publishes one event to RabbitMQ, after the change
is saved. Reads, and writes that are refused (not found, product still in a
catalog, invalid data), publish nothing. The service only publishes for now;
nothing consumes the events yet.

There is one durable topic exchange per area (`products` and `catalogs`). The
event name is the routing key and the message `type` (and the body is JSON in
camelCase, with an `eventId` and `occurredAt`). Consumers declare their own
queue and bind it to an exchange, for example with the routing key `#` for every
event or `ProductAdded` for one:

Publishing is direct, not transactional: if the broker is down after the change
was saved, the request fails and that event is lost.

Configuration (environment variables or `appsettings.json`):

| Key | Meaning |
|---|---|
| `RabbitMq__ConnectionString` | AMQP URI of the broker, for example `amqp://guest:guest@localhost:5672`. The API declares the two exchanges at startup when missing, so the user needs *configure* permission |

## Run

Docker is required. Create your `.env` once and set a password for the SQL Server
`sa` user (it is git-ignored; avoid `;` and `=` in it):

```powershell
Copy-Item .env.example .env      # then set MSSQL_SA_PASSWORD in .env
```

Two compose files are provided:

| File | Starts |
|---|---|
| `docker-compose.yml` | SQL Server, the schema init job, RabbitMQ **and the API** (built from the `Dockerfile`) |
| `docker-compose.dependencies.yml` | Only the dependencies: SQL Server, the schema init job and RabbitMQ |

`docker-compose.yml` reuses the services of `docker-compose.dependencies.yml`
through `extends`, so both always start SQL Server the same way. The schema comes
from `tests/init-db.sql`, run by a one-shot `mssqltools` container
(`tests/init-db.sh`); the API never creates it.

### Full stack (dependencies + API)

```powershell
docker compose up --build -d
docker compose logs -f api       # follow the API logs
```

The API is served at `http://localhost:8080` (see `tests/Demo.http` for requests).
It starts only after SQL Server is healthy and the schema was created. Stop it with:

```powershell
docker compose down              # add -v to also delete the database volume
```

### Only the dependencies (run the API from your IDE or `dotnet run`)

```powershell
docker compose -f docker-compose.dependencies.yml up -d
```

SQL Server listens on `localhost:1433` and RabbitMQ on `localhost:5672` (management UI at `http://localhost:15672`, user and password `guest`). Point the API at it and run it on the host,
using the password from your `.env`:

```powershell
$env:ConnectionStrings__GeekShop = "Server=localhost,1433;Database=GeekShop;User Id=sa;Password=<MSSQL_SA_PASSWORD>;TrustServerCertificate=True"
$env:RabbitMq__ConnectionString = "amqp://guest:guest@localhost:5672"
dotnet run --project .\src\GeekShop.Api\GeekShop.Api.csproj --launch-profile http
```

The API listens at `http://localhost:8080`. Stop the dependencies with:

```powershell
docker compose -f docker-compose.dependencies.yml down   # add -v to delete the data
```

See `AGENTS.md` for the full workflow and quality gates.
