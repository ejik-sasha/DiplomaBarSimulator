using System.Collections.Generic;
using UnityEngine;

public class NPCVisualController : MonoBehaviour
{
    [System.Serializable]
    private class NPCModelBinding
    {
        public string ModelId;
        public GameObject ModelObject;
    }

    [Header("Models")]
    [SerializeField] private List<NPCModelBinding> models = new();

    public void ApplyModel(string modelId)
    {
        HideAllModels();

        if (string.IsNullOrWhiteSpace(modelId))
        {
            Debug.LogWarning("NPCVisualController: ModelId is empty.");
            return;
        }

        foreach (NPCModelBinding binding in models)
        {
            if (binding == null || binding.ModelObject == null)
                continue;

            if (binding.ModelId == modelId)
            {
                binding.ModelObject.SetActive(true);
                return;
            }
        }

        Debug.LogWarning($"NPCVisualController: model '{modelId}' was not found.");
    }

    private void HideAllModels()
    {
        foreach (NPCModelBinding binding in models)
        {
            if (binding != null && binding.ModelObject != null)
                binding.ModelObject.SetActive(false);
        }
    }
}