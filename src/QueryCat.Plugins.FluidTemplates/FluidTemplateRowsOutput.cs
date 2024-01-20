using System.ComponentModel;
using System.Text;
using Fluid;
using Fluid.Ast;
using Fluid.Values;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Backend.Execution;
using QueryCat.Backend.Relational.Iterators;
using QueryCat.Backend.Storage;

namespace QueryCat.Plugins.FluidTemplates;

internal class FluidTemplateRowsOutput : IRowsOutput
{
    [Description("Writes data to a Fluid template.")]
    [FunctionSignature("fluid_template(template: string, out: string, var_name: string = 'rows'): object<IRowsOutput>")]
    public static VariantValue FluidTemplate(FunctionCallInfo args)
    {
        var templateFile = args.GetAt(0).AsString;
        var outputFile = args.GetAt(1).AsString;
        var variableName = args.GetAt(2).AsString;

        return VariantValue.CreateFromObject(new FluidTemplateRowsOutput(templateFile, outputFile, variableName,
            args.ExecutionThread));
    }

    private const string QueryCatContextKey = "$$qcat_context";
    private const string QueryCatExecutionThreadKey = "$$qcat_exec_thread";
    private const string QueryCatRowsKey = "$$qcat_rows_key";

    private readonly string _templateFile;
    private readonly string _outFile;
    private readonly string _varName;
    private readonly IExecutionThread _executionThread;
    private readonly TemplateOptions _templateOptions;
    private readonly List<VariantValue[]> _rows = new();

    private QueryContext _queryContext = NullQueryContext.Instance;

    /// <inheritdoc />
    public QueryContext QueryContext
    {
        get => _queryContext;
        set => _queryContext = value;
    }

    private static readonly FluidParser Parser = new();

    static FluidTemplateRowsOutput()
    {
        Parser.RegisterEmptyBlock("run", (statements, writer, encoder, context) =>
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
            var result = executionThread.Run(sb.ToString());
            var iterator = ExecutionThreadUtils.ConvertToIterator(result);
            context.SetValue(varKey, new EnumerableRowsIterator(iterator));
            return ValueTask.FromResult(Completion.Normal);
        });
    }

    public FluidTemplateRowsOutput(string templateFile, string outFile, string varName, IExecutionThread executionThread)
    {
        _templateFile = templateFile;
        _outFile = outFile;
        _varName = varName;
        _executionThread = executionThread;

        _templateOptions = new TemplateOptions();
        _templateOptions.MemberAccessStrategy.Register<VariantValue, object>((obj, name) =>
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
                _ => obj.ToString(),
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
    public void Open()
    {
    }

    /// <inheritdoc />
    public void Close()
    {
        var templateText = File.ReadAllText(_templateFile);
        if (Parser.TryParse(templateText, out var template, out var error))
        {
            var subExecutionThread = new ExecutionThread((ExecutionThread)_executionThread); // TODO: not good cast.
            subExecutionThread.Options.DefaultRowsOutput = NullRowsOutput.Instance;
            var context = new TemplateContext(template, _templateOptions);
            context.AmbientValues[QueryCatContextKey] = _queryContext;
            context.AmbientValues[QueryCatExecutionThreadKey] = subExecutionThread;
            context.AmbientValues[QueryCatRowsKey] = _varName;
            context.SetValue(_varName, _rows);

            File.WriteAllText(_outFile, template.Render(context));
        }
        else
        {
            throw new QueryCatException($"Cannot parse template: {error}.");
        }
    }

    /// <inheritdoc />
    public void Reset()
    {
    }

    /// <inheritdoc />
    public RowsOutputOptions Options { get; } = new();

    /// <inheritdoc />
    public void WriteValues(in VariantValue[] values)
    {
        // Cache here and write all on close.
        _rows.Add(values);
    }

    private static FluidValue CreateFluidValue(VariantValue variantValue)
    {
        var type = variantValue.GetInternalType();
        return type switch
        {
            DataType.Null => NilValue.Instance,
            DataType.Boolean => BooleanValue.Create(variantValue.AsBoolean),
            DataType.Float => NumberValue.Create((decimal)variantValue.AsFloat),
            DataType.Integer => NumberValue.Create(variantValue.AsInteger),
            DataType.Numeric => NumberValue.Create(variantValue.AsNumeric),
            DataType.String => StringValue.Create(variantValue.AsString),
            DataType.Timestamp => new DateTimeValue(variantValue.AsTimestamp),
            DataType.Interval => new ObjectValue(variantValue.AsInterval),
            _ => new ObjectValue(variantValue.AsObject),
        };
    }
}
