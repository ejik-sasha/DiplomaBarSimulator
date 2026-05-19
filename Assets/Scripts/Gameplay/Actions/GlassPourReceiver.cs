using UnityEngine;

public class GlassPourReceiver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BartendingManager bartendingManager;
    [SerializeField] private GlassLayeredLiquidView layeredLiquidView;

    private void Awake()
    {
        if (bartendingManager == null)
            bartendingManager = FindFirstObjectByType<BartendingManager>();

        if (layeredLiquidView == null)
            layeredLiquidView = GetComponentInParent<GlassLayeredLiquidView>();
    }

    public void ReceivePour(string ingredientId, float amountMl, Color liquidColor)
    {
        if (string.IsNullOrWhiteSpace(ingredientId) || amountMl <= 0f)
            return;

        if (bartendingManager != null)
        {
            bartendingManager.AddPouredLiquid(ingredientId, amountMl);
        }
        else
        {
            Debug.LogError("GlassPourReceiver: BartendingManager is not assigned.");
        }

        if (layeredLiquidView != null)
        {
            layeredLiquidView.AddLayer(ingredientId, amountMl, liquidColor);
        }
        else
        {
            Debug.LogError("GlassPourReceiver: GlassLayeredLiquidView is not assigned or not found in parent.");
        }
    }

    public void ClearVisual()
    {
        if (layeredLiquidView != null)
            layeredLiquidView.Clear();
    }

    public void MixVisual()
    {
        if (layeredLiquidView != null)
            layeredLiquidView.MixAllLayers();
    }
    public void ShakeVisual()
    {
        if (layeredLiquidView != null)
            layeredLiquidView.ShakeMixAllLayers();
    }
}