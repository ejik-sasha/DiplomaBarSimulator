using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip moneyClip;
    [SerializeField] private AudioClip pourClip;
    [SerializeField] private AudioClip iceClip;
    [SerializeField] private AudioClip shakerClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ApplyVolumes();
    }

    private void Start()
    {
        ApplyVolumes();
    }

    public void SetMusicVolume(float value)
    {
        if (musicSource != null)
            musicSource.volume = Mathf.Clamp01(value);
    }

    public void SetSfxVolume(float value)
    {
        if (sfxSource != null)
            sfxSource.volume = Mathf.Clamp01(value);
    }

    public void ApplyVolumes()
    {
        SetMusicVolume(GameSettings.MusicVolume);
        SetSfxVolume(GameSettings.SfxVolume);
        Screen.fullScreen = GameSettings.Fullscreen;
    }

    public void PlayButtonClick()
    {
        PlaySfx(buttonClickClip);
    }

    public void PlayMoney()
    {
        PlaySfx(moneyClip);
    }

    public void PlayPour()
    {
        PlaySfx(pourClip);
    }

    public void PlayIce()
    {
        PlaySfx(iceClip);
    }

    public void PlayShaker()
    {
        PlaySfx(shakerClip);
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, GameSettings.SfxVolume);
    }
}