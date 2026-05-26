using LAltKey.Services;

namespace LAltKey.Tests.Services;

public class BigramFrequencyStoreTests : IDisposable
{
    private readonly string _testDir;

    public BigramFrequencyStoreTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "LAltKeyTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir)) Directory.Delete(_testDir, recursive: true);
    }

    private BigramFrequencyStore NewStore(string lang = "en") => new(_testDir, lang);

    [Fact]
    public void GetNexts_filters_by_prefix_and_orders_by_count_desc()
    {
        var store = NewStore();
        store.Record("hello", "world");
        store.Record("hello", "world");
        store.Record("hello", "weather");
        store.Record("hello", "work");
        store.Record("hello", "work");
        store.Record("hello", "work");

        var result = store.GetNexts("hello", "wo", 5);

        Assert.Equal([("work", 3), ("world", 2)], result);
    }

    [Fact]
    public void GetNexts_empty_prefix_returns_top_N()
    {
        var store = NewStore();
        store.Record("good", "morning");
        store.Record("good", "morning");
        store.Record("good", "night");

        var result = store.GetNexts("good", "", 1);

        Assert.Equal([("morning", 2)], result);
    }

    [Fact]
    public void GetNexts_with_unknown_prev_returns_empty()
    {
        var store = NewStore();
        store.Record("hello", "world");

        Assert.Empty(store.GetNexts("missing", "", 5));
    }

    [Fact]
    public void RemovePair_removes_only_target_and_cleans_empty_prev()
    {
        var store = NewStore();
        store.Record("hello", "world");
        store.Record("hello", "there");

        Assert.True(store.RemovePair("hello", "world"));
        Assert.DoesNotContain(("world", 1), store.GetNexts("hello", "", 5));
        Assert.Contains(("there", 1), store.GetNexts("hello", "", 5));
    }

    [Fact]
    public void RemoveAllFor_removes_all_nexts_of_prev()
    {
        var store = NewStore();
        store.Record("good", "morning");
        store.Record("good", "night");

        Assert.Equal(2, store.RemoveAllFor("good"));
        Assert.Empty(store.GetNexts("good", "", 5));
    }

    [Fact]
    public void Clear_empties_store()
    {
        var store = NewStore();
        store.Record("one", "two");
        store.Record("three", "four");

        store.Clear();

        Assert.Empty(store.GetAllPairs());
    }

    [Fact]
    public void GetAllPairs_snapshot_is_sorted_prev_asc_then_count_desc()
    {
        var store = NewStore();
        store.Record("beta", "two");
        store.Record("alpha", "one");
        store.Record("alpha", "two");
        store.Record("alpha", "two");

        var pairs = store.GetAllPairs();

        Assert.Equal(("alpha", "two", 2), pairs[0]);
        Assert.Equal(("alpha", "one", 1), pairs[1]);
        Assert.Equal(("beta", "two", 1), pairs[2]);
    }

    [Fact]
    public void Reload_from_disk_round_trips_all_pairs()
    {
        var store = NewStore();
        store.Record("hello", "world");
        store.Record("good", "morning");
        store.Flush();

        var reloaded = NewStore();

        Assert.Equal(2, reloaded.GetAllPairs().Count);
    }
}