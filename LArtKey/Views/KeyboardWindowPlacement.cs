using System.Windows;

namespace LArtKey.Views;

/// <summary>
/// [English text] English text.
/// [English text] English text.
/// </summary>
internal static class KeyboardWindowPlacement
{
    // English text "English text" English text.
    internal const double DockTolerance = 24.0;

    internal enum VerticalAnchor
    {
        Top,
        Freeform,
        Bottom
    }

    /// <summary>
    /// English text.
    /// </summary>
    internal static VerticalAnchor DetectVerticalAnchor(double top, double height, Rect workArea)
    {
        double topGap = top - workArea.Top;
        double bottomGap = workArea.Bottom - (top + height);

        if (bottomGap <= DockTolerance)
            return VerticalAnchor.Bottom;

        if (topGap <= DockTolerance)
            return VerticalAnchor.Top;

        return VerticalAnchor.Freeform;
    }

    /// <summary>
    /// English text.
    /// </summary>
    internal static double ComputeAnchoredTop(
        double currentTop,
        double currentHeight,
        double newHeight,
        Rect workArea,
        VerticalAnchor anchor,
        double? anchorGapOverride = null)
    {
        double topGap = currentTop - workArea.Top;
        double bottomGap = workArea.Bottom - (currentTop + currentHeight);

        double nextTop = anchor switch
        {
            VerticalAnchor.Bottom => workArea.Bottom - (anchorGapOverride ?? bottomGap) - newHeight,
            VerticalAnchor.Top => workArea.Top + (anchorGapOverride ?? topGap),
            _ => currentTop
        };

        double maxTop = Math.Max(workArea.Top, workArea.Bottom - newHeight);
        return Math.Clamp(nextTop, workArea.Top, maxTop);
    }

    /// <summary>
    /// English text.
    /// </summary>
    internal static double ComputePersistedTopForExpandedLaunch(
        double currentTop,
        double currentHeight,
        double expandedHeight,
        Rect workArea,
        VerticalAnchor anchor,
        bool isCollapsed,
        double? anchorGapOverride = null)
    {
        if (!isCollapsed || anchor != VerticalAnchor.Bottom)
            return currentTop;

        return ComputeAnchoredTop(
            currentTop,
            currentHeight,
            expandedHeight,
            workArea,
            anchor,
            anchorGapOverride);
    }
}
