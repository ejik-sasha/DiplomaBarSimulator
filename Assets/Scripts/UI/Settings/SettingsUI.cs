using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider mouseSensitivitySlider;

    [Header("Texts")]
    [SerializeField] private TMP_Text musicValueText;
    [SerializeField] private TMP_Text sfxValueText;
    [SerializeField] private TMP_Text mouseSensitivityValueText;

    [Header("Toggles")]
    [SerializeField] private Toggle fullscreenToggle;

    private void Awake()
    {
        BindUIEvents();
    }

    private void OnEnable()
    {
        LoadValuesToUI();
    }

    private void BindUIEvents()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
            sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        }

        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSensitivityChanged);
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }
    }

    public void LoadValuesToUI()
    {
        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(GameSettings.MusicVolume);

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(GameSettings.SfxVolume);

        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.SetValueWithoutNotify(GameSettings.MouseSensitivity);

        if (fullscreenToggle != null)
            fullscreenToggle.SetIsOnWithoutNotify(GameSettings.Fullscreen);

        UpdateTexts();
    }

    public void OnMusicChanged(float value)
    {
        GameSettings.MusicVolume = value;

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);

        UpdateTexts();
    }

    public void OnSfxChanged(float value)
    {
        GameSettings.SfxVolume = value;

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSfxVolume(value);

        UpdateTexts();
    }

    public void OnMouseSensitivityChanged(float value)
    {
        GameSettings.MouseSensitivity = value;
        UpdateTexts();
    }

    public void OnFullscreenChanged(bool value)
    {
        GameSettings.Fullscreen = value;
        Screen.fullScreen = value;
        UpdateTexts();
    }

    public void ApplyAndSave()
    {
        GameSettings.Save();
    }

    private void UpdateTexts()
    {
        if (musicValueText != null)
        {
            float percent = GameSettings.MusicVolume * 100f;
            musicValueText.text = $"{percent:F0}%";
        }

        if (sfxValueText != null)
        {
            float percent = GameSettings.SfxVolume * 100f;
            sfxValueText.text = $"{percent:F0}%";
        }

        if (mouseSensitivityValueText != null)
        {
            mouseSensitivityValueText.text = $"{GameSettings.MouseSensitivity:F0}";
        }
    }
}