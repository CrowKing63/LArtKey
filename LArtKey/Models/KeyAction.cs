using System.Text.Json.Serialization;

namespace LArtKey.Models;

/// <summary>
/// [text] text 'text'text.
/// [text] text.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SendKeyAction),       "SendKey")]
[JsonDerivedType(typeof(SendComboAction),     "SendCombo")]
[JsonDerivedType(typeof(ToggleStickyAction),  "ToggleSticky")]
[JsonDerivedType(typeof(SwitchLayoutAction),  "SwitchLayout")]
// ── T-9.1 text ──────────────────────────────────────────────────────
[JsonDerivedType(typeof(RunAppAction),        "RunApp")]
[JsonDerivedType(typeof(BoilerplateAction),   "Boilerplate")]
[JsonDerivedType(typeof(ShellCommandAction),  "ShellCommand")]
[JsonDerivedType(typeof(VolumeControlAction), "VolumeControl")]
[JsonDerivedType(typeof(ClipboardPasteAction),"ClipboardPaste")]
[JsonDerivedType(typeof(ToggleInputModeAction), "ToggleInputMode")]
[JsonDerivedType(typeof(ToggleFunctionLayerAction), "ToggleFunctionLayer")]
[JsonDerivedType(typeof(AiAction),               "Ai")]
public abstract record KeyAction;

public record SendKeyAction(string Vk)               : KeyAction;
public record SendComboAction(List<string> Keys)     : KeyAction;
public record ToggleStickyAction(string Vk)          : KeyAction;
public record SwitchLayoutAction(string Name)        : KeyAction;

// ── T-9.1 text ────────────────────────────────────────────────────

/// text: "notepad.exe")
/// Args: text "")
public record RunAppAction(string Path, string Args = "") : KeyAction;

/// text
public record BoilerplateAction(string Text) : KeyAction;

/// text
/// Shell: "cmd" | "powershell" (text "cmd")
/// Hidden: true text true)
public record ShellCommandAction(string Command, string Shell = "cmd", bool Hidden = true) : KeyAction;

/// text
/// Direction: "up" | "down" | "mute"
/// Step: text 1~100 (text 5)
public record VolumeControlAction(string Direction, int Step = 5) : KeyAction;

/// text
public record ClipboardPasteAction(string Text) : KeyAction;

/// "text/A" text.
public sealed record ToggleInputModeAction() : KeyAction;

public sealed record ToggleFunctionLayerAction() : KeyAction;

/// AI tool)
public record AiAction(string Prompt = "") : KeyAction;
