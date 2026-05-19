using UnityEngine;

public class CameraFocusController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    [Header("Move To Focus")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float rotateSpeed = 7f;

    [Header("Mouse Pan Area")]
    [SerializeField] private float maxPanX = 0.45f;
    [SerializeField] private float maxPanZ = 0.35f;
    [SerializeField] private float panSmoothness = 8f;

    [Header("Exit")]
    [SerializeField] private bool exitOnMovementKeys = true;

    public bool IsFocused { get; private set; }

    private Vector3 _defaultLocalPosition;
    private Transform _focusPoint;
    private Vector3 _currentPanOffset;

    private bool _isReturning;

    private void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform != null)
        {
            _defaultLocalPosition = cameraTransform.localPosition;
        }
    }

    private void Update()
    {
        if (!IsFocused)
            return;

        if (exitOnMovementKeys && ShouldExitFocus())
        {
            ClearFocus();
        }
    }

    private void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        if (IsFocused && _focusPoint != null)
        {
            UpdateFocusCamera();
            return;
        }

        if (_isReturning)
        {
            ReturnToDefaultPosition();
        }
    }

    public void FocusOn(Transform focusPoint)
    {
        if (focusPoint == null)
            return;

        _focusPoint = focusPoint;
        _currentPanOffset = Vector3.zero;

        IsFocused = true;
        _isReturning = false;

        CursorLockController.SetCameraFocusMode(true);
    }

    public void ClearFocus()
    {
        IsFocused = false;
        _focusPoint = null;
        _currentPanOffset = Vector3.zero;

        _isReturning = true;

        CursorLockController.SetCameraFocusMode(false);
    }

    private void UpdateFocusCamera()
    {
        Vector3 targetPanOffset = GetMousePanOffset();

        _currentPanOffset = Vector3.Lerp(
            _currentPanOffset,
            targetPanOffset,
            Time.deltaTime * panSmoothness
        );

        Vector3 targetPosition = _focusPoint.position + _currentPanOffset;
        Quaternion targetRotation = _focusPoint.rotation;

        cameraTransform.position = Vector3.Lerp(
            cameraTransform.position,
            targetPosition,
            Time.deltaTime * moveSpeed
        );

        cameraTransform.rotation = Quaternion.Lerp(
            cameraTransform.rotation,
            targetRotation,
            Time.deltaTime * rotateSpeed
        );
    }

    private void ReturnToDefaultPosition()
    {
        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            _defaultLocalPosition,
            Time.deltaTime * moveSpeed
        );

        float distance = Vector3.Distance(
            cameraTransform.localPosition,
            _defaultLocalPosition
        );

        if (distance < 0.01f)
        {
            cameraTransform.localPosition = _defaultLocalPosition;
            _isReturning = false;
        }
    }

    private Vector3 GetMousePanOffset()
    {
        Vector3 mousePosition = Input.mousePosition;

        float normalizedX = Mathf.Clamp01(mousePosition.x / Screen.width);
        float normalizedY = Mathf.Clamp01(mousePosition.y / Screen.height);

        float x = (normalizedX - 0.5f) * 2f;
        float z = (normalizedY - 0.5f) * 2f;

        float offsetX = x * maxPanX;
        float offsetZ = z * maxPanZ;

        Vector3 right = _focusPoint.right;

        Vector3 tableForward = Vector3.ProjectOnPlane(_focusPoint.up, Vector3.up).normalized;

        if (tableForward.sqrMagnitude < 0.001f)
            tableForward = Vector3.forward;

        return right * offsetX + tableForward * offsetZ;
    }

    private bool ShouldExitFocus()
    {
        return Input.GetKeyDown(KeyCode.W) ||
               Input.GetKeyDown(KeyCode.A) ||
               Input.GetKeyDown(KeyCode.S) ||
               Input.GetKeyDown(KeyCode.D) ||
               Input.GetKeyDown(KeyCode.Space);
    }
}