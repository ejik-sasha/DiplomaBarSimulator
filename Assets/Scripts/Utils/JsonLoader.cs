using UnityEngine;

public static class JsonLoader
{
    public static T LoadFromResources<T>(string resourcePath) where T : class
    {
        TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);

        if (textAsset == null)
        {
            Debug.LogError($"JSON file not found in Resources: {resourcePath}");
            return null;
        }

        T data = JsonUtility.FromJson<T>(textAsset.text);

        if (data == null)
        {
            Debug.LogError($"Failed to parse JSON file: {resourcePath}");
        }

        return data;
    }
}