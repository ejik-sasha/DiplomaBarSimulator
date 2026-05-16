using System.Collections.Generic;
using UnityEngine;

public class NPCClient : MonoBehaviour
{
    public NPCProfile CurrentProfile;
    public OrderContext CurrentOrder;

    public bool HasGivenOrder => _hasShownOrder;

    private DialogueManager _dialogueManager;
    private NPCGenerator _npcGenerator;
    private UIManager _uiManager;
    private List<RecipeData> _recipes;
    private List<NPCProfile> _profiles;

    private bool _isGenerating = false;
    private bool _isReady = false;
    private bool _isServed = false;
    private bool _hasShownOrder = false;

    private string _cachedOrderLine = "";
    private bool _usedFallback = false;

    public void Initialize(
        DialogueManager dialogueManager,
        List<NPCProfile> profiles,
        List<RecipeData> recipes,
        UIManager uiManager)
    {
        _dialogueManager = dialogueManager;
        _npcGenerator = new NPCGenerator();
        _uiManager = uiManager;
        _profiles = profiles;
        _recipes = recipes;

        SpawnNewClient();
    }

    public void SpawnNewClient()
    {
        CurrentProfile = _npcGenerator.GenerateRandomProfile(_profiles);
        CurrentOrder = _npcGenerator.GenerateOrderForProfile(CurrentProfile, _recipes);

        _isGenerating = false;
        _isReady = false;
        _isServed = false;
        _hasShownOrder = false;
        _cachedOrderLine = "";
        _usedFallback = false;

        gameObject.SetActive(true);

        _uiManager.SetNPCText("К вам подошёл новый клиент. Нажмите ПКМ, чтобы принять заказ.");
        _uiManager.SetStatusText("Новый клиент...");
        _uiManager.ClearScore();

        GenerateOrderLineOnSpawn();
    }

    private async void GenerateOrderLineOnSpawn()
    {
        if (_isGenerating || _dialogueManager == null)
            return;

        _isGenerating = true;

        _uiManager.SetStatusText("Клиент формулирует заказ...");

        DialogueResult result =
            await _dialogueManager.GenerateOrderDialogueAsync(CurrentProfile, CurrentOrder);

        _cachedOrderLine = result.NPCLine;
        _usedFallback = result.UsedFallback;

        _isReady = true;
        _isGenerating = false;

        _uiManager.SetNPCText("Клиент готов сделать заказ. Нажмите ПКМ.");
        _uiManager.SetStatusText(
            _usedFallback
                ? "Клиент готов. Реплика подготовлена через fallback."
                : "Клиент готов. Реплика сгенерирована ИИ."
        );
    }

    public void RequestOrderDialogue()
    {
        if (_isServed)
        {
            _uiManager.SetNPCText("Я уже получил напиток.");
            return;
        }

        if (_isReady)
        {
            _hasShownOrder = true;

            _uiManager.SetNPCText(_cachedOrderLine);
            _uiManager.SetStatusText(
                _usedFallback
                    ? "Заказ показан. Использован fallback."
                    : "Заказ показан. Реплика сгенерирована ИИ."
            );

            return;
        }

        if (_isGenerating)
        {
            _uiManager.SetNPCText("Подожди секунду, я ещё думаю...");
            _uiManager.SetStatusText("Реплика ещё генерируется.");
            return;
        }

        _uiManager.SetNPCText("Клиент ещё не готов сделать заказ.");
    }

    public async void ReactToDrink(CocktailEvaluationResult result)
    {
        if (_isServed)
            return;

        _isServed = true;

        _uiManager.SetStatusText("Клиент пробует напиток...");

        string reaction =
            await _dialogueManager.GenerateReactionAsync(
                CurrentProfile,
                CurrentOrder,
                result
            );

        _uiManager.SetNPCText($"{CurrentProfile.DisplayName}: {reaction}");
        _uiManager.SetStatusText("Клиент уходит...");

        Invoke(nameof(SpawnNewClient), 7f);
    }

    public RecipeData GetTargetRecipe()
    {
        return _recipes.Find(r => r.Id == CurrentOrder.RecipeId);
    }
}