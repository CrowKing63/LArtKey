namespace LArtKey.Services;

/// <summary>
/// text.
/// </summary>
public interface IUserDictionaryRepository
{
    void SelectLanguage(bool korean);

    string NormalizeWord(string rawWord);

    IReadOnlyList<(string Word, int Frequency)> GetAllWords();

    void SetWordFrequency(string word, int frequency);

    bool RemoveWord(string word);

    void ClearWords();

    IReadOnlyList<(string Prev, string Next, int Count)> GetAllBigrams();

    void SetBigramCount(string prev, string next, int count);

    bool RemoveBigramPair(string prev, string next);

    int RemoveAllBigramsFor(string prev);

    void ClearBigrams();

    void Flush();
}
