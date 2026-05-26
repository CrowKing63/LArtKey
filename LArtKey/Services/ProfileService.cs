using System.Diagnostics;
using System.Runtime.InteropServices;
using LArtKey.Platform;

namespace LArtKey.Services;

/// <summary>
/// [English text] English text.
/// [English text] English text.
/// </summary>
public class ProfileService : IDisposable
{
    private static readonly uint OwnProcessId = (uint)Environment.ProcessId;

    private IntPtr _hook;
    private Win32.WinEventDelegate? _delegateRef; // GC English text

    public event Action<string>? ForegroundAppChanged;

    /// <summary>
    /// LArtKeyEnglish text.
    /// [English text] English text.
    /// </summary>
    public IntPtr LastExternalForegroundHwnd { get; private set; }
    
    /// T-2.10b: English text
    public event Action? ElevatedAppDetected;

    public void Start()
    {
        _delegateRef = OnWinEvent;
        _hook = Win32.SetWinEventHook(
            0x0003,         // EVENT_SYSTEM_FOREGROUND
            0x0003,
            IntPtr.Zero,
            _delegateRef,
            0, 0,
            0x0000);        // WINEVENT_OUTOFCONTEXT
    }

    private void OnWinEvent(IntPtr hook, uint evt, IntPtr hwnd,
        int idObj, int idChild, uint thread, uint time)
    {
        if (hwnd == IntPtr.Zero) return;
        Win32.GetWindowThreadProcessId(hwnd, out var pid);
        try
        {
            if (pid != OwnProcessId)
                LastExternalForegroundHwnd = hwnd;

            using var proc = Process.GetProcessById((int)pid);
            ForegroundAppChanged?.Invoke(proc.ProcessName.ToLower() + ".exe");
            
            // T-2.10b: English text
            if (IsProcessElevated((uint)pid))
            {
                ElevatedAppDetected?.Invoke();
            }
        }
        catch { /* English text */ }
    }

    /// T-2.10b: English text
    private static bool IsProcessElevated(uint processId)
    {
        IntPtr hProcess = Win32.OpenProcess(Win32.PROCESS_QUERY_INFORMATION, false, processId);
        if (hProcess == IntPtr.Zero)
            return false;

        try
        {
            if (!Win32.OpenProcessToken(hProcess, Win32.TOKEN_QUERY, out IntPtr hToken))
                return false;

            try
            {
                // TokenIntegrityLevel English text
                uint dwLength = 0;
                Win32.GetTokenInformation(hToken, Win32.TOKEN_INFORMATION_CLASS.TokenIntegrityLevel, IntPtr.Zero, 0, out dwLength);
                
                if (dwLength == 0 || dwLength > 1024)
                    return false;

                IntPtr pTokenInfo = Marshal.AllocHGlobal((int)dwLength);
                try
                {
                    if (!Win32.GetTokenInformation(hToken, Win32.TOKEN_INFORMATION_CLASS.TokenIntegrityLevel, pTokenInfo, dwLength, out _))
                        return false;

                    var tokenLabel = Marshal.PtrToStructure<Win32.TOKEN_MANDATORY_LABEL>(pTokenInfo);
                    if (tokenLabel.Label.Sid == IntPtr.Zero)
                        return false;

                    // SIDEnglish text
                    IntPtr pSubAuthority = Win32.GetSidSubAuthority(tokenLabel.Label.Sid, 0);
                    if (pSubAuthority == IntPtr.Zero)
                        return false;

                    int integrityLevel = Marshal.ReadInt32(pSubAuthority);
                    
                    // SECURITY_MANDATORY_HIGH_RID (0x3000) English text
                    return integrityLevel >= 0x3000;
                }
                finally
                {
                    Marshal.FreeHGlobal(pTokenInfo);
                }
            }
            finally
            {
                Win32.CloseHandle(hToken);
            }
        }
        catch
        {
            return false;
        }
        finally
        {
            Win32.CloseHandle(hProcess);
        }
    }

    public void Dispose()
    {
        if (_hook != IntPtr.Zero)
            Win32.UnhookWinEvent(_hook);
    }
}
