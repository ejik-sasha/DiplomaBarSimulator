using UnityEngine;

public class StirMiniGame : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BartendingManager bartendingManager;
    [SerializeField] private GlassPourReceiver glassPourReceiver;
    [SerializeField] private UIManager uiManager;

    [Header("Spoon Focus Pose")]
    [SerializeField] private Vector3 spoonBaseLocalPosition = new Vector3(0f, -0.15f, 0.55f);
    [SerializeField] private Vector3 spoonFocusEulerAngles = new Vector3(0f, 0f, 0f);

    [Header("Cursor Circle Area")]
    [SerializeField] private float screenCircleRadius = 170f;
    [SerializeField] private float minCursorRadius = 45f;

    [Header("Spoon Movement")]
    [SerializeField] private float spoonMoveRadius = 0.18f;
    [SerializeField] private float spoonDepthOffset = 0f;

    [Header("Stir Logic")]
    [SerializeField] private float angleNeededForOneStir = 360f;
    [SerializeField] private float minAngleDelta = 0.5f;
    [SerializeField] private float maxAngleDelta = 80f;
    [Header("Spoon In Glass")]
    [SerializeField] private Transform spoonStirPoint;
    [SerializeField] private Vector3 spoonWorldEulerOffset = Vector3.zero;


    private HoldableObject _activeSpoon;
    private bool _isActive;

    private float _previousAngle;
    private float _accumulatedAngle;
    private int _completedStirs;

    public bool IsActive => _isActive;

    private void Awake()
    {
        if (bartendingManager == null)
            bartendingManager = FindFirstObjectByType<BartendingManager>();

        if (glassPourReceiver == null)
            glassPourReceiver = GetComponentInChildren<GlassPourReceiver>();

        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();
    }

    private void Update()
    {
        if (!_isActive || _activeSpoon == null)
            return;

        UpdateSpoonPositionFromCursor();
        TrackCircularMotion();

        if (Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.S) ||
            Input.GetKeyDown(KeyCode.D) ||
            Input.GetKeyDown(KeyCode.Space))
        {
            StopStirring();
        }
    }

    public void StartStirring(HoldableObject spoon)
    {
        if (spoon == null)
            return;

        _activeSpoon = spoon;
        _isActive = true;

        _accumulatedAngle = 0f;
        _completedStirs = 0;
        _previousAngle = GetCurrentCursorAngle();

        if (spoonStirPoint != null)
        {
            Quaternion spoonRotation =
                spoonStirPoint.rotation * Quaternion.Euler(spoonWorldEulerOffset);

            _activeSpoon.SnapToWorldPose(
                spoonStirPoint.position,
                spoonRotation
            );
        }
        else
        {
            _activeSpoon.SnapToFocusPose(
                spoonBaseLocalPosition,
                spoonFocusEulerAngles
            );
        }

        if (uiManager != null)
        {
            uiManager.ShowFocusHint(
                "[КРУГИ МЫШЬЮ] мешать\n[WASD/Space] выйти"
            );
        }
    }

    public void StopStirring()
    {
        if (!_isActive)
            return;

        _isActive = false;

        if (_activeSpoon != null)
            _activeSpoon.ExitFocusPose();

        _activeSpoon = null;
        _accumulatedAngle = 0f;

        if (uiManager != null)
            uiManager.HideFocusHint();
    }

    private void UpdateSpoonPositionFromCursor()
    {
        if (spoonStirPoint == null || _activeSpoon == null)
            return;

        Vector2 offset = GetClampedCursorOffset();

        float normalizedX = offset.x / screenCircleRadius;
        float normalizedY = offset.y / screenCircleRadius;

        Vector3 worldOffset =
            spoonStirPoint.right * (normalizedX * spoonMoveRadius) +
            spoonStirPoint.forward * (normalizedY * spoonMoveRadius);

        Vector3 targetWorldPosition = spoonStirPoint.position + worldOffset;

        Quaternion targetWorldRotation =
            spoonStirPoint.rotation * Quaternion.Euler(spoonWorldEulerOffset);

        _activeSpoon.SetWorldPoseTarget(
            targetWorldPosition,
            targetWorldRotation
        );
    }

    private void TrackCircularMotion()
    {
        Vector2 offset = GetCursorOffsetFromCenter();

        if (offset.magnitude < minCursorRadius)
            return;

        float currentAngle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;

        float delta = Mathf.DeltaAngle(_previousAngle, currentAngle);

        _previousAngle = currentAngle;

        float absDelta = Mathf.Abs(delta);

        if (absDelta < minAngleDelta || absDelta > maxAngleDelta)
            return;

        _accumulatedAngle += absDelta;

        if (_accumulatedAngle >= angleNeededForOneStir)
        {
            _accumulatedAngle -= angleNeededForOneStir;
            RegisterOneStir();
        }
    }

    private void RegisterOneStir()
    {
        _completedStirs++;

        if (bartendingManager != null)
        {
            bartendingManager.ApplyAction(
                new CocktailAction(
                    CocktailActionType.Stir,
                    "stir",
                    1f
                )
            );
        }

        if (glassPourReceiver != null)
        {
            glassPourReceiver.MixVisual();
        }

        if (uiManager != null)
        {
            uiManager.ShowFocusHint(
                $"[КРУГИ МЫШЬЮ] мешать\nСмешиваний: {_completedStirs}\n[WASD/Space] выйти"
            );
        }

        Debug.Log($"Stir completed. Total: {_completedStirs}");
    }

    private Vector2 GetCursorOffsetFromCenter()
    {
        Vector2 screenCenter = new Vector2(
            Screen.width * 0.5f,
            Screen.height * 0.5f
        );

        return (Vector2)Input.mousePosition - screenCenter;
    }

    private Vector2 GetClampedCursorOffset()
    {
        Vector2 offset = GetCursorOffsetFromCenter();

        if (offset.magnitude > screenCircleRadius)
            offset = offset.normalized * screenCircleRadius;

        return offset;
    }

    private float GetCurrentCursorAngle()
    {
        Vector2 offset = GetCursorOffsetFromCenter();

        if (offset.magnitude < 0.01f)
            return 0f;

        return Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
    }
}