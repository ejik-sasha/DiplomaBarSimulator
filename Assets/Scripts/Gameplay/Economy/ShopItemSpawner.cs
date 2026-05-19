using System.Collections.Generic;
using UnityEngine;

public class ShopItemSpawner : MonoBehaviour
{
    [System.Serializable]
    private class IngredientPrefabBinding
    {
        public string IngredientId;
        public GameObject Prefab;
    }

    [Header("Prefabs")]
    [SerializeField] private List<IngredientPrefabBinding> ingredientPrefabs = new();

    [Header("Spawn Slots")]
    [SerializeField] private List<ItemSlot> spawnSlots = new();

    public bool TrySpawnItem(string ingredientId)
    {
        GameObject prefab = GetPrefab(ingredientId);

        if (prefab == null)
        {
            Debug.LogWarning($"ShopItemSpawner: prefab for '{ingredientId}' not found.");
            return false;
        }

        HoldableObject prefabHoldable = prefab.GetComponent<HoldableObject>();

        if (prefabHoldable == null)
        {
            Debug.LogWarning($"ShopItemSpawner: prefab '{prefab.name}' has no HoldableObject.");
            return false;
        }

        ItemSlot freeSlot = FindFreeSlotFor(prefabHoldable);

        if (freeSlot == null)
        {
            Debug.LogWarning("ShopItemSpawner: no free slot for item.");
            return false;
        }

        GameObject itemObject = Instantiate(prefab);
        HoldableObject item = itemObject.GetComponent<HoldableObject>();

        if (item == null)
        {
            Destroy(itemObject);
            return false;
        }

        bool placed = freeSlot.Place(item);

        if (!placed)
        {
            Destroy(itemObject);
            return false;
        }

        return true;
    }

    private GameObject GetPrefab(string ingredientId)
    {
        foreach (var binding in ingredientPrefabs)
        {
            if (binding != null && binding.IngredientId == ingredientId)
                return binding.Prefab;
        }

        return null;
    }

    private ItemSlot FindFreeSlotFor(HoldableObject prefabHoldable)
    {
        foreach (ItemSlot slot in spawnSlots)
        {
            if (slot == null)
                continue;

            if (slot.CanAccept(prefabHoldable))
                return slot;
        }

        return null;
    }
}