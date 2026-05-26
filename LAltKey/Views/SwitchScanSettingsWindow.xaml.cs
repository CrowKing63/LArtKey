using System.Windows;
using LAltKey.Services;
using LAltKey.ViewModels;

namespace LAltKey.Views;

/// <summary>
/// [text] text.
/// </summary>
public partial class SwitchScanSettingsWindow : Window
{
    public SwitchScanSettingsWindow(SettingsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        FocusTracker.Register(this);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
