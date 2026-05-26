namespace LArtKey.Views;

/// <summary>
/// [text] text.
/// [text] text.
/// </summary>
public partial class SuggestionBar : System.Windows.Controls.UserControl
{
    public SuggestionBar()
    {
        InitializeComponent();
    }

    /// <summary>
    /// text.
    /// </summary>
    private void CurrentWordSlot_RightClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        e.Handled = true;
    }
}
