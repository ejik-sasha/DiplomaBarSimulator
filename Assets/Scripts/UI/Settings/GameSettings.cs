using UnityEngine;

public static class GameSettings
{
    public const string MusicVolumeKey = "music_volume";
    public const string SfxVolumeKey = "sfx_volume";
    public const string MouseSensitivityKey = "mouse_sensitivity";
    public const string FullscreenKey = "fullscreen";

    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(MusicVolumeKey, 0.7f);
        set => PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(value));
    }

    public static float SfxVolume
    {
        get => PlayerPrefs.GetFloat(SfxVolumeKey, 0.8f);
        set => PlayerPrefs.SetFloat(SfxVolumeKey, Mathf.Clamp01(value));
    }

    public static float MouseSensitivity
    {
        //get => PlayerPrefs.GetFloat(MouseSensitivityKey, 120f);
        //set => PlayerPrefs.SetFloat(MouseSensitivityKey, Mathf.Clamp(value, 20f, 300f));
    }

    public static bool Fullscreen
    {
        get => PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
        set => PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }
}