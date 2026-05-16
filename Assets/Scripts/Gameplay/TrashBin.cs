using UnityEngine;

public class TrashBin : MonoBehaviour
{
    [SerializeField] private BartendingManager bartendingManager;

    private void Awake()
    {
        if (bartendingManager == null)
            bartendingManager = FindFirstObjectByType<BartendingManager>();
    }

    public void ClearGlass()
    {
        if (bartendingManager != null)
            bartendingManager.ResetGlass();
    }
}