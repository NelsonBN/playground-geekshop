---
name: dotnet-tests
description: 'Write, run and gate tests for .NET code: unit tests, narrow integration tests and mutation tests, all in one fixed Arrange-Act-Assert / Given_When_Then pattern, with a coverage gate. Test-first (TDD: red, fix, green) is mandatory for every fix or change made by an AI. Use when fixing a bug, a ticket, an issue, a review comment, a debug finding or any instruction (reproduce with a failing test first), implementing or changing a feature, adding or reviewing tests, checking coverage, killing surviving mutants, or before declaring a change done. Command form: /dotnet-tests [unit|integration|coverage|mutation|quality|all].'
argument-hint: '[unit|integration|coverage|mutation|quality|all]'
license: MIT
allowed-tools: Bash(dotnet:*), Bash(reportgenerator:*), Bash(docker:*), Bash(git:*), PowerShell(dotnet:*), PowerShell(reportgenerator:*), PowerShell(docker:*), PowerShell(git:*), Read, Edit, Write, Grep, Glob
---

# .NET Tests

Every feature or fix ships with tests that prove it works and keep it working. Do not call the work done until the quality gate passes.

> ## HARD RULE: no fix and no change without a failing test first (TDD)
>
> For **every** issue, whatever its source (ticket, review comment, prompt instruction, debug session, failing build) and for new behaviour:
>
> 1. **Write the test first** (unit, or integration when the bug needs real infrastructure) that reproduces the problem.
> 2. **Red:** run only that test (`dotnet test <P>.Tests.csproj --filter "DisplayName~<test>"`) and watch it fail on an assertion that states the symptom. A test that passes, does not compile, or matches nothing is not a red. Quote the failure message in your reply.
> 3. **Fix** with the smallest production change. Never edit the test to make it pass.
> 4. **Green:** run the same test again, watch it pass, say so.
> 5. **Refactor** only while green, then run the gates.
>
> If the issue cannot be reproduced by a test, stop and ask the user. Only pure refactors already covered by passing tests, docs, config and formatting may skip it, and you must say that you skipped it and why. Details: [references/tdd-bug-workflow.md](references/tdd-bug-workflow.md).

## Core rules

1. **AAA.** `// Arrange`, `// Act`, `// Assert` in order, one Act per test. Names are `Given_<state>_When_<action>_Then_<outcome>`, one behaviour each.
2. **AwesomeAssertions** (`.Should()`) only. A test without an assertion is a bug. Deterministic: no real clock, randomness, network or shared state.
3. **Test behaviour, not data.** Never test DTOs, request/response records, entities or POCOs that only hold data (constructors, getters/setters, record equality, `ToString`). The tests of the behaviour that uses them already cover them.
4. **Unit tests isolate.** Substitute every interface or boundary dependency (NSubstitute, or a Fake). Never mock the type under test or plain value objects. Code whose behaviour only exists against real infrastructure or a framework runtime is not unit-tested: prove it in `*.IntegrationTests`. Decide with [references/unit-tests.md](references/unit-tests.md#unit-or-integration).
5. **Mirror structure:** one `<P>.Tests` project per source project and one `<Name>Tests.cs` per source file with behaviour, same relative folder; `<P>.IntegrationTests` for web/API projects. Data-only files need no test; infrastructure-bound files are proven by `<Name>Tests.cs` in the `<P>.IntegrationTests` project instead (only the project is named `IntegrationTests`; files and classes there are named like unit tests). Enforced by `structure`, see [references/project-structure.md](references/project-structure.md).
6. **Test through the public API.** Do not add `InternalsVisibleTo`, make members `public` or use reflection just to reach internals: reach them through the public type that uses them (use case, endpoint through the host, an interface resolved from DI). Grant `InternalsVisibleTo` only when there is no other way (e.g. the entry-point type for `WebApplicationFactory<Program>`), only to the test project that uses an internal type, and tell the user why.
7. **Prove it can fail.** Each test must go red if the behaviour breaks; mutation testing confirms it.
8. Follow `.editorconfig`, the repo's `AGENTS.md`/`CLAUDE.md`, and keep test project setup plain (unconditional `<Using Include="Xunit" />`, no MSBuild tricks).

### NEVER

- Lower thresholds in `quality-gate.json`, or edit it, without the user's approval.
- Use `[ExcludeFromCodeCoverage]`, coverage excludes, `[Fact(Skip=...)]`, `#if` or test deletion to pass the gate; weaken or delete an assertion to get green.
- Write the test after the fix, write a test just to pass, or fix blind when the problem cannot be reproduced.
- Add `InternalsVisibleTo` for a test project that uses no internal type, or just to make a unit test easier to write.
- Write data-only tests to satisfy the mirror rule or the coverage gate.
- Start real infrastructure or a framework host in a unit test (database or in-memory database stand-in, web host, DI container, broker, network).
- Retry flaky tests blindly; fix the cause or tell the user.
- Use a tool from the [never list](references/suggested-tools.md#tools-to-never-use) or add one beyond the [base set](references/suggested-tools.md#tools-to-use) without asking.
- Install Stryker globally or run it without the repo's tool manifest and root `mutation-tests.*` scripts.
- Commit or push.

## Workflow

1. **Reproduce first:** test in the mirror file (create the project/file when missing, see [project-structure](references/project-structure.md)), red, fix, green, as in the hard rule. For new API, add the signature first so the test compiles and fails on the assertion.
2. **List the remaining behaviours:** happy path, boundaries, invalid input, failure paths, and concurrency or cancellation when relevant; each is one test, test-first ([test-pattern.md](references/test-pattern.md), [unit-tests.md](references/unit-tests.md), [example](examples/UnitTest.cs.example)).
3. **Integration tests** only for what a unit test cannot prove ([integration-tests.md](references/integration-tests.md)), tagged `[Trait("Category", "Integration")]`.
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

- .NET SDK on `PATH`.
- No test project yet: create it from [examples/TestProject.csproj.example](examples/TestProject.csproj.example).
- Mutation: Stryker local tool and root scripts ([mutation-tests.md](references/mutation-tests.md#setup)); coverage reports: `dotnet tool install -g dotnet-reportgenerator-globaltool`; Docker only for Testcontainers. The script says what is missing.

## Hook

A `Stop` hook (`.claude/settings.json`) runs `<tests> hook stop` when the agent tries to end its turn. It does nothing without changed `.cs` files or a test project (or with `DOTNET_TESTS_HOOK=off`); otherwise it checks structure, unit tests and the coverage gate on changed code and blocks with the gaps; continue from workflow step 1. It cannot see whether the test was written first: that is your obligation. It never runs mutation, and lets the agent stop with a warning after 3 consecutive blocks.

## References

[tdd-bug-workflow](references/tdd-bug-workflow.md) · [project-structure](references/project-structure.md) · [test-pattern](references/test-pattern.md) · [unit-tests](references/unit-tests.md) · [integration-tests](references/integration-tests.md) · [quality-gate](references/quality-gate.md) (thresholds, coverage, DoD, smells, CI) · [mutation-tests](references/mutation-tests.md) · [suggested-tools](references/suggested-tools.md)
