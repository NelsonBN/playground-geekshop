# GeekShop

Backend API for a shop, built with .NET 11 and ASP.NET Core Minimal APIs. It
currently manages products and catalogs, stored in SQL Server through EF Core.

## Business Features

- **Product management**: add, view, list, update and delete products.
- **Catalog management**: create, view, list, update and delete catalogs, and
  add or remove the products they contain. A catalog shows its products and a
  product shows the catalogs it belongs to. Products that belong to a catalog
  cannot be deleted.

## Run

Docker is required. Create your `.env` once and set a password for the SQL Server
`sa` user (it is git-ignored; avoid `;` and `=` in it):

```powershell
Copy-Item .env.example .env      # then set MSSQL_SA_PASSWORD in .env
```

Two compose files are provided:

| File | Starts |
|---|---|
| `docker-compose.yml` | SQL Server, the schema init job **and the API** (built from the `Dockerfile`) |
| `docker-compose.dependencies.yml` | Only the dependencies: SQL Server and the schema init job |

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

SQL Server listens on `localhost:1433`. Point the API at it and run it on the host,
using the password from your `.env`:

```powershell
$env:ConnectionStrings__GeekShop = "Server=localhost,1433;Database=GeekShop;User Id=sa;Password=<MSSQL_SA_PASSWORD>;TrustServerCertificate=True"
dotnet run --project .\src\GeekShop.Api\GeekShop.Api.csproj --launch-profile http
```

The API listens at `http://localhost:8080`. Stop the dependencies with:

```powershell
docker compose -f docker-compose.dependencies.yml down   # add -v to delete the data
```

See `AGENTS.md` for the full workflow and quality gates.
