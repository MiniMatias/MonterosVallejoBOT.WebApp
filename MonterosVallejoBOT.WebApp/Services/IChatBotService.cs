namespace MonterosVallejoBOT.WebApp.Services;

public enum AIProvider
{
    Gemini,
    OtherFreeAI
}

public class AIResponse
{
    public string? Content { get; set; }
    public DateTime Timestamp { get; set; }
    public AIProvider Provider { get; set; }
}

public interface IChatBotService
{
    Task<AIResponse> GetResponseAsync(string prompt, AIProvider provider);
    Task StoreResponseAsync(string responseContent, AIProvider provider, string savedBy);
}
