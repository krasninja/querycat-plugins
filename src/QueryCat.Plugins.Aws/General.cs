using Amazon.Runtime;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Types;

namespace QueryCat.Plugins.Aws;

/// <summary>
/// AWS functions.
/// </summary>
internal static class General
{
    public const string AwsAccessKey = "aws-access-key";
    public const string AwsSecretKey = "aws-secret-key";
    public const string AwsCredentials = "aws-credentials";

    internal static async Task<BasicAWSCredentials> GetConfigurationAsync(IConfigStorage configStorage, CancellationToken cancellationToken)
    {
        if (await configStorage.HasAsync(AwsCredentials, cancellationToken))
        {
            return (await configStorage.GetAsync(AwsCredentials, cancellationToken)).AsRequired<BasicAWSCredentials>();
        }

        var accessKey = await configStorage.GetOrDefaultAsync(AwsAccessKey, cancellationToken: cancellationToken);
        var secretKey = await configStorage.GetOrDefaultAsync(AwsSecretKey, cancellationToken: cancellationToken);
        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
        {
            throw new QueryCatException("Access key or secret key are not set.");
        }

        var credentials = new BasicAWSCredentials(accessKey, secretKey);

        await configStorage.SetAsync(AwsCredentials, VariantValue.CreateFromObject(credentials), cancellationToken);
        return credentials;
    }
}
