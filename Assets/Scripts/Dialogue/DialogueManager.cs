using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DialogueManager
{
    private readonly ILLMProvider _provider;
    private readonly PromptBuilder _promptBuilder;
    private readonly NPCReactionGenerator _fallbackGenerator;
    private readonly DialogueCandidateParser _candidateParser;
    private readonly DialogueVerifier _verifier;

    private const int TimeoutMs = 15000;
    private const int MaxGenerationAttempts = 2;

    public DialogueManager(ILLMProvider provider)
    {
        _provider = provider;
        _promptBuilder = new PromptBuilder();
        _fallbackGenerator = new NPCReactionGenerator();
        _candidateParser = new DialogueCandidateParser();
        _verifier = new DialogueVerifier();
    }

    public async Task<DialogueResult> GenerateOrderDialogueAsync(
        NPCProfile profile,
        OrderContext order)
    {
        string prompt = _promptBuilder.BuildOrderPrompt(profile, order);

        for (int attempt = 1; attempt <= MaxGenerationAttempts; attempt++)
        {
            try
            {
                string attemptPrompt = attempt == 1
                    ? prompt
                    : prompt + "\n\nПредыдущий ответ не прошёл проверку. Сгенерируй новые варианты строже по правилам.";

                string response = await TryGenerateWithTimeoutAsync(attemptPrompt);

                if (string.IsNullOrWhiteSpace(response))
                {
                    Debug.LogWarning($"Order dialogue attempt {attempt}: empty response.");
                    continue;
                }

                DialogueCandidate bestCandidate =
                    SelectBestOrderCandidate(response, profile, order);

                if (bestCandidate != null && bestCandidate.IsValid)
                {
                    Debug.Log(
                        $"Selected order candidate on attempt {attempt}: {bestCandidate.Text} " +
                        $"Score: {bestCandidate.Score:F1}"
                    );

                    return new DialogueResult
                    {
                        NPCLine = bestCandidate.Text.Trim(),
                        SystemPromptUsed = prompt,
                        UsedFallback = false
                    };
                }

                Debug.LogWarning($"Order dialogue attempt {attempt}: no valid candidates.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Order dialogue attempt {attempt} failed: {ex.Message}");
            }
        }

        Debug.LogWarning("All order dialogue attempts failed. Used fallback.");

        return CreateFallbackOrderResult(profile, order, prompt);
    }

    public async Task<string> GenerateReactionAsync(
        NPCProfile profile,
        OrderContext order,
        CocktailEvaluationResult result)
    {
        string prompt = _promptBuilder.BuildReactionPrompt(profile, order, result);

        for (int attempt = 1; attempt <= MaxGenerationAttempts; attempt++)
        {
            try
            {
                string attemptPrompt = attempt == 1
                    ? prompt
                    : prompt + "\n\nПредыдущий ответ не прошёл проверку. Сгенерируй новые варианты строже по правилам.";

                string response = await TryGenerateWithTimeoutAsync(attemptPrompt);

                if (string.IsNullOrWhiteSpace(response))
                {
                    Debug.LogWarning($"Reaction attempt {attempt}: empty response.");
                    continue;
                }

                DialogueCandidate bestCandidate =
                    SelectBestReactionCandidate(response, profile, result);

                if (bestCandidate != null && bestCandidate.IsValid)
                {
                    Debug.Log(
                        $"Selected reaction candidate on attempt {attempt}: {bestCandidate.Text} " +
                        $"Score: {bestCandidate.Score:F1}"
                    );

                    return bestCandidate.Text.Trim();
                }

                Debug.LogWarning($"Reaction attempt {attempt}: no valid candidates.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Reaction attempt {attempt} failed: {ex.Message}");
            }
        }

        Debug.LogWarning("All reaction attempts failed. Used fallback.");

        return _fallbackGenerator.GetFallbackReaction(result.FinalScore, profile);
    }

    private DialogueCandidate SelectBestOrderCandidate(
        string rawResponse,
        NPCProfile profile,
        OrderContext order)
    {
        List<string> rawCandidates =
            _candidateParser.ParseCandidates(rawResponse);

        DialogueCandidate best = null;

        foreach (string rawCandidate in rawCandidates)
        {
            DialogueCandidate candidate =
                _verifier.VerifyOrderCandidate(rawCandidate, profile, order);

            if (!candidate.IsValid)
            {
                Debug.Log(
                    $"Rejected order candidate: {candidate.Text}. " +
                    $"Reason: {candidate.RejectReason}"
                );

                continue;
            }

            if (best == null || candidate.Score > best.Score)
                best = candidate;
        }

        return best;
    }

    private DialogueCandidate SelectBestReactionCandidate(
        string rawResponse,
        NPCProfile profile,
        CocktailEvaluationResult result)
    {
        List<string> rawCandidates =
            _candidateParser.ParseCandidates(rawResponse);

        DialogueCandidate best = null;

        foreach (string rawCandidate in rawCandidates)
        {
            DialogueCandidate candidate =
                _verifier.VerifyReactionCandidate(rawCandidate, profile, result);

            if (!candidate.IsValid)
            {
                Debug.Log(
                    $"Rejected reaction candidate: {candidate.Text}. " +
                    $"Reason: {candidate.RejectReason}"
                );

                continue;
            }

            if (best == null || candidate.Score > best.Score)
                best = candidate;
        }

        return best;
    }

    private DialogueResult CreateFallbackOrderResult(
        NPCProfile profile,
        OrderContext order,
        string prompt)
    {
        return new DialogueResult
        {
            NPCLine = _fallbackGenerator.GetFallbackOrderHint(profile, order),
            SystemPromptUsed = prompt,
            UsedFallback = true
        };
    }
    private async Task<string> TryGenerateWithTimeoutAsync(string prompt)
    {
        Task<string> generationTask = _provider.GenerateTextAsync(prompt);
        Task timeoutTask = Task.Delay(TimeoutMs);

        Task completedTask = await Task.WhenAny(generationTask, timeoutTask);

        if (completedTask == timeoutTask)
            throw new TimeoutException("LLM request timeout.");

        return await generationTask;
    }
}