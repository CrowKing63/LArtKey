using LAltKey.Services;

namespace LAltKey.Tests.Services;

public class WordFrequencyStoreTests : IDisposable
{
    private readonly string _testDir;

    public WordFrequencyStoreTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "LAltKeyTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir)) Directory.Delete(_testDir, recursive: true);
    }

    private WordFrequencyStore NewStore(string lang = "en") => new(_testDir, lang);

    [Fact]
    public void RecordWord_and_GetSuggestions_rank_by_frequency()
    {
        var store = NewStore();
        store.RecordWord("hello");
        store.RecordWord("help");
        store.RecordWord("help");

        var result = store.GetSuggestions("he", 5);

        Assert.Equal(["help", "hello"], result);
    }

    [Fact]
    public void SetFrequency_updates_or_removes_words()
    {
        var store = NewStore();
        store.SetFrequency("hello", 3);
        store.SetFrequency("help", 1);
        store.SetFrequency("help", 0);

        Assert.Equal([("hello", 3)], store.GetAllWords());
    }

    [Fact]
    public void RemoveWord_removes_existing_word()
    {
        var store = NewStore();
        store.RecordWord("hello");

        Assert.True(store.RemoveWord("hello"));
        Assert.Empty(store.GetAllWords());
    }

    [Fact]
    public void Clear_removes_all_words_and_persists()
    {
        var store = NewStore();
        store.RecordWord("alpha");
        store.RecordWord("beta");
        store.Clear();

        var reloaded = NewStore();

        Assert.Empty(reloaded.GetAllWords());
    }

    [Fact]
    public void GetAllWords_Returns_SortedByFrequencyDesc_ThenWordAsc()
    {
        var store = NewStore();
        store.RecordWord("beta");
        store.RecordWord("alpha");
        store.RecordWord("alpha");
        store.RecordWord("gamma");
        store.RecordWord("gamma");

        var all = store.GetAllWords();

        Assert.Equal(("alpha", 2), all[0]);
        Assert.Equal(("gamma", 2), all[1]);
        Assert.Equal(("beta", 1), all[2]);
    }
}