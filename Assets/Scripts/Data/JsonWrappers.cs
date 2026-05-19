using System;
using System.Collections.Generic;

[Serializable]
public class IngredientListWrapper
{
    public List<IngredientData> Items;
}

[Serializable]
public class RecipeListWrapper
{
    public List<RecipeData> Items;
}

[Serializable]
public class NPCProfileListWrapper
{
    public List<NPCProfile> Items;
}