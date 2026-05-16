using System;
using System.Threading.Tasks;
using UnityEngine;

public class DialogueManager
{
    private readonly ILLMProvider _provider;
    private readonly PromptBuilder _promptBuilder;
    private readonly NPCReactionGenerator _fallbackGenerator;

    private const int TimeoutMs = 15000;

    public DialogueManager(ILLMProvider provider)
    {
        _provider = provider;
        _promptBuilder = new PromptBuilder();
        _fallbackGenerator = new NPCReactionGenerator();
    }

    public async Task<DialogueResult> GenerateOrderDialogueAsync(NPCProfile profile, OrderContext order)
    {
        string prompt = _promptBuilder.BuildOrderPrompt(profile, order);

        try
        {
            Task<string> generationTask = _provider.GenerateTextAsync(prompt);
            Task timeoutTask = Task.Delay(TimeoutMs);

            Task completedTask = await Task.WhenAny(generationTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                return new DialogueResult
                {
                    NPCLine = _fallbackGenerator.GetFallbackOrderHint(profile, order),
                    SystemPromptUsed = prompt,
                    UsedFallback = true
                };
            }

            string response = await generationTask;

            if (string.IsNullOrWhiteSpace(response))
            {
                return new DialogueResult
                {
                    NPCLine = _fallbackGenerator.GetFallbackOrderHint(profile, order),
                    SystemPromptUsed = prompt,
                    UsedFallback = true
                };
            }

            return new DialogueResult
            {
                NPCLine = response.Trim(),
                SystemPromptUsed = prompt,
                UsedFallback = false
            };
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"LLM generation failed: {ex.Message}");

            return new DialogueResult
            {
                NPCLine = _fallbackGenerator.GetFallbackOrderHint(profile, order),
                SystemPromptUsed = prompt,
                UsedFallback = true
            };
        }
    }
    public async Task<string> GenerateReactionAsync(
        NPCProfile profile,
        OrderContext order,
        CocktailEvaluationResult result)
    {
        string prompt = _promptBuilder.BuildReactionPrompt(profile, order, result);

        try
        {
            string response = await _provider.GenerateTextAsync(prompt);

            if (string.IsNullOrWhiteSpace(response))
                return _fallbackGenerator.GetFallbackReaction(result.FinalScore, profile);

            return response.Trim();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"LLM reaction failed: {ex.Message}");
            return _fallbackGenerator.GetFallbackReaction(result.FinalScore, profile);
        }
    }
}