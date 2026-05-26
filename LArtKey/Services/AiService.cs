using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LArtKey.Models;

namespace LArtKey.Services;

/// <summary>
/// [English text] OpenAI-compatible Chat APIEnglish text.
/// [English text] English text.
/// [English text] OpenAI, Ollama, LM Studio, llama.cpp English text.
/// </summary>
public class AiService : IDisposable
{
    private readonly ConfigService _configService;
    private readonly HttpClient _httpClient;
    // English text.
    private const string OutputOnlyGuardrail =
        "Final answer must contain only the transformed result text. No explanation, no preface, no quotes, no markdown, no labels.";

    public AiService(ConfigService configService)
    {
        _configService = configService;
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// English text.
    /// </summary>
    /// <param name="inputText">English text</param>
    /// <param name="prompt">English text)</param>
    /// <param name="ct">English text</param>
    /// <returns>AIEnglish text</returns>
    /// <exception cref="AiServiceException">API English text</exception>
    public async Task<string> ProcessTextAsync(string inputText, string prompt = "", CancellationToken ct = default)
    {
        var config = _configService.Current;

        // English text
        if (string.IsNullOrWhiteSpace(config.AiEndpoint))
            throw new AiServiceException("AI English text → AI English text.");

        var endpoint = NormalizeChatCompletionsEndpoint(config.AiEndpoint);
        if (string.IsNullOrWhiteSpace(config.AiModel))
            throw new AiServiceException("English text → AI English text.");

        // English text > English text)
        var basePrompt = string.IsNullOrWhiteSpace(prompt)
            ? config.AiDefaultPrompt
            : prompt;

        if (string.IsNullOrWhiteSpace(basePrompt))
            throw new AiServiceException("English text → AI English text.");

        // English text.
        var systemPrompt = BuildSystemPromptWithGuardrail(basePrompt);

        // English text (OpenAI-compatible Chat Completions)
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

        // API English text)
        var apiKey = SecureStorage.Decrypt(config.AiApiKeyEncrypted);
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };

        if (!string.IsNullOrWhiteSpace(apiKey))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        // English text
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
                    $"AI API English text (HTTP {(int)response.StatusCode}): {TruncateErrorMessage(errorBody)}");
            }

            var responseJson = await response.Content.ReadAsStringAsync(ct);
            ChatCompletionResponse? chatResponse;
            try
            {
                chatResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(responseJson, AiJsonOptions.Default);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"[AiService] JSON English text: {ex.Message}");
                throw new AiServiceException($"AI English text. ({TruncateErrorMessage(ex.Message)})");
            }

            var result = chatResponse?.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrEmpty(result))
                throw new AiServiceException("AIEnglish text.");

            return result.Trim();
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !ct.IsCancellationRequested)
        {
            throw new AiServiceException($"AI API English text {config.AiTimeoutSeconds}English text.");
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"[AiService] English text: {ex.Message}");
            throw new AiServiceException($"AI English text: {ex.Message}");
        }
    }

    /// <summary>
    /// English text.
    /// </summary>
    /// <returns>English text</returns>
    public async Task<string> TestConnectionAsync(CancellationToken ct = default)
    {
        var result = await ProcessTextAsync("Hello", "Respond with only the word 'OK'.", ct);
        return $"English text: {TruncateErrorMessage(result)}";
    }

    /// <summary>
    /// English text.
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
    /// English text.
    /// </summary>
    private static string BuildSystemPromptWithGuardrail(string basePrompt)
    {
        return $"{basePrompt.Trim()}\n\n{OutputOnlyGuardrail}";
    }

    /// English text.
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
/// AI English text.
/// </summary>
public class AiServiceException : Exception
{
    public AiServiceException(string message) : base(message) { }
}

// ── OpenAI-compatible Chat API DTO ──────────────────────────────────────────

/// English text
file class ChatCompletionRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "";

    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; } = [];

    // streamEnglish text.
    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;
}

/// English text
file class ChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "";

    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
}

/// English text
file class ChatCompletionResponse
{
    [JsonPropertyName("choices")]
    public List<ChatChoice>? Choices { get; set; }
}

/// English text
file class ChatChoice
{
    [JsonPropertyName("message")]
    public ChatMessage? Message { get; set; }
}

/// AI English text
file static class AiJsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}
