using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace LArtKey.Services;

/// <summary>
/// [English text] English text.
/// </summary>
public class UpdateService
{
    private const string ApiUrl =
        "https://api.github.com/repos/CrowKing63/LArtKey/releases/latest"; // English text.

    public async Task<(bool HasUpdate, string Version, string Url, string InstallerUrl)> CheckAsync()
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("LArtKey");

            var json = await client.GetStringAsync(ApiUrl);
            var doc  = JsonDocument.Parse(json);
            var tag  = doc.RootElement.GetProperty("tag_name").GetString()!;
            var url  = doc.RootElement.GetProperty("html_url").GetString()!;

            var current = Assembly.GetExecutingAssembly().GetName().Version
                          ?? new Version(0, 1, 0);
            var remote  = Version.Parse(tag.TrimStart('v'));

            // T-9.5: English text
            var installerUrl = ExtractInstallerUrl(doc.RootElement);

            return (remote > current, tag, url, installerUrl);
        }
        catch
        {
            return (false, string.Empty, string.Empty, string.Empty);
        }
    }

    /// <summary>GitHub English text</summary>
    private static string ExtractInstallerUrl(JsonElement root)
    {
        try
        {
            if (root.TryGetProperty("assets", out var assets))
            {
                foreach (var asset in assets.EnumerateArray())
                {
                    var name = asset.GetProperty("name").GetString() ?? "";
                    if (name.StartsWith("LArtKey-Setup-") && name.EndsWith(".exe"))
                    {
                        return asset.GetProperty("browser_download_url").GetString()!;
                    }
                }
            }
        }
        catch
        {
            // English text
        }
        return string.Empty;
    }
}
