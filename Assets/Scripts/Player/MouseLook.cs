using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 20f;
    [SerializeField] private Transform playerBody;

    private float _xRotation = 0f;

    private void Start()
    {
        //mouseSensitivity = GameSettings.MouseSensitivity;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        //mouseSensitivity = GameSettings.MouseSensitivity;
        if (Mouse.current == null)
            return;

        Vector2 delta = Mouse.current.delta.ReadValue();

        float mouseX = delta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = delta.y * mouseSensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}