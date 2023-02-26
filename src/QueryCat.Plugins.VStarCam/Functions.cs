using System.ComponentModel;
using QueryCat.Backend.Functions;
using QueryCat.Backend.Types;
using QueryCat.Plugins.VStarCam.Domain;

namespace QueryCat.Plugins.VStarCam;

internal static class Functions
{
    [Description("Set camera IR on/off.")]
    [FunctionSignature("vstar_set_ir(login: string, password: string, camera_id: string, ir: boolean): void")]
    public static VariantValue VStarSetIrFunction(FunctionCallInfo args)
    {
        var login = args.GetAt(0).AsString;
        var password = args.GetAt(1).AsString;
        var id = args.GetAt(2).AsString;
        var irState = args.GetAt(3).AsBoolean;

        var camerasFinder = new CamerasFinder();
        var cameras = camerasFinder.FindAsync(CancellationToken.None).GetAwaiter().GetResult();
        var camera = cameras.FirstOrDefault(c => c.Id == id);
        if (camera == null)
        {
            Serilog.Log.Logger.Error("Cannot find camera with id {Id},", id);
            return VariantValue.Null;
        }

        var credentials = new CredentialsDictionary();
        credentials[camera.Ip] = new CameraCredentials(login, password);
        new CamerasCredentialsUpdater(silent: true).SetCredentialsByIp(camera, credentials);

        camera.SetIrAsync(irState, CancellationToken.None).GetAwaiter().GetResult();
        return VariantValue.Null;
    }
}
