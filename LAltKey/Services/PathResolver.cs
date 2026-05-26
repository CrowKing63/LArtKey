using System.IO;

namespace LAltKey.Services;

public static class PathResolver
{
    private static readonly string _exeDir =
        Path.GetDirectoryName(Environment.ProcessPath ?? "") ?? "";
    private static string? _overrideDataDir;

    /// <summary>exe text</summary>
    public static bool IsPortable =>
        File.Exists(Path.Combine(_exeDir, "config.json"));

    /// <summary>
    /// text.
    /// </summary>
    public static void OverrideDataDir(string? dataDir)
    {
        _overrideDataDir = string.IsNullOrWhiteSpace(dataDir) ? null : dataDir;
    }

    public static string DataDir => !string.IsNullOrWhiteSpace(_overrideDataDir)
        ? _overrideDataDir
        : IsPortable
            ? _exeDir
            : Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "LAltKey");

    public static string LayoutsDir => Path.Combine(DataDir, "layouts");
    public static string ConfigPath  => Path.Combine(DataDir, "config.json");

    /// <summary>
    /// LAltKey.Tools text.
    /// [text] text.
    /// [text] text.
    /// </summary>
    public static string ToolsExePath
    {
        get
        {
            var sameDirectory = Path.Combine(_exeDir, "LAltKey.Tools.exe");
            if (File.Exists(sameDirectory))
            {
                return sameDirectory;
            }

            var toolsSubDirectory = Path.Combine(_exeDir, "Tools", "LAltKey.Tools.exe");
            if (File.Exists(toolsSubDirectory))
            {
                return toolsSubDirectory;
            }

            // text.
            var projectRoot = Directory.GetParent(_exeDir)?.Parent?.Parent?.Parent?.FullName;
            if (!string.IsNullOrEmpty(projectRoot))
            {
                var configurationName = new DirectoryInfo(_exeDir).Parent?.Parent?.Name;
                var tfmName = new DirectoryInfo(_exeDir).Parent?.Name;
                if (!string.IsNullOrEmpty(configurationName) && !string.IsNullOrEmpty(tfmName))
                {
                    var developmentPath = Path.Combine(
                        projectRoot,
                        "LAltKey.Tools",
                        "bin",
                        configurationName,
                        tfmName,
                        "LAltKey.Tools.exe");

                    if (File.Exists(developmentPath))
                    {
                        return developmentPath;
                    }
                }
            }

            return sameDirectory;
        }
    }
}
