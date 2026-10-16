using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Octokit;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Github.Inputs;

/// <summary>
/// GitHub package versions input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/packages.
/// </remarks>
internal sealed class PackageVersionsRowsInput : BaseRowsInput<PackageVersion>
{
    private readonly ILogger _logger = QueryCat.Backend.Core.Application.LoggerFactory.CreateLogger(typeof(PackageVersionsRowsInput));

    [SafeFunction]
    [Description("Return GitHub package versions for a specific package in an organization.")]
    [FunctionSignature("github_package_versions(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> GitHubPackageVersionsFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new PackageVersionsRowsInput(token));
    }

    public PackageVersionsRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<PackageVersion> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_package_version.go.
        builder
            .AddDataPropertyAsJson()
            .AddProperty("organization", DataType.String, _ => GetKeyColumnValue("organization"), "The organization login name.")
            .AddProperty("package_name", DataType.String, _ => GetKeyColumnValue("package_name"), "The name of the package.")
            .AddProperty("package_type", DataType.String, _ => GetKeyColumnValue("package_type"), "The type of the package (npm, maven, rubygems, docker, nuget, or container).")
            .AddProperty("id", p => p.Id, "The unique ID of the package version.")
            .AddProperty("name", p => p.Name, "The name of the package version.")
            .AddProperty("url", p => p.Url, "The API URL of the package version.")
            .AddProperty("html_url", p => p.HtmlUrl, "The HTML URL of the package version.")
            .AddProperty("package_html_url", p => p.PackageHtmlUrl, "The HTML URL of the package.")
            .AddProperty("created_at", p => p.CreatedAt, "The timestamp when the package version was created.")
            .AddProperty("updated_at", p => p.UpdatedAt, "The timestamp when the package version was last updated.")
            .AddKeyColumn("organization", isRequired: true)
            .AddKeyColumn("package_name", isRequired: true)
            .AddKeyColumn("package_type", isRequired: true);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<PackageVersion> GetDataAsync(Fetcher<PackageVersion> fetcher,
        CancellationToken cancellationToken = default)
    {
        var organization = GetKeyColumnValue("organization");
        var packageName = GetKeyColumnValue("package_name");
        var packageType = GetKeyColumnValue("package_type");

        var packageTypeEnum = Enum.Parse<PackageType>(packageType.AsString, ignoreCase: true);

        fetcher.PageStart = 1;
        return fetcher.FetchPagedAsync(async (page, limit, ct) =>
        {
            _logger.LogDebug("Get package versions for organization = {Organization}, package_name = {PackageName}, package_type = {PackageType}.",
                organization, packageName, packageTypeEnum);
            return await Client.Packages.PackageVersions.GetAllForOrg(organization.AsString, packageTypeEnum, packageName.AsString,
                PackageVersionState.Active,
                new ApiOptions { StartPage = page, PageCount = 1, PageSize = limit });
        }, cancellationToken);
    }
}
