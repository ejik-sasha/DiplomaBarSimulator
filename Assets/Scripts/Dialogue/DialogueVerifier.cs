using System.Collections.Generic;
using System.Linq;

public class DialogueVerifier
{
    private readonly Dictionary<string, string[]> _recipeForbiddenWords = new()
    {
        { "mojito", new[] { "мохито", "mojito" } },
        { "negroni", new[] { "негрони", "negroni" } },
        { "gin_tonic", new[] { "джин-тоник", "джин тоник", "gin tonic" } },
        { "daiquiri", new[] { "дайкири", "daiquiri" } }
    };

    private readonly string[] _ingredientWords =
    {
        "джин",
        "ром",
        "кампари",
        "вермут",
        "содовая",
        "лайм",
        "мята",
        "лёд",
        "лед"
    };

    public DialogueCandidate VerifyOrderCandidate(
        string text,
        NPCProfile profile,
        OrderContext order)
    {
        DialogueCandidate candidate = new()
        {
            Text = text,
            Score = 0f
        };

        if (string.IsNullOrWhiteSpace(text))
        {
            candidate.RejectReason = "Пустая реплика.";
            return candidate;
        }

        string normalized = text.ToLowerInvariant();

        if (IsTooLong(text))
        {
            candidate.RejectReason = "Реплика слишком длинная.";
            return candidate;
        }

        if (MentionsRecipeName(normalized, order))
        {
            candidate.RejectReason = "Реплика напрямую называет коктейль.";
            return candidate;
        }

        if (MentionsTooManyIngredients(normalized))
        {
            candidate.RejectReason = "Реплика слишком явно перечисляет ингредиенты.";
            return candidate;
        }

        candidate.Score += ScoreLength(text);
        candidate.Score += ScoreTasteHint(normalized, order);
        candidate.Score += ScoreStrengthHint(normalized, order);
        candidate.Score += ScoreIceHint(normalized, order);
        candidate.Score += ScorePersonalityHint(normalized, profile);
        candidate.Score += ScoreNaturalness(normalized);

        return candidate;
    }

    public DialogueCandidate VerifyReactionCandidate(
        string text,
        NPCProfile profile,
        CocktailEvaluationResult result)
    {
        DialogueCandidate candidate = new()
        {
            Text = text,
            Score = 0f
        };

        if (string.IsNullOrWhiteSpace(text))
        {
            candidate.RejectReason = "Пустая реакция.";
            return candidate;
        }

        if (IsTooLong(text))
        {
            candidate.RejectReason = "Реакция слишком длинная.";
            return candidate;
        }

        string normalized = text.ToLowerInvariant();

        candidate.Score += ScoreLength(text);
        candidate.Score += ScoreReactionTone(normalized, result.FinalScore);
        candidate.Score += ScorePersonalityHint(normalized, profile);
        candidate.Score += ScoreNaturalness(normalized);

        return candidate;
    }

    private bool IsTooLong(string text)
    {
        string[] words = text.Split(' ');
        return words.Length > 45;
    }

    private bool MentionsRecipeName(string normalized, OrderContext order)
    {
        if (order == null)
            return false;

        string recipeName = order.RecipeName?.ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(recipeName) &&
            normalized.Contains(recipeName))
            return true;

