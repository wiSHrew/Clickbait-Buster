using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class cursorclicker : MonoBehaviour
{
    void Update()
    {
        // Need a mouse
        if (Mouse.current == null) return;

        // Only on left-click press
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        // If the pointer is over any UI element (e.g., our buttons),
        // let the UI handle it and do NOT do a world raycast.
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // Get mouse position from the new Input System
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Main camera might not be tagged MainCamera in some scenes;
        // fall back to the camera this script lives on if needed.
        Camera cam = Camera.main;
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("Clicked: " + hit.collider.gameObject.name);

            objectclickable clickable = hit.collider.GetComponent<objectclickable>();
            if (clickable != null)
            {
                clickable.Click();
            }
        }
    }
}