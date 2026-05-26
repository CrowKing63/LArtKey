using System.Windows;

namespace LAltKey.Views;

/// <summary>
/// [text] text.
/// [text] text.
/// </summary>
public partial class CloseToTrayConfirmWindow : Window
{
    public CloseToTrayConfirmWindow()
    {
        InitializeComponent();

        // text.
        Loaded += (_, _) => CancelButtonElement.Focus();
    }

    /// <summary>
    /// text "text"text.
    /// </summary>
    public bool DontAskAgain => DontAskAgainCheckBox.IsChecked == true;

    private void HideToTrayButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
