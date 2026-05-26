using LArtKey.Models;

namespace LArtKey.ViewModels;

public class KeyRowViewModel
{
    public IReadOnlyList<KeySlot> Keys { get; init; } = [];
}
