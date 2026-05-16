using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("References")]
    [SerializeField] private GameSaveManager saveManager;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool _isPaused;

    private void Awake()
    {
        if (saveManager == null)
            saveManager = FindFirstObjectByType<GameSaveManager>();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused)
                ResumeGame();
            else
                OpenPauseMenu();
        }
    }

    public void OpenPauseMenu()
    {
        _isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        CursorLockController.SetUIOpen(true);
    }

    public void ResumeGame()
    {
        _isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        CursorLockController.SetUIOpen(false);
    }

    public void OpenSettings()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void BackToPause()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    public void SaveGame()
    {
        if (saveManager != null)
            saveManager.SaveGame();
    }

    public void LoadGame()
    {
        if (saveManager != null)
            saveManager.LoadGameIfExists();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        //CursorLockController.SetUIOpen(false);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}