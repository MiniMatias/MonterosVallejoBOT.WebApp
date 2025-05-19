using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization; 
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging; 

namespace MonterosVallejoBOT.WebApp.Services;

public class OpenAIApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _apiUrl = "https://api.openai.com/v1/chat/completions"; 
    private readonly ILogger<OpenAIApiService> _logger;

    public OpenAIApiService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenAIApiService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["ApiKeys:OpenAI"] ?? throw new InvalidOperationException("API Key for OpenAI not found in configuration.");
        _logger = logger;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<string?> GetCompletionAsync(string prompt)
    {
        string model = "gpt-3.5-turbo";

        var requestPayload = new
        {
            model = model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.7, 
            max_tokens = 1000  
        };

        try
        {
            var jsonPayload = JsonSerializer.Serialize(requestPayload);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            _logger.LogInformation("Enviando solicitud a OpenAI API. Modelo: {Model}, Prompt: {PromptShort}", model, prompt.Substring(0, Math.Min(prompt.Length, 50)) + "...");

            HttpResponseMessage response = await _httpClient.PostAsync(_apiUrl, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error en la solicitud a OpenAI API. Código de estado: {StatusCode}. Respuesta: {ErrorContent}", response.StatusCode, errorContent);
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<OpenAIErrorResponse>(errorContent);
                    return $"Error de OpenAI ({response.StatusCode}): {errorResponse?.Error?.Message ?? "Detalles no disponibles."}";
                }
                catch
                {
                    return $"Error de OpenAI ({response.StatusCode}): {errorContent}";
                }
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<OpenAIResponseStructure>(); 
            _logger.LogInformation("Respuesta recibida de OpenAI API.");
            return apiResponse?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HttpRequestException al llamar a OpenAI API.");
            return $"Error de conexión con OpenAI: {ex.Message}";
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JsonException al deserializar la respuesta de OpenAI API.");
            return "Error procesando la respuesta de OpenAI.";
        }
        catch (Exception ex) // Captura general
        {
            _logger.LogError(ex, "Error inesperado en OpenAIApiService.");
            return "Ocurrió un error inesperado con el servicio de OpenAI.";
        }
    }
}

public class OpenAIResponseStructure
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("choices")]
    public List<OpenAIChoiceStructure>? Choices { get; set; }

    [JsonPropertyName("usage")]
    public OpenAIUsageStructure? Usage { get; set; }

    [JsonPropertyName("system_fingerprint")]
    public string? SystemFingerprint { get; set; }
}

public class OpenAIChoiceStructure
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public OpenAIMessageStructure? Message { get; set; }

    [JsonPropertyName("logprobs")]
    public object? Logprobs { get; set; } 

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

public class OpenAIMessageStructure
{
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

public class OpenAIUsageStructure
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

public class OpenAIErrorResponse
{
    [JsonPropertyName("error")]
    public OpenAIErrorDetails? Error { get; set; }
}

public class OpenAIErrorDetails
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("param")]
    public string? Param { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }
}