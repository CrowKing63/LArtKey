namespace LArtKey.ViewModels;

/// <summary>
/// [English text][L3] English text.
/// [English text] English text.
/// </summary>
public sealed class ScanTargetVm
{
    // English text.
    public required string AccessibleName { get; init; }

    // English text.
    public required Action Activate { get; init; }

    // English text.
    public required Action<bool> SetScanFocused { get; init; }

    // UIEnglish text.
    public required string DisplayText { get; init; }

    // English text.
    public required string Kind { get; init; }
}
