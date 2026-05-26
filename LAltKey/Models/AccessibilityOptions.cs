namespace LAltKey.Models;

/// <summary>
/// Defines which controls are included in keyboard accessibility navigation.
/// </summary>
public enum KeyboardA11yNavigationScope
{
    KeysOnly,
    AllControls
}

/// <summary>
/// Defines how often switch scanning announces target changes.
/// </summary>
public enum SwitchScanAnnounceMode
{
    Off,
    SelectionOnly,
    EveryMove
}

/// <summary>
/// Tracks which accessibility mode currently owns the visual focus indicator.
/// </summary>
public enum A11yFocusOwner
{
    None,
    KeyboardNavigation,
    SwitchScan
}