# Develop Plugin

Skill for creating and updating QueryCat plugins. Use when the user asks to create, develop, or scaffold a new plugin, or needs guidance on plugin structure, functions, rows inputs, or registration.

## Overview

QueryCat plugins extend the system with custom data sources (rows inputs) and functions. Plugins are .NET libraries discovered via reflection. They are packaged as `.dll` or `.nupkg` files and loaded by the QueryCat host at runtime.

## Plugin Project Structure

```
QueryCat.Plugins.<Name>/
├── QueryCat.Plugins.<Name>.csproj   # Project file
├── Registration.cs                   # Entry point — registers all functions
├── Functions/                        # Custom functions (optional)
│   └── MyFunction.cs
├── Inputs/                           # Rows input sources (optional)
│   └── MyRowsInput.cs
├── Models/                           # Domain models (optional)
│   └── MyModel.cs
└── Docs/                             # Plugin documentation (optional)
```

**IMPORTANT:** The project name MUST contain the word "Plugin" (case-insensitive).

### Project File (.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <PackageVersion>0.1.0</PackageVersion>
    <Title>My Plugin</Title>
    <Authors>Author Name</Authors>
    <Description>Plugin description.</Description>
    <PackageTags>myplugin querycat</PackageTags>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="QueryCat" Version="0.19.0" />
  </ItemGroup>
</Project>
```

### Registration Class

Every plugin MUST have a `Registration` static class. The plugin loader discovers it via reflection.

```csharp
using QueryCat.Backend.Core.Functions;

namespace QueryCat.Plugins.MyPlugin;

public static class Registration
{
    public static void RegisterFunctions(IFunctionsManager functionsManager)
    {
        functionsManager.RegisterFunction(MyFunction.MyFunctionDelegate);
        functionsManager.RegisterFunction(MyRowsInput.MyRowsInputFunction);
    }

    // Optional. Called after plugin is loaded.
    public static Task OnLoadAsync(IExecutionThread executionThread, CancellationToken cancellationToken)
        => Task.CompletedTask;
}
```

## Two Plugin Connection Types

### 1. .NET Assembly Plugin (recommended)
- All calls in same process — very efficient.
- Reference `QueryCat` NuGet package.
- Published as `.dll` or `.nupkg`.
- `Registration` class discovered automatically by reflection.

### 2. Thrift Connector Plugin (standalone executable)
- Plugin runs as separate process, communicates via Apache Thrift.
- Reference `QueryCat.Plugins.Client` NuGet package.

## Functions

Functions extend QueryCat's query language. Every function is a static method with specific attributes.

### Required Attributes

| Attribute | Purpose |
|-----------|---------|
| `[FunctionSignature("name(a: type): ret")]` | Declares function name, parameters, return type |
| `[Description("...")]` | Human-readable description |
| `[SafeFunction]` | Marks function as read-only (optional, recommended for reads) |

### Function Delegate Signatures

Functions must match one of:
```csharp
Func<IExecutionThread, VariantValue>                                    // sync
Func<IExecutionThread, CancellationToken, ValueTask<VariantValue>>      // async
```

### Function Signature Syntax

```
function_name(param1: type1, param2?: type2 = default): return_type
```

Return types: `void`, `integer`, `string`, `float`, `numeric`, `boolean`, `timestamp`, `interval`, `blob`, `object<InterfaceName>`.

### Simple Function Example

```csharp
using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

