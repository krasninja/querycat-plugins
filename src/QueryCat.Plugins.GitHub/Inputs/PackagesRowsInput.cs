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
/// GitHub packages input.
/// </summary>
/// <remarks>
/// https://docs.github.com/en/rest/packages.
/// </remarks>
internal sealed class PackagesRowsInput : BaseRowsInput<Package>
{
    private readonly ILogger _logger = QueryCat.Backend.Core.Application.LoggerFactory.CreateLogger(typeof(PackagesRowsInput));

    [SafeFunction]
    [Description("Return GitHub packages for an organization.")]
    [FunctionSignature("github_packages(): object<IRowsInput>")]
    public static async ValueTask<VariantValue> GitHubPackagesFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var token = await thread.ConfigStorage.GetOrDefaultAsync(General.GitHubToken, cancellationToken: cancellationToken);
        return VariantValue.CreateFromObject(new PackagesRowsInput(token));
    }

    public PackagesRowsInput(string token) : base(token)
    {
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<Package> builder)
    {
        // For reference: https://github.com/turbot/steampipe-plugin-github/blob/main/github/table_github_package.go.
        builder
            .AddDataPropertyAsJson()
            .AddProperty("organization", DataType.String, _ => GetKeyColumnValue("organization"), "The organization login name.")
            .AddProperty("id", p => p.Id, "The unique ID of the package.")
            .AddProperty("name", p => p.Name, "The name of the package.")
            .AddProperty("package_type", p => p.PackageType.Value, "The type of the package (npm, maven, rubygems, docker, nuget, or container).")
            .AddProperty("visibility", p => p.Visibility.Value, "The visibility of the package (public, private, or internal).")
            .AddProperty("version_count", p => p.VersionCount, "The number of versions of the package.")
            .AddProperty("url", p => p.Url, "The API URL of the package.")
            .AddProperty("html_url", p => p.HtmlUrl, "The HTML URL of the package.")
            .AddProperty("created_at", p => p.CreatedAt, "The timestamp when the package was created.")
            .AddProperty("updated_at", p => p.UpdatedAt, "The timestamp when the package was last updated.")
            .AddProperty("owner_login", p => p.Owner.Login, "The login name of the package owner.")
            .AddProperty("owner_id", p => p.Owner.Id, "The unique ID of the package owner.")
            .AddProperty("owner_type", p => p.Owner.Type, "The type of the package owner.")
            .AddKeyColumn("organization", isRequired: true)
            .AddKeyColumn("package_type", isRequired: false)
            .AddKeyColumn("visibility", isRequired: false);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<Package> GetDataAsync(Fetcher<Package> fetcher,
        CancellationToken cancellationToken = default)
    {
        var organization = GetKeyColumnValue("organization");
        var packageType = GetKeyColumnValue("package_type");
        var visibility = GetKeyColumnValue("visibility");

        // Default to container if package_type is not specified
        var packageTypeEnum = packageType.IsNull ? PackageType.Container : Enum.Parse<PackageType>(packageType.AsString, ignoreCase: true);
        PackageVisibility? packageVisibility = visibility.IsNull ? null : Enum.Parse<PackageVisibility>(visibility.AsString, ignoreCase: true);

        fetcher.PageStart = 1;
        return fetcher.FetchPagedAsync(async (page, limit, ct) =>
        {
            _logger.LogDebug("Get packages for organization = {Organization}, package_type = {PackageType}, visibility = {Visibility}.",
                organization, packageTypeEnum, packageVisibility);
            return await Client.Packages.GetAllForOrg(organization.AsString, packageTypeEnum, packageVisibility,
                new ApiOptions { StartPage = page, PageCount = 1, PageSize = limit });
        }, cancellationToken);
    }
}
