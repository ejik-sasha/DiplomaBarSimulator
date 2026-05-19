using UnityEngine;

public class GlassItemReceiver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BartendingManager bartendingManager;

    [Header("Garnish")]
    [SerializeField] private Transform garnishParent;
    [SerializeField] private Transform[] garnishPoints;

    private int _garnishCount;

    private void Awake()
    {
        if (bartendingManager == null)
            bartendingManager = FindFirstObjectByType<BartendingManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        IceCube ice = other.GetComponentInParent<IceCube>();

        if (ice != null)
        {
            RegisterIce(ice);
        }
    }

    private void RegisterIce(IceCube ice)
    {
        if (ice == null)
            return;

        if (ice.IsRegistered)
            return;

        ice.MarkRegistered();
        ice.transform.SetParent(transform);

        if (bartendingManager != null)
        {
            bartendingManager.ApplyAction(
                new CocktailAction(
                    CocktailActionType.AddIce,
                    "ice",
                    1f
                )
            );
        }
        AudioManager.Instance?.PlayIce();

        Debug.Log("Ice added to glass.");
    }

    public void AddGarnish(Garnish garnish)
    {
        if (garnish == null)
            return;

        if (bartendingManager != null)
        {
            bartendingManager.ApplyAction(
                new CocktailAction(
                    CocktailActionType.AddGarnish,
                    garnish.GarnishId,
                    1f
                )
            );
        }

        PlaceGarnishOnGlass(garnish.gameObject);
    }

    public void ClearVisuals()
    {
        ClearChildren(garnishParent);

        IceCube[] iceCubes = GetComponentsInChildren<IceCube>();

        foreach (IceCube ice in iceCubes)
        {
            if (ice != null)
                Destroy(ice.gameObject);
        }

        _garnishCount = 0;
    }

    private void PlaceGarnishOnGlass(GameObject garnishObject)
    {
        if (garnishObject == null)
            return;

        if (garnishPoints == null || garnishPoints.Length == 0)
        {
            Debug.LogWarning("GlassItemReceiver: garnishPoints are not assigned.");
            return;
        }

        int index = Mathf.Clamp(_garnishCount, 0, garnishPoints.Length - 1);
        Transform point = garnishPoints[index];
        _garnishCount++;

        Rigidbody rb = garnishObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider[] colliders = garnishObject.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
            col.enabled = false;

        Transform parent = garnishParent != null ? garnishParent : point;

        garnishObject.transform.SetParent(parent);
        garnishObject.transform.position = point.position;
        garnishObject.transform.rotation = point.rotation;
    }

    private void ClearChildren(Transform parent)
    {
        if (parent == null)
            return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }
}