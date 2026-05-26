namespace LAltKey.Services;

/// <summary>
/// Provides editor access to the English user dictionary and learned bigram store.
/// </summary>
public sealed class UserDictionaryRepository : IUserDictionaryRepository
{
    private readonly EnglishDictionary _dictionary;

    public UserDictionaryRepository(EnglishDictionary dictionary)
    {
        _dictionary = dictionary;
    }

    public string NormalizeWord(string rawWord) => rawWord.Trim().ToLowerInvariant();

    public IReadOnlyList<(string Word, int Frequency)> GetAllWords() => _dictionary.UserStore.GetAllWords();
    public void SetWordFrequency(string word, int frequency) => _dictionary.UserStore.SetFrequency(word, frequency);
    public bool RemoveWord(string word) => _dictionary.UserStore.RemoveWord(word);
    public void ClearWords() => _dictionary.UserStore.Clear();

    public IReadOnlyList<(string Prev, string Next, int Count)> GetAllBigrams() => _dictionary.BigramStore.GetAllPairs();
    public void SetBigramCount(string prev, string next, int count) => _dictionary.BigramStore.SetPairCount(prev, next, count);
    public bool RemoveBigramPair(string prev, string next) => _dictionary.BigramStore.RemovePair(prev, next);
    public int RemoveAllBigramsFor(string prev) => _dictionary.BigramStore.RemoveAllFor(prev);
    public void ClearBigrams() => _dictionary.BigramStore.Clear();

    public void Flush() => _dictionary.Flush();
}
