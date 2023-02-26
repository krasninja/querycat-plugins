namespace QueryCat.Plugins.VStarCam.Domain;

/// <summary>
/// Camera credentials for basic auth.
/// </summary>
public class CameraCredentials
{
    public string Login { get; } = string.Empty;

    public string Password { get; } = string.Empty;

    public static CameraCredentials Empty { get; } = new(string.Empty, string.Empty);

    public CameraCredentials()
    {
    }

    public CameraCredentials(string login, string password)
    {
        Login = login;
        Password = password;
    }
}
