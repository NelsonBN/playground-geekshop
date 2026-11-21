# Test pattern

One pattern for every test, so any test reads the same way.

## Arrange-Act-Assert

```csharp
[Fact]
public void Given_ValidAmount_When_Withdraw_Then_BalanceDecreases()
{
    // Arrange
    var account = new AccountBuilder().WithBalance(100m).Build();

    // Act
    account.Withdraw(30m);

    // Assert
    account.Balance.Should().Be(70m);
}
```

- Exactly one `// Arrange`, one `// Act`, one `// Assert` (merge `// Act & Assert` only for exception assertions).
- One Act per test: the single action that triggers the behaviour. In a unit test it is one call on the unit under test; in an integration flow it is the trigger of the entry point (the request, the published message, the job run), see [integration-tests.md](integration-tests.md#conventions).
- No `if`, loops or computed expectations in the test body; write the literal expected value. A shared helper may loop, for example the bounded poll for asynchronous outcomes ([integration-tests.md](integration-tests.md#asynchronous-outcomes)).

## Naming: Given_When_Then

`Given_<precondition>_When_<action>_Then_<observable outcome>`, PascalCase parts joined by underscores.

| Good | Bad |
|------|-----|
| `Given_EmptyCart_When_Checkout_Then_ThrowsInvalidOperation` | `CheckoutTest1` |
| `Given_ExpiredToken_When_Validate_Then_ReturnsFailure` | `Validate_Works` |

The name states the behaviour; if you need "and" in the name, split the test.

## Layout

- Project and file mirror structure: see [project-structure.md](project-structure.md) (`src/<P>/<Folder>/<Name>.cs` is tested by `tests/<P>.Tests/<Folder>/<Name>Tests.cs`).
- One test class per unit under test: `OrderService` is tested by `OrderServiceTests`, in the namespace `<P>.Tests.<Folder>`.
- Group by method with nested classes only when a class exceeds roughly 20 tests.
- Shared helpers live in `Builders/`, `Fakes/` and `Fixtures/` inside the test project.

## Test data

- **Builders** (`AccountBuilder`) give a valid default object; each test overrides only what matters to it. Never copy-paste 15-line object setups.
- Use literal, readable values; avoid random data unless the data generator is seeded.
- Data-driven: `[Theory]` + `[InlineData]` for simple values, `[MemberData]`/`TheoryData<>` for objects. One theory per behaviour, not a bundle of unrelated cases.

## Sensitive data in tests

This is the one place these rules live; the other files link here.

- Fixtures, seeds, builders and container initialisation scripts use **synthetic or anonymised data only**. Never real customer data, KYC documents, payment instructions, bank details or real balances and treasury positions.
- Never put secrets, API keys, tokens, certificates or private keys in tests, seeds, settings or container configuration. Use obviously fake values (`test-api-key`) generated for the test.
- Never populate a container or fixture from a production export, backup or dump, even "just anonymised by hand".
- Keep sensitive-looking values out of logs, assertion messages and files written to `artifacts/dotnet-tests/`.
- Regulated flows (payments, balances, FX pricing, settlement, reconciliation, audit logs) need tests for the cases that matter there: currency precision and rounding at the boundaries, idempotency (the same message or request sent twice changes state once), ledger integrity (the entries balance), and the audit-log entry for a sensitive action.

## Exceptions

```csharp
// Act
var act = () => account.Withdraw(500m);

// Assert
act.Should().Throw<InvalidOperationException>().WithMessage("*insufficient*");
```

## Async

Return `Task`, `await` the Act, use `await act.Should().ThrowAsync<T>()`. Never `.Result` or `.Wait()`. Pass `CancellationToken.None` to async calls.

## Time, randomness, IO

Inject `TimeProvider` (use `FakeTimeProvider` when approved, see [suggested-tools.md](suggested-tools.md)), `IFileSystem`, `Random` seeds. Never `DateTime.Now` in a test or in code that a test covers without a seam.

## What to cover per feature

Happy path, each boundary (min, max, off-by-one), null/empty/whitespace, invalid state, each failure path and its message or error type, idempotency, and (when relevant) concurrency and cancellation. Bug fix: a test that fails on the old code, written and seen failing before the fix ([tdd-bug-workflow.md](tdd-bug-workflow.md)), named after the bug's behaviour.

## Isolation and flaky tests

No static mutable state, no test order dependence, no shared files; unique names for anything created on disk or in a database. xunit runs classes in parallel: keep fixtures read-only and per-test state local; a `[Collection]` is the last resort for a truly shared resource.

A flaky test is never retried blindly and never gets a sleep. Reproduce it (loop the test), find the shared state, race or time dependency, and fix it. If it cannot be fixed now, tell the user and propose a quarantine (`[Trait("Category", "Quarantine")]` plus an issue); never silently skip.
