using Qapptia.Core.Abstractions;
using Qapptia.Core.Theme;

namespace Qapptia.App.Config.Common;

/// <summary>Traduce estados y valores persistidos a textos de Configuración.</summary>
public static class ConfigPresentation
{
    private static readonly System.Text.CompositeFormat s_serverUnavailableFormat = System.Text.CompositeFormat.Parse(Constants.UpdateErrorServerUnavailableFormat);

    // Opciones disponibles para selectores de tema.
    public static IReadOnlyList<string> ThemeNames { get; } = Array.AsReadOnly(new[]
    {
        Constants.ThemeSystem, Constants.ThemeDark, Constants.ThemeLight
    });

    private static readonly Dictionary<string, string> s_themeNames = new()
    {
        [ThemeConstants.System] = Constants.ThemeSystem,
        [ThemeConstants.Dark] = Constants.ThemeDark,
        [ThemeConstants.Light] = Constants.ThemeLight
    };

    private static readonly Dictionary<string, string> s_themeCodes = new()
    {
        [Constants.ThemeSystem] = ThemeConstants.System,
        [Constants.ThemeDark] = ThemeConstants.Dark,
        [Constants.ThemeLight] = ThemeConstants.Light
    };

    private static readonly Dictionary<AutoStartScope, string> s_autoStartScopeMessages = new()
    {
        [AutoStartScope.CurrentUser] = Constants.AutoStartUserDescription,
        [AutoStartScope.AllUsers] = Constants.AutoStartMachineDescription,
        [AutoStartScope.Portable] = Constants.AutoStartPortableDescription
    };

    private static readonly Dictionary<AutoStartError, string> s_autoStartErrorMessages = new()
    {
        [AutoStartError.RegistryReadFailed] = Constants.AutoStartErrorQuery,
        [AutoStartError.AmbiguousScope] = Constants.AutoStartErrorAmbiguousScope,
        [AutoStartError.OtherCopy] = Constants.AutoStartErrorOtherCopy
    };

    private static readonly Dictionary<UpdateCheckError, string> s_updateErrorMessages = new()
    {
        [UpdateCheckError.EmptyResponse] = Constants.UpdateErrorEmptyResponse,
        [UpdateCheckError.MissingVersion] = Constants.UpdateErrorMissingVersion,
        [UpdateCheckError.InvalidVersionFormat] = Constants.UpdateErrorInvalidVersionFormat,
        [UpdateCheckError.CheckCancelled] = Constants.UpdateErrorCheckCancelled,
        [UpdateCheckError.Timeout] = Constants.UpdateErrorTimeout,
        [UpdateCheckError.UnrecognizedResponse] = Constants.UpdateErrorUnrecognizedResponse,
        [UpdateCheckError.ConnectionFailed] = Constants.UpdateErrorConnectionFailed,
        [UpdateCheckError.NoInternet] = Constants.UpdateErrorNoInternet
    };

    public static string GetUpdateErrorMessage(UpdateCheckResult result) => result.HttpStatusCode.HasValue
        ? string.Format(System.Globalization.CultureInfo.InvariantCulture, s_serverUnavailableFormat, result.HttpStatusCode.Value)
        : s_updateErrorMessages.GetValueOrDefault(result.Error, Constants.UpdateStatusConnectError);

    // Convierte código interno a nombre para mostrar.
    public static string ToThemeName(string? theme) => s_themeNames[ThemeConstants.Normalize(theme)];

    // Convierte nombre para mostrar a código interno.
    public static string FromThemeName(string? name) =>
        s_themeCodes.GetValueOrDefault(name?.Trim() ?? string.Empty, ThemeConstants.System);

    public static string GetAutoStartStatusMessage(AutoStartState state) => state.HasError
        ? s_autoStartErrorMessages.GetValueOrDefault(state.Error, string.Empty)
        : s_autoStartScopeMessages.GetValueOrDefault(state.Scope, string.Empty);
}
