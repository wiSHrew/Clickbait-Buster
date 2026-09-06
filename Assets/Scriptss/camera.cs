using UnityEngine;
using UnityEngine.InputSystem;

public class camera : MonoBehaviour
{
    [Header("FNAF-Style Camera Settings")]
    [SerializeField] private float maxLeftAngle = 45f;
    [SerializeField] private float leftThreshold = 0.25f;
    [SerializeField] private float rightThreshold = 0.75f;
    [SerializeField] private float smoothSpeed = 5f;
    
    private float anchorYRotation;
    private float currentYRotation = 0f;
    private float targetYRotation = 0f;
    
    // Track the last known angle when in the left zone
    private float lastLeftAngle = 0f;
    private bool isInLeftZone = false;

    void OnEnable()
    {
        transform.localRotation = Quaternion.identity;
        anchorYRotation = transform.parent.eulerAngles.y;
        currentYRotation = 0f;
        targetYRotation = 0f;
        lastLeftAngle = 0f;
        isInLeftZone = false;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        // Get mouse position in screen space (0 to 1)
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        float normalizedX = mousePosition.x / Screen.width;

        // --- DETERMINE TARGET ROTATION ---
        float targetAngle = currentYRotation; // Default: hold current position

        if (normalizedX < leftThreshold)
        {
            // Mouse is on the LEFT side
            isInLeftZone = true;
            
            // Calculate how far into the left zone (0 to 1)
            // 0 = at threshold (no rotation), 1 = at far left (max rotation)
            float leftProgress = 1f - (normalizedX / leftThreshold);
            leftProgress = Mathf.Clamp01(leftProgress);
            
            // Scale to max left angle (NEGATIVE = look LEFT)
            float newAngle = -leftProgress * maxLeftAngle;
            
            // ONLY update the target if the new angle is MORE LEFT (more negative)
            // This prevents the camera from rotating right while in the left zone
            if (newAngle < lastLeftAngle)
            {
                // Moving further left - update target
                lastLeftAngle = newAngle;
                targetAngle = newAngle;
            }
            else
            {
                // Moving right or staying still - hold the last left angle
                targetAngle = lastLeftAngle;
            }
        }
        else if (normalizedX > rightThreshold)
        {
            // Mouse is on the RIGHT side - return to anchor
            isInLeftZone = false;
            lastLeftAngle = 0f;
            targetAngle = 0f;
        }
        else
        {
            // Mouse is in the middle zone (25% - 75%)
            // If we were in the left zone and moved out, keep the current angle
            // This is the "hold position" behavior
            if (isInLeftZone)
            {
                // We just left the left zone - keep the last angle
                targetAngle = lastLeftAngle;
            }
            else
            {
                // We're in the middle zone normally - hold current position
                targetAngle = currentYRotation;
            }
        }

        // Store the target rotation
        targetYRotation = targetAngle;

        // Smoothly interpolate current rotation toward target
        currentYRotation = Mathf.Lerp(currentYRotation, targetYRotation, Time.deltaTime * smoothSpeed);

        // Apply rotation to parent
        transform.parent.rotation = Quaternion.Euler(
            0f,
            anchorYRotation + currentYRotation,
            0f
        );
    }

    // Optional: Visualize the zones in the editor
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        // Draw left zone (red)
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawLine(new Vector3(0, 0, 0), new Vector3(Screen.width * leftThreshold, Screen.height, 0));
        
        // Draw middle zone (yellow)
        Gizmos.color = new Color(1, 1, 0, 0.15f);
        Gizmos.DrawLine(new Vector3(Screen.width * leftThreshold, 0, 0), new Vector3(Screen.width * rightThreshold, Screen.height, 0));
        
        // Draw right zone (blue)
        Gizmos.color = new Color(0, 0, 1, 0.3f);
        Gizmos.DrawLine(new Vector3(Screen.width * rightThreshold, 0, 0), new Vector3(Screen.width, Screen.height, 0));
    }
}