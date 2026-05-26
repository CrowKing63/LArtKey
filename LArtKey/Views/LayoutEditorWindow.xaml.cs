using System.Windows;
using LArtKey.Services;
using LArtKey.ViewModels;

namespace LArtKey.Views;

public partial class LayoutEditorWindow : Window
{
    public LayoutEditorWindow(LayoutEditorViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        FocusTracker.Register(this);
    }
}
