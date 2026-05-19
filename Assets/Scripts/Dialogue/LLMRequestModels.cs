using System;
using System.Collections.Generic;

[Serializable]
public class LLMMessage
{
    public string role;
    public string content;

    public LLMMessage(string role, string content)
    {
        this.role = role;
        this.content = content;
    }
}

[Serializable]
public class ChatCompletionsRequest
{
    public string model;
    public List<LLMMessage> messages;
    public float temperature = 0.8f;
    public int max_tokens = 120;
    public bool stream = false;
}

[Serializable]
public class ChatChoiceMessage
{
    public string role;
    public string content;
}

[Serializable]
public class ChatChoice
{
    public int index;
    public ChatChoiceMessage message;
}

[Serializable]
public class ChatCompletionsResponse
{
    public List<ChatChoice> choices;
}

[Serializable]
public class GigaChatTokenResponse
{
    public string access_token;
    public long expires_at;
}