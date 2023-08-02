using System.ComponentModel;
using Amazon.EC2;
using Amazon.EC2.Model;
using QueryCat.Backend;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Storage;

namespace QueryCat.Plugins.Aws.Inputs;

[Description("Get EC2 instances.")]
[FunctionSignature("aws_ec2_instances")]
internal sealed class Ec2InstancesRowsInput : FetchInput<Instance>
{
    private AmazonEC2Client? _client;

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<Instance> builder)
    {
        var credentials = General.GetConfiguration(QueryContext.InputConfigStorage);
        _client = new AmazonEC2Client(credentials);

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
    protected override IEnumerable<Instance> GetData(Fetcher<Instance> fetcher)
    {
        return fetcher.FetchAll(async ct =>
        {
            if (_client == null)
            {
                throw new QueryCatException("Client is not initialized.");
            }
            var result = (await _client.DescribeInstancesAsync(ct)).Reservations.SelectMany(r => r.Instances);
            return result;
        });
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        _client?.Dispose();
        base.Dispose(disposing);
    }
}
