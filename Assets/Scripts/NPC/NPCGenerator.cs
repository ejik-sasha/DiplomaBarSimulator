using System.Collections.Generic;
using UnityEngine;

public class NPCGenerator
{
    public NPCProfile GenerateRandomProfile(List<NPCProfile> profiles)
    {
        if (profiles == null || profiles.Count == 0)
        {
            Debug.LogError("NPC profile list is empty.");
            return null;
        }

        int index = Random.Range(0, profiles.Count);
        Debug.Log(profiles[index].DisplayName);
        return profiles[index];
    }

    public OrderContext GenerateOrderForProfile(NPCProfile profile, List<RecipeData> recipes)
    {
        if (profile == null || recipes == null || recipes.Count == 0)
        {
            Debug.LogError("Cannot generate order: invalid profile or recipes.");
            return null;
        }

        List<RecipeData> filtered = recipes.FindAll(r =>
            (string.IsNullOrEmpty(profile.PreferredTaste) || r.TasteProfile == profile.PreferredTaste) &&
            (string.IsNullOrEmpty(profile.PreferredStrength) || r.StrengthProfile == profile.PreferredStrength));
        
        Debug.Log(filtered.Count);

    
        if (filtered.Count == 0)
            filtered = recipes;
            
        Debug.Log(filtered.Count);
        RecipeData selected = filtered[Random.Range(0, filtered.Count)];
        Debug.Log(selected.Name);
        return new OrderContext
        {
            RecipeId = selected.Id,
            RecipeName = selected.Name,
            TasteHint = selected.TasteProfile,
            StrengthHint = selected.StrengthProfile,
            WantsIce = profile.LikesIce
        };
    }
}