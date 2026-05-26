using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using LArtKey.ViewModels;
using Application = System.Windows.Application;

namespace LArtKey.Services;

/// <summary>
/// [text] text.
/// [text] text.
/// </summary>
public class TrayService : IDisposable
{
    private readonly LayoutService _layoutService; // text
    private readonly MainViewModel _mainViewModel;
    private readonly UpdateService _updateService;
    private readonly InputService _inputService;
    private readonly ConfigService _configService;

    private NotifyIcon _notifyIcon = null!;
    private Window?    _mainWindow;

    public TrayService(LayoutService layoutService, MainViewModel mainViewModel, UpdateService updateService, InputService inputService, ConfigService configService)
    {
        _layoutService = layoutService;
        _mainViewModel = mainViewModel;
        _updateService = updateService;
        _inputService = inputService;
        _configService = configService;

        _layoutService.LayoutsChanged += OnLayoutsChanged;
        _configService.ConfigChanged += OnConfigChanged;
    }

    private void OnLayoutsChanged()
    {
        if (_notifyIcon is not null)
        {
            var menu = BuildContextMenu();
            _notifyIcon.ContextMenuStrip = menu;
        }
    }

    private void OnConfigChanged(string? propertyName)
    {
        if (_notifyIcon is null)
        {
            return;
        }

        if (propertyName is null or nameof(Models.AppConfig.AskBeforeHideToTray))
        {
            _notifyIcon.ContextMenuStrip = BuildContextMenu();
        }
    }

    public void Initialize(Window window)
    {
        _mainWindow = window;

        _notifyIcon = new NotifyIcon
        {
            Text    = "LArtKey",
            Visible = true,
        };

        // text)
        try
        {
            var iconPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "Assets", "icon.ico");
            if (System.IO.File.Exists(iconPath))
                _notifyIcon.Icon = new System.Drawing.Icon(iconPath);
            else
                _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
        }
        catch
        {
            _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
        }

        var menu = BuildContextMenu();
        _notifyIcon.ContextMenuStrip = menu;
        _notifyIcon.DoubleClick     += (_, _) => ToggleVisibility();
    }

    // ── text ────────────────────────────────────────────────────────

    private ContextMenuStrip BuildContextMenu()
    {
        var menu = new ContextMenuStrip();

        // T-9.5: text
        var asmVersion = Assembly.GetExecutingAssembly().GetName().Version;
        var versionText = $"LArtKey v{asmVersion?.ToString(3) ?? "0.1.0"}";
        var versionItem = new ToolStripMenuItem(versionText) { Enabled = false };
        menu.Items.Add(versionItem);

        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Show / hide", null, (_, _) => ToggleVisibility());

        // text "text"text.
        var skipCloseConfirmItem = new ToolStripMenuItem("Hide to tray without confirmation")
        {
            Checked = !_configService.Current.AskBeforeHideToTray,
            CheckOnClick = false
        };
        skipCloseConfirmItem.Click += (_, _) => ToggleCloseConfirmPreference();
        menu.Items.Add(skipCloseConfirmItem);

        // T-5.11: text
        var layoutMenu = new ToolStripMenuItem("Layouts");
        foreach (var name in _layoutService.GetAvailableLayouts())
        {
            var itemName = name; // text
            var item = new ToolStripMenuItem(itemName);
            item.Click += (_, _) =>
                Application.Current.Dispatcher.Invoke(() =>
                    _mainViewModel.SwitchLayout(itemName));
            layoutMenu.DropDownItems.Add(item);
        }
        menu.Items.Add(layoutMenu);

        menu.Items.Add(new ToolStripSeparator());

        // T-9.5: text
        menu.Items.Add("Check for updates", null, async (_, _) => await CheckForUpdateFromTray());

        menu.Items.Add("Settings", null, (_, _) =>
            Application.Current.Dispatcher.Invoke(() =>
                _mainViewModel.Settings.OpenSettingsCommand.Execute(null)));
        menu.Items.Add("Exit", null, (_, _) =>
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Application.Current.MainWindow is MainWindow mw)
                    mw.IsShuttingDown = true;
                Application.Current.Shutdown();
            }));

        return menu;
    }

    // ── T-9.5: text ──────────────────────────────────────

    private async Task CheckForUpdateFromTray()
    {
        try
        {
            var (hasUpdate, version, url, installerUrl) = await _updateService.CheckAsync();

            if (string.IsNullOrEmpty(version))
            {
                _notifyIcon.ShowBalloonTip(3000, "LArtKey", "Could not read the latest release information.", ToolTipIcon.Warning);
                return;
            }

            if (hasUpdate)
            {
                _notifyIcon.ShowBalloonTip(
                    5000,
                    "LArtKey update",
                    $"Version {version} is available.\nOpen settings or GitHub Releases to install it.",
                    ToolTipIcon.Info);

                // text
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _mainViewModel.UpdateVersion = version;
                    _mainViewModel.UpdateUrl = url;
                    _mainViewModel.UpdateInstallerUrl = installerUrl;
                });
            }
            else
            {
                _notifyIcon.ShowBalloonTip(3000, "LArtKey", "LArtKey is up to date.", ToolTipIcon.Info);
            }
        }
        catch (Exception ex)
        {
            _notifyIcon.ShowBalloonTip(3000, "LArtKey", $"Update check failed: {ex.Message}", ToolTipIcon.Error);
        }
    }

    // ── text ──────────────────────────────────────────────────────────

    public void ToggleVisibility()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (_mainWindow is null) return;
            if (_mainWindow.IsVisible)
            {
                ModifierSafety.PrepareForWindowHide(_inputService, "TrayService.ToggleVisibility");
                _mainWindow.Hide();
            }
            else
            {
                _mainWindow.Show();
            }
        });
    }

    // ── text ────────────────────────────────────────────────────────────

    /// <summary>
    /// text.
    /// </summary>
    public void ShowWindowFromExternalActivation()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (_mainWindow is null)
                return;

            if (_mainWindow.WindowState == WindowState.Minimized)
            {
                _mainWindow.WindowState = WindowState.Normal;
            }

            if (!_mainWindow.IsVisible)
            {
                _mainWindow.Show();
            }
        });
    }

    public void ShowBalloon(string message)
    {
        _notifyIcon.ShowBalloonTip(3000, "LArtKey", message, ToolTipIcon.Info);
    }

    /// <summary>
    /// text = text = text.
    /// </summary>
    private void ToggleCloseConfirmPreference()
    {
        var nextAskBeforeHideToTray = !_configService.Current.AskBeforeHideToTray;
        _configService.Update(
            c => c.AskBeforeHideToTray = nextAskBeforeHideToTray,
            nameof(Models.AppConfig.AskBeforeHideToTray));
    }

    public void Dispose() => _notifyIcon?.Dispose();
}
