using Qapptia.Core.Theme;

namespace Qapptia.Core.Tests;

public class ThemeConstantsTests
{
    [Theory]
    [InlineData("dark", ThemeConstants.Dark)]
    [InlineData("DARK", ThemeConstants.Dark)]
    [InlineData("light", ThemeConstants.Light)]
    [InlineData("LIGHT", ThemeConstants.Light)]
    [InlineData("system", ThemeConstants.System)]
    [InlineData("SYSTEM", ThemeConstants.System)]
    [InlineData("", ThemeConstants.System)]
    [InlineData(null, ThemeConstants.System)]
    [InlineData("unknown", ThemeConstants.System)]
    public void NormalizeReturnsExpectedTheme(string? input, string expected)
    {
        var result = ThemeConstants.Normalize(input);
        Assert.Equal(expected, result);
    }

}
