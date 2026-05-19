using UnityEngine;

public class ItemSlot : MonoBehaviour
{
    [Header("Slot Settings")]
    [SerializeField] private ItemType acceptedType = ItemType.Any;
    [SerializeField] private Transform placePoint;
    [SerializeField] private HoldableObject startingItem;

    [Header("Highlight")]
    [SerializeField] private GameObject highlightObject;

    private HoldableObject _placedObject;

    public bool IsOccupied => _placedObject != null;

    private void Awake()
    {
        if (placePoint == null)
            placePoint = transform;

        SetHighlight(false);
    }

    private void Start()
    {
        if (startingItem != null)
        {
            ForcePlaceStartingItem();
        }
    }

    private void ForcePlaceStartingItem()
    {
        _placedObject = startingItem;
        startingItem.PlaceInSlot(this, placePoint);

        Debug.Log($"Starting item {startingItem.DisplayName} placed in slot {name}");
    }

    public bool CanAccept(HoldableObject item)
    {
        if (item == null)
            return false;

        if (IsOccupied)
            return false;

        return acceptedType == ItemType.Any || acceptedType == item.ItemType;
    }

    public bool Place(HoldableObject item)
    {
        if (!CanAccept(item))
        {
            Debug.LogWarning(
                $"Slot {name} cannot accept {item.DisplayName}. " +
                $"Slot type: {acceptedType}, item type: {item.ItemType}, occupied: {IsOccupied}"
            );

            return false;
        }

        _placedObject = item;
        item.PlaceInSlot(this, placePoint);
        SetHighlight(false);

        Debug.Log($"Placed {item.DisplayName} into slot {name}");

        return true;
    }

    public void ClearPlacedObject(HoldableObject item)
    {
        if (_placedObject == item)
        {
            _placedObject = null;
            Debug.Log($"Slot {name} is now empty");
        }
    }

    public void SetHighlight(bool active)
    {
        if (highlightObject != null)
            highlightObject.SetActive(active);
    }
}