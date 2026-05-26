using System.Security.Cryptography;
using System.Text;

namespace LArtKey.Services;

/// <summary>
/// [text] Windows DPAPI(Data Protection API)text.
/// [text] text.
/// [text] Windows text.
/// </summary>
public static class SecureStorage
{
    // text.
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("LArtKey.SecureStorage");

    /// <summary>
    /// text.
    /// </summary>
    /// <param name="plainText">text</param>
    /// <returns>text</returns>
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return "";

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] encrypted = ProtectedData.Protect(
            plainBytes, Entropy, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    /// <summary>
    /// DPAPItext.
    /// </summary>
    /// <param name="base64Encrypted">text</param>
    /// <returns>text</returns>
    public static string Decrypt(string base64Encrypted)
    {
        if (string.IsNullOrEmpty(base64Encrypted)) return "";

        try
        {
            byte[] encrypted = Convert.FromBase64String(base64Encrypted);
            byte[] decrypted = ProtectedData.Unprotect(
                encrypted, Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch (CryptographicException)
        {
            // text
            return "";
        }
        catch (FormatException)
        {
            // Base64 text)
            return "";
        }
    }
}
