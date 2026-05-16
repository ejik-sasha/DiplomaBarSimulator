using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;

    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "Game";

    private void Start()
    {
        ShowMainMenu();

        if (continueButton != null)
            continueButton.interactable = SaveSystem.HasSave();
    }

    public void NewGame()
    {
        SaveSystem.DeleteSave();
        SceneManager.LoadScene(gameSceneName);
    }

    public void ContinueGame()
    {
        if (!SaveSystem.HasSave())
            return;

        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettings()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}