using LArtKey.Models;

namespace LArtKey.Services;

/// <summary>
/// [English text] English text.
/// [English text] English text.
/// </summary>
public static class WindowOpacityProfile
{
    /// <summary>
    /// English text.
    /// </summary>
    public static double GetBaseOpacity(AppConfig config)
    {
        return config.ActiveOpacityEnabled
            ? ClampOpacity(config.OpacityActive)
            : 1.0;
    }

    /// <summary>
    /// English text.
    /// </summary>
    public static double GetIdleOpacity(AppConfig config)
    {
        var baseOpacity = GetBaseOpacity(config);
        return config.IdleOpacityEnabled
            ? Math.Min(ClampOpacity(config.OpacityIdle), baseOpacity)
            : baseOpacity;
    }

    /// <summary>
    /// English text.
    /// </summary>
    public static double GetIdleOpacityMaximum(AppConfig config) => GetBaseOpacity(config);

    /// <summary>
    /// English text.
    /// </summary>
    public static bool ShouldStartIdleTimer(AppConfig config) => config.IdleOpacityEnabled;

    private static double ClampOpacity(double value) => Math.Clamp(value, 0.1, 1.0);
}
