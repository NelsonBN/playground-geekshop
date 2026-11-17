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

`coverlet.MTP` collects Cobertura on Microsoft.Testing.Platform (`coverlet.collector` does not work with xunit v3 here); `dotnet-reportgenerator-globaltool` writes summaries and HTML. `tests.cs coverage` runs the unit projects, excludes migrations, `*.g.cs`, `obj`, `GeneratedCode` and `CompilerGenerated`, and lists the uncovered lines and branches of each failing file; write tests for them and re-run.

- Coverage is a floor: it finds code no test touches. Branch coverage exposes missing negative cases. 100% is not required; an unreachable defensive branch may stay uncovered (list it in the report).
- Excluding code needs the user's approval and a comment, and only fits generated code, migrations and `Program.cs` bootstrapping. Never `[ExcludeFromCodeCoverage]` on logic, extra `--coverlet-exclude*` options or `--coverlet-skip-auto-props` tricks.
- High coverage with weak assertions is worthless: that is what mutation testing catches (`quality`).

## Definition of done

- [ ] Test first: every fix and change started with a test seen failing for the reported reason, then passing, with the evidence quoted ([tdd-bug-workflow.md](tdd-bug-workflow.md)).
- [ ] Every new or changed behaviour has a test; no data-only tests; no unneeded `InternalsVisibleTo`; unit tests substitute interfaces; real infrastructure is only in integration tests.
- [ ] Tests follow AAA and Given_When_Then, one behaviour each, no smells ([unit-tests.md](unit-tests.md#smells)).
- [ ] `all` passes; `quality` passes (every changed file meets line, branch and mutation gates; remaining survivors are explained).
- [ ] No skipped tests, no coverage excludes added, no thresholds changed; `dotnet-lint` clean on the tests too.
- [ ] Report printed (`--report`): tests added, results, coverage vs gate, mutation score, survivors, decisions needed.

## Flaky tests

Never retry blindly or add sleeps. Reproduce (loop the test), find the shared state, race or time dependency, fix it. If it cannot be fixed now, tell the user and propose a quarantine (`[Trait("Category", "Quarantine")]` plus an issue); never silently skip. xunit runs classes in parallel: keep fixtures read-only and per-test state local; a `[Collection]` is the last resort for a truly shared resource.

## CI

```yaml
- uses: actions/setup-dotnet@v4
  with: { global-json-file: global.json }
- run: dotnet tool install -g dotnet-reportgenerator-globaltool
- run: dotnet run .claude/skills/dotnet-tests/scripts/tests.cs -- all
- run: dotnet tool restore
- run: dotnet run .claude/skills/dotnet-tests/scripts/tests.cs -- quality
- uses: actions/upload-artifact@v4
  with: { name: test-reports, path: artifacts/dotnet-tests }
```

Mutation in CI needs full history (`fetch-depth: 0`) so `--since` can diff against the base branch.
