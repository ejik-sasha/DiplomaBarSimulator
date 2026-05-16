using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine; 

public class OllamaProvider : HttpLLMProviderBase
{
    private readonly LLMSettings _settings;

    public OllamaProvider(LLMSettings settings)
    {
        _settings = settings;
    }

    public override async Task<string> GenerateTextAsync(string prompt)
    {
        ChatCompletionsRequest payload = new ChatCompletionsRequest
        {
            model = string.IsNullOrWhiteSpace(_settings.Model) ? "llama3.1" : _settings.Model,
            messages = new List<LLMMessage>
            {
                new LLMMessage("system", _settings.SystemPrompt),
                new LLMMessage("user", prompt)
            },
            temperature = 0.8f,
            max_tokens = 120
        };

        string json = JsonUtility.ToJson(payload);

        string responseJson = await PostJsonAsync(_settings.OllamaUrl, json);

        if (string.IsNullOrWhiteSpace(responseJson))
            return null;

        ChatCompletionsResponse response = JsonUtility.FromJson<ChatCompletionsResponse>(responseJson);

        if (response?.choices == null || response.choices.Count == 0)
            return null;

        return response.choices[0].message.content;
    }
}