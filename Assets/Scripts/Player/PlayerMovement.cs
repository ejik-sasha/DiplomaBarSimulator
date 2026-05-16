using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody _rb;
    private Vector3 _movementInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (CursorLockController.IsUIOpen || 
        CursorLockController.IsInteractionLocked ||
        CursorLockController.IsCursorUnlocked)
        {
            _movementInput = Vector3.zero;
            return;
        }
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 forward = transform.forward * moveZ;
        Vector3 right = transform.right * moveX;

        _movementInput = (forward + right).normalized;
    }

    private void FixedUpdate()
    {
        Vector3 targetPosition = _rb.position + _movementInput * moveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(targetPosition);
    }
}