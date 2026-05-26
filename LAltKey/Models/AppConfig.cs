using System.Text.Json.Serialization;

namespace LAltKey.Models;

/// <summary>
/// [text] text.
/// </summary>
public class WindowConfig
{
    // text.
    public double Left   { get; set; } = -1;
    
    // text.
    public double Top    { get; set; } = -1;

    // text, 60~200 text.
    public int Scale { get; set; } = 100;
}

/// <summary>
/// [text] LAltKey text.
/// [text] text.
/// </summary>
public class AppConfig
{
    // text.
    public string Version           { get; set; } = "1.0.0";
    
    // text.
    public string DefaultLayout     { get; set; } = "Basic";
    
    // text)
    public bool   AlwaysOnTop       { get; set; } = true;
    
    // text.
    public bool   ActiveOpacityEnabled { get; set; } = false;

    // text ~ 1.0: text 80% text.
    public double OpacityActive        { get; set; } = 1.0;

    // text.
    public bool   IdleOpacityEnabled   { get; set; } = false;
    
    // text ~ 1.0: text 20% text.
    public double OpacityIdle          { get; set; } = 1.0;
    
    // text 'text(Idle)'text, 1000 = 1text).
    public int    FadeDelayMs       { get; set; } = 5000;
    
    // [text] text.
    public bool   DwellEnabled      { get; set; } = false;
    
    // [text] Dwell text).
    public int    DwellTimeMs       { get; set; } = 800;
    
    // [text] Shift, Ctrl text.
    public bool   StickyKeysEnabled { get; set; } = true;
    
    // text ("light", "dark", "system" text).
    public string Theme             { get; set; } = "system";
    
    // text.
    public string GlobalHotkey      { get; set; } = "Ctrl+Alt+K";
    
    // text.
    public bool   AutoProfileSwitch { get; set; } = true;
    
    // text.
    public Dictionary<string, string> Profiles { get; set; } = [];
    
    // text.
    public WindowConfig Window      { get; set; } = new();

    // text.
    public bool RunOnStartup        { get; set; } = false;

    // text.
    public bool SoundEnabled        { get; set; } = true;
    
    // text.
    public string? SoundFilePath    { get; set; } = null;

    // text.
    public bool AskBeforeHideToTray { get; set; } = true;

    // text.
    public bool ClipboardPanelEnabled { get; set; } = false;

    // text.
    public bool AutoCompleteEnabled   { get; set; } = false;

    // [text] text.
    public bool KeyRepeatEnabled      { get; set; } = false;
    
    // text).
    public int  KeyRepeatDelayMs      { get; set; } = 300;
    
    // text.
    public int  KeyRepeatIntervalMs   { get; set; } = 50;

    // [text] text). 80~220 text.
    public int  KeyFontScalePercent   { get; set; } = 100;

    // [text] text.
    public bool KeyboardA11yNavigationEnabled { get; set; } = false;
    
    // [text] text.
    public KeyboardA11yNavigationScope KeyboardA11yNavigationScope { get; set; } = KeyboardA11yNavigationScope.KeysOnly;
    
    // [text] text Esc(VK_ESCAPE)text.
    public string KeyboardA11yExitKey { get; set; } = "VK_ESCAPE";
    
    // [text] text.
    public bool KeyboardA11yAnnounceFocus { get; set; } = false;

    // ── AI tool ──────────────────────────────────────────────

    // AI tool ✨ text.
    public bool AiEnabled { get; set; } = false;

    // AI API text: http://localhost:11434/v1/chat/completions)
    public string AiEndpoint { get; set; } = "";

    // DPAPItext.
    public string AiApiKeyEncrypted { get; set; } = "";

    // AI tool: "gpt-4o-mini", "llama3", "gemma")
    public string AiModel { get; set; } = "";

    // AItext.
    public string AiDefaultPrompt { get; set; } = "Rewrite the selected text clearly and concisely.";

    // AI API text.
    public int AiTimeoutSeconds { get; set; } = 30;

    // ── text ─────────────────────────────────────────────────

    // text.
    public List<HeaderButtonConfig> HeaderButtons { get; set; } = [];

    // ── L2/L3 text ────────────────────────────────────────────────

    // [text][L2] text.
    public bool TtsEnabled { get; set; } = false;

    // [text][L2] text)
    public bool TtsOnHover { get; set; } = false;

    // [text][L2] TTS text) ~ 5(text.
    public int TtsRate { get; set; } = 0;

    // [text][L2] text.
    public bool ReducedMotionEnabled { get; set; } = false;

    // [text][L3] text.
    public bool SwitchScanEnabled { get; set; } = false;

    // [text][L3] text.
    public int SwitchScanIntervalMs { get; set; } = 800;

    // [text][L3] truetext.
    public bool SwitchScanTwoSwitch { get; set; } = false;

    // [text][L3] text→text.
    public SwitchScanMode SwitchScanMode { get; set; } = SwitchScanMode.Linear;

    // [text][L3] text.
    public int SwitchScanInitialDelayMs { get; set; } = 800;

    // [text][L3] text.
    public int SwitchScanSelectPauseMs { get; set; } = 500;

    // [text][L3] text.
    public int SwitchScanCyclesBeforePause { get; set; } = 0;

    // [text][L3] text.
    public bool SwitchScanWrapEnabled { get; set; } = true;

    // [text][L3] text "text" text: VK_TAB)
    public string SwitchScanNextKey { get; set; } = "VK_TAB";

    // [text][L3] text "text" text: VK_RETURN)
    public string SwitchScanSelectKey { get; set; } = "VK_RETURN";

    // [text][L3] text "text" text: VK_SPACE)
    public string SwitchScanSecondarySelectKey { get; set; } = "VK_SPACE";

    // [text][L3] text "text" text.
    public string SwitchScanPreviousKey { get; set; } = "";

    // [text][L3] text "text" text.
    public string SwitchScanPauseKey { get; set; } = "";

    // [text][L3] text.
    public bool SwitchScanIncludeSuggestions { get; set; } = true;

    // [text][L3] text.
    public SwitchScanSuggestionPriority SwitchScanSuggestionPriority { get; set; } = SwitchScanSuggestionPriority.BeforeKeyboard;

    // [text][L3] text.
    public SwitchScanAnnounceMode SwitchScanAnnounceMode { get; set; } = SwitchScanAnnounceMode.SelectionOnly;
}
