---
name: dotnet-tests
description: 'Write, run and gate tests for .NET code: unit tests, integration tests and mutation tests, all in one fixed Arrange-Act-Assert / Given_When_Then pattern, with a coverage gate. Test-first (TDD: red, fix, green) is mandatory for every fix or change made by an AI. Use when fixing a bug, a ticket, an issue, a review comment, a debug finding or any instruction (reproduce with a failing test first), implementing or changing a feature, adding or reviewing tests, checking coverage, killing surviving mutants, or before declaring a change done. Command form: /dotnet-tests [unit|integration|coverage|mutation|quality|all].'
argument-hint: '[unit|integration|coverage|mutation|quality|all]'
license: MIT
allowed-tools: Bash(dotnet:*), Bash(reportgenerator:*), Bash(docker info), Bash(git:*), PowerShell(dotnet:*), PowerShell(reportgenerator:*), PowerShell(docker info), PowerShell(git:*), Read, Edit, Write, Grep, Glob
---

# .NET Tests

Every feature or fix ships with tests that prove it works and keep it working. Do not call the work done until the quality gate passes.

> ## HARD RULE: no fix and no change without a failing test first (TDD)
>
> For **every** issue, whatever its source (ticket, review comment, prompt instruction, debug session, failing build) and for new behaviour: write the test first, run only that test and watch it **fail on an assertion that states the symptom** (quote the message), make the smallest production fix, watch the same test pass, refactor only while green, then run the gates. Never edit the test to make it pass. If the issue cannot be reproduced by a test, stop and ask the user. Waiver (say so and why), steps, accepted evidence and NEVER list: [references/tdd-bug-workflow.md](references/tdd-bug-workflow.md).

## Core rules

