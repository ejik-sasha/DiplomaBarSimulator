using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class OpenRouterProvider : HttpLLMProviderBase
{
    private readonly LLMSettings _settings;

    public OpenRouterProvider(LLMSettings settings)
    {
        _settings = settings;
    }

    public override async Task<string> GenerateTextAsync(string prompt)
    {
        ChatCompletionsRequest payload = new ChatCompletionsRequest
        {
            model = string.IsNullOrWhiteSpace(_settings.Model) ? "openrouter/free" : _settings.Model,
            messages = new List<LLMMessage>
            {
                new LLMMessage("system", _settings.SystemPrompt),
                new LLMMessage("user", prompt)
            },
            temperature = 0.8f,
            max_tokens = 120
        };

        string json = JsonUtility.ToJson(payload);

        string responseJson = await PostJsonAsync(
            string.IsNullOrWhiteSpace(_settings.BaseUrl)
                ? "https://openrouter.ai/api/v1/chat/completions"
                : _settings.BaseUrl,
            json,
            request =>
            {
                request.SetRequestHeader("Authorization", $"Bearer {_settings.ApiKey}");
                request.SetRequestHeader("HTTP-Referer", _settings.AppUrl);
                request.SetRequestHeader("X-Title", _settings.AppName);
            });

        if (string.IsNullOrWhiteSpace(responseJson))
            return null;

        ChatCompletionsResponse response = JsonUtility.FromJson<ChatCompletionsResponse>(responseJson);

        if (response?.choices == null || response.choices.Count == 0)
            return null;

        return response.choices[0].message.content;
    }
}