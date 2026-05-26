using System.Threading;
namespace LArtKey.Services;

/// <summary>
/// [text] Handles lightweight reload signals between LArtKey.Tools and the main app.
/// [text] text.
/// [text] 1text.
/// </summary>
public sealed class ToolsReloadSignalService : IDisposable
{
    // text.
    private const string ReloadLayoutsEventName = "LArtKey.Tools.ReloadLayouts";
    private const string ReloadUserDictionaryEventName = "LArtKey.Tools.ReloadUserDictionary";
    private const string ReloadBigramDataEventName = "LArtKey.Tools.ReloadBigramData";
    private const string ReloadProfilesEventName = "LArtKey.Tools.ReloadProfiles";
    private const string ReloadAiSettingsEventName = "LArtKey.Tools.ReloadAiSettings";
    private const string ReloadHeaderButtonsEventName = "LArtKey.Tools.ReloadHeaderButtons";

    private readonly LayoutService _layoutService;
    private readonly ConfigService _configService;
    private readonly EnglishDictionary _englishDictionary;

    private readonly EventWaitHandle _reloadLayoutsEvent;
    private readonly EventWaitHandle _reloadUserDictionaryEvent;
    private readonly EventWaitHandle _reloadBigramDataEvent;
    private readonly EventWaitHandle _reloadProfilesEvent;
    private readonly EventWaitHandle _reloadAiSettingsEvent;
    private readonly EventWaitHandle _reloadHeaderButtonsEvent;

    private readonly RegisteredWaitHandle _reloadLayoutsWaitHandle;
    private readonly RegisteredWaitHandle _reloadUserDictionaryWaitHandle;
    private readonly RegisteredWaitHandle _reloadBigramDataWaitHandle;
    private readonly RegisteredWaitHandle _reloadProfilesWaitHandle;
    private readonly RegisteredWaitHandle _reloadAiSettingsWaitHandle;
    private readonly RegisteredWaitHandle _reloadHeaderButtonsWaitHandle;

    public ToolsReloadSignalService(
        ConfigService configService,
        LayoutService layoutService,
        EnglishDictionary englishDictionary)
    {
        _configService = configService;
        _layoutService = layoutService;
        _englishDictionary = englishDictionary;

        _reloadLayoutsEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ReloadLayoutsEventName);
        _reloadUserDictionaryEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ReloadUserDictionaryEventName);
        _reloadBigramDataEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ReloadBigramDataEventName);
        _reloadProfilesEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ReloadProfilesEventName);
        _reloadAiSettingsEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ReloadAiSettingsEventName);
        _reloadHeaderButtonsEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ReloadHeaderButtonsEventName);

        _reloadLayoutsWaitHandle = ThreadPool.RegisterWaitForSingleObject(
            _reloadLayoutsEvent,
            (_, _) => System.Windows.Application.Current.Dispatcher.BeginInvoke(() => _layoutService.NotifyExternalLayoutsChanged()),
            null,
            Timeout.Infinite,
            false);

        _reloadUserDictionaryWaitHandle = ThreadPool.RegisterWaitForSingleObject(
            _reloadUserDictionaryEvent,
            (_, _) => System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                _englishDictionary.ReloadUserWords();
            }),
            null,
            Timeout.Infinite,
            false);

        _reloadBigramDataWaitHandle = ThreadPool.RegisterWaitForSingleObject(
            _reloadBigramDataEvent,
            (_, _) => System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                _englishDictionary.ReloadBigrams();
            }),
            null,
            Timeout.Infinite,
            false);

        _reloadProfilesWaitHandle = ThreadPool.RegisterWaitForSingleObject(
            _reloadProfilesEvent,
            (_, _) => System.Windows.Application.Current.Dispatcher.BeginInvoke(() => _configService.ReloadFromDiskAndNotify(nameof(Models.AppConfig.Profiles))),
            null,
            Timeout.Infinite,
            false);

        _reloadAiSettingsWaitHandle = ThreadPool.RegisterWaitForSingleObject(
            _reloadAiSettingsEvent,
            // AI tool.
            (_, _) => System.Windows.Application.Current.Dispatcher.BeginInvoke(() => _configService.ReloadFromDiskAndNotify(nameof(Models.AppConfig.AiDefaultPrompt))),
            null,
            Timeout.Infinite,
            false);

        _reloadHeaderButtonsWaitHandle = ThreadPool.RegisterWaitForSingleObject(
            _reloadHeaderButtonsEvent,
            (_, _) => System.Windows.Application.Current.Dispatcher.BeginInvoke(() => _configService.ReloadFromDiskAndNotify(nameof(Models.AppConfig.HeaderButtons))),
            null,
            Timeout.Infinite,
            false);
    }

    /// <summary>text.</summary>
    public static void NotifyReloadLayouts() => Signal(ReloadLayoutsEventName);

    /// <summary>text.</summary>
    public static void NotifyReloadUserDictionary() => Signal(ReloadUserDictionaryEventName);

    /// <summary>text.</summary>
    public static void NotifyReloadBigramData() => Signal(ReloadBigramDataEventName);

    /// <summary>text.</summary>
    public static void NotifyReloadProfiles() => Signal(ReloadProfilesEventName);

    /// <summary>AI tool.</summary>
    public static void NotifyReloadAiSettings() => Signal(ReloadAiSettingsEventName);

    /// <summary>text.</summary>
    public static void NotifyReloadHeaderButtons() => Signal(ReloadHeaderButtonsEventName);

    private static void Signal(string eventName)
    {
        try
        {
            using var ev = EventWaitHandle.OpenExisting(eventName);
            ev.Set();
        }
        catch (WaitHandleCannotBeOpenedException)
        {
            // text.
        }
    }

    public void Dispose()
    {
        _reloadLayoutsWaitHandle.Unregister(_reloadLayoutsEvent);
        _reloadUserDictionaryWaitHandle.Unregister(_reloadUserDictionaryEvent);
        _reloadBigramDataWaitHandle.Unregister(_reloadBigramDataEvent);
        _reloadProfilesWaitHandle.Unregister(_reloadProfilesEvent);
        _reloadAiSettingsWaitHandle.Unregister(_reloadAiSettingsEvent);
        _reloadHeaderButtonsWaitHandle.Unregister(_reloadHeaderButtonsEvent);

        _reloadLayoutsEvent.Dispose();
        _reloadUserDictionaryEvent.Dispose();
        _reloadBigramDataEvent.Dispose();
        _reloadProfilesEvent.Dispose();
        _reloadAiSettingsEvent.Dispose();
        _reloadHeaderButtonsEvent.Dispose();
    }
}
