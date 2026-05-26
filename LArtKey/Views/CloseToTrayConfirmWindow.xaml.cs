using System.Windows;

namespace LArtKey.Views;

/// <summary>
/// [English text] English text.
/// [English text] English text.
/// </summary>
public partial class CloseToTrayConfirmWindow : Window
{
    public CloseToTrayConfirmWindow()
    {
        InitializeComponent();

        // English text.
        Loaded += (_, _) => CancelButtonElement.Focus();
    }

    /// <summary>
    /// English text "English text"English text.
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
