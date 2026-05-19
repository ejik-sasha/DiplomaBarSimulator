using System.Collections;
using UnityEngine;

public class NPCMovementController : MonoBehaviour
{
    [Header("Movement Points")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform servicePoint;
    [SerializeField] private Transform exitPoint;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 1.8f;
    [SerializeField] private float rotateSpeed = 8f;
    [SerializeField] private float arriveDistance = 0.05f;

    private Coroutine _moveCoroutine;
    private bool _isMoving;

    public bool IsMoving => _isMoving;

    public void MoveFromSpawnToService(System.Action onArrived = null)
    {
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }

        MoveTo(servicePoint, onArrived);
    }

    public void MoveToExit(System.Action onExited = null)
    {
        MoveTo(exitPoint, onExited);
    }

    private void MoveTo(Transform targetPoint, System.Action onArrived = null)
    {
        if (targetPoint == null)
        {
            Debug.LogWarning("NPCMovementController: target point is not assigned.");
            onArrived?.Invoke();
            return;
        }

        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);

        _moveCoroutine = StartCoroutine(MoveRoutine(targetPoint, onArrived));
    }

    private IEnumerator MoveRoutine(Transform targetPoint, System.Action onArrived)
    {
        _isMoving = true;

        while (Vector3.Distance(transform.position, targetPoint.position) > arriveDistance)
        {
            Vector3 direction = targetPoint.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * rotateSpeed
                );
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPoint.position,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPoint.position;
        transform.rotation = targetPoint.rotation;

        _isMoving = false;
        _moveCoroutine = null;

        onArrived?.Invoke();
    }
}