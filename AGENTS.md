# QueryCat Plugins

This repository holds the main plugins for [QueryCat](https://github.com/krasninja/querycat). Each plugin is a .NET class library. It adds functions and rows inputs (data sources) to the QueryCat query language. The QueryCat host loads plugins at runtime from `.dll` or `.nupkg` files.

## Repository Layout

```
backend/                      # Git submodule: the QueryCat engine (krasninja/querycat). Read-only reference, do not edit here.
src/
├── Directory.Build.props     # Shared settings: Nullable, ImplicitUsings, StyleCop, license, repo URLs
├── QueryCat.Plugins.Build/   # Cake Frosting build tool (build/pack tasks, list of published plugins)
└── QueryCat.Plugins.<Name>/  # One directory per plugin
output/                       # Build/pack artifacts (.nupkg, published binaries)
build.sh / build.ps1          # Entry point for the Cake build tool
QueryCat.Plugins.sln
```

The `backend` submodule is the source of truth for the plugin API (`backend/src/QueryCat.Backend.Core`). If a type or method is unclear, look it up there instead of guessing. Plugins reference the **NuGet** `QueryCat` package, not the submodule, so the API that plugins can use is the one in the package version given in the `.csproj` (currently `0.19.0`).

If `backend/` is empty, run `git submodule update --init`.

## Build, Pack, Run

```bash
# Build one plugin
dotnet build src/QueryCat.Plugins.GitHub

# Pack all plugins as .nupkg into output/ (what CI runs on push to main)
./build.sh -t pack-plugins
./build.sh -t pack-plugins --Project=GitHub      # only plugins whose name contains "GitHub"

# Publish standalone binaries for linux-x64, linux-arm64, win-x64
./build.sh -t build-plugins --Project=GitHub --Platform=linux-x64

# Try a plugin with the qcat CLI
qcat "select * from github_gists() limit 5" --plugin-dirs ./src/QueryCat.Plugins.GitHub/bin/Debug/net10.0/
```

There are no unit tests in this repository. To check a change, build it and run real queries with `qcat`.

## Plugin Project Layout

```
src/QueryCat.Plugins.<Name>/
├── QueryCat.Plugins.<Name>.csproj
├── Registration.cs     # Required entry point
├── General.cs          # Shared constants, such as config keys (optional)
├── BaseRowsInput.cs    # Shared base class for the plugin's inputs (optional)
├── Functions/          # Scalar functions
├── Inputs/             # Rows inputs
├── Docs/               # Extra docs (optional)
├── README.md           # Usage, setup, example queries
├── CHANGELOG.md        # Keep a Changelog format
├── Functions.md        # Generated, do not edit by hand
└── Schema.md           # Generated, do not edit by hand
```

### Adding a New Plugin

1. Create `src/QueryCat.Plugins.<Name>/`. Copy the `.csproj` from a similar plugin (for example, GitHub for an HTTP API).
2. Add the project to `QueryCat.Plugins.sln`.
3. Add the project name to `Projects` in `src/QueryCat.Plugins.Build/BuildContext.cs`. If you skip this step, the plugin is not packed or published.
4. Add `README.md` and `CHANGELOG.md`, then generate `Functions.md` and `Schema.md` (see [Generated Docs](#generated-docs)).

### Project File

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <PackageVersion>0.1.0</PackageVersion>
    <Title>My Plugin</Title>
    <Authors>Ivan Kozhin</Authors>
    <Description>My plugin.</Description>
    <PackageTags>myplugin querycat</PackageTags>
    <AssemblyName Condition=" '$(UseAssemblyName)' == 'true' ">qcat-plugins-myplugin-$(PackageVersion)-$(Runtime)</AssemblyName>
    <PublishTrimmed>True</PublishTrimmed>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="SomeThirdParty.Library" Version="1.2.3" />
    <PackageReference Include="QueryCat" Version="0.19.0" />
  </ItemGroup>

  <!-- Third-party assemblies must be packed into the .nupkg explicitly, otherwise the plugin fails to load. -->
  <ItemGroup>
    <Content Include="bin/$(Configuration)/$(TargetFramework)/SomeThirdParty.Library.dll">
      <CopyToOutputDirectory>Never</CopyToOutputDirectory>
      <Pack>true</Pack>
      <PackagePath>lib/$(TargetFramework)</PackagePath>
    </Content>
  </ItemGroup>
</Project>
```

- The assembly/project name **must contain "Plugin"** (case-insensitive). The loader ignores other files.
- Keep the `QueryCat` package version the same across all plugins.
- `Nullable`, `ImplicitUsings`, and StyleCop already come from `src/Directory.Build.props`. Do not repeat them.

### Versioning

When you change a plugin, bump `<PackageVersion>` in its `.csproj` and add a matching entry at the top of its `CHANGELOG.md`:

```markdown
## [0.5.15]

- Add `github_gists` function.
- Fix `author_login` property mapping in `github_commits`.
```

### Generated Docs

`Functions.md` and `Schema.md` are generated from the built assembly by tasks in the backend build tool. Regenerate them after you add or change functions or columns:

```bash
dotnet build src/QueryCat.Plugins.<Name>
DLL=$PWD/src/QueryCat.Plugins.<Name>/bin/Debug/net10.0/QueryCat.Plugins.<Name>.dll
OUT=$PWD/src/QueryCat.Plugins.<Name>
(cd backend && ./build.sh -t get-functions-markdown --File=$DLL --Out=$OUT)
(cd backend && ./build.sh -t get-inputs-markdown --File=$DLL --Out=$OUT)
```

Use absolute paths, because the build tool runs from its own project directory.

## Plugin API

The `.agents/skills/develop-plugin.md` skill has full walkthroughs. The essentials are below.

### Registration

The loader uses reflection to find a public static class named `Registration`:

```csharp
namespace QueryCat.Plugins.Github;

/// <summary>
/// The special registration class that is called by plugin loader.
/// </summary>
public static class Registration
{
    public static void RegisterFunctions(IFunctionsManager functionsManager)
    {
        functionsManager.RegisterFunction(SetToken.SetTokenFunction);
        functionsManager.RegisterFunction(GistsRowsInput.GitHubGistsFunction);
    }

    // Optional, called after the plugin is loaded.
    public static Task OnLoadAsync(IExecutionThread executionThread, CancellationToken cancellationToken)
        => Task.CompletedTask;
}
```

Every new function must be registered here. If you forget, it compiles but is not available.

### Functions

A function is a static method with one of these signatures:

```csharp
VariantValue Fn(IExecutionThread thread)
ValueTask<VariantValue> Fn(IExecutionThread thread, CancellationToken cancellationToken)
```

Attributes:
- `[FunctionSignature("name(a: string, b?: integer = 10): string")]` (required)
- `[Description("...")]` (required)
- `[SafeFunction]` for functions with no side effects

Signature types are `void`, `integer`, `string`, `float`, `numeric`, `boolean`, `timestamp`, `interval`, `blob`, `any`, and `object<IRowsInput>` / `object<IRowsIterator>` / `object<IRowsOutput>` (an output target; see `FluidTemplates`).

Arguments come from `thread.Stack`: either by index (`thread.Stack[0].AsString`) or with `thread.Stack.Pop()` in argument order. To read a value, use `.AsString`, `.AsInteger`, `.AsFloat`, `.AsBoolean`, `.AsNumeric`, `.AsTimestamp`, and check `.IsNull` first.

```csharp
internal static class SetToken
{
    [SafeFunction]
    [Description("GitHub authentication.")]
    [FunctionSignature("github_set_token(token: string): void")]
    public static async ValueTask<VariantValue> SetTokenFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = thread.Stack.Pop().AsString;
        await thread.ConfigStorage.SetAsync(General.GitHubToken, new VariantValue(token), cancellationToken);
        return VariantValue.Null;
    }
}
```

`IConfigStorage` methods are async: `SetAsync`, `GetAsync`, `HasAsync`, and the extension method `GetOrDefaultAsync`. Store credentials there and read them in the input factory function.

### Rows Inputs

Rows inputs are table-valued functions, for example `select * from my_input()`. Choose the highest level that fits:

| Base | Use when |
|------|----------|
| `AsyncEnumerableRowsInput<T>` | Async data source such as an HTTP API. Most plugins here use this. |
| `EnumerableRowsInput<T>` | Synchronous data source |
| `IRowsIterator` | Simple streaming without key columns |
| `IRowsInput` | You need full control over the open/read/close lifecycle and key handling |

Typical pattern (from `QueryCat.Plugins.GitHub/Inputs/GistsRowsInput.cs`):

```csharp
internal sealed class GistsRowsInput : BaseRowsInput<Gist>
{
    [SafeFunction]
    [Description("Return GitHub gists.")]
    [FunctionSignature("github_gists(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> GitHubGistsFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new GistsRowsInput(token));
    }

    protected override void Initialize(ClassRowsFrameBuilder<Gist> builder)
    {
        builder
            .AddDataPropertyAsJson()
            .AddProperty("id", p => p.Id, "The unique id of the gist.")
            .AddProperty("created_at", p => p.CreatedAt, "The timestamp when the gist was created.")
            .AddKeyColumn("id", isRequired: false)
            .AddKeyColumn("user_login", isRequired: false);
    }

    protected override IAsyncEnumerable<Gist> GetDataAsync(Fetcher<Gist> fetcher, CancellationToken cancellationToken = default)
    {
        var id = GetKeyColumnValue("id");           // value from WHERE id = ...
        if (!id.IsNull)
        {
            return fetcher.FetchOneAsync(async ct => await Client.Gist.Get(id.AsString), cancellationToken);
        }
        fetcher.PageStart = 1;
        return fetcher.FetchPagedAsync(async (page, limit, ct) =>
            await Client.Gist.GetAll(new ApiOptions { StartPage = page, PageCount = 1, PageSize = limit }), cancellationToken);
    }
}
```

- `AddKeyColumn` declares a column whose `WHERE` value is passed to the input. Use it to push filters down to the API. Required key columns (`isRequired: true`) must appear in the query.
- `Fetcher<T>` handles paging and limits: `FetchOneAsync`, `FetchPagedAsync`, `FetchAllAsync`, and others.
- Give every column a description. It appears in `Schema.md`.
- Columns whose names start with `__` are hidden.
- Use a `UniqueKey` for inputs whose results can be cached.

## Conventions

- **Names:** functions and columns use `snake_case` with a plugin prefix (`github_pulls`, `pull_request_id`). Classes use `PascalCase` with a suffix: `*RowsInput` for inputs, plus a function class named after what it does. Factory methods use the `<Plugin><Thing>Function` pattern (`GitHubGistsFunction`).
- **Namespaces:** `QueryCat.Plugins.<Name>`, `.Functions`, `.Inputs`.
- **Visibility:** make implementation classes `internal`. Only `Registration` needs to be public.
- **Style:** follow StyleCop. Add XML doc comments (`/// <summary>`, `/// <inheritdoc />`) the way the code around them does. Use file-scoped namespaces.
- **Logging:** `QueryCat.Backend.Core.Application.LoggerFactory.CreateLogger(typeof(MyClass))`. Use structured messages.
- **Errors:** throw `QueryCatException` for errors the user should see, such as bad arguments or a missing token.
- **Trimming:** plugins are published with `PublishTrimmed`. Avoid reflection-heavy serialization. Use source-generated `JsonSerializerContext` (see `DeepSeek/SourceGenerationContext.cs`).

## Checklist for Changes

1. The project builds without new warnings (`dotnet build src/QueryCat.Plugins.<Name>`).
2. New functions are registered in `Registration.cs` and have `[FunctionSignature]` and `[Description]`. Read-only ones also have `[SafeFunction]`.
3. Rows input factories return `VariantValue.CreateFromObject(...)` and declare `object<IRowsInput>` or `object<IRowsIterator>`.
4. `<PackageVersion>` is bumped and `CHANGELOG.md` is updated.
5. `Functions.md` and `Schema.md` are regenerated if signatures or columns changed. Update `README.md` examples if the change affects them.
6. New third-party dependencies are added as packed `Content` items in the `.csproj`.
7. New plugins are listed in the `.sln` and in `BuildContext.Projects`.
8. The change was tried with `qcat ... --plugin-dirs`.
