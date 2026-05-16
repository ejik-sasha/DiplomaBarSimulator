using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [System.Serializable]
    private class IngredientPrefabBinding
    {
        public string IngredientId;
        public GameObject Prefab;
    }

    [Header("References")]
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private UIManager uiManager;

    [Header("Data")]
    [SerializeField] private TextAsset ingredientsJson;

    [Header("UI")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private Button itemButtonPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private List<IngredientPrefabBinding> ingredientPrefabs = new();

    private IngredientDatabase _database;
    private bool _isOpen;
    private int _nextSpawnIndex;

    private void Awake()
    {
        if (moneyManager == null)
            moneyManager = FindFirstObjectByType<MoneyManager>();

        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();

        if (shopPanel != null)
            shopPanel.SetActive(false);

        LoadIngredients();
        BuildShopUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleShop();
        }
    }

    public void ToggleShop()
    {
        _isOpen = !_isOpen;

        if (shopPanel != null)
            shopPanel.SetActive(_isOpen);

        CursorLockController.SetUIOpen(_isOpen);
    }

    public void CloseShop()
    {
        _isOpen = false;

        if (shopPanel != null)
            shopPanel.SetActive(false);

        CursorLockController.SetUIOpen(false);
    }

    private void LoadIngredients()
    {
        if (ingredientsJson == null)
        {
            Debug.LogError("ShopManager: ingredientsJson is not assigned.");
            return;
        }

        _database = JsonUtility.FromJson<IngredientDatabase>(ingredientsJson.text);

        if (_database == null || _database.Items == null)
        {
            Debug.LogError("ShopManager: ingredients.json could not be parsed.");
        }
    }

    private void BuildShopUI()
    {
        if (_database == null || _database.Items == null)
            return;

        if (itemsContainer == null || itemButtonPrefab == null)
        {
            Debug.LogError("ShopManager: UI references are not assigned.");
            return;
        }

        ClearContainer();

        foreach (IngredientData ingredient in _database.Items)
        {
            Button button = Instantiate(itemButtonPrefab, itemsContainer);

            TMP_Text text = button.GetComponentInChildren<TMP_Text>();

            if (text != null)
            {
                text.text = $"{ingredient.Name}\n{ingredient.Price:F0} ₽";
            }

            IngredientData capturedIngredient = ingredient;

            button.onClick.AddListener(() =>
            {
                BuyIngredient(capturedIngredient);
            });
        }
    }

    private void BuyIngredient(IngredientData ingredient)
    {
        if (ingredient == null)
            return;

        GameObject prefab = GetPrefabForIngredient(ingredient.Id);

        if (prefab == null)
        {
            Debug.LogWarning($"ShopManager: prefab for ingredient '{ingredient.Id}' was not found.");

            if (uiManager != null)
                uiManager.SetStatusText($"Нет prefab для товара: {ingredient.Name}");

            return;
        }

        if (moneyManager == null)
        {
            Debug.LogError("ShopManager: MoneyManager is not assigned.");
            return;
        }

        bool canBuy = moneyManager.TrySpendMoney(ingredient.Price);

        if (!canBuy)
        {
            if (uiManager != null)
                uiManager.SetStatusText("Недостаточно денег.");

            return;
        }

        SpawnIngredient(prefab);

        if (uiManager != null)
            uiManager.SetStatusText($"Куплено: {ingredient.Name} за {ingredient.Price:F0} ₽");
    }

    private GameObject GetPrefabForIngredient(string ingredientId)
    {
        foreach (IngredientPrefabBinding binding in ingredientPrefabs)
        {
            if (binding != null && binding.IngredientId == ingredientId)
                return binding.Prefab;
        }

        return null;
    }

    private void SpawnIngredient(GameObject prefab)
    {
        if (prefab == null)
            return;

        Transform spawnPoint = GetNextSpawnPoint();

        Vector3 position = spawnPoint != null
            ? spawnPoint.position
            : transform.position;

        Quaternion rotation = spawnPoint != null
            ? spawnPoint.rotation
            : Quaternion.identity;

        Instantiate(prefab, position, rotation);
    }

    private Transform GetNextSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return null;

        Transform point = spawnPoints[_nextSpawnIndex];

        _nextSpawnIndex++;

        if (_nextSpawnIndex >= spawnPoints.Length)
            _nextSpawnIndex = 0;

        return point;
    }

    private void ClearContainer()
    {
        for (int i = itemsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(itemsContainer.GetChild(i).gameObject);
        }
    }
}