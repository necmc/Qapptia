using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Qapptia.Core.Abstractions;
using Qapptia.Core.Configuration;
using Qapptia.Core.Services;
using Qapptia.Editor.Services;

namespace Qapptia.App.Editor.ViewModels;

/// <summary>
/// Modelo de vista desacoplado para la guía interactiva por bloques contextuales (Coach Marks) del Editor.
/// Gestiona la presentación secuencial de secciones, atajos y persistencia de preferencias.
/// </summary>
public partial class EditorHelpViewModel : ObservableObject
{
    private readonly IConfigReader _configReader;
    private readonly IEditorStateService _stateService;
    private readonly IShellService _shellService;
    private readonly HashSet<int> _visitedSlides = [];

    public const int TotalSlidesCount = 7;

    [ObservableProperty]
    private bool _isVisible;

    public bool ShouldShowOnStartup => _stateService.Load().Session.ShowEditorHelpOnStartup;

    public bool DontShowAgainOnStartup
    {
        get => !ShouldShowOnStartup;
        set
        {
            if (value != DontShowAgainOnStartup)
                SaveStartupPreference(!value);
        }
    }

    [ObservableProperty]
    private string _shortcutScreenText = "Ctrl + Shift + Q";

    [ObservableProperty]
    private string _shortcutAreaText = "Ctrl + Shift + A";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Requerido como propiedad de instancia para binding en Avalonia")]
    public string ShortcutRequirementText =>
        "Para poder capturar la pantalla, el Capturador debe estar activo:";

    [ObservableProperty]
    private bool _hasSeenAllSlides;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoPrevious))]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    [NotifyPropertyChangedFor(nameof(IsLastSlide))]
    [NotifyPropertyChangedFor(nameof(SlideStepDisplay))]
    [NotifyPropertyChangedFor(nameof(IsSlide0))]
    [NotifyPropertyChangedFor(nameof(IsSlide1))]
    [NotifyPropertyChangedFor(nameof(IsSlide2))]
    [NotifyPropertyChangedFor(nameof(IsSlide3))]
    [NotifyPropertyChangedFor(nameof(IsSlide4))]
    [NotifyPropertyChangedFor(nameof(IsSlide5))]
    [NotifyPropertyChangedFor(nameof(IsSlide6))]
    private int _currentSlideIndex;

    public static int TotalSlides => TotalSlidesCount;

    public bool CanGoPrevious => CurrentSlideIndex > 0;

    public bool CanGoNext => CurrentSlideIndex < TotalSlidesCount - 1;

    public bool IsLastSlide => CurrentSlideIndex >= TotalSlidesCount - 1;

    public string SlideStepDisplay => $"{CurrentSlideIndex + 1} de {TotalSlidesCount}";

    public bool IsSlide0 => CurrentSlideIndex == 0;
    public bool IsSlide1 => CurrentSlideIndex == 1;
    public bool IsSlide2 => CurrentSlideIndex == 2;
    public bool IsSlide3 => CurrentSlideIndex == 3;
    public bool IsSlide4 => CurrentSlideIndex == 4;
    public bool IsSlide5 => CurrentSlideIndex == 5;
    public bool IsSlide6 => CurrentSlideIndex == 6;

    public EditorHelpViewModel(IConfigReader configReader, IEditorStateService stateService, IShellService? shellService = null)
    {
        _configReader = configReader ?? throw new ArgumentNullException(nameof(configReader));
        _stateService = stateService ?? throw new ArgumentNullException(nameof(stateService));
        _shellService = shellService ?? NullShellService.Instance;
        RefreshShortcuts();
    }

    /// <summary>
    /// Abre el enlace de apoyo/patrocinio del proyecto en el navegador predeterminado.
    /// </summary>
    [RelayCommand]
    public void OpenSponsor()
    {
        try
        {
            _shellService.OpenUrl(Qapptia.Core.Constants.DefaultSponsorUrl);
        }
        catch
        {
            // Resiliencia ante fallos del navegador
        }
    }

    partial void OnCurrentSlideIndexChanged(int value)
    {
        _visitedSlides.Add(value);
        if (_visitedSlides.Count >= TotalSlidesCount)
        {
            HasSeenAllSlides = true;
        }
    }

    /// <summary>
    /// Abre la guía refrescando atajos y restableciendo el recorrido en el primer bloque.
    /// </summary>
    [RelayCommand]
    public void Open()
    {
        try
        {
            _configReader.Reload();
        }
        catch
        {
            // Resiliencia ante fallos temporales de lectura de config
        }

        RefreshShortcuts();
        _visitedSlides.Clear();
        _visitedSlides.Add(0);
        HasSeenAllSlides = false;
        CurrentSlideIndex = 0;

        // Cada apertura restablece la preferencia de mostrar la ayuda al iniciar.
        SaveStartupPreference(show: true);
        IsVisible = true;
    }

    /// <summary>
    /// Avanza al siguiente bloque, o cierra si está en el último bloque.
    /// </summary>
    [RelayCommand]
    public void NextSlide()
    {
        if (CanGoNext)
        {
            CurrentSlideIndex++;
        }
        else
        {
            Close();
        }
    }

    /// <summary>
    /// Retrocede al bloque previo si no está en el primero.
    /// </summary>
    [RelayCommand]
    public void PreviousSlide()
    {
        if (CanGoPrevious)
        {
            CurrentSlideIndex--;
        }
    }

    /// <summary>
    /// Salta directamente a un bloque específico por índice.
    /// </summary>
    [RelayCommand]
    public void GoToSlide(object? parameter)
    {
        int index = parameter switch
        {
            int i => i,
            string s when int.TryParse(s, out var parsed) => parsed,
            _ => 0
        };

        if (index >= 0 && index < TotalSlidesCount)
        {
            CurrentSlideIndex = index;
        }
    }

    /// <summary>
    /// Cierra la guía.
    /// </summary>
    [RelayCommand]
    public void Close() => IsVisible = false;

    private void SaveStartupPreference(bool show)
    {
        var state = _stateService.Load();
        state.Session.ShowEditorHelpOnStartup = show;
        _stateService.Save(state);
        OnPropertyChanged(nameof(ShouldShowOnStartup));
        OnPropertyChanged(nameof(DontShowAgainOnStartup));
    }

    /// <summary>
    /// Actualiza y formatea los atajos de teclado desde la configuración actual.
    /// </summary>
    public void RefreshShortcuts()
    {
        var current = _configReader.Current?.Capture;
        ShortcutScreenText = FormatShortcut(current?.ShortcutScreen, "Ctrl + Shift + Q");
        ShortcutAreaText = FormatShortcut(current?.ShortcutArea, "Ctrl + Shift + A");
    }

    /// <summary>
    /// Formatea una cadena de atajo (ej. "ctrl+shift+q") a una representación legible ("Ctrl + Shift + Q").
    /// </summary>
    public static string FormatShortcut(string? rawShortcut, string fallback = "")
    {
        if (string.IsNullOrWhiteSpace(rawShortcut))
        {
            return fallback;
        }

        var tokens = rawShortcut.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length == 0)
        {
            return fallback;
        }

        var formatted = tokens.Select(token =>
        {
            return token.ToLowerInvariant() switch
            {
                "ctrl" or "control" => "Ctrl",
                "shift" => "Shift",
                "alt" => "Alt",
                "win" or "meta" => "Win",
                _ => token.Length == 1 ? token.ToUpperInvariant() : char.ToUpperInvariant(token[0]) + token[1..].ToLowerInvariant()
            };
        });

        return string.Join(" + ", formatted);
    }
}
