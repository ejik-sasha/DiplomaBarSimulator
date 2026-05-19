using System.Collections.Generic;

public class Glass
{
    private readonly Dictionary<string, float> _ingredients = new();
    private readonly List<string> _garnishes = new();
    private const float MaxVolume = 300f;

    public IReadOnlyDictionary<string, float> Ingredients => _ingredients;
    public IReadOnlyList<string> Garnishes => _garnishes;

    public float IceAmount { get; private set; }
    public float StirLevel { get; private set; }
    public float ShakeLevel { get; private set; }

    public void AddIngredient(string ingredientId, float amountMl)
    {
        if (string.IsNullOrWhiteSpace(ingredientId) || amountMl <= 0)
            return;

        if (!_ingredients.ContainsKey(ingredientId))
            _ingredients[ingredientId] = 0f;

        _ingredients[ingredientId] += amountMl;
    }

    public void AddIce(float amount)
    {
        if (amount <= 0)
            return;

        IceAmount += amount;
    }

    public void AddGarnish(string garnishId)
    {
        if (string.IsNullOrWhiteSpace(garnishId))
            return;

        if (!_garnishes.Contains(garnishId))
        _garnishes.Add(garnishId);
    }

    public void Stir(float amount)
    {
        if (amount <= 0) return;

        StirLevel = System.MathF.Min(StirLevel + amount, 10f);
    }

    public void Shake(float amount)
    {
        if (amount <= 0) return;

        ShakeLevel = System.MathF.Min(ShakeLevel + amount, 10f);
    }

    public float GetIngredientAmount(string ingredientId)
    {
        return _ingredients.TryGetValue(ingredientId, out float amount) ? amount : 0f;
    }

    public void Clear()
    {
        _ingredients.Clear();
        _garnishes.Clear();
        IceAmount = 0f;
        StirLevel = 0f;
        ShakeLevel = 0f;
    }
    public bool IsEmpty()
    {
        return _ingredients.Count == 0 
            && IceAmount <= 0 
            && _garnishes.Count == 0;
    }
    public float GetTotalVolume()
    {
        float total = 0f;

        foreach (var ing in _ingredients)
            total += ing.Value;

        return total;
    }
    public bool IsOverflowing()
    {
        return GetTotalVolume() > MaxVolume;
    }
    public string GetDebugInfo()
    {
        System.Text.StringBuilder sb = new();

        sb.AppendLine("=== СТАКАН ===");

        foreach (var ing in _ingredients)
            sb.AppendLine($"{ing.Key}: {ing.Value} мл");

        sb.AppendLine($"Лёд: {IceAmount}");
        sb.AppendLine($"Stir: {StirLevel}");
        sb.AppendLine($"Shake: {ShakeLevel}");

        if (_garnishes.Count > 0)
            sb.AppendLine($"Украшения: {string.Join(", ", _garnishes)}");

        return sb.ToString();
    }
}