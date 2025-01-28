using System.ComponentModel;
using Microsoft.Extensions.Logging;
using QueryCat.Backend.Core;
using QueryCat.Backend.Core.Execution;
using QueryCat.Backend.Core.Functions;
using QueryCat.Backend.Core.Types;
using QueryCat.Plugins.VStarCam.Domain;

namespace QueryCat.Plugins.VStarCam.Functions;

internal static class SetIr
{
    private static readonly ILogger _logger = Application.LoggerFactory.CreateLogger(typeof(SetIr));

    [Description("Set camera IR on/off.")]
    [FunctionSignature("vstar_set_ir(login: string, password: string, camera_id: string, ir: boolean): void")]
    public static VariantValue VStarSetIrFunction(IExecutionThread thread)
    {
        var login = thread.Stack[0].AsString;
        var password = thread.Stack[1].AsString;
        var id = thread.Stack[2].AsString;
        var irState = thread.Stack[3].AsBoolean;

        using var camerasFinder = new CamerasFinder();
        var cameras = camerasFinder.FindAsync(CancellationToken.None).GetAwaiter().GetResult();
        var camera = cameras.FirstOrDefault(c => c.Id == id);
        if (camera == null)
        {
            _logger.LogError("Cannot find camera with id {Id}.", id);
            return VariantValue.Null;
        }

        var credentials = new CredentialsDictionary();
        credentials[camera.Ip] = new CameraCredentials(login, password);
        new CamerasCredentialsUpdater(silent: true).SetCredentialsByIp(camera, credentials);

        camera.SetIrAsync(irState, CancellationToken.None).GetAwaiter().GetResult();
        return VariantValue.Null;
    }
}
