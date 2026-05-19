using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIManager uiManager;

    [Header("Balance")]
    [SerializeField] private float startBalance = 0f;

    public float Balance { get; private set; }

    private void Awake()
    {
        Balance = startBalance;

        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();

        UpdateBalanceUI();
    }

    public float AddPayment(RecipeData recipe, CocktailEvaluationResult evaluationResult)
    {
        if (recipe == null || evaluationResult == null)
            return 0f;

        float payment = CalculatePayment(recipe, evaluationResult);

        Balance += payment;
        UpdateBalanceUI();
        AudioManager.Instance?.PlayMoney();

        return payment;
    }

    private float CalculatePayment(RecipeData recipe, CocktailEvaluationResult evaluationResult)
    {
        float score = Mathf.Clamp01(evaluationResult.FinalScore);
        float basePrice = Mathf.Max(0f, recipe.BasePrice);

        return Mathf.Round(basePrice * score);
    }

    public void ResetMoney()
    {
        Balance = startBalance;
        UpdateBalanceUI();
    }

    public void UpdateBalanceUI()
    {
        if (uiManager != null)
            uiManager.SetBalanceText(Balance);
    }
    public bool TrySpendMoney(float amount)
    {
        if (amount <= 0f)
            return true;

        if (Balance < amount)
            return false;

        Balance -= amount;
        UpdateBalanceUI();

        return true;
    }
    public void AddRawMoney(float amount)
    {
        if (amount <= 0f)
            return;

        Balance += amount;
        UpdateBalanceUI();
    }
    public void SetBalance(float value)
    {
        Balance = Mathf.Max(0f, value);
        UpdateBalanceUI();
    }

    public SaveData CreateSaveData()
    {
        return new SaveData
        {
            Balance = Balance
        };
    }

    public void LoadFromSave(SaveData data)
    {
        if (data == null)
            return;

        SetBalance(data.Balance);
    }
}