using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public abstract class HttpLLMProviderBase : ILLMProvider
{
    public abstract Task<string> GenerateTextAsync(string prompt);

    protected async Task<string> PostJsonAsync(
        string url,
        string json,
        Action<UnityWebRequest> configureRequest = null)
    {
        using UnityWebRequest request = new UnityWebRequest(url, "POST");

        request.certificateHandler = new BypassCertificateHandler();
        request.disposeCertificateHandlerOnDispose = true;

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Accept", "application/json");

        configureRequest?.Invoke(request);

        UnityWebRequestAsyncOperation operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(
                $"HTTP request failed\n" +
                $"URL: {url}\n" +
                $"Error: {request.error}\n" +
                $"Code: {request.responseCode}\n" +
                $"Response: {request.downloadHandler.text}"
            );

            return null;
        }

        return request.downloadHandler.text;
    }

    protected string ExtractText(string responseJson)
    {
        if (string.IsNullOrWhiteSpace(responseJson))
            return null;

        ChatCompletionsResponse response =
            JsonUtility.FromJson<ChatCompletionsResponse>(responseJson);

        if (response?.choices == null || response.choices.Count == 0)
        {
            Debug.LogError($"Cannot parse LLM response: {responseJson}");
            return null;
        }

        return response.choices[0].message.content;
    }
}