# Narrow integration tests

Write them only for what a unit test cannot prove: behaviour that only exists against real infrastructure or a framework runtime. Narrow means one real boundary per test, everything else faked or in memory. Packages: [suggested-tools.md](suggested-tools.md).

## When they make sense

- A bug that only reproduces at a real boundary: the failing reproduction is then an integration test, written first like any other ([tdd-bug-workflow.md](tdd-bug-workflow.md)).
- Entry points and transport (routing, model binding, auth, serialization, status codes), persistence (queries, mappings, constraints, transactions), external clients, messaging, caching, startup and wiring.
- Not for pure logic, branching or mapping (unit tests), nor "everything wired together" scenarios (slow and brittle).

## Conventions

- Project `tests/<P>.IntegrationTests/<P>.IntegrationTests.csproj`, only the project carries the `IntegrationTests` name: files and classes inside follow `<Name>Tests.cs` like unit tests, mirroring the source folders ([project-structure.md](project-structure.md)); tag tests `[Trait("Category", "Integration")]`. Same AAA and Given_When_Then pattern as unit tests.
- Each test owns its data and leaves nothing behind; no dependence on order; no fixed ports or names.
- The coverage gate runs unit tests only, so core logic must be unit-tested; integration tests prove the infrastructure code that unit tests do not reach.

## Containers and hosts

- One container per class or collection (`IAsyncLifetime`/`IClassFixture`), not per test; reset state between tests instead of restarting. Pin the image tag, never `latest`. Needs Docker; `tests.cs integration` exits 2 with a clear message without it.
- To share a fixture across test classes, declare a collection whose name comes from `nameof`, never a magic string or a `NAME` constant. `[CollectionDefinition]` goes on the definition class, `[Collection]` on each test class:

  ```csharp
  [CollectionDefinition(nameof(DatabaseCollection))]
  public sealed class DatabaseCollection : ICollectionFixture<DatabaseFixture>;

  [Collection(nameof(DatabaseCollection))]
  [Trait("Category", "Integration")]
  public sealed class OrderRepositoryTests(DatabaseFixture database) { }
  ```

- Replace external services with fakes through the host builder (e.g. `WebApplicationFactory.WithWebHostBuilder(... ConfigureTestServices ...)`) and keep the real database when the test is about persistence.

```csharp
// Act
var response = await client.GetAsync("/orders/42", CancellationToken.None);

// Assert
response.Should().Be200Ok().And.BeAs(new { Id = 42, Status = "Placed" });
```

Full example: [../examples/IntegrationTest.cs.example](../examples/IntegrationTest.cs.example). Run: `dotnet test <P>.IntegrationTests.csproj` (wrapped by `tests.cs integration`).
