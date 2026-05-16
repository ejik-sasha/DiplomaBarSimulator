using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField] private NPCClient npcClient;
    [SerializeField] private BartendingManager bartendingManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private RecipeBookUI recipeBookUI;
    [SerializeField] private ShopUI shopUI;

    [Header("LLM settings")]
    [SerializeField] private LLMSettings llmSettings;

    private DialogueManager _dialogueManager;
    private DataRepository _dataRepository;

    private void Start()
    {
        _dataRepository = new DataRepository();
        _dataRepository.LoadAll();

        ILLMProvider provider = LLMProviderFactory.Create(llmSettings);
        _dialogueManager = new DialogueManager(provider);

        npcClient.Initialize(
            _dialogueManager,
            _dataRepository.NPCProfiles,
            _dataRepository.Recipes,
            uiManager
        );
        if (shopUI != null)
            shopUI.Initialize(_dataRepository);
        if (recipeBookUI != null)
            recipeBookUI.Initialize(_dataRepository);

        uiManager.SetStatusText($"Провайдер: {llmSettings.ProviderType}");
        uiManager.SetNPCText("Подойди к клиенту и нажми ЛКМ.");
        uiManager.ClearScore();

    }
}