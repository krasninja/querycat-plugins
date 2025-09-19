using QueryCat.Backend.Core.Functions;
using QueryCat.Plugins.ImdbApi.Inputs;

namespace QueryCat.Plugins.ImdbApi;

/// <summary>
/// The special registration class that is called by plugin loader.
/// </summary>
public static class Registration
{
    /// <summary>
    /// Register plugin functions.
    /// </summary>
    /// <param name="functionsManager">Functions manager.</param>
    public static void RegisterFunctions(IFunctionsManager functionsManager)
    {
        functionsManager.RegisterFunction(EpisodesRowsInput.ImdbEpisodesFunction);
        functionsManager.RegisterFunction(InterestsRowsInput.ImdbInterestsFunction);
        functionsManager.RegisterFunction(NamesRowsInput.ImdbNamesFunction);
        functionsManager.RegisterFunction(TitlesRowsInput.ImdbTitlesFunction);
    }
}
