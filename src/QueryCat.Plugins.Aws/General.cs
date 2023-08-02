using Amazon.Runtime;
using QueryCat.Backend;
using QueryCat.Backend.Abstractions;
using QueryCat.Backend.Storage;
using QueryCat.Backend.Types;

namespace QueryCat.Plugins.Aws;

/// <summary>
/// AWS functions.
/// </summary>
internal static class General
{
    public const string AwsAccessKey = "aws-access-key";
    public const string AwsSecretKey = "aws-secret-key";
    public const string AwsCredentials = "aws-credentials";

    internal static BasicAWSCredentials GetConfiguration(IInputConfigStorage configStorage)
    {
        if (configStorage.Has(AwsCredentials))
        {
            return configStorage.Get(AwsCredentials).As<BasicAWSCredentials>();
        }

        var accessKey = configStorage.GetOrDefault(AwsAccessKey);
        var secretKey = configStorage.GetOrDefault(AwsSecretKey);
        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
        {
            throw new QueryCatException("Access key or secret key are not set.");
        }

        var credentials = new BasicAWSCredentials(accessKey, secretKey);

        configStorage.Set(AwsCredentials, VariantValue.CreateFromObject(credentials));
        return credentials;
    }
}
