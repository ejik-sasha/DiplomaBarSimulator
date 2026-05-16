using UnityEngine;

public class GameSaveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private UIManager uiManager;

    private void Awake()
    {
        if (moneyManager == null)
            moneyManager = FindFirstObjectByType<MoneyManager>();

        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();
    }

    private void Start()
    {
        LoadGameIfExists();
    }

    public void SaveGame()
    {
        if (moneyManager == null)
        {
            Debug.LogError("GameSaveManager: MoneyManager not found.");
            return;
        }

        SaveData data = moneyManager.CreateSaveData();
        SaveSystem.Save(data);

        if (uiManager != null)
            uiManager.SetStatusText("Игра сохранена.");
    }

    public void LoadGameIfExists()
    {
        SaveData data = SaveSystem.Load();

        if (data == null)
            return;

        if (moneyManager != null)
            moneyManager.LoadFromSave(data);

        if (uiManager != null)
            uiManager.SetStatusText("Сохранение загружено.");
    }

    public void DeleteSave()
    {
        SaveSystem.DeleteSave();

        if (uiManager != null)
            uiManager.SetStatusText("Сохранение удалено.");
    }
}