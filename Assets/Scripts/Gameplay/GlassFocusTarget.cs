using UnityEngine;

public class GlassFocusTarget : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraFocusController cameraFocusController;
    [SerializeField] private Transform focusPoint;
    [SerializeField] private StirMiniGame stirMiniGame;

    public Transform FocusPoint => focusPoint;
    public StirMiniGame StirMiniGame => stirMiniGame;

    private void Awake()
    {
        if (cameraFocusController == null)
            cameraFocusController = FindFirstObjectByType<CameraFocusController>();

        if (stirMiniGame == null)
            stirMiniGame = GetComponent<StirMiniGame>();
    }

    public void FocusCamera()
    {
        if (cameraFocusController == null)
        {
            Debug.LogWarning("GlassFocusTarget: CameraFocusController is not assigned.");
            return;
        }

        if (focusPoint == null)
        {
            Debug.LogWarning("GlassFocusTarget: FocusPoint is not assigned.");
            return;
        }

        cameraFocusController.FocusOn(focusPoint);
    }

    public void StartStirMode(HoldableObject heldObject)
    {
        FocusCamera();

        if (heldObject == null)
            return;

        SpoonTool spoon = heldObject.GetComponent<SpoonTool>();

        if (spoon == null)
            spoon = heldObject.GetComponentInChildren<SpoonTool>();

        if (spoon == null)
            return;

        if (stirMiniGame != null)
            stirMiniGame.StartStirring(heldObject);
    }
}