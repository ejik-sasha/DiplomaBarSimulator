using System;
using System.Collections.Generic;
using System.Linq;

public class CocktailEvaluationResult
{
    public float IngredientScore;
    public float IceScore;
    public float MixingScore;
    public float GarnishScore;
    public float ExtraPenalty;
    public float FinalScore;
    public string Comment;
}

public class CocktailEvaluator
{
    public CocktailEvaluationResult Evaluate(Glass cocktail, RecipeData recipe)
    {
        float ingredientScore = EvaluateIngredients(cocktail, recipe);
        float iceScore = EvaluateIce(cocktail, recipe);
        float mixingScore = EvaluateMixing(cocktail, recipe);
        float garnishScore = EvaluateGarnishes(cocktail, recipe);
        float extraPenalty = EvaluateExtraIngredients(cocktail, recipe);

        float finalScore =
            ingredientScore * 0.6f +
            iceScore * 0.1f +
            mixingScore * 0.2f +
            garnishScore * 0.1f;

        finalScore *= extraPenalty;

        finalScore = Math.Clamp(finalScore, 0f, 1f);

        return new CocktailEvaluationResult
        {
            IngredientScore = ingredientScore,
            IceScore = iceScore,
            MixingScore = mixingScore,
            GarnishScore = garnishScore,
            ExtraPenalty = extraPenalty,
            FinalScore = finalScore,
            Comment = BuildComment(finalScore)
        };
    }

    private float EvaluateIngredients(Glass cocktail, RecipeData recipe)
    {
        if (recipe.Ingredients == null || recipe.Ingredients.Count == 0)
            return 0f;

        float total = 0f;

        foreach (var target in recipe.Ingredients)
        {
            float actual = cocktail.GetIngredientAmount(target.IngredientId);
            float expected = target.AmountMl;

            if (expected <= 0)
                continue;

            float diff = Math.Abs(actual - expected);
            float score = Math.Max(0f, 1f - diff / expected);

            total += score;
        }

        return total / recipe.Ingredients.Count;
    }

    private float EvaluateIce(Glass cocktail, RecipeData recipe)
    {
        if (!recipe.RequiresIce)
            return cocktail.IceAmount <= 0 ? 1f : 0.7f;

        return cocktail.IceAmount > 0 ? 1f : 0f;
    }

    private float EvaluateMixing(Glass cocktail, RecipeData recipe)
    {
        float stirScore = CompareActionValue(cocktail.StirLevel, recipe.TargetStirLevel);
        float shakeScore = CompareActionValue(cocktail.ShakeLevel, recipe.TargetShakeLevel);

        return (stirScore + shakeScore) / 2f;
    }

    private float CompareActionValue(float actual, float expected)
    {
        if (expected <= 0)
            return actual <= 0 ? 1f : 0.5f;

        float diff = Math.Abs(actual - expected);
        return Math.Max(0f, 1f - diff / expected);
    }

    private float EvaluateGarnishes(Glass cocktail, RecipeData recipe)
    {
        if (recipe.RequiredGarnishes == null || recipe.RequiredGarnishes.Count == 0)
            return 1f;

        int matched = recipe.RequiredGarnishes.Count(g =>
            cocktail.Garnishes.Contains(g)
        );

        return (float)matched / recipe.RequiredGarnishes.Count;
    }

    private float EvaluateExtraIngredients(Glass cocktail, RecipeData recipe)
    {
        HashSet<string> expectedIds = recipe.Ingredients
            .Select(i => i.IngredientId)
            .ToHashSet();

        int extraCount = cocktail.Ingredients.Keys
            .Count(id => !expectedIds.Contains(id));

        return Math.Max(0.4f, 1f - extraCount * 0.15f);
    }

    private string BuildComment(float score)
    {
        if (score >= 0.9f)
            return "Клиент почти полностью доволен напитком.";

        if (score >= 0.75f)
            return "Хороший результат, напиток близок к ожиданиям.";

        if (score >= 0.5f)
            return "Напиток частично похож на заказ, но есть заметные ошибки.";

        return "Напиток почти не соответствует ожиданиям клиента.";
    }
}