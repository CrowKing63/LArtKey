using System.Collections.ObjectModel;
using System.Windows;
using WpfApp = System.Windows.Application;
using LArtKey.Models;
using LArtKey.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LArtKey.ViewModels;

/// <summary>
/// [English text] English text.
/// [English text] FullText: English text
/// </summary>
public record ClipboardItem(string FullText, string Preview, bool IsFavorite);

public partial class ClipboardViewModel : ObservableObject
{
    private readonly ClipboardService _clipboardService;
    private readonly InputService _inputService;

    [ObservableProperty]
    private bool isVisible;

    // English text)
    [ObservableProperty]
    private bool isFavoritesTab;

    [ObservableProperty]
    private ObservableCollection<ClipboardItem> items = [];

    public ClipboardViewModel(ClipboardService clipboardService, InputService inputService)
    {
        _clipboardService = clipboardService;
        _inputService = inputService;
        _clipboardService.HistoryChanged += RefreshItems;
        _clipboardService.FavoritesChanged += RefreshItems;
    }

    // ── English text ───────────────────────────────────────────────────────────

    partial void OnIsFavoritesTabChanged(bool value)
    {
        RefreshItems();
    }

    /// <summary>
    /// "English text" English text.
    /// </summary>
    [RelayCommand]
    private void SwitchToHistoryTab()
    {
        IsFavoritesTab = false;
    }

    /// <summary>
    /// "English text" English text.
    /// </summary>
    [RelayCommand]
    private void SwitchToFavoritesTab()
    {
        IsFavoritesTab = true;
    }

    // ── English text ─────────────────────────────────────────────────────────

    private void RefreshItems()
    {
        WpfApp.Current.Dispatcher.Invoke(() =>
        {
            Items.Clear();

            if (IsFavoritesTab)
            {
                // English text
                foreach (var text in _clipboardService.Favorites)
                {
                    Items.Add(new ClipboardItem(text, Preview(text), true));
                }
            }
            else
            {
                // English text)
                foreach (var text in _clipboardService.History)
                {
                    var isFav = _clipboardService.IsFavorite(text);
                    Items.Add(new ClipboardItem(text, Preview(text), isFav));
                }
            }
        });
    }

    // ── English text ──────────────────────────────────────────────────────────────

    /// <summary>
    /// English text.
    /// </summary>
    [RelayCommand]
    private void PasteItem(string text)
    {
        _clipboardService.PasteItem(text);
        // Ctrl+V English text
        _inputService.HandleAction(new SendComboAction(["VK_CONTROL", "VK_V"]));
        IsVisible = false; // English text
    }

    /// <summary>
    /// English text).
    /// </summary>
    [RelayCommand]
    private void ToggleFavorite(string text)
    {
        _clipboardService.ToggleFavorite(text);
    }

    /// <summary>
    /// English text.
    /// </summary>
    [RelayCommand]
    private void ClearHistory()
    {
        _clipboardService.ClearHistory();
        Items.Clear();
    }

    /// <summary>
    /// English text.
    /// </summary>
    [RelayCommand]
    private void TogglePanel()
    {
        IsVisible = !IsVisible;
    }

    /// <summary>
    /// English text.
    /// </summary>
    [RelayCommand]
    private void Close() => IsVisible = false;

    // ── English text ──────────────────────────────────────────────────────────

    private static string Preview(string text)
    {
        // English text
        var singleLine = text.Replace('\n', ' ').Replace('\r', ' ');
        return singleLine.Length <= 40 ? singleLine : singleLine[..37] + "...";
    }
}
