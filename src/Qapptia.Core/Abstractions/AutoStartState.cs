namespace Qapptia.Core.Abstractions;

public enum AutoStartScope
{
    Unsupported,
    Portable,
    CurrentUser,
    AllUsers,
    Unknown
}

public enum AutoStartError
{
    None,
    RegistryReadFailed,
    AmbiguousScope,
    OtherCopy
}

/// <summary>
/// Estado consultado en el sistema, independiente de las preferencias guardadas en JSON.
/// </summary>
public sealed record AutoStartState(AutoStartScope Scope, bool IsEnabled, AutoStartError Error = AutoStartError.None)
{
    public bool HasError => Error != AutoStartError.None;
    public bool CanConfigure => !HasError && Scope is AutoStartScope.CurrentUser or AutoStartScope.AllUsers;
}

public sealed record AutoStartChangeResult(bool Succeeded, bool Cancelled, AutoStartState State);
