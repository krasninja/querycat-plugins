using QueryCat.Backend.Core;

namespace QueryCat.Plugins.VStarCam.Domain;

[Serializable]
public class CameraException : QueryCatException
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public CameraException(string message) : base(message)
    {
    }

    protected CameraException(System.Runtime.Serialization.SerializationInfo serializationInfo,
        System.Runtime.Serialization.StreamingContext streamingContext)
    {
    }
}
