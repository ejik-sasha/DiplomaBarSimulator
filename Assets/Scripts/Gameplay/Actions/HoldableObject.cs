using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HoldableObject : MonoBehaviour
{
    [Header("Info")]
    [SerializeField] private string displayName = "предмет";
    [SerializeField] private ItemType itemType = ItemType.Any;

    [Header("Holding")]
    [SerializeField] private Vector3 holdLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 holdLocalEulerAngles = Vector3.zero;

    [Header("Slot Placement")]
    [SerializeField] private bool autoPlaceOnSurface = true;
    [SerializeField] private Vector3 slotManualOffset = Vector3.zero;
    [SerializeField] private Vector3 slotLocalEulerAngles = Vector3.zero;

    [Header("Focus Holding")]
    [SerializeField] private Vector3 focusLocalPosition = new Vector3(0f, -0.15f, 0.55f);
    [SerializeField] private Vector3 focusLocalEulerAngles = new Vector3(0f, 0f, 0f);
    [SerializeField] private float focusMoveSpeed = 8f;

    public string DisplayName => displayName;
    public ItemType ItemType => itemType;
    public bool IsHeld { get; private set; }

    private Rigidbody _rb;
    private Collider[] _colliders;
    private Transform _originalParent;
    private ItemSlot _currentSlot;

    private float _bottomOffsetFromPivot;

    private Vector3 _targetLocalPosition;
    private Quaternion _targetLocalRotation;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _colliders = GetComponentsInChildren<Collider>();
        _originalParent = transform.parent;

        _bottomOffsetFromPivot = CalculateBottomOffsetFromPivot();
    }
    private void Update()
    {
        if (!IsHeld)
            return;

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            _targetLocalPosition,
            Time.deltaTime * focusMoveSpeed
        );

        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            _targetLocalRotation,
            Time.deltaTime * focusMoveSpeed
        );
    }

    public void PickUp(Transform holdPoint)
    {
        if (holdPoint == null)
            return;

        if (_currentSlot != null)
        {
            _currentSlot.ClearPlacedObject(this);
            _currentSlot = null;
        }

        IsHeld = true;

        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        SetCollidersEnabled(false);

        transform.SetParent(holdPoint);


        _targetLocalPosition = holdLocalPosition;
        _targetLocalRotation = Quaternion.Euler(holdLocalEulerAngles);

        transform.localPosition = holdLocalPosition;
        transform.localRotation = Quaternion.Euler(holdLocalEulerAngles);
    }

    public void Drop()
    {
        IsHeld = false;
        _currentSlot = null;

        transform.SetParent(_originalParent);

        SetCollidersEnabled(true);

        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
        }
    }

    public void PlaceInSlot(ItemSlot slot, Transform slotPoint)
    {
        if (slot == null || slotPoint == null)
            return;

        IsHeld = false;
        _currentSlot = slot;

        SetCollidersEnabled(true);

        transform.SetParent(slotPoint);

        Vector3 finalLocalPosition = slotManualOffset;

        if (autoPlaceOnSurface)
            finalLocalPosition += Vector3.up * _bottomOffsetFromPivot;

        transform.localPosition = finalLocalPosition;
        transform.localRotation = Quaternion.Euler(slotLocalEulerAngles);

        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }

    private float CalculateBottomOffsetFromPivot()
    {
        if (_colliders == null || _colliders.Length == 0)
            return 0f;

        Bounds bounds = _colliders[0].bounds;

        for (int i = 1; i < _colliders.Length; i++)
        {
            if (_colliders[i] != null)
                bounds.Encapsulate(_colliders[i].bounds);
        }

        float pivotY = transform.position.y;
        float bottomY = bounds.min.y;

        return pivotY - bottomY;
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
    public void EnterFocusPose()
    {
        if (!IsHeld)
            return;


        _targetLocalPosition = focusLocalPosition;
        _targetLocalRotation = Quaternion.Euler(focusLocalEulerAngles);
    }

    public void ExitFocusPose()
    {
        if (!IsHeld)
            return;


        _targetLocalPosition = holdLocalPosition;
        _targetLocalRotation = Quaternion.Euler(holdLocalEulerAngles);
    }
    public void Throw(Vector3 direction, float force)
    {
        IsHeld = false;
        _currentSlot = null;

        transform.SetParent(_originalParent);

        SetCollidersEnabled(true);

        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;

            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;

            _rb.AddForce(direction.normalized * force, ForceMode.Impulse);
            _rb.AddTorque(Random.insideUnitSphere * force * 0.4f, ForceMode.Impulse);
        }
    }
    public void SetFocusPoseTarget(Vector3 localPosition, Vector3 localEulerAngles)
    {
        if (!IsHeld)
            return;

        _targetLocalPosition = localPosition;
        _targetLocalRotation = Quaternion.Euler(localEulerAngles);
    }
    public void SnapToFocusPose(Vector3 localPosition, Vector3 localEulerAngles)
    {
        if (!IsHeld)
            return;

        _targetLocalPosition = localPosition;
        _targetLocalRotation = Quaternion.Euler(localEulerAngles);

        transform.localPosition = localPosition;
        transform.localRotation = Quaternion.Euler(localEulerAngles);
    }
    public void SetWorldPoseTarget(Vector3 worldPosition, Quaternion worldRotation)
    {
        if (!IsHeld)
            return;

        if (transform.parent == null)
            return;

        _targetLocalPosition = transform.parent.InverseTransformPoint(worldPosition);
        _targetLocalRotation = Quaternion.Inverse(transform.parent.rotation) * worldRotation;
    }

    public void SnapToWorldPose(Vector3 worldPosition, Quaternion worldRotation)
    {
        if (!IsHeld)
            return;

        if (transform.parent == null)
            return;

        _targetLocalPosition = transform.parent.InverseTransformPoint(worldPosition);
        _targetLocalRotation = Quaternion.Inverse(transform.parent.rotation) * worldRotation;

        transform.localPosition = _targetLocalPosition;
        transform.localRotation = _targetLocalRotation;
    }
}