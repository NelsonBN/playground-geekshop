# Integration tests

An integration test proves a whole flow of the application, **boundary to boundary**: it starts at a real entry point and follows the production path until the last boundary the flow touches. Packages: [suggested-tools.md](suggested-tools.md).

| Entry point | Flow under test (example) |
|-------------|---------------------------|
| HTTP endpoint / controller | request -> validation -> use case -> database -> response |
| Message consumer | message published to the broker -> handler -> database write and call to an external API |
| Job / background worker | job triggered -> use case -> database and broker publish |

An **isolated module** (a use case, calculator, mapper or validator with its dependencies substituted) is a unit test, not an integration test ([unit-tests.md](unit-tests.md#unit-or-integration)). Integration tests exist because bugs hide between the pieces: wiring, serialization, queries, transactions, configuration and ordering. A test that replaces those pieces cannot find them.

## Be as close to production as possible

The host under test is the production host, started the way production starts it. Only configuration may differ.

- **Do not replace the application's own paths** (use cases, repositories, handlers, validators, mappers, middleware, hosted services, DI registrations). If the app would run it in production, the test runs it.
- **Connections are the only reason to touch the host.** Set connection strings, URLs and ports through configuration (`UseSetting`, in-memory configuration, environment variables). Replace a DI registration only when configuration cannot do it (for example a client built from a fixed address), keep the production implementation and change only what points at the container, and say why in a comment.
- **No in-memory stand-ins.** Never use an in-memory or embedded database or broker (EF Core InMemory, SQLite in-memory, in-process fake brokers or caches) in an integration test: they behave differently from the real service and hide the bugs these tests exist to find.
- **No mocking libraries** (NSubstitute and the like) in integration tests. Doubles belong to unit tests.
- Realistic data and the production pipeline (authentication, middleware, serialization, background services). Do not bypass auth or disable a hosted service to make a test easier; set up a valid test identity or trigger the job instead.

## External services are real, in Testcontainers

The only thing the test does not own is an **external service**: database, broker, cache, storage, a third-party or sibling HTTP API. Each one runs for real in a container through Testcontainers, so this is emulation by the real thing, not mocking ([below](#dependencies-run-in-testcontainers)).

## When they make sense

- Every entry point of the host has at least one happy-path flow and one failure flow (invalid input, downstream failure, rejected message).
- A bug that only reproduces across boundaries: its failing reproduction is an integration test, written first ([tdd-bug-workflow.md](tdd-bug-workflow.md#integration-reds)).
- **Exceptional case:** an adapter to infrastructure (database repository, file system, external client) whose behaviour the boundary-to-boundary flows do not reach (a complex query, a constraint, a file format, a retry policy) may get its own integration test against the real container or real files. Prefer extending a flow first, and say why it was not enough.
- Not for pure logic, branching or mapping: unit tests.

## Conventions

- Project `tests/<P>.IntegrationTests/<P>.IntegrationTests.csproj`, only the project carries the `IntegrationTests` name. Test classes are aligned with the **entry point** they exercise (controller, endpoint, consumer, job) and follow `<Name>Tests.cs`, mirroring the source folders ([project-structure.md](project-structure.md)); tag tests `[Trait("Category", "Integration")]`.
- **AAA for a flow:** Arrange = containers, host and the data the flow needs; Act = the single action that triggers the entry point (the request, the published message, the job run); Assert = the observable end state at the boundaries (response, rows, published messages, calls received by the external service). Reading the result back with a second call is part of Assert, not a second Act. Given_When_Then naming as in unit tests.
- Each test owns its data and leaves nothing behind; no dependence on order; no fixed ports or names.
- Data in containers and fixtures is synthetic, with no secrets or real customer, KYC, payment or balance data ([test-pattern.md](test-pattern.md#sensitive-data-in-tests)). Flows over money, ledgers or audit logs also assert precision and rounding, idempotency on a repeated trigger, balanced ledger entries and the audit-log entry.
- The coverage gate runs unit tests only, so core logic must be unit-tested; integration tests prove the wiring and the flows that unit tests do not reach.

## Asynchronous outcomes

Consumers and workers finish after the trigger returns. Never `Thread.Sleep` or a fixed `Task.Delay`.

- Trigger deterministically: publish the message, or invoke the job's entry point (or its scheduler API) directly instead of waiting for a timer.
- Wait for the observable outcome with a bounded poll: check the database row, the received call or the output queue every short interval until it holds or a timeout (seconds, not minutes) expires, then fail with a message saying what was awaited. Pass a `CancellationToken`.
- Assert on the outcome, then also that nothing unexpected happened (no extra message, no duplicate row) when idempotency matters.

## Containers and hosts

- One container per class or collection (`IAsyncLifetime`/`IClassFixture`), not per test; reset state between tests instead of restarting. Pin the image tag, never `latest`. Needs Docker; `tests.cs integration` exits 2 with a clear message without it.
- To share a fixture across test classes, declare a collection whose name comes from `nameof`, never a magic string or a `NAME` constant. `[CollectionDefinition]` goes on the definition class, `[Collection]` on each test class:

  ```csharp
  [CollectionDefinition(nameof(IntegrationCollection))]
  public sealed class IntegrationCollection : ICollectionFixture<DatabaseFixture>;

  [Collection(nameof(IntegrationCollection))]
  [Trait("Category", "Integration")]
  public sealed class OrdersEndpointTests(DatabaseFixture database) { }
  ```

- Inject the containers' connection details into the host through settings; never hardcode ports or hosts.

## Dependencies run in Testcontainers

Every external service runs as a real container. Pick the first option that exists:

1. **The service's own Testcontainers module** (`Testcontainers.MsSql`, `.PostgreSql`, `.MongoDb`, `.Redis`, `.Kafka`, `.RabbitMq`, `.ServiceBus`, `.Azurite`, `.LocalStack`, ...). It brings the pinned image, wait strategy and connection string.
2. **The vendor's official image through the generic container** (`ContainerBuilder`) when no module exists: pin the tag, add a wait strategy, map ports to random host ports, mount the config the image needs and start any container it depends on on a shared network.
3. **A customized container** when no image of the service exists (internal service, third-party or SaaS HTTP API): a protocol-level emulator such as `WireMock.Net.Testcontainers`, or a small purpose-built image (`ImageFromDockerfileBuilder`) that speaks the same protocol, loaded with the responses the test needs. This is how an external API is represented, not a mock of the app.

Rules:

- The fixture applies what the dependency needs before the tests (schema scripts, broker topics, seed data); tests never rely on manual setup.
- There is no fake or in-memory fallback. If no container can be built at all, stop and ask the user.
- Do not drop a real dependency because the container is slower or harder to configure.

Full example: [../examples/IntegrationTest.cs.example](../examples/IntegrationTest.cs.example) (endpoint flow) and [../examples/ConsumerIntegrationTest.cs.example](../examples/ConsumerIntegrationTest.cs.example) (consumer and worker flow). Run: `dotnet test <P>.IntegrationTests.csproj` (wrapped by `tests.cs integration`).
