using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LArtKey.Services;
using LArtKey.ViewModels;

using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace LArtKey.Views;

/// <summary>
/// English text.
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _vm;

    public SettingsWindow(SettingsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;

        // English text.
        Loaded += (_, _) => FocusFirstControlInSelectedTab();

        // English text.
        FocusTracker.Register(this);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    protected override void OnKeyDown(WpfKeyEventArgs e)
    {
        if (e.Key == Key.Escape) { Close(); return; }
        base.OnKeyDown(e);
    }

    protected override void OnClosed(System.EventArgs e)
    {
        _vm.OnSettingsWindowClosed();
        base.OnClosed(e);
    }

    private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

    /// <summary>
    /// English text.
    /// </summary>
    private void SettingsTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!ReferenceEquals(sender, SettingsTabControl))
        {
            return;
        }

        if (e.Source is not System.Windows.Controls.TabControl)
        {
            return;
        }

        FocusFirstControlInSelectedTab();
    }

    private void FocusFirstControlInSelectedTab()
    {
        if (SettingsTabControl?.SelectedItem is not TabItem tab)
        {
            return;
        }

        // English text.
        FrameworkElement? primary = SettingsTabControl.SelectedIndex switch
        {
            0 => AppearanceFirstFocusable,
            1 => TopBarFirstFocusable,
            2 => BehaviorFirstFocusable,
            3 => A11yFirstFocusable,
            5 => AdvancedFirstFocusable,
            _ => null
        };

        if (primary is { IsVisible: true, Focusable: true } && primary.Focus())
        {
            return;
        }

        if (tab.Content is DependencyObject root && root is UIElement element)
        {
            var request = new TraversalRequest(FocusNavigationDirection.First);
            element.MoveFocus(request);
        }
    }
}
