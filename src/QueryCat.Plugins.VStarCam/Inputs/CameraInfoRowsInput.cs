using System.ComponentModel;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Storage;
using QueryCat.Backend.Types;
using QueryCat.Plugins.VStarCam.Domain;

namespace QueryCat.Plugins.VStarCam.Inputs;

record CameraInfo(Camera Camera, CameraParameters CameraParameters);

internal class CameraInfoRowsInput : FetchInput<CameraInfo>
{
    [Description("VStar camera information.")]
    [FunctionSignature("vstar_camera_info(login: string, password: string): object<IRowsInput>")]
    public static VariantValue CameraInfoRowsInputFunction(FunctionCallInfo args)
    {
        var login = args.GetAt(0).AsString;
        var password = args.GetAt(1).AsString;
        return VariantValue.CreateFromObject(new CameraInfoRowsInput(login, password));
    }

    private readonly string _username;
    private readonly string _password;
    private string _id = string.Empty;

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
            .AddProperty(p => p.CameraParameters.Ir);
    }

    /// <inheritdoc />
    protected override void InitializeInputInfo(QueryContextInputInfo inputInfo)
    {
        AddKeyColumn(
            "id",
            isRequired: true,
            set: v => _id = v.AsString);
    }

    /// <inheritdoc />
    protected override IEnumerable<CameraInfo> GetData(Fetcher<CameraInfo> fetch)
    {
        return fetch.FetchAll(async ct =>
        {
            var finder = new CamerasFinder();
            var cameras = await finder.FindAsync(ct);
            var camera = cameras.FirstOrDefault(c => c.Id == _id);
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
