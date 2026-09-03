using UnityEngine;
using UnityEngine.InputSystem;

public class camera : MonoBehaviour
{
    public float mouseSensitivity = .5f;
    public float xRotation = 0f;
    public Vector3 screenPosition;

    void OnEnable()
    {
        Vector3 rot = transform.localEulerAngles;
        xRotation = rot.x;

        if (xRotation > 180f)
            xRotation -= 360f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Get mouse position
        screenPosition = Mouse.current.position.ReadValue();

        // Get mouse movement
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        // Vertical camera rotation
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Horizontal player rotation
        transform.parent.Rotate(Vector3.up * mouseX);
    }
}