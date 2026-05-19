using System.Collections.Generic;
using UnityEngine;

public class DataRepository
{
    public List<IngredientData> Ingredients { get; private set; } = new();
    public List<RecipeData> Recipes { get; private set; } = new();
    public List<NPCProfile> NPCProfiles { get; private set; } = new();

    public void LoadAll()
    {
        IngredientListWrapper ingredientsWrapper = JsonLoader.LoadFromResources<IngredientListWrapper>("ingredients");
        RecipeListWrapper recipesWrapper = JsonLoader.LoadFromResources<RecipeListWrapper>("recipes");
        NPCProfileListWrapper profilesWrapper = JsonLoader.LoadFromResources<NPCProfileListWrapper>("npc_profiles");

        Ingredients = ingredientsWrapper?.Items ?? new List<IngredientData>();
        Recipes = recipesWrapper?.Items ?? new List<RecipeData>();
        NPCProfiles = profilesWrapper?.Items ?? new List<NPCProfile>();

        Debug.Log($"Loaded {Ingredients.Count} ingredients, {Recipes.Count} recipes, {NPCProfiles.Count} NPC profiles.");
    }

    public RecipeData GetRecipeById(string id)
    {
        return Recipes.Find(r => r.Id == id);
    }
}