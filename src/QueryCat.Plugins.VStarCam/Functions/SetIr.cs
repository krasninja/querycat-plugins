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
    public static async ValueTask<VariantValue> VStarSetIrFunction(IExecutionThread thread, CancellationToken cancellationToken)
    {
        var login = thread.Stack[0].AsString;
        var password = thread.Stack[1].AsString;
        var id = thread.Stack[2].AsString;
        var irState = thread.Stack[3].AsBoolean;

        return await VStarSetIrFunctionInternal(login, password, id, irState, cancellationToken);
    }

    public static async ValueTask<VariantValue> VStarSetIrFunctionInternal(
        string login,
        string password,
        string cameraId,
        bool irState,
        CancellationToken cancellationToken)
    {
        using var camerasFinder = new CamerasFinder();
        var cameras = await camerasFinder.FindAsync(cancellationToken);
        var camera = cameras.FirstOrDefault(c => c.Id == cameraId);
        if (camera == null)
        {
            _logger.LogError("Cannot find camera with id {Id}.", cameraId);
            return VariantValue.Null;
        }

        var credentials = new CredentialsDictionary();
        credentials[camera.Ip] = new CameraCredentials(login, password);
        new CamerasCredentialsUpdater(silent: true).SetCredentialsByIp(camera, credentials);

        await camera.SetIrAsync(irState, cancellationToken);
        return VariantValue.Null;
    }
}
