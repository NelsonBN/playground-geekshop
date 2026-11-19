# Test-first (TDD) for every fix and change

Mandatory for AI-driven coding (agents, automation, software factories) whenever the work is debugging, solving an issue or changing behaviour. A fix that was not first reproduced by a failing test is not accepted.

```
issue -> test (unit or integration) -> RED -> fix -> GREEN -> (refactor) -> gates
```

| Step | Do | Accepted only if |
|------|----|------------------|
| Test | Reproduce the issue in the mirror test file, named after the symptom | |
| **Red** | `dotnet test <P>.Tests.csproj --filter "DisplayName~<test>"` (integration: `<P>.IntegrationTests.csproj`) | It fails on an assertion that states the reported symptom |
| Fix | Smallest production change; do not touch the test | |
| **Green** | Same command | It passes, and production code (not the test) changed since the red |
| Refactor | Optional, while green (`<tests> unit`) | Still green |
| Gates | `<tests> all`, `<tests> quality` | See [quality-gate.md](quality-gate.md) |

## Where the test comes from

| Source | The test comes from |
|--------|---------------------|
| Ticket / bug report | Steps to reproduce and expected result |
| Review comment | The behaviour the reviewer says is wrong or missing |
| Prompt instruction ("make X do Y") | The new behaviour, written as an example |
| Debug session / stack trace | The minimized input that triggers the failure |
| Failing CI or test | The failing case as a stable test (fix the code, not the test, unless the test is wrong, and then say so) |
| New feature | The first behaviour, then the next, one red-green cycle each |

Prefer a unit test; use an integration test only when the bug lives in real infrastructure a unit test cannot reproduce ([integration-tests.md](integration-tests.md)).

## A good red

- **Fails on an assertion**, e.g. `Expected result to be 90M, but found 100M`. A compile error, a missing type, "No tests ran" (exit 8) or a setup crash is not a red.
- **Fails for the reported reason:** the message describes the issue's symptom, not an unrelated problem.
- **Is minimal and durable:** one behaviour, smallest input, literal expected values, named after the behaviour (`Given_PriceOf100_When_Discount_Then_TenPercentOff`).
- **New API:** add the signature first with a placeholder body returning a default value, then record the red.

## Worked example

Ticket: "A price of exactly 100 gets no discount."

1. Test in `tests/Lib.Tests/PriceTests.cs`:
   ```csharp
   [Fact]
   public void Given_PriceOf100_When_Discount_Then_TenPercentOff()
   {
       // Act
       var result = Price.Discount(100m);

       // Assert
       result.Should().Be(90m);
   }
   ```
2. Red: `dotnet test tests/Lib.Tests/Lib.Tests.csproj --filter "DisplayName~Given_PriceOf100"` fails with `Expected result to be 90M, but found 100M`. Quote that line in the reply.
3. Fix `price > 100` to `price >= 100`.
4. Green: run the same command; it passes. Say so in the reply.
5. `all` and `quality`: the boundary is covered and the mutant `>=` to `>` is killed.

## Never

Fix first and test after; write a test to pass; edit, skip or delete the test to reach green; fix blind when the issue cannot be reproduced (ask the user); bundle several bugs in one test; weaken an existing test that now fails (decide with the user whether the contract changed).

## Waiver

Only these may skip the failing test, and you must say so and why: pure refactors already covered by passing tests; docs, comments, formatting, config, build files. Never valid for a bug, ticket, review comment or behaviour change; if unsure, do the cycle.

## How it is followed

No script enforces the order and the hook cannot see it. The gates catch the consequences (missing tests fail structure and coverage; mutation exposes tests written to pass), so keep the evidence in your reply: the red message, the fix, the green result.
