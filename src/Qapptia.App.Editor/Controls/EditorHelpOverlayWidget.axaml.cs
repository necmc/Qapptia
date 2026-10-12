using Avalonia.Controls;
using Avalonia.Input;
using Qapptia.App.Editor.ViewModels;

namespace Qapptia.App.Editor.Controls;

/// <summary>
/// Control de usuario desacoplado para la visualización y navegación por diapositivas de la guía de ayuda.
/// </summary>
public partial class EditorHelpOverlayWidget : UserControl
{
    public EditorHelpOverlayWidget()
    {
        InitializeComponent();
    }

    private void BackdropGrid_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is EditorHelpViewModel vm)
        {
            vm.CloseCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void DialogCard_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Evitar que los clics dentro de la tarjeta se propaguen al backdrop
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (DataContext is EditorHelpViewModel vm)
        {
            if (e.Key == Key.Right)
            {
                vm.NextSlideCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                vm.PreviousSlideCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                vm.CloseCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}
