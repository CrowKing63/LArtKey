using LAltKey.Services;

namespace LAltKey.Tests.Services;

public class LiveRegionServiceTests
{
    [Fact]
    public void Announce_blocks_same_message_within_500ms()
    {
        var service = new LiveRegionService();
        var received = new List<string>();
        service.Announced += msg => received.Add(msg);

        service.Announce("sample text A");
        service.Announce("sample text A");

        Assert.Single(received);
        Assert.Equal("sample text A", received[0]);
    }

    [Fact]
    public void Announce_allows_different_message_even_within_500ms()
    {
        var service = new LiveRegionService();
        var received = new List<string>();
        service.Announced += msg => received.Add(msg);

        service.Announce("sample text A");
        service.Announce("sample text B");

        Assert.Equal(2, received.Count);
    }
}
