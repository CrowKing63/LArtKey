using System.Windows;
using LAltKey.Services;
using System.Windows.Threading;
using LAltKey.ViewModels;
using LAltKey.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LAltKey.Tools;

/// <summary>
/// [text] LAltKey text.
/// [text] text.
/// </summary>
public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public App()
    {
        // text.
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);

            // text.
            PathResolver.OverrideDataDir(GetDataDirArgument(e.Args));

            var services = new ServiceCollection();

            // [text] text.
            services.AddSingleton<ConfigService>();
            services.AddSingleton<ThemeService>();
            services.AddSingleton<LayoutService>();
            services.AddSingleton<ILayoutRepository, LayoutRepository>();
            services.AddSingleton<LayoutEditorViewModel>();

            // [text] text.
            services.AddSingleton<Func<string, WordFrequencyStore>>(_ => lang => new WordFrequencyStore(lang));
            services.AddSingleton<Func<string, BigramFrequencyStore>>(_ => lang => new BigramFrequencyStore(lang));
            services.AddSingleton<EnglishDictionary>();
            services.AddSingleton<EnglishDictionary>();
            services.AddSingleton<IUserDictionaryRepository, UserDictionaryRepository>();
            services.AddSingleton<UserDictionaryEditorViewModel>();

            Services = services.BuildServiceProvider();
            ApplyInitialTheme();

            // text.
            var directToolWindow = CreateDirectToolWindow(e.Args);
            if (directToolWindow is not null)
            {
                MainWindow = directToolWindow;
                directToolWindow.Show();
                return;
            }

            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            ShowFatalStartupError(ex);
            Shutdown(-1);
        }
    }

    /// <summary>
    /// text.
    /// </summary>
    private static void ApplyInitialTheme()
    {
        var configService = Services.GetRequiredService<ConfigService>();
        var themeService = Services.GetRequiredService<ThemeService>();
        themeService.Apply(configService.Current.Theme);
    }

    /// <summary>
    /// text: layout, dictionary, profile, ai-prompt, header-shortcut
    /// </summary>
    private static Window? CreateDirectToolWindow(string[] args)
    {
        var toolName = GetToolArgument(args);
        if (string.IsNullOrWhiteSpace(toolName))
        {
            return null;
        }

        if (string.Equals(toolName, "layout", StringComparison.OrdinalIgnoreCase))
        {
            var vm = Services.GetRequiredService<LayoutEditorViewModel>();
            return new LayoutEditorWindow(vm);
        }

        if (string.Equals(toolName, "dictionary", StringComparison.OrdinalIgnoreCase))
        {
            var vm = Services.GetRequiredService<UserDictionaryEditorViewModel>();
            return new UserDictionaryEditorWindow(vm);
        }

        if (string.Equals(toolName, "profile", StringComparison.OrdinalIgnoreCase))
        {
            return new ProfileMappingEditorWindow();
        }

        if (string.Equals(toolName, "ai-prompt", StringComparison.OrdinalIgnoreCase))
        {
            return new AiPromptEditorWindow();
        }

        if (string.Equals(toolName, "header-shortcut", StringComparison.OrdinalIgnoreCase))
        {
            var headerButtonId = GetArgumentValue(args, "--header-button-id");
            var isCreateMode = string.Equals(GetArgumentValue(args, "--header-button-mode"), "create", StringComparison.OrdinalIgnoreCase);
            return new HeaderShortcutEditorWindow(headerButtonId, isCreateMode);
        }

        return null;
    }

    /// <summary>
    /// "--tool {name}" text.
    /// </summary>
    private static string? GetToolArgument(string[] args)
    {
        if (args is null || args.Length == 0)
        {
            return null;
        }

        for (var i = 0; i < args.Length; i++)
        {
            if (!string.Equals(args[i], "--tool", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }

        return null;
    }

    /// <summary>
    /// "--data-dir {absolute-path}" text.
    /// </summary>
    private static string? GetDataDirArgument(string[] args)
    {
        if (args is null || args.Length == 0)
        {
            return null;
        }

        for (var i = 0; i < args.Length; i++)
        {
            if (!string.Equals(args[i], "--data-dir", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }

        return null;
    }

    private static string? GetArgumentValue(string[] args, string optionName)
    {
        if (args is null || args.Length == 0)
        {
            return null;
        }

        for (var i = 0; i < args.Length; i++)
        {
            if (!string.Equals(args[i], optionName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }

        return null;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // text.
        Services.GetService<IUserDictionaryRepository>()?.Flush();

        if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }

        base.OnExit(e);
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        ShowFatalStartupError(e.Exception);
        e.Handled = true;
        Shutdown(-1);
    }

    private void OnCurrentDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            ShowFatalStartupError(ex);
        }
    }

    private static void ShowFatalStartupError(Exception ex)
    {
        MessageBox.Show(
            "LAltKey.Tools failed to start.\n\n" + ex,
            "LAltKey.Tools error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}
