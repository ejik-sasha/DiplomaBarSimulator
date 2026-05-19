using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class GigaChatProvider : HttpLLMProviderBase
{
    private readonly LLMSettings _settings;

    private string _accessToken;
    private long _expiresAt;

    public GigaChatProvider(LLMSettings settings)
    {
        _settings = settings;
    }

    public override async Task<string> GenerateTextAsync(string prompt)
    {
        bool tokenReady = await EnsureAccessTokenAsync();

        if (!tokenReady)
        {
            Debug.LogError("GigaChat: access token was not received.");
            return null;
        }

        string model = string.IsNullOrWhiteSpace(_settings.Model)
            ? "GigaChat"
            : _settings.Model;

        ChatCompletionsRequest payload = new ChatCompletionsRequest
        {
            model = model,
            messages = new List<LLMMessage>
            {
                new LLMMessage("system", _settings.SystemPrompt),
                new LLMMessage("user", prompt)
            },
            temperature = 0.8f,
            max_tokens = 120,
            stream = false
        };

        string json = JsonUtility.ToJson(payload);

        string responseJson = await PostJsonAsync(
            _settings.GigaChatApiUrl,
            json,
            request =>
            {
                request.SetRequestHeader("Authorization", $"Bearer {_accessToken}");
            }
        );

        return ExtractText(responseJson);
    }

    private async Task<bool> EnsureAccessTokenAsync()
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        if (!string.IsNullOrWhiteSpace(_accessToken) && now < _expiresAt - 60_000)
            return true;

        return await RequestAccessTokenAsync();
    }

    private async Task<bool> RequestAccessTokenAsync()
    {
        if (string.IsNullOrWhiteSpace(_settings.GigaChatAuthorizationKey))
        {
            Debug.LogError("GigaChat Authorization Key is empty.");
            return false;
        }

        using UnityWebRequest request = new UnityWebRequest(_settings.GigaChatTokenUrl, "POST");
        
        request.certificateHandler = new BypassCertificateHandler();
        request.disposeCertificateHandlerOnDispose = true;

        string body = $"scope={Uri.EscapeDataString(_settings.GigaChatScope)}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(body);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        request.SetRequestHeader("Accept", "application/json");
        request.SetRequestHeader("RqUID", Guid.NewGuid().ToString());
        request.SetRequestHeader("Authorization", $"Basic {_settings.GigaChatAuthorizationKey}");

        UnityWebRequestAsyncOperation operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(
                $"GigaChat token request failed\n" +
                $"Error: {request.error}\n" +
                $"Code: {request.responseCode}\n" +
                $"Response: {request.downloadHandler.text}"
            );

            return false;
        }

        string responseText = request.downloadHandler.text;

        GigaChatTokenResponse tokenResponse =
            JsonUtility.FromJson<GigaChatTokenResponse>(responseText);

        if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.access_token))
        {
            Debug.LogError($"GigaChat token parse failed: {responseText}");
            return false;
        }

        _accessToken = tokenResponse.access_token;
        _expiresAt = tokenResponse.expires_at;

        Debug.Log("GigaChat access token received successfully.");

        return true;
    }
}