internal static class MyFunction
{
    [SafeFunction]
    [Description("Returns current timestamp as string.")]
    [FunctionSignature("my_timestamp(): string")]
    public static VariantValue MyFunctionDelegate(IExecutionThread thread)
    {
        return new VariantValue(DateTime.Now.ToString("o"));
    }
}
```

### Function with Arguments

Arguments are read from `thread.Stack` in order (index 0 = first argument):

```csharp
[SafeFunction]
[Description("Adds two integers.")]
[FunctionSignature("my_add(a: integer, b: integer): integer")]
public static VariantValue AddFunction(IExecutionThread thread)
{
    var a = thread.Stack[0].AsInteger;  // or thread.Stack.Pop()
    var b = thread.Stack[1].AsInteger;
    return new VariantValue((a ?? 0) + (b ?? 0));
}
```

### Async Function Example

```csharp
[SafeFunction]
[Description("Fetches data from remote service.")]
[FunctionSignature("my_fetch(url: string): string")]
public static async ValueTask<VariantValue> FetchFunctionAsync(
    IExecutionThread thread, CancellationToken cancellationToken)
{
    var url = thread.Stack[0].AsString;
    var result = await httpClient.GetStringAsync(url, cancellationToken);
    return new VariantValue(result);
}
```

### VariantValue Accessors

Use `.AsString`, `.AsInteger`, `.AsFloat`, `.AsBoolean`, `.AsNumeric`, etc. to read typed values from the stack.


## Rows Inputs (Data Sources)

Rows inputs provide tabular data queryable with `SELECT * FROM my_input()`. Three approaches from lowest to highest level.

### Approach 1: IRowsInput (Low-Level)

Full control over reading lifecycle. Implement all methods directly.

```csharp
public class MyRowsInput : IRowsInput
{
    [SafeFunction]
    [Description("My data source.")]
    [FunctionSignature("my_input(param: string): object<IRowsInput>")]
    public static VariantValue MyInputFunction(IExecutionThread thread)
    {
        var param = thread.Stack[0].AsString;
        return VariantValue.CreateFromObject(new MyRowsInput(param));
    }

    public Column[] Columns { get; } = [
        new("id", DataType.Integer, "Row identifier."),
        new("name", DataType.String, "Row name."),
    ];
    public string[] UniqueKey { get; } = [];
    public QueryContext QueryContext { get; set; } = NullQueryContext.Instance;

    public Task OpenAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task CloseAsync(CancellationToken ct = default) => Task.CompletedTask;

    public ErrorCode ReadValue(int columnIndex, out VariantValue value)
    {
        value = columnIndex switch { 0 => new VariantValue(_id), 1 => new VariantValue(_name), _ => VariantValue.Null };
        return ErrorCode.OK;
    }

    public ValueTask<bool> ReadNextAsync(CancellationToken ct = default) { /* advance; return false when done */ }
    public Task ResetAsync(CancellationToken ct = default) => Task.CompletedTask;
    public void Explain(IndentedStringBuilder sb) => sb.AppendLine("MyRowsInput");
    public IReadOnlyList<KeyColumn> GetKeyColumns() => [];
    public void SetKeyColumnValue(int ci, VariantValue v, VariantValue.Operation op) { }
    public void UnsetKeyColumnValue(int ci, VariantValue.Operation op) { }
}
```

**Lifecycle:** `OpenAsync()` → set `QueryContext` → `ReadNextAsync()` → `ReadValue()` per column → repeat → `CloseAsync()`.

### Approach 2: IRowsIterator (Simpler)

No open/close lifecycle. Set values on a shared `Row` instance.

```csharp
public class MyIterator : IRowsIterator
{
    [SafeFunction]
    [Description("My iterator.")]
    [FunctionSignature("my_iterator(): object<IRowsIterator>")]
    public static VariantValue MyIteratorFunction(IExecutionThread thread)
        => VariantValue.CreateFromObject(new MyIterator());

    public Column[] Columns { get; } = [ new("id", DataType.Integer), new("name", DataType.String) ];
    public Row Current { get; }

    public MyIterator() { Current = new Row(Columns); }

    public ValueTask<bool> MoveNextAsync(CancellationToken ct = default)
    {
        Current["id"] = new VariantValue(_id);
        Current["name"] = new VariantValue(_name);
        return ValueTask.FromResult(true); // false when done
    }

    public Task ResetAsync(CancellationToken ct = default) => Task.CompletedTask;
    public void Explain(IndentedStringBuilder sb) { }
}
```

### Approach 3: EnumerableRowsInput<T> (Highest Level — Recommended)

Best for API-based data sources. Maps a .NET class to columns automatically.

```csharp
public class MyDataInput : EnumerableRowsInput<MyModel>
{
    [SafeFunction]
    [Description("My data source.")]
    [FunctionSignature("my_data(): object<IRowsInput>")]
    public static VariantValue MyDataFunction(IExecutionThread thread)
        => VariantValue.CreateFromObject(new MyDataInput());

    protected override void Initialize(ClassRowsFrameBuilder<MyModel> builder)
    {
        builder
            .AddProperty(m => m.Id)
            .AddProperty(m => m.Name)
            .AddProperty(m => m.CreatedAt, "Creation date.")
            .AddKeyColumn("name", isRequired: false, operation: VariantValue.Operation.Equals);
    }

