using FluentAssertions;
using Moq;
using Qapptia.App.Editor.ViewModels;
using Qapptia.Core.Configuration;
using Qapptia.Editor.Models;
using Qapptia.Editor.Services;
using Xunit;

namespace Qapptia.Editor.Tests.ViewModels;

public sealed class EditorHelpViewModelTests
{
    private readonly Mock<IConfigReader> _configReaderMock = new();
    private readonly Mock<IEditorStateService> _stateServiceMock = new();
    private readonly EditorState _state = new();
    private readonly QapptiaConfig _config;

    public EditorHelpViewModelTests()
    {
        _config = new QapptiaConfig
        {
            Capture = { ShortcutScreen = "ctrl+shift+q", ShortcutArea = "ctrl+shift+a" }
        };

        _configReaderMock.Setup(c => c.Current).Returns(_config);
        _stateServiceMock.Setup(s => s.Load()).Returns(_state);
    }

    [Fact]
    public void InitializationShouldHaveCorrectInitialStateAndFormattedShortcuts()
    {
        // Act
        var vm = new EditorHelpViewModel(_configReaderMock.Object, _stateServiceMock.Object);

        // Assert
        vm.IsVisible.Should().BeFalse();
        vm.DontShowAgainOnStartup.Should().BeFalse();
        vm.CurrentSlideIndex.Should().Be(0);
        EditorHelpViewModel.TotalSlides.Should().Be(7);
        vm.CanGoPrevious.Should().BeFalse();
        vm.CanGoNext.Should().BeTrue();
        vm.IsLastSlide.Should().BeFalse();
        vm.HasSeenAllSlides.Should().BeFalse();
        vm.SlideStepDisplay.Should().Be("1 de 7");
        vm.ShortcutScreenText.Should().Be("Ctrl + Shift + Q");
        vm.ShortcutAreaText.Should().Be("Ctrl + Shift + A");
        vm.ShortcutRequirementText.Should().Be("Para poder capturar la pantalla, el Capturador debe estar activo:");
    }

