using System.Text.Json.Serialization;

namespace LArtKey.Models;

/// <summary>
/// [English text] English text 'English text'English text.
/// [English text] English text.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SendKeyAction),       "SendKey")]
[JsonDerivedType(typeof(SendComboAction),     "SendCombo")]
[JsonDerivedType(typeof(ToggleStickyAction),  "ToggleSticky")]
[JsonDerivedType(typeof(SwitchLayoutAction),  "SwitchLayout")]
// ── T-9.1 English text ──────────────────────────────────────────────────────
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

// ── T-9.1 English text ────────────────────────────────────────────────────

/// English text: "notepad.exe")
/// Args: English text "")
public record RunAppAction(string Path, string Args = "") : KeyAction;

/// English text
public record BoilerplateAction(string Text) : KeyAction;

/// English text
/// Shell: "cmd" | "powershell" (English text "cmd")
/// Hidden: true English text true)
public record ShellCommandAction(string Command, string Shell = "cmd", bool Hidden = true) : KeyAction;

/// English text
/// Direction: "up" | "down" | "mute"
/// Step: English text 1~100 (English text 5)
public record VolumeControlAction(string Direction, int Step = 5) : KeyAction;

/// English text
public record ClipboardPasteAction(string Text) : KeyAction;

/// "English text/A" English text.
public sealed record ToggleInputModeAction() : KeyAction;

public sealed record ToggleFunctionLayerAction() : KeyAction;

/// AI English text)
public record AiAction(string Prompt = "") : KeyAction;
