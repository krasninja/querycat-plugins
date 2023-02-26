using System.ComponentModel;
using System.Text;

namespace QueryCat.Plugins.VStarCam.Domain;

/// <summary>
/// Camera extended parameters.
/// </summary>
public class CameraParameters
{
    /*
     Query: curl http://192.168.100.24:18258/get_camera_params.cgi\?loginuse\=admin\&loginpas\=X
     The dump looks like this:
    var cameratype=3;
    var resolution=2;
    var resolutionsub=0;
    var resolutionsubsub=1;
    var vbright=126;
    var vcontrast=126;
    var vhue=126;
    var vsaturation=126;
    var OSDEnable=0;
    var mode=1;
    var flip=0;
    var enc_size=2;
    var enc_framerate=25;
    var enc_keyframe=25;
    var enc_quant=0;
    var enc_ratemode=0;
    var enc_bitrate=4096;
    var enc_main_mode=0;
    var sub_enc_size=0;
    var sub_enc_framerate=15;
    var sub_enc_keyframe=15;
    var sub_enc_quant=0;
    var sub_enc_ratemode=0;
    var sub_enc_bitrate=1024;
    var sub_sub_enc_size=1;
    var sub_sub_enc_framerate=10;
    var sub_sub_enc_keyframe=10;
    var sub_sub_enc_quant=0;
    var sub_sub_enc_ratemode=0;
    var sub_sub_enc_bitrate=256;
    var speed=0;
    var ircut=0;
    var involume=19;
    var outvolume=31;
    var MainStreamWidth=1280;
    var MainStreamHeight=720;
    */

    /// <summary>
    /// Is IR enabled.
    /// </summary>
    [Description("Is IR enabled.")]
    public bool Ir { get; set; }

    [Description("Main stream width.")]
    public int MainStreamWidth { get; set; }

    [Description("Main stream height.")]
    public int MainStreamHeight { get; set; }

    [Description("Bitrate.")]
    public int Bitrate { get; set; }

    public int Framerate { get; set; }

    /// <summary>
    /// Dump info.
    /// </summary>
    /// <returns></returns>
    public string DumpInfo()
    {
        var sb = new StringBuilder()
            .AppendLine("IR: " + Ir)
            .AppendLine("MainStreamWidth: " + MainStreamWidth)
            .AppendLine("MainStreamHeight: " + MainStreamHeight)
            .AppendLine("Bitrate: " + Bitrate)
            .AppendLine("Framerate: " + Framerate);
        return sb.ToString();
    }
}
