using UnityEngine;
using UnityEngine.UI;

public class Shaker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BartendingManager bartendingManager;
    [SerializeField] private GlassPourReceiver glassPourReceiver;
    [SerializeField] private UIManager uiManager;

    [Header("Shake Mini Game")]
    [SerializeField] private float minMouseDelta = 0.15f;
    [SerializeField] private float strokeDistanceNeeded = 10f;
    [SerializeField] private int strokesNeeded = 4;

    [Header("Visual Shake Movement")]
    [SerializeField] private float glassMoveDistance = 0.35f;
    [SerializeField] private float glassMoveSmoothness = 12f;
    [SerializeField] private float mouseToMoveMultiplier = 0.08f;

    [Header("Attach")]
    [SerializeField] private Vector3 localAttachPosition = Vector3.zero;
    [SerializeField] private Vector3 localAttachEulerAngles = Vector3.zero;

    [Header("UI")]
    [SerializeField] private GameObject shakeProgressPanel;
    [SerializeField] private Slider shakeProgressSlider;
    private bool _hasStartedMoving;

    private Rigidbody _rb;
    private Collider[] _colliders;

    private Transform _originalParent;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    private HoldableObject _activeGlass;
    private Transform _activeGlassTransform;

    private Vector3 _glassStartLocalPosition;
    private Vector3 _glassTargetLocalPosition;

    private bool _isShaking;

    private float _currentStrokeDistance;
    private int _completedStrokes;
    private int _lastDirection;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _colliders = GetComponentsInChildren<Collider>();

        if (bartendingManager == null)
            bartendingManager = FindFirstObjectByType<BartendingManager>();

        if (glassPourReceiver == null)
            glassPourReceiver = FindFirstObjectByType<GlassPourReceiver>();

        if (uiManager == null)
            uiManager = FindFirstObjectByType<UIManager>();

        _originalParent = transform.parent;
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;

        HideShakeProgress();
    }

    private void Update()
    {
        if (!_isShaking)
            return;

        if (Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.S) ||
            Input.GetKeyDown(KeyCode.D) ||
            Input.GetKeyDown(KeyCode.Space))
        {
            CancelShakeMode();
            return;
        }

        UpdateShakeByMouse();
        UpdateGlassVisualMovement();
    }

    public void StartShakeMode(HoldableObject heldGlass)
    {
        if (_isShaking)
            return;

        if (heldGlass == null || heldGlass.ItemType != ItemType.Glass)
        {
            if (uiManager != null)
                uiManager.SetStatusText("Чтобы использовать шейкер, возьмите стакан в руку.");

            return;
        }

        _activeGlass = heldGlass;
        _activeGlassTransform = heldGlass.transform;

        _isShaking = true;

        _currentStrokeDistance = 0f;
        _completedStrokes = 0;
        _lastDirection = 0;

        _glassStartLocalPosition = _activeGlassTransform.localPosition;
        _glassTargetLocalPosition = _glassStartLocalPosition;

        AttachToGlass(heldGlass);

        CursorLockController.SetCameraFocusMode(true);

        _hasStartedMoving = false;
        HideShakeProgress();

        if (uiManager != null)
        {
            uiManager.ShowFocusHint(
                "[МЫШЬ ВВЕРХ-ВНИЗ] встряхнуть\n" +
                "[WASD/Space] отменить"
            );

            uiManager.SetStatusText("Шейкер надет на стакан. Встряхните напиток.");
        }
    }

   private void UpdateShakeByMouse()
    {
        float mouseY = Input.GetAxisRaw("Mouse Y");

        if (Mathf.Abs(mouseY) < minMouseDelta)
            return;

        if (!_hasStartedMoving)
        {
            _hasStartedMoving = true;
            ShowShakeProgress();
            UpdateShakeProgress();
        }

        int direction = mouseY > 0f ? 1 : -1;

        if (_lastDirection == 0)
            _lastDirection = direction;

        _currentStrokeDistance += Mathf.Abs(mouseY);

        if (direction != _lastDirection)
        {
            if (_currentStrokeDistance >= strokeDistanceNeeded)
            {
                RegisterStroke();
            }

            _currentStrokeDistance = 0f;
            _lastDirection = direction;
        }

        float visualOffsetY = Mathf.Clamp(
            mouseY * mouseToMoveMultiplier,
            -glassMoveDistance,
            glassMoveDistance
        );

        _glassTargetLocalPosition = _glassStartLocalPosition + Vector3.up * visualOffsetY;
    }

    private void UpdateGlassVisualMovement()
    {
        if (_activeGlassTransform == null)
            return;

        _activeGlassTransform.localPosition = Vector3.Lerp(
            _activeGlassTransform.localPosition,
            _glassTargetLocalPosition,
            Time.deltaTime * glassMoveSmoothness
        );
    }

    private void RegisterStroke()
    {
        _completedStrokes++;
        UpdateShakeProgress();

        if (uiManager != null)
        {
            uiManager.ShowFocusHint(
                "[МЫШЬ ВВЕРХ-ВНИЗ] встряхнуть\n" +
                $"Прогресс: {_completedStrokes}/{strokesNeeded}\n" +
                "[WASD/Space] отменить"
            );
        }
        AudioManager.Instance?.PlayShaker();

        if (_completedStrokes >= strokesNeeded)
        {
            CompleteShake();
        }
    }

    private void CompleteShake()
    {
        if (bartendingManager != null)
        {
            bartendingManager.ApplyAction(
                new CocktailAction(
                    CocktailActionType.Shake,
                    "shake",
                    1f
                )
            );
        }

        if (glassPourReceiver != null)
            glassPourReceiver.ShakeVisual();

        if (uiManager != null)
            uiManager.SetStatusText("Вы встряхнули коктейль.");

        StopShakeMode();
    }

    private void CancelShakeMode()
    {
        if (uiManager != null)
            uiManager.SetStatusText("Встряхивание отменено.");

        StopShakeMode();
    }

    private void StopShakeMode()
    {
        _isShaking = false;

        if (_activeGlassTransform != null)
        {
            _activeGlassTransform.localPosition = _glassStartLocalPosition;
        }

        _activeGlass = null;
        _activeGlassTransform = null;

        CursorLockController.SetCameraFocusMode(false);

        HideShakeProgress();

        if (uiManager != null)
            uiManager.HideFocusHint();

        ReturnToOriginalPlace();
    }

    private void AttachToGlass(HoldableObject heldGlass)
    {
        ShakerAttachPoint attachPoint =
            heldGlass.GetComponentInChildren<ShakerAttachPoint>();

        Transform target = attachPoint != null
            ? attachPoint.transform
            : heldGlass.transform;

        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        SetCollidersEnabled(false);

        transform.SetParent(target);
        transform.localPosition = localAttachPosition;
        transform.localRotation = Quaternion.Euler(localAttachEulerAngles);
    }

    private void ReturnToOriginalPlace()
    {
        transform.SetParent(_originalParent);
        transform.position = _originalPosition;
        transform.rotation = _originalRotation;

        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        SetCollidersEnabled(true);
    }

    private void ShowShakeProgress()
    {
        if (shakeProgressPanel != null)
            shakeProgressPanel.SetActive(true);

        if (shakeProgressSlider != null)
        {
            shakeProgressSlider.minValue = 0f;
            shakeProgressSlider.maxValue = strokesNeeded;
            shakeProgressSlider.value = 0f;
        }
    }

    private void HideShakeProgress()
    {
        if (shakeProgressPanel != null)
            shakeProgressPanel.SetActive(false);

        if (shakeProgressSlider != null)
            shakeProgressSlider.value = 0f;
    }

    private void UpdateShakeProgress()
    {
        if (shakeProgressSlider != null)
            shakeProgressSlider.value = _completedStrokes;
    }

    private void SetCollidersEnabled(bool enabled)
    {
        if (_colliders == null)
            return;

        foreach (Collider col in _colliders)
        {
            if (col != null)
                col.enabled = enabled;
        }
    }
}