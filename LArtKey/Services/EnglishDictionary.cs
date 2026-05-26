using System.IO;
using System.Reflection;
using System.Text;

namespace LArtKey.Services;

public class EnglishDictionary
{
    private const int BigramBoost = 3;

    private readonly WordFrequencyStore _userStore;
    private readonly BigramFrequencyStore _bigramStore;
    private readonly IReadOnlyList<string> _builtIn;

    public EnglishDictionary(
        Func<string, WordFrequencyStore> userStoreFactory,
        Func<string, BigramFrequencyStore> bigramStoreFactory)
    {
        _userStore = userStoreFactory("en");
        _bigramStore = bigramStoreFactory("en");
        _builtIn = LoadBuiltIn();
    }

    public WordFrequencyStore UserStore => _userStore;
    public BigramFrequencyStore BigramStore => _bigramStore;

    public IReadOnlyList<string> GetSuggestions(string prefix, int count = 20)
    {
        if (prefix.Length < 2) return [];

        var userSuggestions = _userStore.GetSuggestions(prefix, count);

        var needed = count - userSuggestions.Count;
        if (needed <= 0) return userSuggestions;

        var userSet = new HashSet<string>(userSuggestions);
        var builtInSuggestions = _builtIn
            .Where(w => w.StartsWith(prefix) && w.Length > prefix.Length
                        && !userSet.Contains(w))
            .Take(needed)
            .ToList();

        return [..userSuggestions, ..builtInSuggestions];
    }

    /// English text)
    public void RecordWord(string word)
    {
        if (word.Length < 2) return;
        _userStore.RecordWord(word.ToLowerInvariant());
    }

    /// English text).
    public bool TryRemoveUserWord(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return false;
        return _userStore.RemoveWord(word.Trim().ToLowerInvariant());
    }

    /// (prev, next) English text.
    public void RecordBigram(string prevWord, string nextWord)
    {
        if (string.IsNullOrWhiteSpace(prevWord) || string.IsNullOrWhiteSpace(nextWord)) return;
        if (prevWord.Length < 2 || nextWord.Length < 2) return;
        _bigramStore.Record(
            prevWord.ToLowerInvariant().Trim(),
            nextWord.ToLowerInvariant().Trim());
    }

    /// English text.
    public IReadOnlyList<string> GetSuggestions(string prefix, string? prevWord, int count = 20)
    {
        var baseList = GetSuggestions(prefix, count);

        if (string.IsNullOrEmpty(prevWord)) return baseList;

        var bigramHits = _bigramStore.GetNexts(prevWord.ToLowerInvariant(), prefix.ToLowerInvariant(), count * 2);
        if (bigramHits.Count == 0) return baseList;

        var baseIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < baseList.Count; i++)
            baseIndex[baseList[i]] = i;

        // English text.
        int maxNewInserts = string.IsNullOrEmpty(prefix) ? count : count / 2;
        int newInserts = 0;

        var scored = new List<(string Word, double Score)>();
        foreach (var w in baseList)
        {
            double rank = baseList.Count - baseIndex[w];
            double boost = 0;
            foreach (var (next, c) in bigramHits)
            {
                if (next.Equals(w, StringComparison.OrdinalIgnoreCase)) { boost = c * BigramBoost; break; }
            }
            scored.Add((w, rank + boost));
        }

        foreach (var (next, c) in bigramHits)
        {
            if (baseIndex.ContainsKey(next)) continue;
            if (newInserts >= maxNewInserts) break;
            scored.Add((next, c * BigramBoost));
            newInserts++;
        }

        return scored
            .OrderByDescending(t => t.Score)
            .ThenBy(t => t.Word, StringComparer.OrdinalIgnoreCase)
            .Take(count)
            .Select(t => t.Word)
            .ToList();
    }

    /// English text.
    public bool TryRemoveBigramPair(string prev, string next) =>
        _bigramStore.RemovePair(prev.ToLowerInvariant(), next.ToLowerInvariant());

    /// English text — English text
    public void Flush()
    {
        _userStore.Flush();
        _bigramStore.Flush();
    }

    /// <summary>
    /// English text.
    /// </summary>
    public void ReloadUserWords() => _userStore.ReloadFromDisk();

    /// <summary>
    /// bigram English text.
    /// </summary>
    public void ReloadBigrams() => _bigramStore.ReloadFromDisk();

    private static IReadOnlyList<string> LoadBuiltIn()
    {
        var asm = Assembly.GetExecutingAssembly();
        var stream = asm.GetManifestResourceStream("LArtKey.Assets.Data.en-words.txt");
        if (stream is null) return [];
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd()
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToArray();
    }
}
