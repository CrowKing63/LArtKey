using System.Collections.Concurrent;
using LAltKey.Models;
using LAltKey.Services;
using LAltKey.Services.InputLanguage;

namespace LAltKey.Tests.InputLanguage;

/// <summary>
/// Test double that records outgoing input instead of calling SendInput.
/// </summary>
public sealed class FakeInputService : InputService
{
    public ConcurrentBag<string> SentUnicodes { get; } = new();
    public ConcurrentBag<(int prevLen, string next)> AtomicReplaces { get; } = new();
    public List<VirtualKeyCode> KeyPresses { get; } = new();

    public override void SendUnicode(string text) => SentUnicodes.Add(text);

    public override void SendAtomicReplace(int prevLen, string next)
    {
        AtomicReplaces.Add((prevLen, next));
        TrackedOnScreenLength = next.Length;
    }

    public override void SendKeyPress(VirtualKeyCode vk) => KeyPresses.Add(vk);
}

internal sealed class WordFrequencyStoreInMemory : WordFrequencyStore
{
    private readonly Dictionary<string, int> _freq = new(StringComparer.OrdinalIgnoreCase);
    public int UserWordCount => _freq.Count;

    public WordFrequencyStoreInMemory() : base("test") { }

    public new IReadOnlyList<string> GetSuggestions(string prefix, int count = 5)
    {
        if (string.IsNullOrEmpty(prefix)) return [];
        return _freq
            .Where(kv => kv.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && kv.Key.Length > prefix.Length)
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Take(count)
            .Select(kv => kv.Key)
            .ToList();
    }

    public new void RecordWord(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return;
        word = word.Trim().ToLowerInvariant();
        _freq[word] = (_freq.TryGetValue(word, out var c) ? c : 0) + 1;
    }
}

public sealed class EnglishDictionaryTestable : EnglishDictionary
{
    public EnglishDictionaryTestable()
        : base(_ => new WordFrequencyStoreInMemory(), _ => new BigramFrequencyStore(
            Directory.CreateTempSubdirectory("laltkey-test-en-bigram-").FullName, "en"))
    { }
}

internal static class TestSlotFactory
{
    public static KeySlot English(string label, string? shiftLabel = null, VirtualKeyCode vk = VirtualKeyCode.VK_A) =>
        new(Label: label, ShiftLabel: shiftLabel, Action: new SendKeyAction(vk.ToString()), EnglishLabel: label.ToLowerInvariant(), EnglishShiftLabel: shiftLabel?.ToLowerInvariant());

    public static KeySlot Number(string label, string? shiftLabel = null, VirtualKeyCode vk = VirtualKeyCode.VK_0) =>
        new(Label: label, ShiftLabel: shiftLabel, Action: new SendKeyAction(vk.ToString()), EnglishLabel: label, EnglishShiftLabel: shiftLabel);

    public static KeySlot Symbol(string label, string? shiftLabel = null, VirtualKeyCode vk = VirtualKeyCode.VK_OEM_MINUS) =>
        new(Label: label, ShiftLabel: shiftLabel, Action: new SendKeyAction(vk.ToString()), EnglishLabel: label, EnglishShiftLabel: shiftLabel);

    public static KeySlot Backspace() =>
        new(Label: "Backspace", ShiftLabel: null, Action: new SendKeyAction(VirtualKeyCode.VK_BACK.ToString()));

    internal static (EnglishInputModule module, FakeInputService input, EnglishDictionaryTestable dict) CreateModuleWithInput(
        bool autoCompleteEnabled = true)
    {
        var input = new FakeInputService();
        var dict = new EnglishDictionaryTestable();
        var config = new ConfigService();
        config.Current.AutoCompleteEnabled = autoCompleteEnabled;
        var module = new EnglishInputModule(input, dict, config);
        return (module, input, dict);
    }

    internal static void FeedEnglish(EnglishInputModule module, string text, KeyContext ctx)
    {
        foreach (char ch in text)
        {
            var vk = (VirtualKeyCode)(0x41 + (char.ToUpperInvariant(ch) - 'A'));
            var slot = English(ch.ToString(), null, vk);
            module.HandleKey(slot, ctx);
        }
    }
}