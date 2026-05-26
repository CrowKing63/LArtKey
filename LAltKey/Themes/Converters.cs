using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LAltKey.Themes;

/// T-5.2: text(0.0~1.0) → StrokeDashOffset text
/// StrokeDashArray="100" text → progress=0 text offset=100(text), progress=1 text offset=0(text)
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

/// bool → Visibility text)
[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility.Visible;
}

/// T-9.4: bool text (false → Visible)
[ValueConversion(typeof(bool), typeof(Visibility))]
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility.Collapsed;
}

/// T-9.4: EditWidth(double) → text = 50px
[ValueConversion(typeof(double), typeof(double))]
public class WidthToPixelConverter : IValueConverter
{
    private const double Unit = 50.0;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is double w ? Math.Max(Unit * w, 30.0) : 50.0;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// Gap text(double) → text Thickness(0,0,gap*Unit,0) text
[ValueConversion(typeof(double), typeof(Thickness))]
public class GapToRightMarginConverter : IValueConverter
{
    private const double Unit = 50.0;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => new Thickness(0, 0, (value is double g ? g : 0.0) * Unit, 0);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// null text → Collapsed, text Visible
[ValueConversion(typeof(object), typeof(Visibility))]
public class NullToCollapsedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is string s && !string.IsNullOrEmpty(s) ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// bool → text(★/☆) text)
[ValueConversion(typeof(bool), typeof(string))]
public class BoolToStarConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? "★" : "☆";

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is string s && s == "★";
}
