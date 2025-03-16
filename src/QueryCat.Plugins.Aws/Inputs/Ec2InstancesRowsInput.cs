using System.ComponentModel;
using System.Runtime.CompilerServices;
using Amazon.EC2;
using Amazon.EC2.Model;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Data;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Backend.Core.Utils;

namespace QueryCat.Plugins.Aws.Inputs;

[SafeFunction]
[Description("Get EC2 instances.")]
[FunctionSignature("aws_ec2_instances")]
internal sealed class Ec2InstancesRowsInput : AsyncEnumerableRowsInput<Instance>
{
    private AmazonEC2Client? _client;

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<Instance> builder)
    {
        builder.NamingConvention = NamingConventionStyle.SnakeCase;
        builder
            .AddProperty(p => p.InstanceId)
            .AddProperty(p => p.ImageId)
            .AddProperty(p => p.PrivateIpAddress)
            .AddProperty(p => p.PublicIpAddress)
            .AddProperty(p => p.Ipv6Address)
            .AddProperty("instance_type", p => p.InstanceType.ToString())
            .AddProperty("architecture", p => p.Architecture.ToString())
            .AddProperty("hypervisor", p => p.Hypervisor.ToString());
    }

    /// <inheritdoc />
    public override async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        var credentials = await General.GetConfigurationAsync(QueryContext.InputConfigStorage, cancellationToken);
        _client = new AmazonEC2Client(credentials);
        await base.OpenAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override IAsyncEnumerable<Instance> GetDataAsync(Fetcher<Instance> fetcher, CancellationToken cancellationToken = default)
    {
        if (_client == null)
        {
            throw new QueryCatException("Client is not initialized.");
        }
        return fetcher.FetchAllAsync(async _ =>
        {
            return (await _client.DescribeInstancesAsync(cancellationToken))
                .Reservations
                .SelectMany(r => r.Instances);
        }, cancellationToken);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        _client?.Dispose();
        base.Dispose(disposing);
    }
}
