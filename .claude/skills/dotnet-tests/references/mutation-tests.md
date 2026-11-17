# Mutation tests

Coverage says which lines ran; mutation testing says whether the tests would notice a bug. Stryker.NET changes the code (flips `>` to `>=`, removes a statement, returns a default) and reruns the tests; a mutant that no test fails is a **survivor**.

## Setup

Stryker is a **local tool** pinned in the repository, never a global install, so every machine and CI run uses the same version.

| File (repo root) | Copy from | Purpose |
|------------------|-----------|---------|
| `.config/dotnet-tools.json` | [../examples/dotnet-tools.json](../examples/dotnet-tools.json) | Pins `dotnet-stryker` (restored with `dotnet tool restore`) |
| `stryker-config.json` | [../examples/stryker-config.json](../examples/stryker-config.json) | Runner, reporters, thresholds, excludes |
| `mutation-tests.bat` | [../examples/mutation-tests.bat](../examples/mutation-tests.bat) | Windows script. **Create it at the root** (the skill script refuses to run mutation without it) |
| `mutation-tests.sh` | [../examples/mutation-tests.sh](../examples/mutation-tests.sh) | Linux and macOS script. **Create it at the root**, same rule (keep LF line endings; `chmod +x`, or `git update-index --chmod=+x mutation-tests.sh`) |

Requirements, all verified with Stryker 5.1.0:

- A solution file (`.sln` or `.slnx`) at the repository root. Stryker then runs in solution context, where `--test-project` / `test-projects` is **rejected**; it finds the test projects from the solution. Use `--project <Source>.csproj` to limit it to one source project.
- `"test-runner": "mtp"` in the config: xunit v3 runs on Microsoft.Testing.Platform. With the default VSTest runner Stryker cannot see the tests and reports 0% ("test coverage capture failed"). The MTP runner is marked preview by Stryker; treat the score as indicative and verify surprising results.
- Add `StrykerOutput/` to `.gitignore`.
- Thresholds: `break` must be lower than or equal to `low` (Stryker refuses to start otherwise). The numbers in `stryker-config.json` and in `quality-gate.json` should match (80).

## Run

```bash
./mutation-tests.sh                       # Linux, macOS
mutation-tests.bat                        # Windows
./mutation-tests.sh -o --project MyProject.csproj --since:main     # -o opens the HTML report; other arguments go to Stryker
```

The scripts do what a developer would do by hand: `dotnet tool restore`, `dotnet restore`, `dotnet build --no-restore`, then `dotnet stryker --config-file stryker-config.json` (the HTML report is opened in the browser only when you pass `-o` or `--open`; agents and CI never pass it).

For the agent and the quality gate use the skill script, which mutates only what changed and writes to `artifacts/dotnet-tests/mutation/<project>/`:

```bash
dotnet run .claude/skills/dotnet-tests/scripts/tests.cs -- mutation     # changed files, one source project per unit test project
dotnet run .claude/skills/dotnet-tests/scripts/tests.cs -- quality      # coverage + mutation, correlated (below)
```

`--since` needs a branch, tag or commit SHA (not `HEAD`); the script resolves it. Exit codes: `0` score at or above the threshold, `1` below it, `2` tooling error.

## Coverage x mutation: the `quality` report

Neither number is enough alone. `quality` runs both and joins them per file and per surviving mutant, writing `artifacts/dotnet-tests/quality.md`:

| File | Line | Branch | Mutation | Verdict |
|------|------|--------|----------|---------|
| src/Lib/Price.cs | 83.3% | 50.0% | 0.0% | FAIL: line coverage, branch coverage, mutation score |
| src/Lib/Calc.cs | 100% | 100% | 75.0% | FAIL: mutation score (covered but not verified) |

Each survivor is classified by whether coverage reached its line:

| Class | Meaning | Action |
|-------|---------|--------|
| **NOT COVERED** | No test executes the line (Stryker `NoCoverage`, or the line is in the coverage report's uncovered list) | Write a test that reaches it; this also lifts line and branch coverage |
| **COVERED BUT NOT VERIFIED** | A test executes the line but nothing fails when it changes | Strengthen the assertion, add the boundary or negative case. This is the gap coverage alone hides, so fix these first |

Pass only when every changed file meets line, branch **and** mutation gates.

## The improve loop

The agent owns this loop. After every `mutation` or `quality` run, read the survivors in the report, strengthen or add tests, and run again. Keep going while rounds still pay off, and stop deliberately instead of looping forever.

Per round:

1. Triage every survivor: **killable** (a boundary input, the exact expected value instead of `BeGreaterThan(0)`, the negative path or a matched argument separates it from the original), **equivalent** (no observable change, e.g. `x < 0` to `x <= 0` before `-x`, logging text, cache hints) or **not worth it** (see below).
2. Kill the killable ones, the *covered but not verified* ones first, then the *not covered* ones.
3. Re-run and compare the survivor count and the score with the previous round.

Stop when any of these holds:

- Every changed file meets the mutation gate **and** the remaining survivors are all triaged as equivalent or not worth it.
- A round kills nothing new (no progress), or two rounds in a row raise the score by only a negligible amount.
- A survivor would need a test that is contorted, brittle, tied to implementation details, or asserts something no caller can observe (logging text, cache hints, defensive code that cannot be reached, compiler-generated or tooling-forced code).
- The tool, not the tests, is the blocker (Stryker cannot parse or compile the code, test server failures): do not retry blindly; report it.
- A hard cap of **5 rounds** per run is reached. After that, report the state and let the user decide.

When stopping with survivors left, list each one in the report's "Decisions needed" with file, line, mutation and the reason it was left (equivalent, not worth it, tool limit). Never lower the threshold, exclude files or mutators, or weaken assertions to end the loop.

A repository can opt a source project out of mutation runs that its tool cannot handle by listing its name (without extension) in `.config/dotnet-tools.json` as `{ "mutationIgnore": ["<SourceProject>"] }` (ask the user first, state the reason in the report); `mutation` then prints `SKIP` for it.

Excluding mutants in the config (`mutate` globs, `ignore-mutations`, `ignore-methods`) requires the user's approval and a reason; never exclude to raise the score.

## Tips

Keep unit tests fast (Stryker reruns them thousands of times; never mutate against integration tests). Run coverage first, since plain tests fix NoCoverage cheaply. Spend effort on logic with branches and arithmetic, not trivial code. The skill script mutates only the changed files that have a unit test, so infrastructure-bound code is not mutated.
