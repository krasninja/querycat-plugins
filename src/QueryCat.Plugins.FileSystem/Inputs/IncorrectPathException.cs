using QueryCat.Backend.Core;

namespace QueryCat.Plugins.FileSystem.Inputs;

public sealed class IncorrectPathException : QueryCatException
{
    public IncorrectPathException(string message) : base(message)
    {
    }
}