1. **AAA.** `// Arrange`, `// Act`, `// Assert` in order, one Act per test. Names are `Given_<state>_When_<action>_Then_<outcome>`, one behaviour each.
2. **AwesomeAssertions** (`.Should()`) only. A test without an assertion is a bug. Deterministic: no real clock, randomness, network or shared state.
3. **Test behaviour, not data.** Never test types that only hold data (DTOs, records, POCOs); their behaviour's tests already cover them ([unit-tests.md](references/unit-tests.md#what-not-to-unit-test)).
4. **Unit tests isolate a module; integration tests run a flow.** A unit test substitutes every dependency (NSubstitute, Fake or Stub). An integration test goes boundary to boundary, from a real entry point to the last boundary, on the production host with only configuration changed, and runs every external service for real in Testcontainers. Choose with [unit-tests.md](references/unit-tests.md#unit-or-integration); all integration rules (no own-path replacement, no in-memory stand-ins, no mocks, no fake fallback, container selection order) are in [integration-tests.md](references/integration-tests.md).
5. **Mirror structure:** one `<P>.Tests` project per source project and one `<Name>Tests.cs` per source file with behaviour, same relative folder; `<P>.IntegrationTests` for host projects (web/API, worker, consumer, job), with test classes aligned with the entry point they exercise. Enforced by `structure`, see [references/project-structure.md](references/project-structure.md).
6. **Test through the public API.** Reach internals through the public type that uses them; `InternalsVisibleTo` is a last resort, with the rules and the exception in [unit-tests.md](references/unit-tests.md#internals); tell the user why when you grant it.
7. **Prove it can fail.** Each test must go red if the behaviour breaks; mutation testing confirms it.
8. Follow `.editorconfig`, the repo's `AGENTS.md`/`CLAUDE.md`, and keep test project setup plain (unconditional `<Using Include="Xunit" />`, no MSBuild tricks).

### NEVER

- Lower thresholds, add coverage excludes, skip, delete or weaken tests to pass the gate ([quality-gate.md](references/quality-gate.md)).
- Write a data-only test to satisfy the mirror rule or the coverage gate.
- Start real infrastructure or a framework host in a unit test, or put an own-path replacement, in-memory stand-in or mock in an integration test (see rule 4).
- Retry flaky tests blindly ([test-pattern.md](references/test-pattern.md#isolation-and-flaky-tests)).
- Use a tool from the [never list](references/suggested-tools.md#tools-to-never-use) or add one beyond the [base set](references/suggested-tools.md#tools-to-use) without asking; run Stryker only as the repo's local tool through the root `mutation-tests.*` scripts ([mutation-tests.md](references/mutation-tests.md#setup)).
- Use real or production-derived data, secrets or credentials in a test, seed or container ([test-pattern.md](references/test-pattern.md#sensitive-data-in-tests)).
- Commit or push.

## Workflow

1. **Reproduce first:** test in the mirror file (create the project/file when missing, see [project-structure](references/project-structure.md)), red, fix, green, as in the hard rule. For new API, add the signature first so the test compiles and fails on the assertion.
2. **List the remaining behaviours:** happy path, boundaries, invalid input, failure paths, and concurrency or cancellation when relevant; each is one test, test-first ([test-pattern.md](references/test-pattern.md), [unit-tests.md](references/unit-tests.md), [example](examples/UnitTest.cs.example)).
3. **Integration tests** for each entry point of a host: at least one happy and one failure flow, boundary to boundary ([integration-tests.md](references/integration-tests.md), [endpoint example](examples/IntegrationTest.cs.example), [consumer/worker example](examples/ConsumerIntegrationTest.cs.example)), tagged `[Trait("Category", "Integration")]`. Run them with `<tests> integration`: `all` and the hook do not run them.
4. **Run the gates** (`<tests>` is `dotnet run <skill-dir>/scripts/tests.cs --`; run from the repo root; outputs in `artifacts/dotnet-tests/`; exit `0` pass, `1` gate or test failure, `2` tooling error):

   | Command | Does |
   |---------|------|
   | `<tests> structure` | Mirror project and file check |
   | `<tests> unit` / `integration` | Run that category |
   | `<tests> coverage` | Coverage gate on changed unit-tested files |
   | `<tests> mutation` | Stryker on changed unit-tested files |
   | `<tests> quality` | Coverage x mutation, correlated, per survivor |
   | `<tests> all` | Structure + unit + coverage (what the hook runs) |
   | `<tests> --report` | Write and print `report.md` |

5. **Kill survivors** with `quality`: improve the tests, re-run, stop by the rules in [mutation-tests.md](references/mutation-tests.md#the-improve-loop).
6. **Review** against [quality-gate.md](references/quality-gate.md) (definition of done, smells), run `dotnet-lint`, then **report** with `--report`.

## Prerequisites

- .NET SDK 10 or later on `PATH`: `dotnet run tests.cs` is a file-based app and does not run on SDK 9 or older. Test projects keep the `TargetFramework` of their source project, and the installed SDK must support it.
- No test project yet: create it from [examples/TestProject.csproj.example](examples/TestProject.csproj.example) (unit) or [examples/IntegrationTestProject.csproj.example](examples/IntegrationTestProject.csproj.example) (integration).
- Mutation: Stryker local tool and root scripts ([mutation-tests.md](references/mutation-tests.md#setup)); coverage reports: `dotnet tool install dotnet-reportgenerator-globaltool`; Docker for the integration tests (Testcontainers). The script says what is missing.

## Hook

A `Stop` hook (`.claude/settings.json`) runs `<tests> hook stop` when the agent tries to end its turn. It does nothing without changed `.cs` files or a test project (or with `DOTNET_TESTS_HOOK=off`); otherwise it checks structure, unit tests and the coverage gate on changed code and blocks with the gaps; continue from workflow step 1. It cannot see whether the test was written first: that is your obligation. It never runs mutation, and lets the agent stop with a warning after 3 consecutive blocks.

## References

[tdd-bug-workflow](references/tdd-bug-workflow.md) · [project-structure](references/project-structure.md) · [test-pattern](references/test-pattern.md) · [unit-tests](references/unit-tests.md) · [integration-tests](references/integration-tests.md) · [quality-gate](references/quality-gate.md) (thresholds, coverage, DoD, smells, CI) · [mutation-tests](references/mutation-tests.md) · [suggested-tools](references/suggested-tools.md)
