# Tools

Every tool the skill knows, in one table, and the tools that must never be used.

**Usage** column: **Always** = every test project of that kind uses it; **Specific cases** = use it only when the situation in the last column applies.

Tools marked (base) are the skill's fixed stack and need no approval. Every other tool is an optional addition: **propose it to the user with the reason, and add it only after they agree.** Ask in one short message, listing only the tools that match the current change.

## Tools to use

Every package is referenced with an **exact version** (e.g. `9.6.0`); wildcards and floating versions are forbidden. After `dotnet add package`, check the csproj, and replace any `*`/`2.*`.

| Tool | Use for | Usage | When |
|------|---------|-------|------|
| `xunit` 2.x (base) | Test framework | Always | Every test project |
| `xunit.runner.visualstudio` (base) | Runner for `dotnet test` and IDEs | Always | Every test project |
| `coverlet.collector` (base) | Cobertura coverage for the coverage gate (`--collect "XPlat Code Coverage"`) | Always | Every test project |
| `Microsoft.NET.Test.Sdk` (base) | Test host | Always | Every test project |
| `xunit.analyzers` (base, included with xunit) | Test-quality analyzer rules | Always | Every test project |
| `AwesomeAssertions` (base) | All assertions (`.Should()`) | Always | Every test |
| `Stryker.NET` (`dotnet-stryker`, base) | Mutation testing, local tool only ([mutation-tests.md](mutation-tests.md#setup)) | Always | Mutation step on changed files |
| `NSubstitute` (base) | Dummy, stub, spy, mock, fake, in unit tests only | Specific cases | The unit depends on a boundary (I/O, clock, external service); never in integration tests |
| `Testcontainers` + module (base) | Every external service of an integration test in a disposable container; which container to pick: [integration-tests.md](integration-tests.md#dependencies-run-in-testcontainers) | Always | Every integration test project: databases, brokers, caches, storage, external APIs, any infrastructure |
| `Microsoft.AspNetCore.Mvc.Testing` (base) | `WebApplicationFactory` in-process API host | Specific cases | Integration tests of web/API projects |
| `AwesomeAssertions.Web` (base) | Assertions on `HttpResponseMessage` | Specific cases | HTTP integration tests |
| `ReportGenerator` (base) | Merged summary and HTML coverage report | Specific cases | A human-readable or CI report is wanted (the gate works without it) |
| `Microsoft.Extensions.TimeProvider.Testing` (`FakeTimeProvider`) | Deterministic time | Specific cases | Code uses clocks, delays or expiry |
| `Bogus` | Realistic generated test data | Specific cases | Builders become long, or varied data helps |
| `WireMock.Net.Testcontainers` (`WireMockContainer`) | Protocol-level emulator of an external HTTP service, run as a container through Testcontainers (not in-process, not a mock of the app); the customized-container option when an API has no image | Specific cases | Integration tests call third-party or internal HTTP APIs |
| `Microsoft.Extensions.Diagnostics.Testing` | Assert logs and metrics | Specific cases | Logging or metrics are part of the contract |
| `ReportGenerator` report type `MarkdownSummaryGithub` | Coverage summary on pull requests | Specific cases | The repo uses GitHub Actions |

## Tools to never use

Do not add these packages, do not write code against them, and replace them when found in a test project.

| Tool | Never because | Use instead |
|------|---------------|-------------|
| `FluentAssertions` | The skill standardizes on its maintained fork; two assertion libraries in one codebase is noise | `AwesomeAssertions` |
| `FluentAssertions.Web` | Built on `FluentAssertions` | `AwesomeAssertions.Web` |
| `Moq` | The skill standardizes on one mocking library | `NSubstitute` |
