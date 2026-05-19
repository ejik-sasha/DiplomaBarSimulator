public static class LLMProviderFactory
{
    public static ILLMProvider Create(LLMSettings settings)
    {
        return settings.ProviderType switch
        {
            LLMProviderType.OpenRouter => new OpenRouterProvider(settings),
            LLMProviderType.Ollama => new OllamaProvider(settings),
            LLMProviderType.GigaChat => new GigaChatProvider(settings),
            _ => new MockLLMProvider()
        };
    }
}