using System;
using FluentAssertions;
using Moq;
using Qapptia.App.Editor.ViewModels;
using Qapptia.Core.Configuration;
using Xunit;

namespace Qapptia.Editor.Tests.ViewModels;

public sealed class EditorHelpViewModelTests
{
    private readonly Mock<IConfigService> _configServiceMock;
    private readonly QapptiaConfig _config;

    public EditorHelpViewModelTests()
    {
        _config = new QapptiaConfig
        {
            ShortcutScreen = "ctrl+shift+q",
            ShortcutArea = "ctrl+shift+a",
            ShowEditorHelpOnStartup = true
        };

        _configServiceMock = new Mock<IConfigService>();
        _configServiceMock.Setup(c => c.Current).Returns(_config);
    }

    [Fact]
    public void InitializationShouldHaveCorrectInitialStateAndFormattedShortcuts()
    {
        // Act
        var vm = new EditorHelpViewModel(_configServiceMock.Object);

        // Assert
        vm.IsVisible.Should().BeFalse();
        vm.DontShowAgainOnStartup.Should().BeFalse();
        vm.CurrentSlideIndex.Should().Be(0);
        EditorHelpViewModel.TotalSlides.Should().Be(6);
        vm.CanGoPrevious.Should().BeFalse();
        vm.CanGoNext.Should().BeTrue();
        vm.IsLastSlide.Should().BeFalse();
        vm.HasSeenAllSlides.Should().BeFalse();
        vm.SlideStepDisplay.Should().Be("1 de 6");
        vm.ShortcutScreenText.Should().Be("Ctrl + Shift + Q");
        vm.ShortcutAreaText.Should().Be("Ctrl + Shift + A");
        vm.ShortcutRequirementText.Should().Be("Para capturar con el teclado (Ctrl + Shift + Q y Ctrl + Shift + A), el Capturador debe estar en ejecución:");
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
        var vm = new EditorHelpViewModel(_configServiceMock.Object);
        vm.CurrentSlideIndex = 3;

        // Modificamos atajos en config para simular cambio externo
        _config.ShortcutScreen = "ctrl+alt+s";
        _config.ShortcutArea = "ctrl+alt+w";

        // Act
        vm.OpenCommand.Execute(null);

        // Assert
        _configServiceMock.Verify(c => c.Reload(), Times.Once);
        vm.IsVisible.Should().BeTrue();
        vm.CurrentSlideIndex.Should().Be(0);
        vm.HasSeenAllSlides.Should().BeFalse();
        vm.ShortcutScreenText.Should().Be("Ctrl + Alt + S");
        vm.ShortcutAreaText.Should().Be("Ctrl + Alt + W");
        vm.ShortcutRequirementText.Should().Be("Para capturar con el teclado (Ctrl + Alt + S y Ctrl + Alt + W), el Capturador debe estar en ejecución:");
    }

