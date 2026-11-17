# Unit tests

Fast (milliseconds), in-process, no network, disk, database or clock. A unit test checks one unit's observable behaviour: a return value, a state change, or an interaction at a boundary. Packages: [suggested-tools.md](suggested-tools.md). xunit v3 runs on Microsoft.Testing.Platform: pass the project with `--project`, filter with `--filter-display-name` or `--filter-trait`. Project file: [../examples/TestProject.csproj.example](../examples/TestProject.csproj.example).

## Pattern and assertions

AAA layout, Given_When_Then naming, builders, data-driven tests, exceptions, async, time and isolation, with examples: [test-pattern.md](test-pattern.md). Assert the specific outcome, never just `NotBeNull`/`NotThrow` when a value is available; use `Assert` only for what AwesomeAssertions cannot express; add a `because` only when the reason is not obvious.

## Test doubles (NSubstitute)

| Double | Use when | Example |
|--------|----------|---------|
| Dummy | Parameter required but unused | `Substitute.For<ILogger>()` |
| Stub | Canned answers from a dependency | `repo.Find(id).Returns(order)` |
| Spy | Check afterwards what happened | `logger.Received(1).Log(...)` |
| Mock | The interaction is the behaviour | `bus.Received(1).Publish(Arg.Is<OrderPlaced>(e => e.Id == id))` |
| Fake | A working lightweight implementation is clearer | hand-written class in `Fakes/` |

- Whenever the unit depends on an interface, substitute it. Never mock the unit under test, value objects, DTOs or collections.
- Prefer state verification; use `Received` only when the interaction is the contract; `DidNotReceive()` is strong for "must not happen".
- Match the argument that matters (`Arg.Is`), not `Arg.Any`. More than about three doubles means the unit does too much: simplify or use a Fake.

## What NOT to unit-test

```csharp
// BAD: tests data, not behaviour (fails only when the compiler would)
[Fact]
public void Given_Values_When_Created_Then_ExposesThem()
{
    var dto = new OrderResponse(id, "Keyboard", 5);
    dto.Sku.Should().Be("Keyboard");
}

// BAD: unit test starting real infrastructure instead of substituting the interface
var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("x").Options;
var sut = new CreateOrderUseCase(new OrderRepository(new AppDbContext(options)));

// GOOD: substitute the interface, assert the behaviour
var repository = Substitute.For<IOrderRepository>();
var sut = new CreateOrderUseCase(repository);
var id = await sut.ExecuteAsync(request, TestContext.Current.CancellationToken);
repository.Received(1).Add(Arg.Is<Order>(o => o.Sku == request.Sku));
```

- Data-only types (DTOs, records, entities, POCOs): constructors, getters/setters, equality, `ToString`. A type with real behaviour (validation, calculation, state transitions) is tested through that behaviour.
- Framework wiring and configuration: registration, routing, persistence mapping, option binding.
- Anything needing real infrastructure or a framework runtime.

## Internals

Test internal code through the public type that uses it. `InternalsVisibleTo` is a last resort, never a convenience.

```xml
<!-- BAD: grants the unit test project access although it uses no internal type -->
<InternalsVisibleTo Include="Orders.Tests" />
```

```csharp
// BAD: reaches an internal helper directly (needs InternalsVisibleTo)
new PriceRounder().Round(10.005m).Should().Be(10.01m);

// GOOD: observe the same rule through the public service that uses it
new PricingService(rounding).Quote(10.005m).Total.Should().Be(10.01m);
```

When an integration test needs an internal implementation, resolve it through its public interface from the host (`scope.ServiceProvider.GetRequiredService<IOrderRepository>()`). A legitimate exception is a host entry-point type such as `Program` for `WebApplicationFactory<Program>`: grant it only to the integration project and say why.

## Unit or integration

Infer it from what the code does, not from its name:

| The code... | Test |
|-------------|------|
| Decides, calculates, maps or orchestrates through interfaces | Unit, interfaces substituted |
| Only holds data | None; covered through the behaviour that uses it |
| Behaves only against real infrastructure or a framework runtime: transport, persistence, external clients, messaging, startup wiring, serialization | Integration, against the real thing |

If the logic is buried in infrastructure code, extract it behind an interface so the logic is unit-tested and the adapter stays thin.

## Smells

Tests of data-only types; unit tests that start real infrastructure or a framework host; a real implementation where an interface could be substituted; mocks of own types or value objects; no assertion or only `NotBeNull`; several Acts; logic or recomputed expectations in the test; `Thread.Sleep`; testing private members; asserting call counts for pure logic; reusing mutable state; catching exceptions in tests; a test that mirrors the implementation instead of the contract; `InternalsVisibleTo` granted to a test project that uses no internal type.
