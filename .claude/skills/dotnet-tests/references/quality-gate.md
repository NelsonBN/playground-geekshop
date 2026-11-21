# Quality gate

Thresholds live in [../quality-gate.json](../quality-gate.json); never lower them without the user's approval. The gates apply to the **changed files that have a unit test** (versus the default branch; the hook: versus `HEAD`), so legacy code does not block new work. Data-only files and infrastructure-bound files (proven by an integration test) are not unit-gated; whole-solution numbers are printed for information only.

| Metric | Default | Checked by |
|--------|---------|------------|
| Mirror structure | required | hook, `structure`, `all` |
| All unit tests pass | required | hook, `all` |
| Line / branch coverage | 90% / 85% | hook, `coverage` |
| Mutation score | 80% | `mutation`, `quality` (on demand, too slow for the hook) |
| Coverage and mutation agree | required | `quality` |

## Coverage

Every test project references `coverlet.collector`, which writes a Cobertura report when `dotnet test` runs with `--collect "XPlat Code Coverage"`; without the package no report is produced. `dotnet-reportgenerator-globaltool` writes summaries and HTML. `tests.cs coverage` runs the unit projects, excludes migrations, `*.g.cs`, `obj`, `GeneratedCode` and `CompilerGenerated`, and lists the uncovered lines and branches of each failing file; write tests for them and re-run.

- Coverage is a floor: it finds code no test touches. Branch coverage exposes missing negative cases. 100% is not required; an unreachable defensive branch may stay uncovered (list it in the report).
- Excluding code needs the user's approval and a comment, and only fits generated code, migrations and `Program.cs` bootstrapping. Never `[ExcludeFromCodeCoverage]` on logic, or extra exclusion options.
- The gate is never met by skipping (`[Fact(Skip=...)]`, `#if`), deleting tests, weakening or removing an assertion, or lowering `quality-gate.json` without the user's approval. This is the one place these rules live; the other files link here.
- High coverage with weak assertions is worthless: that is what mutation testing catches (`quality`).

## Definition of done

- [ ] Test first: every fix and change started with a test seen failing for the reported reason, then passing, with the evidence quoted ([tdd-bug-workflow.md](tdd-bug-workflow.md)).
- [ ] Every new or changed behaviour has a test, split between unit and integration as in [unit-tests.md](unit-tests.md#unit-or-integration) and [integration-tests.md](integration-tests.md).
- [ ] Every entry point of a changed host (endpoint, consumer, job/worker) has at least one happy and one failure integration flow, boundary to boundary, and `integration` passes (it is not part of `all` or the hook).
- [ ] Tests follow AAA and Given_When_Then, one behaviour each, no smells ([unit-tests.md](unit-tests.md#smells)).
- [ ] `all` passes; `quality` passes (every changed file meets line, branch and mutation gates; remaining survivors are explained).
- [ ] No skipped tests, no coverage excludes added, no thresholds changed; `dotnet-lint` clean on the tests too.
- [ ] Report printed (`--report`): tests added, results, coverage vs gate, mutation score, survivors, decisions needed.

## Flaky tests

See [test-pattern.md](test-pattern.md#isolation-and-flaky-tests).
