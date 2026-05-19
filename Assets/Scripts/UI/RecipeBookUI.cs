using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class RecipeBookUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject recipeBookPanel;
    [SerializeField] private TMP_Text recipeBookText;
    [SerializeField] private TMP_Text pageCounterText;

    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.R;
    [SerializeField] private float swipeThreshold = 80f;

    private DataRepository _dataRepository;
    private List<RecipeData> _recipes = new();

    private bool _isOpen;
    private int _currentPage;

    private bool _isDragging;
    private Vector2 _dragStartPosition;

    public void Initialize(DataRepository dataRepository)
    {
        _dataRepository = dataRepository;

        if (_dataRepository != null && _dataRepository.Recipes != null)
            _recipes = _dataRepository.Recipes;

        _currentPage = 0;
        _isOpen = false;

        if (recipeBookPanel != null)
            recipeBookPanel.SetActive(false);

        CursorLockController.SetUIOpen(false);

        ShowCurrentPage();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            ToggleRecipeBook();

        if (!_isOpen)
            return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            PreviousPage();

        if (Input.GetKeyDown(KeyCode.RightArrow))
            NextPage();

        HandleMouseSwipe();
    }

    public void ToggleRecipeBook()
    {
        if (recipeBookPanel == null)
            return;

        _isOpen = !_isOpen;
        recipeBookPanel.SetActive(_isOpen);

        CursorLockController.SetUIOpen(_isOpen);

        if (_isOpen)
            ShowCurrentPage();
    }

    public void OpenRecipeBook()
    {
        if (recipeBookPanel == null)
            return;

        _isOpen = true;
        recipeBookPanel.SetActive(true);
        CursorLockController.SetUIOpen(true);
        ShowCurrentPage();
    }

    public void CloseRecipeBook()
    {
        if (recipeBookPanel == null)
            return;

        _isOpen = false;
        recipeBookPanel.SetActive(false);
        CursorLockController.SetUIOpen(false);
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

    public void NextPage()
    {
        if (_recipes == null || _recipes.Count == 0)
            return;

        _currentPage++;

        if (_currentPage >= _recipes.Count)
            _currentPage = 0;

        ShowCurrentPage();
    }

    public void PreviousPage()
    {
        if (_recipes == null || _recipes.Count == 0)
            return;

        _currentPage--;

        if (_currentPage < 0)
            _currentPage = _recipes.Count - 1;

        ShowCurrentPage();
    }

    private void ShowCurrentPage()
    {
        if (recipeBookText == null)
            return;

        if (_recipes == null || _recipes.Count == 0)
        {
            recipeBookText.text = "КНИГА РЕЦЕПТОВ\n\nРецепты не загружены.";

            if (pageCounterText != null)
                pageCounterText.text = "0 / 0";

            return;
        }

        RecipeData recipe = _recipes[_currentPage];
        recipeBookText.text = BuildRecipePage(recipe);

        if (pageCounterText != null)
            pageCounterText.text = $"{_currentPage + 1} / {_recipes.Count}";
    }

    private string BuildRecipePage(RecipeData recipe)
    {
        StringBuilder sb = new();

        //sb.AppendLine("КНИГА РЕЦЕПТОВ");
        //sb.AppendLine("–––––––––––––––––");
        //sb.AppendLine();

        sb.AppendLine(recipe.Name.ToUpper());
        sb.AppendLine();
        sb.AppendLine($"Вкус: {TranslateTaste(recipe.TasteProfile)}");
        sb.AppendLine($"Крепость: {TranslateStrength(recipe.StrengthProfile)}");
        sb.AppendLine();

        sb.AppendLine("Состав:");

        foreach (var ingredient in recipe.Ingredients)
        {
            string ingredientName = GetIngredientName(ingredient.IngredientId);
            sb.AppendLine($"• {ingredientName} — {ingredient.AmountMl:F0} мл");
        }

        if (recipe.RequiresIce)
            sb.AppendLine("• лёд");

        if (recipe.RequiredGarnishes != null && recipe.RequiredGarnishes.Count > 0)
        {
            sb.AppendLine();
            sb.Append("Украшение: ");

            for (int i = 0; i < recipe.RequiredGarnishes.Count; i++)
            {
                sb.Append(GetIngredientName(recipe.RequiredGarnishes[i]));

                if (i < recipe.RequiredGarnishes.Count - 1)
                    sb.Append(", ");
            }

            sb.AppendLine();
        }

        if (recipe.TargetStirLevel > 0)
            sb.AppendLine($"Перемешать: {recipe.TargetStirLevel:F0} раз");

        if (recipe.TargetShakeLevel > 0)
            sb.AppendLine($"Встряхнуть: {recipe.TargetShakeLevel:F0} раз");

        sb.AppendLine();
        sb.AppendLine("← проведите мышью →");

        return sb.ToString();
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

    private string TranslateTaste(string taste)
    {
        return taste switch
        {
            "bitter" => "горький",
            "sweet" => "сладкий",
            "fresh" => "освежающий",
            "sour" => "кислый",
            _ => taste
        };
    }

    private string TranslateStrength(string strength)
    {
        return strength switch
        {
            "strong" => "крепкий",
            "medium" => "средний",
            "light" => "лёгкий",
            _ => strength
        };
    }
}