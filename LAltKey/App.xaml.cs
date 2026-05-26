using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using LAltKey.Services;
using LAltKey.Services.InputLanguage;
using LAltKey.ViewModels;
using LAltKey.Models;

namespace LAltKey;

/// <summary>
/// [text] LAltKey text.
/// [text] text.
/// </summary>
public partial class App : System.Windows.Application
{
    public static IServiceProvider Services { get; private set; } = null!; // text
    private ConfigService? _configService;

    // T-6.6: text
    private static readonly long _startTick = Environment.TickCount64;

    // text)
    private static Mutex? _instanceMutex;

    // text.
    private static EventWaitHandle? _restoreWindowEvent;

    // text.
    private static RegisteredWaitHandle? _restoreWindowWaitHandle;

    protected override void OnStartup(StartupEventArgs e)
    {
        // text
        const string mutexName = "LAltKey_SingleInstance_Mutex";
        _instanceMutex = new Mutex(initiallyOwned: true, name: mutexName, out bool createdNew);
        if (!createdNew)
        {
            SignalExistingInstanceToRestore();
            _instanceMutex.Dispose();
            _instanceMutex = null;
            Shutdown();
            return;
        }

        // text.
        _restoreWindowEvent = new EventWaitHandle(
            initialState: false,
            mode: EventResetMode.AutoReset,
            name: "LAltKey_RestoreWindow_Event");
        // T-6.7: text)
        // [text] text COMException(0x800401D0~0x800401D3)text
        static bool IsClipboardComError(System.Runtime.InteropServices.COMException ex)
            => ex.ErrorCode >= unchecked((int)0x800401D0) && ex.ErrorCode <= unchecked((int)0x800401D3);

        var _shownErrors = new System.Collections.Generic.HashSet<string>();

        DispatcherUnhandledException += (s, args) =>
        {
            args.Handled = true;

            // text)
            if (args.Exception is System.Runtime.InteropServices.COMException comEx
                && IsClipboardComError(comEx))
            {
                return;
            }

            LogError(args.Exception);
            var key = args.Exception.GetType().FullName + args.Exception.Message;
            if (_shownErrors.Add(key))
            {
                System.Windows.MessageBox.Show(
                    $"Unexpected error:\n{args.Exception.Message}\n\nDetails: laltkey-error.log",
                    "LAltKey error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        };

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                // text
                if (ex is System.Runtime.InteropServices.COMException comEx
                    && IsClipboardComError(comEx))
                {
                    return;
                }
                LogError(ex);
            }
        };

        try
        {
            var services = new ServiceCollection();

            // text
            services.AddSingleton<ConfigService>();
            services.AddSingleton<LayoutService>();
            services.AddSingleton<ILayoutRepository, LayoutRepository>();
            services.AddSingleton<InputService>();
            services.AddSingleton<WindowService>();
            services.AddSingleton<ProfileService>();
            services.AddSingleton<TrayService>();
            services.AddSingleton<ThemeService>();
            services.AddSingleton<HotkeyService>();
            services.AddSingleton<AccessibilityNavigationService>();
            services.AddSingleton<StartupService>();
            services.AddSingleton<SoundService>();
            services.AddSingleton<ClipboardService>();
            services.AddSingleton<UpdateService>();
            // T-9.5: text
            services.AddSingleton<DownloadService>();
            services.AddSingleton<InstallerService>();
            services.AddSingleton<OskLauncherService>();
            // L2: text
            services.AddSingleton<AccessibilityService>();
            // T-9.3: text
            services.AddSingleton<Func<string, WordFrequencyStore>>(_ => lang => new WordFrequencyStore(lang));
            services.AddSingleton<Func<string, BigramFrequencyStore>>(_ => lang => new BigramFrequencyStore(lang));
            services.AddSingleton<EnglishDictionary>();
            services.AddSingleton<EnglishDictionary>();
            services.AddSingleton<IUserDictionaryRepository, UserDictionaryRepository>();
            services.AddSingleton<EnglishInputModule>();
            services.AddSingleton<IInputLanguageModule>(sp => sp.GetRequiredService<EnglishInputModule>());
            services.AddSingleton<AutoCompleteService>();
            // 08: text
            services.AddSingleton<LiveRegionService>();
            // AI tool
            services.AddSingleton<AiService>();
            services.AddSingleton<ToolsReloadSignalService>();

            // ViewModel
            services.AddSingleton<KeyboardViewModel>();
            services.AddSingleton<EmojiViewModel>();
            services.AddSingleton<ClipboardViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<MainViewModel>();
            // T-9.3: text ViewModel
            services.AddSingleton<SuggestionBarViewModel>();
            // text
            services.AddSingleton<MainWindow>();

            Services = services.BuildServiceProvider();

            // text
            var themeService = Services.GetRequiredService<ThemeService>();
            var config       = Services.GetRequiredService<ConfigService>();
            themeService.Apply(config.Current.Theme);

            // text
            _configService = config;
            _configService.ConfigChanged += OnConfigChanged;
            UpdateScaledFontSize();

            // T-5.3: text
            var profileService = Services.GetRequiredService<ProfileService>();
            profileService.Start();

            // T-2.10b: ProfileService → InputService text
            var inputService = Services.GetRequiredService<InputService>();
            profileService.ElevatedAppDetected += () => inputService.NotifyElevatedApp();

            // 06: text OFF
            if (inputService.IsElevated && config.Current.AutoCompleteEnabled)
            {
                config.Current.AutoCompleteEnabled = false;
                config.Save();
            }

            // text
            if (!inputService.IsElevated)
            {
                var targetMode = config.Current.AutoCompleteEnabled ? InputMode.Unicode : InputMode.VirtualKey;
                inputService.TrySetMode(targetMode);
            }

            var window = Services.GetRequiredService<MainWindow>();
            window.Show();

            RegisterRestoreWindowHandler();

            // Warm up the external tools path so settings can open editors quickly.
            _ = Services.GetRequiredService<ToolsReloadSignalService>();

            // L1: text
            Services.GetRequiredService<AccessibilityNavigationService>().Start();

            // T-9.5: text)
            _ = Task.Run(async () =>
            {
                var updateSvc = Services.GetRequiredService<UpdateService>();
                var (hasUpdate, version, url, installerUrl) = await updateSvc.CheckAsync();
                if (hasUpdate)
                {
                    Dispatcher.Invoke(() =>
                    {
                        var vm = Services.GetRequiredService<ViewModels.MainViewModel>();
                        vm.UpdateVersion = version;
                        vm.UpdateUrl = url;
                        vm.UpdateInstallerUrl = installerUrl;
                    });
                }
            });

            // T-6.6: text)
            var elapsed = Environment.TickCount64 - _startTick;
            Debug.WriteLine($"[LAltKey] Startup time: {elapsed}ms");

            // T-6.6: text
            var memMb = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
            Debug.WriteLine($"[LAltKey] Initial managed memory: {memMb:F1} MB");
        }
        catch (Exception ex)
        {
            LogError(ex);
            System.Windows.MessageBox.Show(
                ex.ToString(), "LAltKey Startup Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
            Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            if (Services.GetService<InputService>() is { } inputService)
                ModifierSafety.PrepareForAppExit(inputService, "App.OnExit");
        }
        catch { /* text. */ }

        // TASK-05: text Flush)
        try
        {
            Services.GetService<EnglishDictionary>()?.Flush();
            Services.GetService<EnglishDictionary>()?.Flush();
        }
        catch { /* Flush text) */ }

        // text
        if (Services is IDisposable d) d.Dispose();

        _restoreWindowWaitHandle?.Unregister(null);
        _restoreWindowWaitHandle = null;
        _restoreWindowEvent?.Dispose();
        _restoreWindowEvent = null;

        // text
        _instanceMutex?.ReleaseMutex();
        _instanceMutex?.Dispose();
        _instanceMutex = null;

        base.OnExit(e);
    }

    // L1: text
    private void OnConfigChanged(string? propertyName)
    {
        if (propertyName == nameof(AppConfig.KeyFontScalePercent))
        {
            UpdateScaledFontSize();
        }
    }

    private void UpdateScaledFontSize()
    {
        if (_configService == null) return;

        int scalePercent = _configService.Current.KeyFontScalePercent;
        double baseSize = 13.0; // KeyFontSize in Generic.xaml
        double scaled = baseSize * scalePercent / 100.0;

        // Apply the scaled font size to the application resource.
        // Direct assignment works with merged dictionaries as well.
        var currentApp = System.Windows.Application.Current;
        if (currentApp != null)
        {
            currentApp.Resources["ScaledKeyFontSize"] = scaled;
            currentApp.Resources["ScaledSubLabelFontSize"] = 8.0 * scalePercent / 100.0;
        }
    }

    // T-6.7: text
    // [text] text(%AppData%\LAltKey) text.
    internal static void LogError(Exception ex)
    {
        try
        {
            var logPath = Path.Combine(
                PathResolver.DataDir, "laltkey-error.log");
            File.AppendAllText(logPath,
                $"[{DateTime.Now:u}] {ex}\n\n");
        }
        catch { /* text — text */ }
    }

    /// <summary>
    /// text.
    /// </summary>
    private static void SignalExistingInstanceToRestore()
    {
        try
        {
            using var restoreEvent = EventWaitHandle.OpenExisting("LAltKey_RestoreWindow_Event");
            restoreEvent.Set();
        }
        catch (WaitHandleCannotBeOpenedException)
        {
            // text.
        }
    }

    /// <summary>
    /// text.
    /// </summary>
    private void RegisterRestoreWindowHandler()
    {
        if (_restoreWindowEvent is null)
            return;

        _restoreWindowWaitHandle = ThreadPool.RegisterWaitForSingleObject(
            _restoreWindowEvent,
            static (_, _) =>
            {
                Current.Dispatcher.Invoke(() =>
                {
                    if (Services.GetService<TrayService>() is { } trayService)
                    {
                        trayService.ShowWindowFromExternalActivation();
                    }
                });
            },
            state: null,
            millisecondsTimeOutInterval: Timeout.Infinite,
            executeOnlyOnce: false);
    }
}