        string recipeId = order.RecipeId?.ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(recipeId) &&
            _recipeForbiddenWords.TryGetValue(recipeId, out string[] forbidden))
        {
            return forbidden.Any(normalized.Contains);
        }

        return false;
    }

    private bool MentionsTooManyIngredients(string normalized)
    {
        int count = 0;

        foreach (string ingredient in _ingredientWords)
        {
            if (normalized.Contains(ingredient))
                count++;
        }

        return count >= 3;
    }

    private float ScoreLength(string text)
    {
        int words = text.Split(' ').Length;

        if (words >= 8 && words <= 25)
            return 2f;

        if (words <= 35)
            return 1f;

        return 0f;
    }

    private float ScoreTasteHint(string normalized, OrderContext order)
    {
        float score = 0f;

        string taste = order.TasteHint?.ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(taste))
            return score;

        if (taste.Contains("свеж") &&
            (normalized.Contains("свеж") || normalized.Contains("освеж")))
            score += 2f;

        if (taste.Contains("гор") &&
            (normalized.Contains("горчин") || normalized.Contains("горьк")))
            score += 2f;

        if (taste.Contains("кисл") &&
            (normalized.Contains("кисл") || normalized.Contains("цитрус")))
            score += 2f;

        if (taste.Contains("слад") &&
            normalized.Contains("слад"))
            score += 2f;

        return score;
    }

    private float ScoreStrengthHint(string normalized, OrderContext order)
    {
        string strength = order.StrengthHint?.ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(strength))
            return 0f;

        if (strength.Contains("креп") &&
            (normalized.Contains("покрепче") || normalized.Contains("крепк")))
            return 2f;

        if (strength.Contains("лег") &&
            (normalized.Contains("лёгк") || normalized.Contains("легк") || normalized.Contains("не креп")))
            return 2f;

        if (strength.Contains("сред") &&
            (normalized.Contains("не слишком") || normalized.Contains("умеренно")))
            return 1.5f;

        return 0f;
    }

    private float ScoreIceHint(string normalized, OrderContext order)
    {
        if (order.WantsIce)
        {
            if (normalized.Contains("холод") ||
                normalized.Contains("освеж") ||
                normalized.Contains("лёд") ||
                normalized.Contains("лед"))
                return 1.5f;
        }
        else
        {
            if (normalized.Contains("без льда") ||
                normalized.Contains("безо льда") ||
                normalized.Contains("не ледя"))
                return 1.5f;
        }

        return 0f;
    }

    private float ScorePersonalityHint(string normalized, NPCProfile profile)
    {
        float score = 0f;

        string archetype = profile.Archetype?.ToLowerInvariant();
        string temper = profile.Temper?.ToLowerInvariant();
        string mood = profile.Mood?.ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(archetype) &&
            normalized.Contains(archetype))
            score += 1f;

        if (!string.IsNullOrWhiteSpace(temper) &&
            normalized.Contains(temper))
            score += 0.5f;

        if (!string.IsNullOrWhiteSpace(mood) &&
            normalized.Contains(mood))
            score += 0.5f;

        // Более мягкая проверка на “жизненный контекст”
        if (normalized.Contains("работ") ||
            normalized.Contains("смен") ||
            normalized.Contains("вечер") ||
            normalized.Contains("день") ||
            normalized.Contains("дорог") ||
            normalized.Contains("устал") ||
            normalized.Contains("встреч"))
            score += 1.5f;

        return score;
    }

    private float ScoreNaturalness(string normalized)
    {
        float score = 0f;

        if (!normalized.Contains("как npc") &&
            !normalized.Contains("вариант") &&
            !normalized.Contains("реплика"))
            score += 1f;

        if (!normalized.Contains("скрытый коктейль") &&
            !normalized.Contains("ингредиент"))
            score += 1f;

        return score;
    }

    private float ScoreReactionTone(string normalized, float finalScore)
    {
        if (finalScore >= 0.8f)
        {
            if (normalized.Contains("отлич") ||
                normalized.Contains("идеаль") ||
                normalized.Contains("вкусно") ||
                normalized.Contains("то что надо"))
                return 2f;
        }

        if (finalScore >= 0.45f && finalScore < 0.8f)
        {
            if (normalized.Contains("неплохо") ||
                normalized.Contains("почти") ||
                normalized.Contains("не совсем") ||
                normalized.Contains("нормально"))
                return 2f;
        }

        if (finalScore < 0.45f)
        {
            if (normalized.Contains("не то") ||
                normalized.Contains("плохо") ||
                normalized.Contains("ожидал") ||
                normalized.Contains("стран"))
                return 2f;
        }

        return 0f;
    }
}