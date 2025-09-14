using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Moex.Inputs;

/// <summary>
/// Moscow Exchange.
/// </summary>
/// <remarks>
/// Docs: https://www.moex.com/a2193.
/// </remarks>
internal sealed class MoexInput : RowsInput
{
    [SafeFunction]
    [Description("Moscow Exchange table.")]
    [FunctionSignature("moex_input(path: string): object<IRowsIterator>")]
    public static VariantValue MoexFunction(IExecutionThread thread)
    {
        var path = thread.Stack[0].AsString;
        return VariantValue.CreateFromObject(new MoexInput(path));
    }

    public const string BaseUri = "https://iss.moex.com/";

    internal sealed class MoexDataModel
    {
        public Dictionary<string, MoexType> Metadata { get; set; } = new();

        public List<JsonArray> Data { get; set; } = new();

        public MoexCursor Cursor { get; set; } = new();

        public bool Empty => Data.Count == 0;
    }

    internal sealed class MoexType
    {
        public string Type { get; set; } = string.Empty;
    }

    internal sealed class MoexCursor
    {
        public long[] Data { get; set; } = [-1, 0, 0];

        public long Index => Data[0];

        public long Total => Data[1];

        public long PageSize => Data[2];

        public MoexCursor()
        {
        }

        public MoexCursor(long index, long total, long pageSize)
        {
            Data = [index, total, pageSize];
        }
    }

    /// <inheritdoc />
    public override Column[] Columns { get; protected set; } = [];

    private static readonly HttpClient _httpClient = new();
    private readonly string _path;
    private MoexDataModel? _currentModel;
    private int _currentRecordIndex = -1;
    private int _currentPageIndex = -1;

    static MoexInput()
    {
        _httpClient.BaseAddress = new Uri(BaseUri);
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(
            QueryCat.Backend.Core.Application.ProductName, QueryCat.Backend.Core.Application.GetVersion()));
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public MoexInput(string path)
    {
        _path = $"iss/{path}.json";
    }

    /// <inheritdoc />
    public override async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        var uriBuilder = new UriBuilder(BaseUri);
        uriBuilder.Path = _path + "?iss.data=off";
        var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var model = await ParseFromStreamAsync(stream, cancellationToken);
        if (model == null)
        {
            return;
        }

        var columns = new List<Column>();
        foreach (var column in model.Metadata)
        {
            columns.Add(new Column(column.Key, GetDataTypeByMoexType(column.Value.Type)));
        }
        Columns = columns.ToArray();
    }

    private static async Task<MoexDataModel?> ParseFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var jsonDocument = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var firstObject = jsonDocument.RootElement.EnumerateObject().FirstOrDefault();
        if (string.IsNullOrEmpty(firstObject.Name))
        {
            return null;
        }
        return firstObject.Value.Deserialize(SourceGenerationContext.Default.MoexDataModel);
    }

    /// <inheritdoc />
    public override Task CloseAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override ErrorCode ReadValue(int columnIndex, out VariantValue value)
    {
        if (_currentModel == null)
        {
            value = VariantValue.Null;
            return ErrorCode.NoData;
        }
        if (_currentRecordIndex >= _currentModel.Data.Count)
        {
            value = VariantValue.Null;
            return ErrorCode.InvalidColumnIndex;
        }
        value = VariantValue.CreateFromObject(_currentModel.Data[_currentRecordIndex][columnIndex]);
        return ErrorCode.OK;
    }

    /// <inheritdoc />
    public override async ValueTask<bool> ReadNextAsync(CancellationToken cancellationToken = default)
    {
        if (!await base.ReadNextAsync(cancellationToken))
        {
            return false;
        }

        if (_currentModel == null)
        {
            _currentModel = await ReadNextDataAsync(cancellationToken);
            if (_currentModel == null)
            {
                return false;
            }
        }

        _currentRecordIndex++;
        if (_currentRecordIndex >= _currentModel.Data.Count)
        {
            _currentRecordIndex = 0;
            _currentModel = await ReadNextDataAsync(cancellationToken);
            if (_currentModel == null || _currentModel.Empty)
            {
                return false;
            }
        }

        return true;
    }

    private async Task<MoexDataModel?> ReadNextDataAsync(CancellationToken cancellationToken = default)
    {
        var uriBuilder = new UriBuilder(BaseUri);
        uriBuilder.Path = _path + "?iss.meta=off&iss.json=compact";
        if (_currentModel != null)
        {
            if (_currentPageIndex > 0)
            {
                var beforePageIndex = _currentPageIndex * _currentModel.Cursor.PageSize;
                _currentPageIndex++;
                var afterPageIndex = _currentPageIndex * _currentModel.Cursor.PageSize;
                if (afterPageIndex > _currentModel.Cursor.Total)
                {
                    // Out of total.
                    return null;
                }
                _currentModel.Cursor = new MoexCursor(
                    afterPageIndex,
                    _currentModel.Cursor.Total,
                    _currentModel.Cursor.PageSize);
                if (beforePageIndex == afterPageIndex)
                {
                    // The data set is not iterable.
                    return null;
                }
                uriBuilder.Path = _path + "?start=" + _currentModel.Cursor.Index;
            }
            else
            {
                _currentPageIndex++;
            }
        }
        var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await ParseFromStreamAsync(stream, cancellationToken);
    }

    /// <inheritdoc />
    public override Task ResetAsync(CancellationToken cancellationToken = default)
    {
        _currentModel = null;
        _currentRecordIndex = -1;
        _currentPageIndex = -1;
        return base.ResetAsync(cancellationToken);
    }

    private static DataType GetDataTypeByMoexType(string type)
        => type switch
        {
            "int16" => DataType.Integer,
            "int32" => DataType.Integer,
            "int64" => DataType.Integer,
            "double" => DataType.Numeric,
            "date" => DataType.Timestamp,
            _ => DataType.String
        };
}
