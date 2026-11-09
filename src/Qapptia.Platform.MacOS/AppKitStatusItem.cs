using System.Runtime.InteropServices;
using Avalonia.Threading;
using Qapptia.Core;
using Qapptia.Core.Abstractions;
using Qapptia.Core.Extensions;
using Serilog;

namespace Qapptia.Platform.MacOS;

internal sealed partial class AppKitStatusItem : IDisposable
{
    private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";
    private const string ActionSelector = "qapptiaPerformTrayAction:";
    private const double VariableStatusItemLength = -1;
    private const double IconSize = 18;
    private const int LeftMouseUp = 2;
    private const int RightMouseUp = 4;
    private const int MouseUpMask = (1 << LeftMouseUp) | (1 << RightMouseUp);

    private static readonly Dictionary<nint, AppKitStatusItem> s_targets = new();
    private static readonly ActionCallback s_actionCallback = OnAction;
    private static nint s_targetClass;

    private readonly TrayMenuDefinition _definition;
    private readonly ILogger _logger;
    private readonly Dictionary<nint, Action> _menuActions = new();
    private nint _statusBar;
    private nint _statusItem;
    private nint _button;
    private nint _target;
    private nint _menu;
    private IDisposable? _pendingSingleClick;

    public AppKitStatusItem(TrayMenuDefinition definition, string iconPath, ILogger logger)
    {
        _definition = definition;
        _logger = logger;

        try
        {
            _statusBar = Send(objc_getClass("NSStatusBar"), "systemStatusBar");
            var statusItem = SendDoubleArgument(_statusBar, sel_registerName("statusItemWithLength:"), VariableStatusItemLength);
            // NSStatusBar no retiene el icono devuelto.
            _statusItem = Send(statusItem, "retain");
            _button = Send(_statusItem, "button");
            _target = Send(GetTargetClass(), "new");
            if (_button == 0 || _target == 0)
            {
                throw new InvalidOperationException("AppKit no pudo crear el botón de la bandeja.");
            }

            s_targets.Add(_target, this);
            Set(_button, "setTarget:", _target);
            Set(_button, "setAction:", sel_registerName(ActionSelector));
            _ = SendWithArgument(_button, sel_registerName("sendActionOn:"), MouseUpMask);
            Set(_button, "setToolTip:", ToNativeString(Constants.CaptureAppName));
            SetIcon(iconPath);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    private void SetIcon(string iconPath)
    {
        string pngPath = Path.ChangeExtension(iconPath, ".png");
        string imagePath = File.Exists(pngPath) ? pngPath : iconPath;
        var image = SendWithArgument(Send(objc_getClass("NSImage"), "alloc"),
            sel_registerName("initWithContentsOfFile:"), ToNativeString(imagePath));
        if (image == 0)
        {
            Set(_button, "setTitle:", ToNativeString(Constants.CaptureAppName));
            return;
        }

        SetSize(image, sel_registerName("setSize:"), new NativeSize(IconSize, IconSize));
        Set(_button, "setImage:", image);
        SendVoid(image, sel_registerName("release"));
    }

    private void HandleAction(nint sender)
    {
        if (sender != _button)
        {
            if (_menuActions.TryGetValue(sender, out var action)) Dispatcher.UIThread.Post(action);
            return;
        }

        var app = Send(objc_getClass("NSApplication"), "sharedApplication");
        var mouseEvent = Send(app, "currentEvent");
        switch (Send(mouseEvent, "type"))
        {
            case LeftMouseUp:
                HandleLeftClick(Send(mouseEvent, "clickCount"));
                break;
            case RightMouseUp:
                CancelSingleClick();
                ShowMenu(mouseEvent);
                break;
        }
    }

    private void HandleLeftClick(nint clickCount)
    {
        CancelSingleClick();
        if (clickCount == 2 && _definition.DoubleClickAction is { } doubleClick)
        {
            _ = Task.Run(doubleClick);
        }
        else if (clickCount == 1 && _definition.SingleClickAction is { } singleClick)
        {
            double interval = SendDoubleResult(objc_getClass("NSEvent"), sel_registerName("doubleClickInterval"));
            _pendingSingleClick = DispatcherTimer.RunOnce(() =>
            {
                _pendingSingleClick = null;
                _ = Task.Run(singleClick);
            }, TimeSpan.FromSeconds(interval), DispatcherPriority.Background);
        }
    }

    private void ShowMenu(nint mouseEvent)
    {
        ReleaseMenu();
        var menuClass = objc_getClass("NSMenu");
        var itemClass = objc_getClass("NSMenuItem");
        _menu = Send(menuClass, "new");
        Set(_menu, "setAutoenablesItems:", 0);
        var emptyKey = ToNativeString(string.Empty);

        foreach (var item in _definition.Items)
        {
            if (!item.IsVisible) continue;

            if (item is TrayMenuSeparatorItem)
            {
                Set(_menu, "addItem:", Send(itemClass, "separatorItem"));
            }
            else if (item is TrayMenuActionItem action)
            {
                string? shortcut = action.ShortcutTextProvider?.Invoke();
                string label = string.IsNullOrWhiteSpace(shortcut) ? action.Text : $"{action.Text}    {shortcut.ToShortcutTitleCase()}";
                var nativeItem = SendThreeArguments(Send(itemClass, "alloc"), sel_registerName("initWithTitle:action:keyEquivalent:"),
                    ToNativeString(label), sel_registerName(ActionSelector), emptyKey);
                Set(nativeItem, "setTarget:", _target);
                Set(nativeItem, "setState:", action.IsChecked ? 1 : 0);
                _menuActions.Add(nativeItem, action.OnClick);
                Set(_menu, "addItem:", nativeItem);
                SendVoid(nativeItem, sel_registerName("release"));
            }
        }

        SendThreeArgumentsVoid(menuClass, sel_registerName("popUpContextMenu:withEvent:forView:"), _menu, mouseEvent, _button);
    }

    private void CancelSingleClick()
    {
        _pendingSingleClick?.Dispose();
        _pendingSingleClick = null;
    }

    private void ReleaseMenu()
    {
        _menuActions.Clear();
        SendVoid(_menu, sel_registerName("release"));
        _menu = 0;
    }

    public void Dispose()
    {
        CancelSingleClick();
        ReleaseMenu();
        if (_target != 0)
        {
            s_targets.Remove(_target);
            Set(_button, "setTarget:", 0);
            Set(_button, "setAction:", 0);
            SendVoid(_target, sel_registerName("release"));
            _target = 0;
        }
        if (_statusItem != 0)
        {
            Set(_statusBar, "removeStatusItem:", _statusItem);
            SendVoid(_statusItem, sel_registerName("release"));
            _statusItem = 0;
            _button = 0;
        }
    }

    private static nint GetTargetClass()
    {
        if (s_targetClass != 0) return s_targetClass;

        var targetClass = objc_allocateClassPair(objc_getClass("NSObject"), "QapptiaTrayActionTarget", 0);
        if (targetClass == 0)
        {
            throw new InvalidOperationException("No se pudo crear el receptor de acciones de AppKit.");
        }

        if (!class_addMethod(targetClass, sel_registerName(ActionSelector), Marshal.GetFunctionPointerForDelegate(s_actionCallback), "v@:@"))
        {
            objc_disposeClassPair(targetClass);
            throw new InvalidOperationException("No se pudo registrar la acción de bandeja de AppKit.");
        }

        objc_registerClassPair(targetClass);
        s_targetClass = targetClass;
        return targetClass;
    }

    private static void OnAction(nint target, nint selector, nint sender)
    {
        if (!s_targets.TryGetValue(target, out var item)) return;

        try { item.HandleAction(sender); }
        catch (Exception ex) { item._logger.Error(ex, "Error al procesar una acción de la bandeja de macOS."); }
    }

    private static nint Send(nint receiver, string selector) => SendId(receiver, sel_registerName(selector));
    private static void Set(nint receiver, string selector, nint argument) => SendWithArgumentVoid(receiver, sel_registerName(selector), argument);
    private static nint ToNativeString(string value) => SendStringArgument(objc_getClass("NSString"), sel_registerName("stringWithUTF8String:"), value);

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct NativeSize(double width, double height)
    {
        public readonly double Width = width;
        public readonly double Height = height;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ActionCallback(nint target, nint selector, nint sender);

    [LibraryImport(ObjCLibrary, StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint objc_getClass(string name);

    [LibraryImport(ObjCLibrary, StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint sel_registerName(string name);

    [LibraryImport(ObjCLibrary, StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint objc_allocateClassPair(nint superclass, string name, nuint extraBytes);

    [LibraryImport(ObjCLibrary)]
    private static partial void objc_registerClassPair(nint cls);

    [LibraryImport(ObjCLibrary)]
    private static partial void objc_disposeClassPair(nint cls);

    [LibraryImport(ObjCLibrary, StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool class_addMethod(nint cls, nint selector, nint implementation, string types);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static partial nint SendId(nint receiver, nint selector);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static partial void SendVoid(nint receiver, nint selector);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static partial nint SendWithArgument(nint receiver, nint selector, nint argument);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static partial void SendWithArgumentVoid(nint receiver, nint selector, nint argument);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static partial nint SendThreeArguments(nint receiver, nint selector, nint first, nint second, nint third);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static partial void SendThreeArgumentsVoid(nint receiver, nint selector, nint first, nint second, nint third);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static partial nint SendDoubleArgument(nint receiver, nint selector, double argument);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static partial double SendDoubleResult(nint receiver, nint selector);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static partial void SetSize(nint receiver, nint selector, NativeSize size);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint SendStringArgument(nint receiver, nint selector, string value);
}
