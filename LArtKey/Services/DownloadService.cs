using System.IO;
using System.Net.Http;

namespace LArtKey.Services;

/// <summary>T-9.5: text)</summary>
public class DownloadService
{
    private readonly HttpClient _httpClient = new();

    /// <summary>
    /// text.
    /// </summary>
    /// <param name="url">text URL</param>
    /// <param name="destinationPath">text</param>
    /// <param name="progress">text (0.0 ~ 1.0)</param>
    /// <param name="cancellationToken">text</param>
    public async Task DownloadAsync(
        string url,
        string destinationPath,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        var canReportProgress = totalBytes != -1 && progress != null;

        await using var downloadStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var fileStream = new FileStream(
            destinationPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        var buffer = new byte[81920];
        var totalBytesRead = 0L;

        int bytesRead;
        while ((bytesRead = await downloadStream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            totalBytesRead += bytesRead;

            if (canReportProgress)
            {
                progress!.Report((double)totalBytesRead / totalBytes);
            }
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
