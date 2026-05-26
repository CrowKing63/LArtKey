using System.Windows;
using System.Windows.Input;
using LArtKey.Services;
using LArtKey.ViewModels;

using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace LArtKey.Views;

public partial class UserDictionaryEditorWindow : Window
{
    private readonly UserDictionaryEditorViewModel _vm;

    public UserDictionaryEditorWindow(UserDictionaryEditorViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;
        _vm.OnLoaded();
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
        _vm.OnClosing();
        base.OnClosed(e);
    }
}