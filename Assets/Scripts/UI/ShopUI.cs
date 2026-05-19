using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private TMP_Text shopText;
    [SerializeField] private TMP_Text pageCounterText;
    [SerializeField] private Button buyButton;

    [Header("Managers")]
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private ShopItemSpawner itemSpawner;
    [SerializeField] private UIManager uiManager;

    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.B;
    [SerializeField] private float swipeThreshold = 80f;

    private DataRepository _dataRepository;
    private List<IngredientData> _ingredients = new();

    private bool _isOpen;
    private int _currentPage;

    private bool _isDragging;
    private Vector2 _dragStartPosition;

    public void Initialize(DataRepository dataRepository)
    {
        _dataRepository = dataRepository;

        if (_dataRepository != null && _dataRepository.Ingredients != null)
            _ingredients = _dataRepository.Ingredients;

        if (moneyManager == null)
            moneyManager = FindFirstObjectByType<MoneyManager>();

        if (itemSpawner == null)
            itemSpawner = FindFirstObjectByType<ShopItemSpawner>();

        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();

        _currentPage = 0;
        _isOpen = false;

        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(BuyCurrentItem);
        }

        ShowCurrentPage();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            ToggleShop();

        if (!_isOpen)
            return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            PreviousPage();

        if (Input.GetKeyDown(KeyCode.RightArrow))
            NextPage();

        HandleMouseSwipe();
    }

    public void ToggleShop()
    {
        if (shopPanel == null)
            return;

        _isOpen = !_isOpen;
        shopPanel.SetActive(_isOpen);

        CursorLockController.SetUIOpen(_isOpen);

        if (_isOpen)
            ShowCurrentPage();
    }

    public void OpenShop()
    {
        if (shopPanel == null)
            return;

        _isOpen = true;
        shopPanel.SetActive(true);
        CursorLockController.SetUIOpen(true);
        ShowCurrentPage();
    }

    public void CloseShop()
    {
        if (shopPanel == null)
            return;

        _isOpen = false;
        shopPanel.SetActive(false);
        CursorLockController.SetUIOpen(false);
    }

    public void NextPage()
    {
        if (_ingredients == null || _ingredients.Count == 0)
            return;

        _currentPage++;

        if (_currentPage >= _ingredients.Count)
            _currentPage = 0;

        ShowCurrentPage();
    }

    public void PreviousPage()
    {
        if (_ingredients == null || _ingredients.Count == 0)
            return;

        _currentPage--;

        if (_currentPage < 0)
            _currentPage = _ingredients.Count - 1;

        ShowCurrentPage();
    }

    public void BuyCurrentItem()
    {
        if (_ingredients == null || _ingredients.Count == 0)
            return;

        IngredientData ingredient = _ingredients[_currentPage];

        if (moneyManager == null || itemSpawner == null)
        {
            Debug.LogError("ShopUI: MoneyManager or ShopItemSpawner is missing.");
            return;
        }

        if (!moneyManager.TrySpendMoney(ingredient.Price))
        {
            if (uiManager != null)
                uiManager.SetStatusText("Недостаточно денег.");

            return;
        }

        bool spawned = itemSpawner.TrySpawnItem(ingredient.Id);

        if (!spawned)
        {
            moneyManager.AddRawMoney(ingredient.Price);

            if (uiManager != null)
                uiManager.SetStatusText("Не удалось разместить предмет.");

            return;
        }

        if (uiManager != null)
            uiManager.SetStatusText($"Куплено: {GetIngredientName(ingredient.Id)}");
    }

    private void ShowCurrentPage()
    {
        if (shopText == null)
            return;

        if (_ingredients == null || _ingredients.Count == 0)
        {
            shopText.text = "Товары не загружены.";

            if (pageCounterText != null)
                pageCounterText.text = "0 / 0";

            if (buyButton != null)
                buyButton.interactable = false;

            return;
        }

        IngredientData ingredient = _ingredients[_currentPage];
        shopText.text = BuildShopPage(ingredient);

        if (pageCounterText != null)
            pageCounterText.text = $"{_currentPage + 1} / {_ingredients.Count}";

        if (buyButton != null)
            buyButton.interactable = true;
    }

    private string BuildShopPage(IngredientData ingredient)
    {
        StringBuilder sb = new();

        //sb.AppendLine("МАГАЗИН");
        //sb.AppendLine("–––––––––––––––––");
        //sb.AppendLine();

        sb.AppendLine(GetIngredientName(ingredient.Id).ToUpper());
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(ingredient.Category))
            sb.AppendLine($"Категория: {TranslateCategory(ingredient.Category)}");

        sb.AppendLine($"Цена: {ingredient.Price:F0} р.");
        sb.AppendLine();
        sb.AppendLine("← проведите мышью →");
        sb.AppendLine();

        return sb.ToString();
    }

    private void HandleMouseSwipe()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isDragging = true;
            _dragStartPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0) && _isDragging)
        {
            _isDragging = false;

            Vector2 dragEndPosition = Input.mousePosition;
            float deltaX = dragEndPosition.x - _dragStartPosition.x;

            if (Mathf.Abs(deltaX) < swipeThreshold)
                return;

            if (deltaX < 0)
                NextPage();
            else
                PreviousPage();
        }
    }

    private string GetIngredientName(string id)
    {
        return id switch
        {
            "rum_white" => "белый ром",
            "gin" => "джин",
            "campari" => "кампари",
            "vermouth_red" => "красный вермут",
            "lime" => "лаймовый сок",
            "soda" => "содовая",
            "mint" => "мята",
            "orange_peel" => "апельсиновая цедра",
            "ice" => "лёд",
            _ => id
        };
    }

    private string TranslateCategory(string category)
    {
        return category switch
        {
            "alcohol" => "алкоголь",
            "juice" => "сок",
            "soft" => "безалкогольное",
            "garnish" => "украшение",
            "ice" => "лёд",
            _ => category
        };
    }
}