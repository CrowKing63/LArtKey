using System.Windows;
using LArtKey.Services;
using LArtKey.ViewModels;

namespace LArtKey.Views;

/// <summary>
/// [text] text.
/// </summary>
public partial class FocusA11ySettingsWindow : Window
{
    public FocusA11ySettingsWindow(SettingsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        FocusTracker.Register(this);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
