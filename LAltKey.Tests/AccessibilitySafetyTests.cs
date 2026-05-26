using System.Text.Json;
using System.Threading;
using LAltKey.Controls;
using LAltKey.Models;
using LAltKey.Platform;
using LAltKey.Services;

namespace LAltKey.Tests;

public class AccessibilitySafetyTests
{
    [Fact]
    public void AppConfig_KeyRepeatEnabled_default_is_off()
    {
        var config = new AppConfig();
        Assert.False(config.KeyRepeatEnabled);
    }

    [Fact]
    public void AppConfig_KeyboardA11yNavigationEnabled_default_is_off()
    {
        var config = new AppConfig();
        Assert.False(config.KeyboardA11yNavigationEnabled);
    }

    [Fact]
    public void AppConfig_KeyboardA11yNavigationEnabled_survives_json_round_trip()
    {
        var original = new AppConfig
        {
            KeyboardA11yNavigationEnabled = true
        };

        var json = JsonSerializer.Serialize(original, JsonOptions.Default);
        var restored = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions.Default);

        Assert.NotNull(restored);
        Assert.True(restored!.KeyboardA11yNavigationEnabled);
    }

    [Fact]
    public void AppConfig_SwitchScan_defaults_match_accessibility_spec()
    {
        var config = new AppConfig();

        Assert.Equal("VK_TAB", config.SwitchScanNextKey);
        Assert.Equal("VK_RETURN", config.SwitchScanSelectKey);
        Assert.Equal("VK_SPACE", config.SwitchScanSecondarySelectKey);
        Assert.Equal("", config.SwitchScanPreviousKey);
        Assert.Equal("", config.SwitchScanPauseKey);
        Assert.Equal(SwitchScanMode.Linear, config.SwitchScanMode);
        Assert.True(config.SwitchScanWrapEnabled);
        Assert.True(config.SwitchScanIncludeSuggestions);
    }

    [Fact]
    public void ReleaseDefaults_do_not_include_korean_language_config()
    {
        var configPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "LAltKey", "config.json"));

        var json = File.ReadAllText(configPath);

        Assert.DoesNotContain("\"Language\": \"ko\"", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DefaultHeaderButtons_do_not_include_os_ime_button()
    {
        var defaults = HeaderButtonConfig.CreateDefaults();

        Assert.DoesNotContain(defaults, button => button.Id == HeaderButtonConfig.IdOsIme);
    }

    [Fact]
    public void InjectedInputMarker_is_unique_to_laltkey()
    {
        const ulong altKeyMarker = 0xA17A11UL;

        Assert.NotEqual(altKeyMarker, Win32.INPUT_EXTRAINFO_LALTKEY);
    }

    [Fact]
    public void AppConfig_SwitchScan_settings_survive_json_round_trip()
    {
        var original = new AppConfig
        {
            SwitchScanMode = SwitchScanMode.Manual,
            SwitchScanInitialDelayMs = 1200,
            SwitchScanSelectPauseMs = 700,
            SwitchScanCyclesBeforePause = 3,
            SwitchScanWrapEnabled = false,
            SwitchScanNextKey = "VK_F8",
            SwitchScanSelectKey = "VK_F9",
            SwitchScanSecondarySelectKey = "VK_F10",
            SwitchScanPreviousKey = "VK_F7",
            SwitchScanPauseKey = "VK_F6",
            SwitchScanIncludeSuggestions = false,
            SwitchScanSuggestionPriority = SwitchScanSuggestionPriority.AfterKeyboard
        };

        var json = JsonSerializer.Serialize(original, JsonOptions.Default);
        var restored = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions.Default);

        Assert.NotNull(restored);
        Assert.Equal(SwitchScanMode.Manual, restored!.SwitchScanMode);
        Assert.Equal(1200, restored.SwitchScanInitialDelayMs);
        Assert.Equal(700, restored.SwitchScanSelectPauseMs);
        Assert.Equal(3, restored.SwitchScanCyclesBeforePause);
        Assert.False(restored.SwitchScanWrapEnabled);
        Assert.Equal("VK_F8", restored.SwitchScanNextKey);
        Assert.Equal("VK_F9", restored.SwitchScanSelectKey);
        Assert.Equal("VK_F10", restored.SwitchScanSecondarySelectKey);
        Assert.Equal("VK_F7", restored.SwitchScanPreviousKey);
        Assert.Equal("VK_F6", restored.SwitchScanPauseKey);
        Assert.False(restored.SwitchScanIncludeSuggestions);
        Assert.Equal(SwitchScanSuggestionPriority.AfterKeyboard, restored.SwitchScanSuggestionPriority);
    }

    [Fact]
    public void ConfigService_migrates_legacy_default_layout_names()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"LAltKey-ConfigMigration-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            PathResolver.OverrideDataDir(tempDir);
            var legacyJson = """
                {
                  "DefaultLayout": "Bagic",
                  "Profiles": {
                    "notepad.exe": "Bagic Plus",
                    "calc.exe": "Bagic"
                  }
                }
                """;
            File.WriteAllText(PathResolver.ConfigPath, legacyJson);

            var service = new ConfigService();

            Assert.Equal("Basic", service.Current.DefaultLayout);
            Assert.Equal("Basic Plus", service.Current.Profiles["notepad.exe"]);
            Assert.Equal("Basic", service.Current.Profiles["calc.exe"]);
        }
        finally
        {
            PathResolver.OverrideDataDir(null);
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ConfigService_hides_header_buttons_that_exceed_side_limit()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"LAltKey-HeaderButtons-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            PathResolver.OverrideDataDir(tempDir);

            var config = new AppConfig
            {
                HeaderButtons = HeaderButtonConfig.CreateDefaults()
            };

            for (int i = 0; i < 4; i++)
            {
                config.HeaderButtons.Add(new HeaderButtonConfig
                {
                    Id = $"custom-test-{i}",
                    Kind = HeaderButtonKind.Custom,
                    Visible = true,
                    Position = "Right",
                    DisplayMode = HeaderButtonDisplayMode.IconOnly,
                    IconText = $"C{i}",
                    Tooltip = $"sample text {i}",
                    AccessibleName = $"sample text {i}",
                    CustomAction = new SendKeyAction("VK_A")
                });
            }

            File.WriteAllText(
                PathResolver.ConfigPath,
                JsonSerializer.Serialize(config, JsonOptions.Default));

            var service = new ConfigService();

            var visibleRightButtons = service.Current.HeaderButtons
                .Where(button => button.Visible && HeaderButtonConfig.NormalizePosition(button.Position) == "Right")
                .ToList();

            Assert.Equal(HeaderButtonConfig.MaxVisibleButtonsRight, visibleRightButtons.Count);
            Assert.False(service.Current.HeaderButtons.Last().Visible);
        }
        finally
        {
            PathResolver.OverrideDataDir(null);
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void KeyButton_KeyboardA11yNavigationEnabled_toggles_tab_navigation_flags()
    {
        Exception? captured = null;

        var thread = new Thread(() =>
        {
            try
            {
                var button = new KeyButton();

                button.KeyboardA11yNavigationEnabled = true;

                Assert.True(button.Focusable);
                Assert.True(button.IsTabStop);

                button.KeyboardA11yNavigationEnabled = false;

                Assert.False(button.Focusable);
                Assert.False(button.IsTabStop);
            }
            catch (Exception ex)
            {
                captured = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (captured is not null)
            throw captured;
    }

    [Fact]
    public void BasicLayout_WinKey_uses_toggle_sticky()
    {
        var layoutPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "LAltKey", "layouts", "Basic.json"));

        var json = File.ReadAllText(layoutPath);
        var layout = JsonSerializer.Deserialize<LayoutConfig>(json, JsonOptions.Default);

        Assert.NotNull(layout);
        var allKeys = layout!.Columns!
            .SelectMany(c => c.Rows!.SelectMany(r => r.Keys));

        var winKey = allKeys.First(k => k.Label == "Win");

        var action = Assert.IsType<ToggleStickyAction>(winKey.Action);
        Assert.Equal("VK_LWIN", action.Vk);
    }

    [Theory]
    [InlineData("Basic.json")]
    [InlineData("Basic Plus.json")]
    public void StockLayouts_do_not_ship_ime_or_input_mode_keys(string fileName)
    {
        var layoutPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "LAltKey", "layouts", fileName));

        var json = File.ReadAllText(layoutPath);

        Assert.DoesNotContain("VK_HANGUL", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("VK_HANJA", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ToggleInputMode", json, StringComparison.OrdinalIgnoreCase);
    }
}
