using LAltKey.Models;

namespace LAltKey.ViewModels;

public class KeyRowViewModel
{
    public IReadOnlyList<KeySlot> Keys { get; init; } = [];
}
