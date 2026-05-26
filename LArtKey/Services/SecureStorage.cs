using System.Security.Cryptography;
using System.Text;

namespace LArtKey.Services;

/// <summary>
/// [English text] Windows DPAPI(Data Protection API)English text.
/// [English text] English text.
/// [English text] Windows English text.
/// </summary>
public static class SecureStorage
{
    // English text.
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("LArtKey.SecureStorage");

    /// <summary>
    /// English text.
    /// </summary>
    /// <param name="plainText">English text</param>
    /// <returns>English text</returns>
    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return "";

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] encrypted = ProtectedData.Protect(
            plainBytes, Entropy, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    /// <summary>
    /// DPAPIEnglish text.
    /// </summary>
    /// <param name="base64Encrypted">English text</param>
    /// <returns>English text</returns>
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
            // English text
            return "";
        }
        catch (FormatException)
        {
            // Base64 English text)
            return "";
        }
    }
}
