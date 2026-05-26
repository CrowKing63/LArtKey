namespace LAltKey.ViewModels;

/// <summary>
/// [text][L3] text.
/// [text] text.
/// </summary>
public sealed class ScanTargetVm
{
    // text.
    public required string AccessibleName { get; init; }

    // text.
    public required Action Activate { get; init; }

    // text.
    public required Action<bool> SetScanFocused { get; init; }

    // UItext.
    public required string DisplayText { get; init; }

    // text.
    public required string Kind { get; init; }
}
