using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LAltKey.Models;

namespace LAltKey.Services;

/// <summary>
/// [text] OpenAI-compatible Chat APItext.
/// [text] text.
/// [text] OpenAI, Ollama, LM Studio, llama.cpp text.
/// </summary>
public class AiService : IDisposable
{
    private readonly ConfigService _configService;
    private readonly HttpClient _httpClient;
    // text.
    private const string OutputOnlyGuardrail =
        "Final answer must contain only the transformed result text. No explanation, no preface, no quotes, no markdown, no labels.";

    public AiService(ConfigService configService)
    {
        _configService = configService;
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// text.
    /// </summary>
    /// <param name="inputText">text</param>
    /// <param name="prompt">text)</param>
    /// <param name="ct">text</param>
    /// <returns>AItext</returns>
    /// <exception cref="AiServiceException">API text</exception>
    public async Task<string> ProcessTextAsync(string inputText, string prompt = "", CancellationToken ct = default)
    {
        var config = _configService.Current;

        // text
        if (string.IsNullOrWhiteSpace(config.AiEndpoint))
            throw new AiServiceException("AI endpoint is missing. Check AI tools settings.");

        var endpoint = NormalizeChatCompletionsEndpoint(config.AiEndpoint);
        if (string.IsNullOrWhiteSpace(config.AiModel))
            throw new AiServiceException("AI model or prompt is missing. Check AI tools settings.");

        // text > text)
        var basePrompt = string.IsNullOrWhiteSpace(prompt)
            ? config.AiDefaultPrompt
            : prompt;

        if (string.IsNullOrWhiteSpace(basePrompt))
            throw new AiServiceException("AI model or prompt is missing. Check AI tools settings.");

        // text.
        var systemPrompt = BuildSystemPromptWithGuardrail(basePrompt);

        // text (OpenAI-compatible Chat Completions)
        var requestBody = new ChatCompletionRequest
        {
            Model = config.AiModel.Trim(),
            Messages =
            [
                new ChatMessage { Role = "system", Content = systemPrompt },
                new ChatMessage { Role = "user", Content = inputText }
            ]
        };

        var json = JsonSerializer.Serialize(requestBody, AiJsonOptions.Default);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // API text)
        var apiKey = SecureStorage.Decrypt(config.AiApiKeyEncrypted);
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };

        if (!string.IsNullOrWhiteSpace(apiKey))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        // text
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(5, config.AiTimeoutSeconds)));

        try
        {
            var response = await _httpClient.SendAsync(request, timeoutCts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                Debug.WriteLine($"[AiService] HTTP {(int)response.StatusCode}: {errorBody}");
                throw new AiServiceException(
                    $"AI API request failed (HTTP {(int)response.StatusCode}): {TruncateErrorMessage(errorBody)}");
            }

            var responseJson = await response.Content.ReadAsStringAsync(ct);
            ChatCompletionResponse? chatResponse;
            try
            {
                chatResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(responseJson, AiJsonOptions.Default);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"[AiService] JSON parse failed: {ex.Message}");
                throw new AiServiceException($"AI response could not be parsed. ({TruncateErrorMessage(ex.Message)})");
            }

            var result = chatResponse?.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrEmpty(result))
                throw new AiServiceException("AI response was empty.");

            return result.Trim();
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !ct.IsCancellationRequested)
        {
            throw new AiServiceException($"AI API request failed {config.AiTimeoutSeconds}text.");
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"[AiService] Request failed: {ex.Message}");
            throw new AiServiceException($"AI Request failed: {ex.Message}");
        }
    }

    /// <summary>
    /// text.
    /// </summary>
    /// <returns>text</returns>
    public async Task<string> TestConnectionAsync(CancellationToken ct = default)
    {
        var result = await ProcessTextAsync("Hello", "Respond with only the word 'OK'.", ct);
        return $"Connection test result: {TruncateErrorMessage(result)}";
    }

    /// <summary>
    /// text.
    /// </summary>
    private static string NormalizeChatCompletionsEndpoint(string raw)
    {
        var t = raw.Trim().TrimEnd('/');
        if (t.Length == 0) return t;
        if (t.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
            return t;
        if (t.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
            return t + "/chat/completions";
        return t + "/v1/chat/completions";
    }

    /// <summary>
    /// text.
    /// </summary>
    private static string BuildSystemPromptWithGuardrail(string basePrompt)
    {
        return $"{basePrompt.Trim()}\n\n{OutputOnlyGuardrail}";
    }

    /// text.
    private static string TruncateErrorMessage(string msg)
    {
        const int maxLen = 200;
        return msg.Length <= maxLen ? msg : msg[..maxLen] + "…";
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

/// <summary>
/// AI response could not be parsed.
/// </summary>
public class AiServiceException : Exception
{
    public AiServiceException(string message) : base(message) { }
}

// ── OpenAI-compatible Chat API DTO ──────────────────────────────────────────

/// text
file class ChatCompletionRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "";

    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; } = [];

    // streamtext.
    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;
}

/// text
file class ChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "";

    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
}

/// text
file class ChatCompletionResponse
{
    [JsonPropertyName("choices")]
    public List<ChatChoice>? Choices { get; set; }
}

/// text
file class ChatChoice
{
    [JsonPropertyName("message")]
    public ChatMessage? Message { get; set; }
}

/// AI tool
file static class AiJsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
