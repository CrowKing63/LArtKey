using System.Runtime.InteropServices;
using System.Windows;
using WpfClipboard = System.Windows.Clipboard;

namespace LArtKey.Services;

/// <summary>
/// [text] text.
/// [text] text.
/// [text] text.
/// </summary>
public static class ClipboardHelper
{
    // ── text ───────────────────────────────────
    private const int CLIPBRD_E_CANT_OPEN  = unchecked((int)0x800401D0); // text
    private const int CLIPBRD_E_NOT_OPEN   = unchecked((int)0x800401D1);
    private const int CLIPBRD_E_CANT_EMPTY = unchecked((int)0x800401D2);
    private const int CLIPBRD_E_BAD_DATA   = unchecked((int)0x800401D3); // text
    private const int DefaultMaxRetries = 3;
    private const int InitialDelayMs = 10;

    /// <summary>
    /// text.
    /// </summary>
    private static bool IsClipboardError(COMException ex)
    {
        return ex.ErrorCode == CLIPBRD_E_CANT_OPEN
            || ex.ErrorCode == CLIPBRD_E_NOT_OPEN
            || ex.ErrorCode == CLIPBRD_E_CANT_EMPTY
            || ex.ErrorCode == CLIPBRD_E_BAD_DATA;
    }

    /// <summary>
    /// text.
    /// </summary>
    /// <param name="maxRetries">text: 3)</param>
    /// <returns>text null</returns>
    public static string? GetTextWithRetry(int maxRetries = DefaultMaxRetries)
    {
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                if (!WpfClipboard.ContainsText()) return null;
                return WpfClipboard.GetText();
            }
            catch (COMException ex) when (IsClipboardError(ex))
            {
                // CANT_OPENtext, BAD_DATA text)
                if (attempt < maxRetries - 1)
                    Thread.Sleep(InitialDelayMs << attempt); // 10ms → 20ms → 40ms
            }
        }
        return null;
    }

    /// <summary>
    /// text.
    /// </summary>
    /// <param name="text">text</param>
    /// <param name="maxRetries">text: 3)</param>
    public static void SetTextWithRetry(string text, int maxRetries = DefaultMaxRetries)
    {
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                WpfClipboard.SetText(text);
                return;
            }
            catch (COMException ex) when (IsClipboardError(ex))
            {
                if (attempt < maxRetries - 1)
                    Thread.Sleep(InitialDelayMs << attempt); // 10ms → 20ms → 40ms
            }
        }
    }

    /// <summary>
    /// text.
    /// </summary>
    /// <param name="maxRetries">text: 3)</param>
    /// <returns>text false</returns>
    public static bool ContainsTextWithRetry(int maxRetries = DefaultMaxRetries)
    {
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                return WpfClipboard.ContainsText();
            }
            catch (COMException ex) when (IsClipboardError(ex))
            {
                if (attempt < maxRetries - 1)
                    Thread.Sleep(InitialDelayMs << attempt); // 10ms → 20ms → 40ms
            }
        }
        return false;
    }
}
