using System;
using UnityEngine;

[Serializable]
public class LLMSettings
{
    public LLMProviderType ProviderType = LLMProviderType.Mock;

    [Header("Common")]
    public string Model = "GigaChat";

    [TextArea(2, 5)]
    public string SystemPrompt = "Ты NPC-клиент бара. Отвечай кратко, естественно, на русском языке.";

    [Header("OpenRouter")]
    public string ApiKey = "";
    public string BaseUrl = "";

    [Header("OpenRouter optional")]
    public string AppName = "DiplomaBartenderSim";
    public string AppUrl = "https://example.com";

    [Header("Ollama")]
    public string OllamaUrl = "http://localhost:11434/v1/chat/completions";

    [Header("GigaChat")]
    public string GigaChatTokenUrl = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";
    public string GigaChatApiUrl = "https://gigachat.devices.sberbank.ru/api/v1/chat/completions";

    [Tooltip("Authorization Key из личного кабинета GigaChat.")]
    public string GigaChatAuthorizationKey = "";

    public string GigaChatScope = "GIGACHAT_API_PERS";
}