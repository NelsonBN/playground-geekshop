# Project and file structure

Every implementation project has a sibling test project and every implementation file with behaviour has a sibling test file. The mapping is by name, so the agent, the script and the Stop hook find tests without guessing.

| Source | Mirror |
|--------|--------|
| `src/<P>/<P>.csproj` | `tests/<P>.Tests/<P>.Tests.csproj` |
| `src/<P>/<P>.csproj` (web/API, `Microsoft.NET.Sdk.Web` or an ASP.NET Core reference) | `tests/<P>.IntegrationTests/<P>.IntegrationTests.csproj` |
| `src/<P>/<Folder>/<Name>.cs` (behaviour) | `tests/<P>.Tests/<Folder>/<Name>Tests.cs` |
| `src/<P>/<Folder>/<Name>.cs` (only works against real infrastructure or a framework runtime) | `tests/<P>.IntegrationTests/<Folder>/<Name>Tests.cs` |

```
src/Api/Orders/Order.cs                    ->  tests/Api.Tests/Orders/OrderTests.cs
src/Api/Orders/OrdersEndpoint.cs           ->  tests/Api.IntegrationTests/Orders/OrdersEndpointTests.cs
src/Api/Orders/OrderDto.cs (data only)     ->  none
src/Api/Orders/IOrderRepository.cs         ->  none (interface only)
```

## Naming: only the project says "integration"

The **project** is named `<P>.IntegrationTests`; the files and classes inside it follow the same convention as unit tests: `<Name>Tests.cs` containing `<Name>Tests`, in the folder mirroring the source. Never `<Name>IntegrationTests.cs`. The project, its directory and the `[Trait("Category", "Integration")]` tag are what tell the two kinds apart. One test class per class under test (`CreateOrderEndpointTests` and `CancelOrderEndpointTests`, not one `OrdersEndpointsTests` for several endpoints).

## You decide which mirror applies

The script cannot know what a file does, so you choose by reading the code ([unit-tests.md](unit-tests.md#unit-or-integration)): behaviour that can run with its interfaces substituted gets the unit mirror; code that only makes sense against real infrastructure or a framework runtime gets the integration mirror. The check passes when either mirror exists. The coverage and mutation gates apply to files that have the unit mirror; an integration-mirrored file is proven by its integration tests. Files declaring only interfaces, enums, delegates or data-only types (no method, constructor body or expression body) need neither and are skipped automatically.

## Details

- The test class in `ClientTests.cs` is `ClientTests`, in the namespace `<P>.Tests.<Folder>`. A source file declaring several types is covered by one `<FileName>Tests.cs`. Folders mirror exactly.
- The test project is named after its own source project and references only it, with no `InternalsVisibleTo` unless truly needed ([unit-tests.md](unit-tests.md#internals)) (the integration project may add `Mvc.Testing`, Testcontainers and so on). The integration project is also required for non-web projects that own a real boundary worth testing for real.
- `Builders/`, `Fakes/`, `Fixtures/` have no source counterpart and are not checked. Unit and coverage runs use `*.Tests.csproj` only; integration runs use `*.IntegrationTests.csproj` only.
- Exempt (`structureIgnore` in [../quality-gate.json](../quality-gate.json)): `Program.cs`, `AssemblyInfo.cs`, `*.g.cs`, `*.Designer.cs`, `Migrations/**`. Add to the list only with the user's approval; if a file has behaviour worth keeping, test it instead.

## Enforcement

`tests.cs structure` checks every `.cs` file changed against the default branch (the hook: against `HEAD`) and reports the exact missing path:

```
missing test project  tests/Domain.Tests/Domain.Tests.csproj  (for src/Domain/Domain.csproj)
missing test file     tests/Domain.Tests/Clients/ClientTests.cs  (for src/Domain/Clients/Client.cs; or tests/Domain.IntegrationTests/Clients/ClientTests.cs when it only works against real infrastructure)
missing integration   tests/Api.IntegrationTests/Api.IntegrationTests.csproj  (Api is a web project)
```

Fill each gap with a real test, never an empty file. To create a project, copy [../examples/TestProject.csproj.example](../examples/TestProject.csproj.example) to the mirror path, reference only its source project, and `dotnet sln add` it. Test files do not write `using Xunit;`, `using AwesomeAssertions;` or `using NSubstitute;`: the csproj declares them as plain `<Using Include="..." />` items, one line per referenced library.

Legacy code is not flagged until it is touched; when you edit a legacy file, add its mirror test in the same change. Do not bulk-generate empty test files.
