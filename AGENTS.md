# AGENTS.md

## Project Overview

**GeekShop** is a backend API for a shop. It currently manages products and
should be easy to extend with other business features.

### Tech Stack

The intended stack is:

- **.NET 11**.
- **SQL Server** and **EF Core** for persistence.
- **Docker** for external dependencies.

### Features

- Product management.
- Catalog management (catalogs group products; products can be added to and
  removed from catalogs).

For the user-facing feature list, API routes, health checks and run
instructions, read [`README.md`](README.md) when you need that context (for
example before adding or changing a feature, or when documenting it).

## Commands

### Build

```powershell
dotnet build .\src\GeekShop.Api\GeekShop.Api.csproj
```

The root solution `GeekShop.slnx` contains the API and both test projects
(`dotnet build .\GeekShop.slnx`).

### Run

```powershell
dotnet run --project .\src\GeekShop.Api\GeekShop.Api.csproj --launch-profile http
```

The current `http` launch profile listens at `http://localhost:8080`. For
example, send `GET http://localhost:8080/products` or use `./tests/Demo.http`.
Products are stored in SQL Server through EF Core, so a database must be running
and `ConnectionStrings__GeekShop` must point at it. The application never creates
the schema: `tests/init-db.sql` does, and docker-compose runs it.

Two compose files exist (see `README.md`): `docker-compose.yml` runs the whole
stack (SQL Server, schema init, and the API built from the `Dockerfile`, served at
`http://localhost:8080`), and `docker-compose.dependencies.yml` runs only the
dependencies so the API can run from the IDE. The first reuses the second through
`extends`; both share container names, so stop one before starting the other:

```powershell
Copy-Item .env.example .env   # then set MSSQL_SA_PASSWORD in .env
docker compose up --build -d                              # full stack
docker compose -f docker-compose.dependencies.yml up -d   # dependencies only
docker compose down -v                                    # stop and drop the data volume
```

Never commit `.env` or put a password in a tracked file.

### Test

Test projects live under `tests/` and `global.json` selects the Microsoft
Testing Platform runner. Follow the
[`dotnet-tests` skill](.claude/skills/dotnet-tests/SKILL.md) for the required
project layout, test-first workflow and quality gates.

Test rules (details in the skill):

- Unit tests cover behaviour (use cases, business rules) and substitute every
  interface (e.g. `IProductsRepository`) with NSubstitute.
- Never write tests for data-only types (DTOs, request/response records,
  entities): they have no business value.
- Platform code (endpoints, EF Core repository, `DbContext`, DI/health setup)
  is proven in `GeekShop.Api.IntegrationTests`, never in unit tests.
- Only the integration test **project** is named `*.IntegrationTests`; the files
  and classes inside follow the unit-test convention `<ClassUnderTest>Tests.cs`.

Run the skill's commands from the repository root:

```powershell
# Unit tests
dotnet run .\.claude\skills\dotnet-tests\scripts\tests.cs -- unit

# Integration tests
dotnet run .\.claude\skills\dotnet-tests\scripts\tests.cs -- integration

# Coverage gate
dotnet run .\.claude\skills\dotnet-tests\scripts\tests.cs -- coverage

# Mutation tests
dotnet run .\.claude\skills\dotnet-tests\scripts\tests.cs -- mutation
```

## Architecture

Keep the initial implementation simple: one API project under `src/` and its
test projects under `tests/`.

```text
src/
  GeekShop.Api/*                   # API project (Entry point)
tests/
  GeekShop.Api.Tests/*             # Unit tests
  GeekShop.Api.IntegrationTests/*  # Project integration tests
```

## Code Style

- Use ASP.NET Core Minimal APIs for endpoints.
- Follow [`.editorconfig`](.editorconfig) for encoding, indentation and line
  endings.
- Apply SOLID principles pragmatically and design components for testability.
- Follow **vertical slices** and **screaming architecture**: organize code by
  business capability and use case, keeping each endpoint and its request,
  response and business logic together. Extract shared code or persistence
  infrastructure only when genuinely needed.
- Use dependency injection for dependencies.

## Agent Skills

Use the applicable skills under `.claude/skills/`:

- [`dotnet-tests`](.claude/skills/dotnet-tests/SKILL.md) for tests, coverage
  and mutation testing.
- [`dotnet-lint`](.claude/skills/dotnet-lint/SKILL.md) for build, analyzer and
  style checks.
- [`dotnet-nuget-manager`](.claude/skills/dotnet-nuget-manager/SKILL.md) for
  NuGet package changes.
- [`csharp-15`](.claude/skills/csharp-15/SKILL.md) when working with C# 15 or
  the .NET 11 target.
- [`git-commit`](.claude/skills/git-commit/SKILL.md) for commit conventions.
- [`pr-watch`](.claude/skills/pr-watch/SKILL.md) for pull request follow-up.

## Boundaries

- **ALWAYS** keep commits atomic and self-contained.
- After completing and validating changes, prepare a commit and request human
  review and approval before creating it.
- **NEVER** commit untested code.
- **ALWAYS** add appropriate unit tests for new features and bug fixes, run
  relevant tests and check coverage before committing.
- **ALWAYS** run mutation tests locally before committing and investigate
  surviving mutants using the testing skill.
- **ALWAYS** document every new or changed feature before committing:
  - Add a short, user-level summary to the `README.md` **Features** list, with no
    technical detail (no internals, classes or implementation notes).
  - Put technical or usage detail (routes, options, configuration, setup) in the
    appropriate section of `README.md` or in dedicated user-facing documentation
    (for example under `docs/`) linked from `README.md`.
  - Update `AGENTS.md` only when the stack, features list, commands, architecture
    or conventions for contributors change.
- **NEVER** commit secrets; **ALWAYS** use environment variables or a secure
  secret store.
- **NEVER** remove a failing test to hide a failure. **ALWAYS** investigate
  and fix the cause.
