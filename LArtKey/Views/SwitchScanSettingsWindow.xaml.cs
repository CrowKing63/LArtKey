using System.Windows;
using LArtKey.Services;
using LArtKey.ViewModels;

namespace LArtKey.Views;

/// <summary>
/// [English text] English text.
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
