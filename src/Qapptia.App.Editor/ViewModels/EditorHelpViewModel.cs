using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Qapptia.Core.Configuration;

namespace Qapptia.App.Editor.ViewModels;

/// <summary>
/// Modelo de vista desacoplado para la guía interactiva por bloques contextuales (Coach Marks) del Editor.
/// Gestiona la presentación secuencial de secciones, atajos y persistencia de preferencias.
/// </summary>
public partial class EditorHelpViewModel : ObservableObject
{
    private readonly IConfigService _configService;
    private readonly HashSet<int> _visitedSlides = [];

    public const int TotalSlidesCount = 6;

    [ObservableProperty]
    private bool _isVisible;

    [ObservableProperty]
    private bool _dontShowAgainOnStartup;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShortcutRequirementText))]
    private string _shortcutScreenText = "Ctrl + Shift + Q";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShortcutRequirementText))]
    private string _shortcutAreaText = "Ctrl + Shift + A";

    public string ShortcutRequirementText =>
        $"Para capturar con el teclado ({ShortcutScreenText} y {ShortcutAreaText}), el Capturador debe estar en ejecución:";

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

    public EditorHelpViewModel(IConfigService configService)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        RefreshShortcuts();
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
            _configService.Reload();
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
        
        // Al abrir la ayuda, la casilla siempre inicia desmarcada
        DontShowAgainOnStartup = false;

        // Si estaba previamente suprimida, al abrir manualmente se reactiva la ayuda para el inicio
        if (_configService.Current != null && !_configService.Current.ShowEditorHelpOnStartup)
        {
            try
            {
                _configService.Current.ShowEditorHelpOnStartup = true;
                _configService.Save();
            }
            catch
            {
                // Resiliencia de guardado
            }
        }

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
    /// Cierra la guía y persiste la omisión futura si la casilla fue marcada.
    /// </summary>
    [RelayCommand]
    public void Close()
    {
        if (DontShowAgainOnStartup)
        {
            try
            {
                if (_configService.Current.ShowEditorHelpOnStartup)
                {
                    _configService.Current.ShowEditorHelpOnStartup = false;
                    _configService.Save();
                }
            }
            catch
            {
                // Resiliencia de guardado
            }
        }

        IsVisible = false;
    }

    /// <summary>
    /// Actualiza y formatea los atajos de teclado desde la configuración actual.
    /// </summary>
    public void RefreshShortcuts()
    {
        var current = _configService.Current;
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
