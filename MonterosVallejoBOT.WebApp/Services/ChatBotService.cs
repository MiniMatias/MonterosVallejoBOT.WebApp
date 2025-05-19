using Microsoft.Extensions.Logging;
using MonterosVallejoBOT.WebApp.Data;
using MonterosVallejoBOT.WebApp.Models;
using System.Reflection;

namespace MonterosVallejoBOT.WebApp.Services;

public class ChatBotService : IChatBotService
{
    private readonly GeminiApiService _geminiApi;
    private readonly OpenAIApiService _openAIApi; 
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ChatBotService> _logger;

    public ChatBotService(
        GeminiApiService geminiApi,
        OpenAIApiService openAIApi, 
        ApplicationDbContext dbContext,
        ILogger<ChatBotService> logger)
    {
        _geminiApi = geminiApi;
        _openAIApi = openAIApi; 
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<AIResponse> GetResponseAsync(string prompt, AIProvider provider)
    {
        string? content = null;
        DateTime timestamp = DateTime.UtcNow;

        _logger.LogInformation("Solicitando respuesta al proveedor {Provider} con el prompt: {PromptShort}", provider, prompt.Substring(0, Math.Min(prompt.Length, 50)) + "...");

        try
        {
            switch (provider)
            {
                case AIProvider.Gemini:
                    content = await _geminiApi.GetCompletionAsync(prompt);
                    break;
                case AIProvider.OtherFreeAI: 
                    content = await _openAIApi.GetCompletionAsync(prompt); 
                    break;
                default:
                    _logger.LogWarning("Proveedor de IA no soportado: {Provider}", provider);
                    throw new ArgumentOutOfRangeException(nameof(provider), "Proveedor de IA no soportado.");
            }
            _logger.LogInformation("Respuesta recibida de {Provider}", provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo respuesta del proveedor de IA {Provider}", provider);
            content = $"Se produjo un error al contactar al proveedor {provider}. Por favor, inténtalo de nuevo más tarde.";
        }

        return new AIResponse
        {
            Content = content ?? "No se pudo obtener respuesta del proveedor.",
            Timestamp = timestamp,
            Provider = provider
        };
    }

    public async Task StoreResponseAsync(string responseContent, AIProvider provider, string savedBy)
    {
        if (string.IsNullOrWhiteSpace(responseContent) ||
            responseContent.StartsWith("Error", StringComparison.OrdinalIgnoreCase) ||
            responseContent.StartsWith("Se produjo un error", StringComparison.OrdinalIgnoreCase) ||
            responseContent.Contains("OpenAI Error", StringComparison.OrdinalIgnoreCase)) 
        {
            _logger.LogWarning("Intento de guardar respuesta vacía o con error. Contenido: {ResponseContent}. Se omite el guardado.", responseContent);
            return;
        }

        var chatLog = new Models.ChatResponse
        {
            Respuesta = responseContent,
            Fecha = DateTime.UtcNow,
            Proveedor = provider.ToString(), 
            GuardadoPor = savedBy
        };

        try
        {
            _dbContext.ChatResponses.Add(chatLog);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Respuesta de {Provider} guardada en la BD por {User}. ID: {LogId}", provider, savedBy, chatLog.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar la respuesta en la base de datos.");
        }
    }
}