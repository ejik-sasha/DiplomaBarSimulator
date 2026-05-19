using System;
using System.Collections.Generic;

[Serializable]
public class RecipeIngredientAmount
{
    public string IngredientId;
    public float AmountMl;
}

[Serializable]
public class RecipeData
{
    public string Id;
    public string Name;

    public List<RecipeIngredientAmount> Ingredients = new();

    public string TasteProfile;
    public string StrengthProfile;

    public float BasePrice = 100f;

    public bool RequiresIce;
    public float TargetStirLevel;
    public float TargetShakeLevel;

    public List<string> RequiredGarnishes = new();
}