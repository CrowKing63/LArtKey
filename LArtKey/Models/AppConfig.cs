using System.Text.Json.Serialization;

namespace LArtKey.Models;

/// <summary>
/// [English text] English text.
/// </summary>
public class WindowConfig
{
    // English text.
    public double Left   { get; set; } = -1;
    
    // English text.
    public double Top    { get; set; } = -1;

    // English text, 60~200 English text.
    public int Scale { get; set; } = 100;
}

/// <summary>
/// [English text] LArtKey English text.
/// [English text] English text.
/// </summary>
public class AppConfig
{
    // English text.
    public string Version           { get; set; } = "1.0.0";
    
    // English text.
    public string DefaultLayout     { get; set; } = "Basic";
    
    // English text)
    public bool   AlwaysOnTop       { get; set; } = true;
    
    // English text.
    public bool   ActiveOpacityEnabled { get; set; } = false;

    // English text ~ 1.0: English text 80% English text.
    public double OpacityActive        { get; set; } = 1.0;

    // English text.
    public bool   IdleOpacityEnabled   { get; set; } = false;
    
    // English text ~ 1.0: English text 20% English text.
    public double OpacityIdle          { get; set; } = 1.0;
    
    // English text 'English text(Idle)'English text, 1000 = 1English text).
    public int    FadeDelayMs       { get; set; } = 5000;
    
    // [English text] English text.
    public bool   DwellEnabled      { get; set; } = false;
    
    // [English text] Dwell English text).
    public int    DwellTimeMs       { get; set; } = 800;
    
    // [English text] Shift, Ctrl English text.
    public bool   StickyKeysEnabled { get; set; } = true;
    
    // English text ("light", "dark", "system" English text).
    public string Theme             { get; set; } = "system";
    
    // English text.
    public string GlobalHotkey      { get; set; } = "Ctrl+Alt+K";
    
    // English text.
    public bool   AutoProfileSwitch { get; set; } = true;
    
    // English text.
    public Dictionary<string, string> Profiles { get; set; } = [];
    
    // English text.
    public WindowConfig Window      { get; set; } = new();

    // English text.
    public bool RunOnStartup        { get; set; } = false;

    // English text.
    public bool SoundEnabled        { get; set; } = true;
    
    // English text.
    public string? SoundFilePath    { get; set; } = null;

    // English text.
    public bool AskBeforeHideToTray { get; set; } = true;

    // English text.
    public bool ClipboardPanelEnabled { get; set; } = false;

    // English text.
    public bool AutoCompleteEnabled   { get; set; } = false;

    // [English text] English text.
    public bool KeyRepeatEnabled      { get; set; } = false;
    
    // English text).
    public int  KeyRepeatDelayMs      { get; set; } = 300;
    
    // English text.
    public int  KeyRepeatIntervalMs   { get; set; } = 50;

    // [English text] English text). 80~220 English text.
    public int  KeyFontScalePercent   { get; set; } = 100;

    // [English text] English text.
    public bool KeyboardA11yNavigationEnabled { get; set; } = false;
    
    // [English text] English text.
    public KeyboardA11yNavigationScope KeyboardA11yNavigationScope { get; set; } = KeyboardA11yNavigationScope.KeysOnly;
    
    // [English text] English text Esc(VK_ESCAPE)English text.
    public string KeyboardA11yExitKey { get; set; } = "VK_ESCAPE";
    
    // [English text] English text.
    public bool KeyboardA11yAnnounceFocus { get; set; } = false;

    // ── AI English text ──────────────────────────────────────────────

    // AI English text ✨ English text.
    public bool AiEnabled { get; set; } = false;

    // AI API English text: http://localhost:11434/v1/chat/completions)
    public string AiEndpoint { get; set; } = "";

    // DPAPIEnglish text.
    public string AiApiKeyEncrypted { get; set; } = "";

    // AI English text: "gpt-4o-mini", "llama3", "gemma")
    public string AiModel { get; set; } = "";

    // AIEnglish text.
    public string AiDefaultPrompt { get; set; } = "English text.";

    // AI API English text.
    public int AiTimeoutSeconds { get; set; } = 30;

    // ── English text ─────────────────────────────────────────────────

    // English text.
    public List<HeaderButtonConfig> HeaderButtons { get; set; } = [];

    // ── L2/L3 English text ────────────────────────────────────────────────

    // [English text][L2] English text.
    public bool TtsEnabled { get; set; } = false;

    // [English text][L2] English text)
    public bool TtsOnHover { get; set; } = false;

    // [English text][L2] TTS English text) ~ 5(English text.
    public int TtsRate { get; set; } = 0;

    // [English text][L2] English text.
    public bool ReducedMotionEnabled { get; set; } = false;

    // [English text][L3] English text.
    public bool SwitchScanEnabled { get; set; } = false;

    // [English text][L3] English text.
    public int SwitchScanIntervalMs { get; set; } = 800;

    // [English text][L3] trueEnglish text.
    public bool SwitchScanTwoSwitch { get; set; } = false;

    // [English text][L3] English text→English text.
    public SwitchScanMode SwitchScanMode { get; set; } = SwitchScanMode.Linear;

    // [English text][L3] English text.
    public int SwitchScanInitialDelayMs { get; set; } = 800;

    // [English text][L3] English text.
    public int SwitchScanSelectPauseMs { get; set; } = 500;

    // [English text][L3] English text.
    public int SwitchScanCyclesBeforePause { get; set; } = 0;

    // [English text][L3] English text.
    public bool SwitchScanWrapEnabled { get; set; } = true;

    // [English text][L3] English text "English text" English text: VK_TAB)
    public string SwitchScanNextKey { get; set; } = "VK_TAB";

    // [English text][L3] English text "English text" English text: VK_RETURN)
    public string SwitchScanSelectKey { get; set; } = "VK_RETURN";

    // [English text][L3] English text "English text" English text: VK_SPACE)
    public string SwitchScanSecondarySelectKey { get; set; } = "VK_SPACE";

    // [English text][L3] English text "English text" English text.
    public string SwitchScanPreviousKey { get; set; } = "";

    // [English text][L3] English text "English text" English text.
    public string SwitchScanPauseKey { get; set; } = "";

    // [English text][L3] English text.
    public bool SwitchScanIncludeSuggestions { get; set; } = true;

    // [English text][L3] English text.
    public SwitchScanSuggestionPriority SwitchScanSuggestionPriority { get; set; } = SwitchScanSuggestionPriority.BeforeKeyboard;

    // [English text][L3] English text.
    public SwitchScanAnnounceMode SwitchScanAnnounceMode { get; set; } = SwitchScanAnnounceMode.SelectionOnly;
}
