using System.Windows;
using LAltKey.Services;
using LAltKey.ViewModels;

namespace LAltKey.Views;

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
