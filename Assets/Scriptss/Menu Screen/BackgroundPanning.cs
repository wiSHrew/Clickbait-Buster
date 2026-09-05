using UnityEngine;
using UnityEngine.UI;

public class BackgroundPanning : MonoBehaviour
{
    [Header("Background Settings")]
    [SerializeField] private RectTransform backgroundRect;
    [SerializeField] private float panAmount = 50f; // Maximum pixels to pan
    [SerializeField] private float smoothSpeed = 5f; // Lerp smoothing speed
    
    private Vector2 initialPosition;
    private Vector2 targetPosition;
    private Canvas canvas;
    private RectTransform canvasRect;
    
    void Start()
    {
        // Store initial position
        if (backgroundRect == null)
            backgroundRect = GetComponent<RectTransform>();
            
        initialPosition = backgroundRect.anchoredPosition;
        targetPosition = initialPosition;
        
        // Get canvas reference
        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
    }
    
    void Update()
    {
        UpdateTargetPosition();
        SmoothMove();
    }
    
    void UpdateTargetPosition()
    {
        // Get mouse position in screen space
        Vector2 mousePosition = Input.mousePosition;
        
        // Convert to normalized coordinates (0 to 1)
        float normalizedX = mousePosition.x / Screen.width;
        float normalizedY = mousePosition.y / Screen.height;
        
        // Convert to -1 to 1 range
        float offsetX = (normalizedX * 2) - 1;
        float offsetY = (normalizedY * 2) - 1;
        
        // Calculate target position (inverted so mouse left = view left)
        targetPosition = new Vector2(
            initialPosition.x - (offsetX * panAmount),
            initialPosition.y - (offsetY * panAmount)
        );
    }
    
    void SmoothMove()
    {
        // Smoothly interpolate to target position
        backgroundRect.anchoredPosition = Vector2.Lerp(
            backgroundRect.anchoredPosition,
            targetPosition,
            Time.deltaTime * smoothSpeed
        );
    }
}