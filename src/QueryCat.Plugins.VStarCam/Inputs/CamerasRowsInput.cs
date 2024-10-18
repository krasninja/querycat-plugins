using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.VStarCam.Domain;

namespace QueryCat.Plugins.VStarCam.Inputs;

internal sealed class CamerasRowsInput : FetchRowsInput<Camera>
{
    [SafeFunction]
    [Description("VStar cameras in local network.")]
    [FunctionSignature("vstar_cameras(): object<IRowsInput>")]
    public static VariantValue CamerasRowsInputFunction(IExecutionThread thread)
    {
        return VariantValue.CreateFromObject(new CamerasRowsInput());
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<Camera> builder)
    {
        builder.NamingConvention = NamingConventionStyle.SnakeCase;
        builder
            .AddProperty(p => p.Id)
            .AddProperty(p => p.Ip)
            .AddProperty(p => p.IpGateway)
            .AddProperty(p => p.Port)
            .AddProperty(p => p.IpMask)
            .AddProperty(p => p.Name)
            .AddProperty(p => p.FirmwareVersion)
            .AddProperty(p => p.PrimaryDns)
            .AddProperty(p => p.SecondaryDns);
    }

    /// <inheritdoc />
    protected override IEnumerable<Camera> GetData(Fetcher<Camera> fetch)
    {
        return fetch.FetchAll(async ct =>
        {
            using var finder = new CamerasFinder();
            return await finder.FindAsync(ct);
        });
    }
}
