using System.ComponentModel;
using System.Globalization;
using System.Text;
using Fluid;
using Fluid.Values;
using QueryCat.Backend;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Backend.Execution;
using QueryCat.Backend.Relational.Iterators;
using QueryCat.Backend.Storage;
using Completion = Fluid.Ast.Completion;

namespace QueryCat.Plugins.FluidTemplates;

internal class FluidTemplateRowsOutput : IRowsOutput
{
    [Description("Writes data to a Fluid template.")]
    [FunctionSignature("fluid_template(template: string, out: string, var_name: string = 'rows'): object<IRowsOutput>")]
    public static VariantValue FluidTemplate(IExecutionThread thread)
    {
        var templateFile = thread.Stack[0].AsString;
        var outputFile = thread.Stack[1].AsString;
        var variableName = thread.Stack[2].AsString;

        var optionsThread = (IExecutionThread<ExecutionOptions>)thread;
        return VariantValue.CreateFromObject(new FluidTemplateRowsOutput(templateFile, outputFile, variableName, optionsThread));
    }

    private const string QueryCatContextKey = "$$qcat_context";
    private const string QueryCatExecutionThreadKey = "$$qcat_exec_thread";
    private const string QueryCatRowsKey = "$$qcat_rows_key";

    private readonly string _templateFile;
    private readonly string _outFile;
    private readonly string _varName;
    private readonly IExecutionThread<ExecutionOptions> _executionThread;
    private readonly TemplateOptions _templateOptions;
    private readonly List<VariantValue[]> _rows = new();

    private QueryContext _queryContext = NullQueryContext.Instance;

    /// <inheritdoc />
    public QueryContext QueryContext
    {
        get => _queryContext;
        set => _queryContext = value;
    }

    private static readonly FluidParser _parser = new();

    static FluidTemplateRowsOutput()
    {
        _parser.RegisterEmptyBlock("run", async (statements, writer, encoder, context) =>
        {
            var queryContext = (QueryContext)context.AmbientValues[QueryCatContextKey];
            var executionThread = (ExecutionThread)context.AmbientValues[QueryCatExecutionThreadKey];
            var varKey = (string)context.AmbientValues[QueryCatRowsKey];

            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            foreach (var statement in statements)
            {
                statement.WriteToAsync(stringWriter, encoder, context).GetAwaiter().GetResult();
            }
            var result = await executionThread.RunAsync(sb.ToString());
            var iterator = RowsIteratorConverter.Convert(result);
            context.SetValue(varKey, new EnumerableRowsIterator(iterator));
            return Completion.Normal;
        });
    }

    public FluidTemplateRowsOutput(string templateFile, string outFile, string varName, IExecutionThread<ExecutionOptions> executionThread)
    {
        _templateFile = templateFile;
        _outFile = outFile;
        _varName = varName;
        _executionThread = executionThread;

        _templateOptions = new TemplateOptions();
        _templateOptions.MemberAccessStrategy.Register<VariantValue, object?>((obj, name) =>
        {
            return name switch
            {
                nameof(VariantValue.AsInteger) => obj.AsInteger,
                nameof(VariantValue.AsString) => obj.AsString,
                nameof(VariantValue.AsFloat) => obj.AsFloat,
                nameof(VariantValue.AsTimestamp) => obj.AsTimestamp,
                nameof(VariantValue.AsInterval) => obj.AsInterval,
                nameof(VariantValue.AsBoolean) => obj.AsBoolean,
                nameof(VariantValue.AsNumeric) => obj.AsNumeric,
                _ => obj.ToString(CultureInfo.InvariantCulture),
            };
        });
        _templateOptions.MemberAccessStrategy.Register<Row>();
        _templateOptions.MemberAccessStrategy.Register<Row, object>((obj, name) => obj[name]);
        _templateOptions.Filters.AddFilter("first_value", (input, arguments, context) =>
        {
            var row = input.Enumerate(context).Select(x => x.ToObjectValue()).FirstOrDefault() as Row;
            if (row == null)
            {
                return NilValue.Empty;
            }
            return CreateFluidValue(row[0]);
        });
    }

    /// <inheritdoc />
    public Task OpenAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        var templateText = await File.ReadAllTextAsync(_templateFile, cancellationToken);
        if (_parser.TryParse(templateText, out var template, out var error))
        {
            _executionThread.Options.DefaultRowsOutput = NullRowsOutput.Instance;
            var context = new TemplateContext(template, _templateOptions);
            context.AmbientValues[QueryCatContextKey] = _queryContext;
            context.AmbientValues[QueryCatExecutionThreadKey] = _executionThread;
            context.AmbientValues[QueryCatRowsKey] = _varName;
            context.SetValue(_varName, _rows);

            await File.WriteAllTextAsync(_outFile, await template.RenderAsync(context), cancellationToken);
        }
        else
        {
            throw new QueryCatException($"Cannot parse template: {error}.");
        }
    }

    /// <inheritdoc />
    public Task ResetAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public RowsOutputOptions Options { get; } = new();

    /// <inheritdoc />
    public ValueTask<ErrorCode> WriteValuesAsync(VariantValue[] values, CancellationToken cancellationToken = default)
    {
        // Cache here and write all on close.
        _rows.Add(values);
        return ValueTask.FromResult(ErrorCode.OK);
    }

    private static FluidValue CreateFluidValue(VariantValue variantValue)
    {
        return variantValue.Type switch
        {
            DataType.Null => NilValue.Instance,
            DataType.Boolean => BooleanValue.Create(variantValue.AsBoolean),
            DataType.Float => NumberValue.Create(variantValue.ToDecimal()),
            DataType.Integer => NumberValue.Create(variantValue.ToInt64()),
            DataType.Numeric => NumberValue.Create(variantValue.ToDecimal()),
            DataType.String => StringValue.Create(variantValue.AsString),
            DataType.Timestamp => new DateTimeValue(variantValue.ToDateTime()),
            DataType.Interval => new ObjectValue(variantValue.AsInterval),
            _ => new ObjectValue(variantValue.AsObject),
        };
    }
}
