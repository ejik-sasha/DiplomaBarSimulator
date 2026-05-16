using UnityEngine;

public class CursorLockController : MonoBehaviour
{
    public static bool IsUIOpen { get; private set; }
    public static bool IsInteractionLocked { get; private set; }
    public static bool IsCameraFocusMode { get; private set; }
    public static bool IsCursorUnlocked { get; private set; }

    private MonoBehaviour _mouseLookScript;

    private void Start()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera != null)
            _mouseLookScript = mainCamera.GetComponent<MouseLook>();

        LockCursor();
    }

    private void Update()
    {
        if (IsUIOpen)
        {
            UnlockCursor();
            return;
        }

        if (IsCameraFocusMode)
        {
            UnlockCursor();
            return;
        }

        if (IsInteractionLocked)
        {
            LockCursor();
            return;
        }

        if (Input.GetKey(KeyCode.LeftAlt) ||
            Input.GetKey(KeyCode.RightAlt) ||
            Input.GetKey(KeyCode.LeftControl))
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }

    public static void SetUIOpen(bool isOpen)
    {
        IsUIOpen = isOpen;
    }

    public static void SetInteractionLocked(bool locked)
    {
        IsInteractionLocked = locked;
    }

    public static void SetCameraFocusMode(bool active)
    {
        IsCameraFocusMode = active;
        IsInteractionLocked = active;
    }

    private void UnlockCursor()
    {
        IsCursorUnlocked = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (_mouseLookScript != null)
            _mouseLookScript.enabled = false;
    }

    private void LockCursor()
    {
        IsCursorUnlocked = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (_mouseLookScript != null)
            _mouseLookScript.enabled = true;
    }
}