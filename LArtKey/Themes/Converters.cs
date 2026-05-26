using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LArtKey.Themes;

/// T-5.2: English text(0.0~1.0) → StrokeDashOffset English text
/// StrokeDashArray="100" English text → progress=0 English text offset=100(English text), progress=1 English text offset=0(English text)
[ValueConversion(typeof(double), typeof(double))]
public class ProgressToOffsetConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double progress = value is double d ? d : 0.0;
        return 100.0 * (1.0 - Math.Clamp(progress, 0.0, 1.0));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// bool → Visibility English text)
[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility.Visible;
}

/// T-9.4: bool English text (false → Visible)
[ValueConversion(typeof(bool), typeof(Visibility))]
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility.Collapsed;
}

/// T-9.4: EditWidth(double) → English text = 50px
[ValueConversion(typeof(double), typeof(double))]
public class WidthToPixelConverter : IValueConverter
{
    private const double Unit = 50.0;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is double w ? Math.Max(Unit * w, 30.0) : 50.0;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// Gap English text(double) → English text Thickness(0,0,gap*Unit,0) English text
[ValueConversion(typeof(double), typeof(Thickness))]
public class GapToRightMarginConverter : IValueConverter
{
    private const double Unit = 50.0;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => new Thickness(0, 0, (value is double g ? g : 0.0) * Unit, 0);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// null English text → Collapsed, English text Visible
[ValueConversion(typeof(object), typeof(Visibility))]
public class NullToCollapsedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is string s && !string.IsNullOrEmpty(s) ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// bool → English text(★/☆) English text)
[ValueConversion(typeof(bool), typeof(string))]
public class BoolToStarConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? "★" : "☆";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is string s && s == "★";
}