    [Fact]
    public void StepNavigationShouldAdvanceAndReverseCorrectlyAcrossAllSixSteps()
    {
        // Arrange
        var vm = new EditorHelpViewModel(_configServiceMock.Object);
        vm.OpenCommand.Execute(null);
        vm.HasSeenAllSlides.Should().BeFalse();

        // Act: Paso 0 -> Paso 1
        vm.NextSlideCommand.Execute(null);
        vm.CurrentSlideIndex.Should().Be(1);
        vm.CanGoPrevious.Should().BeTrue();
        vm.CanGoNext.Should().BeTrue();
        vm.IsLastSlide.Should().BeFalse();
        vm.IsSlide1.Should().BeTrue();
        vm.HasSeenAllSlides.Should().BeFalse();

        // Act: Paso 1 -> Paso 2
        vm.NextSlideCommand.Execute(null);
        vm.CurrentSlideIndex.Should().Be(2);
        vm.IsSlide2.Should().BeTrue();
        vm.HasSeenAllSlides.Should().BeFalse();

        // Act: Paso 2 -> Paso 3 (Icono Q)
        vm.NextSlideCommand.Execute(null);
        vm.CurrentSlideIndex.Should().Be(3);
        vm.IsSlide3.Should().BeTrue();
        vm.HasSeenAllSlides.Should().BeFalse();

        // Act: Paso 3 -> Paso 4 (Configuración y Acerca de)
        vm.NextSlideCommand.Execute(null);
        vm.CurrentSlideIndex.Should().Be(4);
        vm.IsSlide4.Should().BeTrue();
        vm.IsLastSlide.Should().BeFalse();
        vm.CanGoNext.Should().BeTrue();
        vm.HasSeenAllSlides.Should().BeFalse();

        // Act: Paso 4 -> Paso 5 (Último bloque: Información de la imagen)
        vm.NextSlideCommand.Execute(null);
        vm.CurrentSlideIndex.Should().Be(5);
        vm.IsLastSlide.Should().BeTrue();
        vm.CanGoNext.Should().BeFalse();
        vm.IsSlide5.Should().BeTrue();
        vm.HasSeenAllSlides.Should().BeTrue("al recorrer todas las secciones debe activarse HasSeenAllSlides");

        // Act: Retroceder de 5 -> 4
        vm.PreviousSlideCommand.Execute(null);
        vm.CurrentSlideIndex.Should().Be(4);
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
        var vm = new EditorHelpViewModel(_configServiceMock.Object);
        vm.OpenCommand.Execute(null);
        vm.CurrentSlideIndex = 5;

        // Act
        vm.NextSlideCommand.Execute(null);

        // Assert
        vm.IsVisible.Should().BeFalse();
    }

    [Fact]
    public void CloseWithoutMarkingCheckboxShouldHideModalWithoutAlteringConfig()
    {
        // Arrange
        var vm = new EditorHelpViewModel(_configServiceMock.Object);
        vm.OpenCommand.Execute(null);
        vm.DontShowAgainOnStartup = false;

        // Act
        vm.CloseCommand.Execute(null);

        // Assert
        vm.IsVisible.Should().BeFalse();
        _config.ShowEditorHelpOnStartup.Should().BeTrue();
        _configServiceMock.Verify(c => c.Save(), Times.Never);
    }

    [Fact]
    public void CloseWithMarkingCheckboxShouldPersistFalseAndHideModal()
    {
        // Arrange
        var vm = new EditorHelpViewModel(_configServiceMock.Object);
        vm.OpenCommand.Execute(null);
        vm.DontShowAgainOnStartup = true;

        // Act
        vm.CloseCommand.Execute(null);

        // Assert
        vm.IsVisible.Should().BeFalse();
        _config.ShowEditorHelpOnStartup.Should().BeFalse();
        _configServiceMock.Verify(c => c.Save(), Times.Once);
    }

    [Fact]
    public void OpeningHelpManuallyWhenPreviouslySuppressedShouldResetCheckboxAndReactivateStartup()
    {
        // Arrange: la ayuda estaba previamente suprimida para el arranque
        _config.ShowEditorHelpOnStartup = false;
        var vm = new EditorHelpViewModel(_configServiceMock.Object);

        // Act: se abre manualmente (desde "Acerca de" o menú)
        vm.OpenCommand.Execute(null);

        // Assert: la casilla aparece desmarcada y se reactiva la persistencia en config
        vm.IsVisible.Should().BeTrue();
        vm.DontShowAgainOnStartup.Should().BeFalse("la casilla debe aparecer desactivada al abrir manualmente");
        _config.ShowEditorHelpOnStartup.Should().BeTrue("debe reactivarse para que vuelva a mostrarse al iniciar a menos que el usuario vuelva a marcar");
        _configServiceMock.Verify(c => c.Save(), Times.Once);

        // Act 2: si el usuario simplemente cierra sin marcar, permanece reactivada
        vm.CloseCommand.Execute(null);
        _config.ShowEditorHelpOnStartup.Should().BeTrue();

        // Act 3: si el usuario vuelve a abrir, marca la casilla y cierra, se vuelve a suprimir
        vm.OpenCommand.Execute(null);
        vm.DontShowAgainOnStartup = true;
        vm.CloseCommand.Execute(null);
        _config.ShowEditorHelpOnStartup.Should().BeFalse();
    }
}