    protected override IEnumerable<MyModel> GetData(Fetcher<MyModel> fetcher)
    {
        var nameFilter = GetKeyColumnValue("name"); // read WHERE filter values
        yield return new MyModel { Id = 1, Name = "Alice", CreatedAt = DateTime.Now };
    }
}
```

**Key features:**
- `AddProperty()` maps class properties to columns (auto-detects types).
- `AddKeyColumn()` declares filterable columns for query optimization.
- `GetKeyColumnValue()` reads filter values set by WHERE clause.
- If `Initialize` adds no properties, all public properties are added automatically.


## Core Types Reference

### DataType Enum

| Value | Description |
|-------|-------------|
| `Void` | No return value |
| `Null` | Unknown/null |
| `Integer` | Integer number |
| `String` | Text |
| `Float` | Floating point |
| `Timestamp` | Date/time |
| `Boolean` | True/false |
| `Numeric` | Decimal 128-bit |
| `Interval` | Time span |
| `Blob` | Binary large object |
| `Object` | Any object |
| `Dynamic` | Type determined at runtime |
| `Array` | Array of values |
| `Map` | Key-value pairs |

### VariantValue

Universal value container:
- `new VariantValue(42)` — integer
- `new VariantValue("text")` — string
- `new VariantValue(3.14)` — float
- `new VariantValue(true)` — boolean
- `VariantValue.Null` — null value
- `VariantValue.CreateFromObject(obj)` — wrap any object (for IRowsInput/IRowsIterator returns)

### Column

```csharp
new Column("name", DataType.String, "Optional description")
new Column("name", "sourceName", DataType.Integer)
```

Columns starting with `__` are hidden from output.

### IExecutionThread

Provides access to:
- `thread.Stack` — function arguments stack
- `thread.FunctionsManager` — register/find/call functions
- `thread.ConfigStorage` — persistent key-value config
- `thread.ObjectSelector` — resolve object property expressions
- `thread.RunAsync(query)` — execute nested queries

## Naming Conventions

- **Project name:** `QueryCat.Plugins.<Name>` (must contain "Plugin")
- **Namespace:** `QueryCat.Plugins.<Name>` or `QueryCat.Plugins.<Name>.<SubNamespace>`
- **Function names:** `snake_case` (e.g., `github_branches`, `clipboard_get`)
- **Function delegate field:** `<ClassName>Function` or `<DescriptiveName>Function`
- **Column names:** `snake_case` (e.g., `pull_request_id`)
- **Class names:** `PascalCase` (e.g., `BranchesRowsInput`)

## Common Patterns

### Plugin with Configuration (Token/Settings)

```csharp
[Description("Set API token.")]
[FunctionSignature("myplugin_set_token(token: string): void")]
public static VariantValue SetTokenFunction(IExecutionThread thread)
{
    var token = thread.Stack[0].AsString;
    thread.ConfigStorage.SetValue("myplugin.token", new VariantValue(token));
    return VariantValue.Null;
}
```

### Base Class for Shared Logic

When multiple inputs share common setup (HTTP client, auth), create a base class:

```csharp
internal abstract class BaseRowsInput : RowsInput
{
    protected static readonly HttpClient HttpClient = new();
    protected readonly string Owner;
    protected readonly string Repo;

    protected BaseRowsInput(string owner, string repo)
    {
        Owner = owner;
        Repo = repo;
    }
}
```

## Testing a Plugin

```bash
dotnet build
qcat "select * from my_input()" --plugin-dirs ./bin/Debug/net10.0/
```

## Checklist for New Plugin

1. ✅ Project name contains "Plugin" (e.g., `QueryCat.Plugins.MyFeature`)
2. ✅ References `QueryCat` or `QueryCat.Plugins.Client` NuGet package
3. ✅ Has `Registration` class with `RegisterFunctions(IFunctionsManager)` method
4. ✅ All functions have `[FunctionSignature]` and `[Description]` attributes
5. ✅ Read-only functions are marked with `[SafeFunction]`
6. ✅ Function delegates match required signatures
7. ✅ Rows inputs return `VariantValue.CreateFromObject(input)` from factory method
8. ✅ Function signature return type for rows inputs uses `object<IRowsInput>` or `object<IRowsIterator>`
9. ✅ Columns are defined with proper `DataType` values
10. ✅ `UniqueKey` is set for cacheable inputs
