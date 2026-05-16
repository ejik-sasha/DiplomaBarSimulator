using UnityEngine;

public class BartendingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NPCClient npcClient;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private GlassPourReceiver glassPourReceiver;
    [SerializeField] private GlassItemReceiver glassItemReceiver;
    [Header("Settings")]
    [SerializeField] private bool showDetailedFeedback = true;
    [SerializeField] private float defaultLiquidAmount = 10f;
    [SerializeField] private float defaultIceAmount = 1f;
    [SerializeField] private float defaultMixAmount = 1f;
    

    private Glass _currentGlass;
    private CocktailEvaluator _evaluator;
    private bool _drinkSubmitted;

    private void Awake()
    {
        _currentGlass = new Glass();
        _evaluator = new CocktailEvaluator();
        _drinkSubmitted = false;
        if (moneyManager == null)
            moneyManager = FindFirstObjectByType<MoneyManager>();
    }

    private void Start()
    {
        SetStatus(
            "Бармен готов! Нажмите на клиента, чтобы принять заказ.",
            "Готов к работе"
        );
    }


    public void ApplyAction(CocktailAction action)
    {
        if (_drinkSubmitted)
        {
            SetStatus(
                "Напиток уже подан. Дождитесь следующего клиента.",
                "Напиток подан"
            );
            return;
        }
        

        if (npcClient == null || !npcClient.HasGivenOrder)
        {
            SetStatus(
                "Сначала примите заказ у клиента (нажмите на него)",
                "Нет заказа"
            );
            return;
        }

        ProcessAction(action);
    }

    private void ProcessAction(CocktailAction action)
    {
        switch (action.Type)
        {
            case CocktailActionType.AddLiquid:
            case CocktailActionType.AddPowder:
            {
                _currentGlass.AddIngredient(action.TargetId, action.Amount);

                float total = _currentGlass.GetIngredientAmount(action.TargetId);
                string name = GetIngredientName(action.TargetId);

                if (total > 100f)
                {
                    SetStatus(
                        $"Вы переборщили с {name}: {total:F0} мл",
                        "Слишком много"
                    );
                }
                else
                {
                    SetStatus(
                        $"Вы добавили {name}: всего {total:F0} мл",
                        $"{name} добавлен"
                    );
                }

                break;
            }

            case CocktailActionType.AddIce:
            {
                _currentGlass.AddIce(action.Amount);

                SetStatus(
                    $"Добавлен лёд: {action.Amount}",
                    "Лёд добавлен"
                );

                break;
            }

            case CocktailActionType.AddGarnish:
            {
                _currentGlass.AddGarnish(action.TargetId);

                SetStatus(
                    $"Добавлено украшение: {action.TargetId}",
                    "Украшение добавлено"
                );

                break;
            }

            case CocktailActionType.Stir:
            {
                _currentGlass.Stir(action.Amount);

                SetStatus(
                    "Вы перемешали напиток",
                    "Перемешано"
                );

                break;
            }

            case CocktailActionType.Shake:
            {
                _currentGlass.Shake(action.Amount);

                SetStatus(
                    "Вы встряхнули напиток",
                    "Встряхнуто"
                );

                break;
            }
        }
    }


    public void AddIngredientButton(string ingredientId)
    {
        ApplyAction(new CocktailAction(
            CocktailActionType.AddLiquid,
            ingredientId,
            defaultLiquidAmount
        ));
    }

    public void AddPowderButton(string powderId)
    {
        ApplyAction(new CocktailAction(
            CocktailActionType.AddPowder,
            powderId,
            10f
        ));
    }

    public void AddIceButton()
    {
        ApplyAction(new CocktailAction(
            CocktailActionType.AddIce,
            "ice",
            defaultIceAmount
        ));
    }

    public void AddGarnishButton(string garnishId)
    {
        ApplyAction(new CocktailAction(
            CocktailActionType.AddGarnish,
            garnishId,
            0f
        ));
    }

    public void StirButton()
    {
        ApplyAction(new CocktailAction(
            CocktailActionType.Stir,
            "stir",
            defaultMixAmount
        ));
    }

    public void ShakeButton()
    {
        ApplyAction(new CocktailAction(
            CocktailActionType.Shake,
            "shake",
            defaultMixAmount
        ));
    }


    public void SubmitDrink()
    {
        if (_drinkSubmitted)
        {
            SetStatus("Этот напиток уже подан!", "Уже подан");
            return;
        }

        if (npcClient == null || !npcClient.HasGivenOrder)
        {
            SetStatus("Сначала примите заказ!", "Нет заказа");
            return;
        }

        if (_currentGlass.IsEmpty())
        {
            SetStatus("Стакан пуст! Приготовьте напиток.", "Пусто");
            return;
        }

        RecipeData targetRecipe = npcClient.GetTargetRecipe();

        if (targetRecipe == null)
        {
            SetStatus("Ошибка рецепта!", "Ошибка");
            Debug.LogError("Recipe is null");
            return;
        }

        _drinkSubmitted = true;

        CocktailEvaluationResult result =
            _evaluator.Evaluate(_currentGlass, targetRecipe);

        float payment = 0f;

        if (moneyManager != null)
            payment = moneyManager.AddPayment(targetRecipe, result);

        ShowResult(result, payment);

        npcClient.ReactToDrink(result);

        SetStatus("Напиток подан! Клиент пробует...", "Подано");

        Invoke(nameof(AutoPrepareForNextClient), 7f);
    }

    
    private void ShowResult(CocktailEvaluationResult result, float payment)
    {
        if (uiManager == null)
            return;

        if (showDetailedFeedback)
        {
            string text =
                $"ОЦЕНКА: {result.FinalScore * 100:F0}%\n" +
                $"Ингредиенты: {result.IngredientScore * 100:F0}%\n" +
                $"Лёд: {result.IceScore * 100:F0}%\n" +
                $"Смешивание: {result.MixingScore * 100:F0}%\n";

            text += $"Украшение: {result.GarnishScore * 100:F0}%\n";

            text += $"Доход: {payment:F0} р.\n";
            text += $"{result.Comment}\n";

            uiManager.SetScoreText(text);
        }
        else
        {
            uiManager.SetScoreText(
                $"Оценка: {result.FinalScore * 100:F0}%\n" +
                $"Доход: {payment:F0} р.\n" +
                $"{result.Comment}"
            );
        }
    }

    public void ResetGlass()
    {
        _currentGlass.Clear();
        _drinkSubmitted = false;

        if (glassPourReceiver != null)
            glassPourReceiver.ClearVisual();

        if (glassItemReceiver != null)
            glassItemReceiver.ClearVisuals();

        if (uiManager != null)
            uiManager.ClearScore();

        SetStatus(
            "Стакан очищен. Можно готовить новый напиток.",
            "Очищено"
        );
    }

    public void PrepareForNextClient()
    {
        ResetGlass();

        SetStatus(
            "Готов к следующему клиенту!",
            "Готов"
        );
    }


    private void SetStatus(string detailed, string simple)
    {
        if (uiManager == null) return;

        uiManager.SetStatusText(
            showDetailedFeedback ? detailed : simple
        );
    }

    private string GetIngredientName(string id)
    {
        return id switch
        {
            "rum_white" => "белый ром",
            "gin" => "джин",
            "campari" => "кампари",
            "vermouth_red" => "красный вермут",
            "lime" => "лайм",
            "soda" => "содовая",
            "ice" => "лёд",
            _ => id
        };
    }

    public bool CanPrepareDrink()
    {
        return !_drinkSubmitted &&
               npcClient != null &&
               npcClient.HasGivenOrder;
    }

    private void OnDestroy()
    {
        _currentGlass?.Clear();
    }
    private void AutoPrepareForNextClient()
    {
        ResetGlass();

        SetStatus(
            "Стакан очищен. Можно готовить для следующего клиента.",
            "Готов"
        );
    }
    public void AddPouredLiquid(string ingredientId, float amountMl)
    {
        if (_drinkSubmitted)
        {
            SetStatus(
                "Напиток уже подан. Дождитесь следующего клиента.",
                "Напиток подан"
            );
            return;
        }

        if (npcClient == null || !npcClient.HasGivenOrder)
        {
            SetStatus(
                "Сначала примите заказ у клиента.",
                "Нет заказа"
            );
            return;
        }

        if (string.IsNullOrWhiteSpace(ingredientId) || amountMl <= 0f)
            return;

        _currentGlass.AddIngredient(ingredientId, amountMl);

        float total = _currentGlass.GetIngredientAmount(ingredientId);
        string name = GetIngredientName(ingredientId);

        SetStatus(
            $"Вы наливаете {name}: всего {total:F0} мл",
            $"{name}: {total:F0} мл"
        );
    }
    
}