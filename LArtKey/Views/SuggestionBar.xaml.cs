namespace LArtKey.Views;

/// <summary>
/// [English text] English text.
/// [English text] English text.
/// </summary>
public partial class SuggestionBar : System.Windows.Controls.UserControl
{
    public SuggestionBar()
    {
        InitializeComponent();
    }

    /// <summary>
    /// English text.
    /// </summary>
    private void CurrentWordSlot_RightClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        e.Handled = true;
    }
}
