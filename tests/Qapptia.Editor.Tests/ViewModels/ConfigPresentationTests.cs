using Qapptia.App.Config.Common;
using Qapptia.Core.Theme;

namespace Qapptia.Editor.Tests.ViewModels;

public sealed class ConfigPresentationTests
{
    [Theory]
    [InlineData(ThemeConstants.Dark, Qapptia.App.Config.Common.Constants.ThemeDark)]
    [InlineData(ThemeConstants.Light, Qapptia.App.Config.Common.Constants.ThemeLight)]
    [InlineData(ThemeConstants.System, Qapptia.App.Config.Common.Constants.ThemeSystem)]
    [InlineData(null, Qapptia.App.Config.Common.Constants.ThemeSystem)]
    [InlineData("other", Qapptia.App.Config.Common.Constants.ThemeSystem)]
    public void ToDisplayNameReturnsExpectedDisplayName(string? code, string expected)
    {
        var result = ConfigPresentation.ToThemeName(code);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(Qapptia.App.Config.Common.Constants.ThemeDark, ThemeConstants.Dark)]
    [InlineData(Qapptia.App.Config.Common.Constants.ThemeLight, ThemeConstants.Light)]
    [InlineData(Qapptia.App.Config.Common.Constants.ThemeSystem, ThemeConstants.System)]
    [InlineData("", ThemeConstants.System)]
    [InlineData(null, ThemeConstants.System)]
    [InlineData("Otro", ThemeConstants.System)]
    public void FromDisplayNameReturnsExpectedCode(string? displayName, string expected)
    {
        var result = ConfigPresentation.FromThemeName(displayName);
        Assert.Equal(expected, result);
    }
}
