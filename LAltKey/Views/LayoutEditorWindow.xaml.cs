using System.Windows;
using LAltKey.Services;
using LAltKey.ViewModels;

namespace LAltKey.Views;

public partial class LayoutEditorWindow : Window
{
    public LayoutEditorWindow(LayoutEditorViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        FocusTracker.Register(this);
    }
}
