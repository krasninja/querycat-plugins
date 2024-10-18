using System.ComponentModel;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Fetch;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.VStarCam.Domain;

namespace QueryCat.Plugins.VStarCam.Inputs;

internal record CameraInfo(Camera Camera, CameraParameters CameraParameters);

internal sealed class CameraInfoRowsInput : FetchRowsInput<CameraInfo>
{
    [SafeFunction]
    [Description("VStar camera information.")]
    [FunctionSignature("vstar_camera_info(login: string, password: string): object<IRowsInput>")]
    public static VariantValue CameraInfoRowsInputFunction(IExecutionThread thread)
    {
        var login = thread.Stack[0].AsString;
        var password = thread.Stack[1].AsString;
        return VariantValue.CreateFromObject(new CameraInfoRowsInput(login, password));
    }

    private readonly string _username;
    private readonly string _password;

    /// <inheritdoc />
    public CameraInfoRowsInput(string username, string password)
    {
        _username = username;
        _password = password;
    }

    /// <inheritdoc />
    protected override void Initialize(ClassRowsFrameBuilder<CameraInfo> builder)
    {
        builder.NamingConvention = NamingConventionStyle.SnakeCase;
        builder
            .AddProperty(p => p.Camera.Id)
            .AddProperty(p => p.Camera.Ip)
            .AddProperty(p => p.Camera.IpGateway)
            .AddProperty(p => p.Camera.Port)
            .AddProperty(p => p.Camera.IpMask)
            .AddProperty(p => p.Camera.Name)
            .AddProperty(p => p.Camera.FirmwareVersion)
            .AddProperty(p => p.Camera.PrimaryDns)
            .AddProperty(p => p.Camera.SecondaryDns)
            .AddProperty(p => p.CameraParameters.Framerate)
            .AddProperty(p => p.CameraParameters.MainStreamWidth)
            .AddProperty(p => p.CameraParameters.MainStreamHeight)
            .AddProperty(p => p.CameraParameters.Bitrate)
            .AddProperty(p => p.CameraParameters.Ir)
            .AddKeyColumn("id", isRequired: true);
    }

    /// <inheritdoc />
    protected override IEnumerable<CameraInfo> GetData(Fetcher<CameraInfo> fetch)
    {
        return fetch.FetchAll(async ct =>
        {
            var finder = new CamerasFinder();
            var cameras = await finder.FindAsync(ct);
            var id = GetKeyColumnValue("id").AsString;
            var camera = cameras.FirstOrDefault(c => c.Id == id);
            if (camera == null)
            {
                return Array.Empty<CameraInfo>();
            }

            var credentials = new CredentialsDictionary();
            credentials[camera.Ip] = new CameraCredentials(_username, _password);
            new CamerasCredentialsUpdater(silent: true).SetCredentialsByIp(camera, credentials);

            var @params = camera.GetParameters(ct).GetAwaiter().GetResult();
            return new[]
            {
                new CameraInfo(camera, @params)
            };
        });
    }
}