    [Theory]
    [InlineData("ctrl+shift+q", "Ctrl + Shift + Q")]
    [InlineData("ctrl+shift+a", "Ctrl + Shift + A")]
    [InlineData("ctrl+alt+delete", "Ctrl + Alt + Delete")]
    [InlineData("win+shift+s", "Win + Shift + S")]
    [InlineData("", "Fallback")]
    [InlineData(null, "Fallback")]
    [InlineData("   ", "Fallback")]
    public void FormatShortcutShouldReturnExpectedOutput(string? raw, string expected)
    {
        // Act
        var result = EditorHelpViewModel.FormatShortcut(raw, "Fallback");

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void OpenShouldReloadConfigAndSetIsVisibleTrueAndResetSlideIndex()
    {
        // Arrange
        var vm = new EditorHelpViewModel(_configReaderMock.Object, _stateServiceMock.Object);
        vm.CurrentSlideIndex = 3;

        // Modificamos atajos en config para simular cambio externo
        _config.Capture.ShortcutScreen = "ctrl+alt+s";
        _config.Capture.ShortcutArea = "ctrl+alt+w";

        // Act
        vm.OpenCommand.Execute(null);

        // Assert
        _configReaderMock.Verify(c => c.Reload(), Times.Once);
        vm.IsVisible.Should().BeTrue();
        vm.CurrentSlideIndex.Should().Be(0);
        vm.HasSeenAllSlides.Should().BeFalse();
        vm.ShortcutScreenText.Should().Be("Ctrl + Alt + S");
        vm.ShortcutAreaText.Should().Be("Ctrl + Alt + W");
        vm.ShortcutRequirementText.Should().Be("Para poder capturar la pantalla, el Capturador debe estar activo:");
    }

    [Fact]
    public void StepNavigationShouldAdvanceAndReverseCorrectlyAcrossAllSevenSteps()
    {
        // Arrange
        var vm = new EditorHelpViewModel(_configReaderMock.Object, _stateServiceMock.Object);
        vm.OpenCommand.Execute(null);
        vm.HasSeenAllSlides.Should().BeFalse();

        // Act: Paso 0 -> Paso 1 (Estado del Capturador - Icono Q)
        vm.NextSlideCommand.Execute(null);
        vm.CurrentSlideIndex.Should().Be(1);
        vm.CanGoPrevious.Should().BeTrue();
        vm.CanGoNext.Should().BeTrue();
        vm.IsLastSlide.Should().BeFalse();
        vm.IsSlide1.Should().BeTrue();
        vm.HasSeenAllSlides.Should().BeFalse();

        // Act: Paso 1 -> Paso 2 (Apoyar el proyecto - Botón Sponsor)
        vm.NextSlideCommand.Execute(null);
        vm.CurrentSlideIndex.Should().Be(2);
        vm.IsSlide2.Should().BeTrue();
        vm.HasSeenAllSlides.Should().BeFalse();

        // Act: Paso 2 -> Paso 3 (Copiar y visualización - Zoom y portapapeles)
        vm.NextSlideCommand.Execute(null);
        vm.CurrentSlideIndex.Should().Be(3);
        vm.IsSlide3.Should().BeTrue();
        vm.HasSeenAllSlides.Should().BeFalse();

        // Act: Paso 3 -> Paso 4 (Herramientas de dibujo y vectores)
        vm.NextSlideCommand.Execute(null);
        vm.CurrentSlideIndex.Should().Be(4);
        vm.IsSlide4.Should().BeTrue();
        vm.IsLastSlide.Should().BeFalse();
        vm.CanGoNext.Should().BeTrue();
        vm.HasSeenAllSlides.Should().BeFalse();

        // Act: Paso 4 -> Paso 5 (Configuración y Acerca de / Descarga)
        vm.NextSlideCommand.Execute(null);
        vm.CurrentSlideIndex.Should().Be(5);
        vm.IsSlide5.Should().BeTrue();
        vm.IsLastSlide.Should().BeFalse();
        vm.CanGoNext.Should().BeTrue();
        vm.HasSeenAllSlides.Should().BeFalse();

        // Act: Paso 5 -> Paso 6 (Último bloque: Información de la imagen)
        vm.NextSlideCommand.Execute(null);
        vm.CurrentSlideIndex.Should().Be(6);
        vm.IsLastSlide.Should().BeTrue();
        vm.CanGoNext.Should().BeFalse();
        vm.IsSlide6.Should().BeTrue();
        vm.HasSeenAllSlides.Should().BeTrue("al recorrer todas las secciones debe activarse HasSeenAllSlides");

        // Act: Retroceder de 6 -> 5
        vm.PreviousSlideCommand.Execute(null);
        vm.CurrentSlideIndex.Should().Be(5);
        vm.IsLastSlide.Should().BeFalse();
        vm.CanGoNext.Should().BeTrue();
        vm.HasSeenAllSlides.Should().BeTrue("permanece activo una vez que se han visitado todas");

        // Act: GoToSlide
        vm.GoToSlideCommand.Execute("0");
        vm.CurrentSlideIndex.Should().Be(0);
        vm.CanGoPrevious.Should().BeFalse();
        vm.CanGoNext.Should().BeTrue();
    }

    [Fact]
    public void NextSlideOnLastStepShouldCloseModal()
    {
        // Arrange
        var vm = new EditorHelpViewModel(_configReaderMock.Object, _stateServiceMock.Object);
        vm.OpenCommand.Execute(null);
        vm.CurrentSlideIndex = 6;

        // Act
        vm.NextSlideCommand.Execute(null);

        // Assert
        vm.IsVisible.Should().BeFalse();
    }

    [Fact]
    public void OpenSponsorCommandShouldInvokeShellServiceWithDefaultSponsorUrl()
    {
        // Arrange
        var shellMock = new Mock<Qapptia.Core.Abstractions.IShellService>();
        var vm = new EditorHelpViewModel(_configReaderMock.Object, _stateServiceMock.Object, shellMock.Object);

        // Act
        vm.OpenSponsorCommand.Execute(null);

        // Assert
        shellMock.Verify(s => s.OpenUrl(Qapptia.Core.Constants.DefaultSponsorUrl), Times.Once);
    }

    [Fact]
    public void CloseShouldHideTheGuideWithoutSavingAgain()
    {
        // Arrange
        var vm = new EditorHelpViewModel(_configReaderMock.Object, _stateServiceMock.Object);
        vm.OpenCommand.Execute(null);
        vm.DontShowAgainOnStartup = false;

        // Act
        vm.CloseCommand.Execute(null);

        // Assert
        vm.IsVisible.Should().BeFalse();
        _state.Session.ShowEditorHelpOnStartup.Should().BeTrue();
        _stateServiceMock.Verify(s => s.Save(_state), Times.Once);
    }

    [Fact]
    public void CheckboxChangesShouldPersistBeforeClosingTheGuide()
    {
        // Arrange
        var vm = new EditorHelpViewModel(_configReaderMock.Object, _stateServiceMock.Object);
        vm.OpenCommand.Execute(null);
        vm.DontShowAgainOnStartup = true;
        vm.ShouldShowOnStartup.Should().BeFalse();
        _stateServiceMock.Verify(s => s.Save(_state), Times.Exactly(2));

        vm.DontShowAgainOnStartup = false;
        vm.ShouldShowOnStartup.Should().BeTrue();
        _stateServiceMock.Verify(s => s.Save(_state), Times.Exactly(3));
        vm.IsVisible.Should().BeTrue();
    }

    [Fact]
    public void GuidePersistsTheCheckboxInEditorStateAndReloadsItOnStartup()
    {
        var directory = Directory.CreateTempSubdirectory("Qapptia_Help_");
        try
        {
            var configPath = Path.Combine(directory.FullName, Qapptia.Core.Constants.ConfigFileName);
            var config = new JsonConfigService(configPath);
            config.Save();
            var originalConfig = File.ReadAllText(configPath);
            var vm = CreateGuide();

            vm.ShouldShowOnStartup.Should().BeTrue();
            vm.OpenCommand.Execute(null);
            vm.DontShowAgainOnStartup = true;

            var reopenedEditor = CreateGuide();
            reopenedEditor.ShouldShowOnStartup.Should().BeFalse();
            reopenedEditor.OpenCommand.Execute(null);
            reopenedEditor.DontShowAgainOnStartup.Should().BeFalse();
            CreateGuide().ShouldShowOnStartup.Should().BeTrue();

            reopenedEditor.DontShowAgainOnStartup = true;
            reopenedEditor.DontShowAgainOnStartup = false;
            reopenedEditor.CloseCommand.Execute(null);
            CreateGuide().ShouldShowOnStartup.Should().BeTrue();
            File.ReadAllText(configPath).Should().Be(originalConfig);

            EditorHelpViewModel CreateGuide() => new(config,
                new EditorStateService(directory.FullName, Qapptia.Core.Constants.EditorStateFileName));
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }
}

