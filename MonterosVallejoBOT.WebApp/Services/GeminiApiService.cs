using System.Net.Http;
using System.Net.Http.Json; 
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace MonterosVallejoBOT.WebApp.Services;

public class GeminiApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-latest:generateContent";

    public GeminiApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["ApiKeys:Gemini"] ?? throw new InvalidOperationException("API Key for Gemini not found in configuration.");
    }

    public async Task<string?> GetCompletionAsync(string prompt)
    {
        var requestPayload = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        var fullApiUrl = $"{_apiUrl}?key={_apiKey}";

        try
        {
            var jsonPayload = JsonSerializer.Serialize(requestPayload);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(fullApiUrl, httpContent);
            response.EnsureSuccessStatusCode(); 

            var apiResponse = await response.Content.ReadFromJsonAsync<GeminiApiResponse>();

            return apiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"Gemini API request failed: {ex.Message}");
            return $"Error al contactar con Gemini: {ex.Message}";
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"Error deserializing Gemini API response: {ex.Message}");
            return "Error processing Gemini response.";
        }
        catch (Exception ex) // Captura general para otros errores inesperados
        {
            Console.Error.WriteLine($"Unexpected error in GeminiApiService: {ex.Message}");
            return "An unexpected error occurred with Gemini service.";
        }
    }
}

public class GeminiApiResponse
{
    [JsonPropertyName("candidates")]
    public List<GeminiCandidate>? Candidates { get; set; }
    // Podría haber un campo "promptFeedback" también.
}

public class GeminiCandidate
{
    [JsonPropertyName("content")]
    public GeminiContent? Content { get; set; }

    [JsonPropertyName("finishReason")]
    public string? FinishReason { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("safetyRatings")]
    public List<GeminiSafetyRating>? SafetyRatings { get; set; }
}

public class GeminiContent
{
    [JsonPropertyName("parts")]
    public List<GeminiPart>? Parts { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }
}

public class GeminiPart
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public class GeminiSafetyRating
{
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("probability")]
    public string? Probability { get; set; }
}

