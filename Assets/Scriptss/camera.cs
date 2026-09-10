using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class camera : MonoBehaviour
{
    [Header("FNAF-Style Camera Settings")]
    [SerializeField] private float maxLeftAngle = 45f;
    [SerializeField] private float leftThreshold = 0.25f;
    [SerializeField] private float rightThreshold = 0.75f;
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Look Down State")]
    [SerializeField] private float lookDownAngle = 70f;
    [SerializeField] private float downSmoothSpeed = 4f;
    [SerializeField] private float anchorSnapTolerance = 5f;
    [SerializeField] private Button lookDownButton;
    [SerializeField] private Button lookUpButton;

    [Header("Hover Trigger")]
    [Tooltip("If true, hovering the button also triggers it (in addition to clicking).")]
    [SerializeField] private bool hoverTriggers = true;
    [Tooltip("Small cooldown after a state change to prevent the hover from instantly re-firing.")]
    [SerializeField] private float stateChangeCooldown = 0.35f;

    private float anchorYRotation;
    private float currentYRotation = 0f;
    private float targetYRotation = 0f;
    private float currentXRotation = 0f;
    private float targetXRotation = 0f;

    private float lastLeftAngle = 0f;
    private bool isInLeftZone = false;
    private bool isLookingDown = false;

    // Cooldown so a hover right after a state change doesn't immediately re-trigger.
    private float lastStateChangeTime = -999f;

    // Hover state reported by the helper components on the buttons.
    private bool hoverDown = false;
    private bool hoverUp = false;

    void OnEnable()
    {
        transform.localRotation = Quaternion.identity;
        anchorYRotation = transform.parent.eulerAngles.y;
        currentYRotation = 0f;
        targetYRotation = 0f;
        currentXRotation = 0f;
        targetXRotation = 0f;
        lastLeftAngle = 0f;
        isInLeftZone = false;
        isLookingDown = false;
        hoverDown = false;
        hoverUp = false;

        if (lookDownButton != null)
        {
            lookDownButton.onClick.RemoveAllListeners();
            lookDownButton.onClick.AddListener(EnterLookDown);
        }
        if (lookUpButton != null)
        {
            lookUpButton.onClick.RemoveAllListeners();
            lookUpButton.onClick.AddListener(ExitLookDown);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        UpdateButtonVisibility();
        HandleHoverTriggers();

        if (isLookingDown)
        {
            currentXRotation = Mathf.Lerp(currentXRotation, targetXRotation, Time.deltaTime * downSmoothSpeed);
            ApplyRotation();
            return;
        }

        // ---- Normal free-look ----
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        float normalizedX = mousePosition.x / Screen.width;

        float targetAngle = currentYRotation;

        if (normalizedX < leftThreshold)
        {
            isInLeftZone = true;
            float leftProgress = Mathf.Clamp01(1f - (normalizedX / leftThreshold));
            float newAngle = -leftProgress * maxLeftAngle;
            if (newAngle < lastLeftAngle) { lastLeftAngle = newAngle; targetAngle = newAngle; }
            else targetAngle = lastLeftAngle;
        }
        else if (normalizedX > rightThreshold)
        {
            isInLeftZone = false;
            lastLeftAngle = 0f;
            targetAngle = 0f;
        }
        else
        {
            targetAngle = isInLeftZone ? lastLeftAngle : currentYRotation;
        }

        targetYRotation = targetAngle;
        currentYRotation = Mathf.Lerp(currentYRotation, targetYRotation, Time.deltaTime * smoothSpeed);
        currentXRotation = Mathf.Lerp(currentXRotation, 0f, Time.deltaTime * downSmoothSpeed);

        ApplyRotation();
    }

    // ---- Hover handling ----
    void HandleHoverTriggers()
    {
        if (!hoverTriggers) return;

        // Cooldown guard
        if (Time.time - lastStateChangeTime < stateChangeCooldown) return;

        // Hover on the bottom (LookDown) button while normal -> look down
        if (!isLookingDown && hoverDown && lookDownButton != null && lookDownButton.gameObject.activeInHierarchy)
        {
            EnterLookDown();
            return;
        }

        // Hover on the top (LookUp) button while looking down -> look up
        if (isLookingDown && hoverUp && lookUpButton != null && lookUpButton.gameObject.activeInHierarchy)
        {
            ExitLookDown();
            return;
        }
    }

    // Called by HoverTrigger components on the buttons.
    public void SetHoverDown(bool value) => hoverDown = value;
    public void SetHoverUp(bool value)   => hoverUp = value;

    void ApplyRotation()
    {
        transform.parent.rotation = Quaternion.Euler(
            currentXRotation,
            anchorYRotation + currentYRotation,
            0f
        );
    }

    void UpdateButtonVisibility()
    {
        bool nearAnchor = Mathf.Abs(Mathf.DeltaAngle(currentYRotation, 0f)) < anchorSnapTolerance;

        if (lookDownButton != null)
            lookDownButton.gameObject.SetActive(!isLookingDown && nearAnchor);

        if (lookUpButton != null)
            lookUpButton.gameObject.SetActive(isLookingDown);
    }

    public void EnterLookDown()
    {
        if (isLookingDown) return;
        isLookingDown = true;
        targetXRotation = lookDownAngle;
        targetYRotation = 0f;
        lastStateChangeTime = Time.time;

        // Clear hover flags so the same hover doesn't immediately bounce us back.
        hoverDown = false;
        hoverUp = false;
    }

    public void ExitLookDown()
    {
        if (!isLookingDown) return;
        isLookingDown = false;
        targetXRotation = 0f;
        lastStateChangeTime = Time.time;

        hoverDown = false;
        hoverUp = false;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawLine(Vector3.zero, new Vector3(Screen.width * leftThreshold, Screen.height, 0));
        Gizmos.color = new Color(1, 1, 0, 0.15f);
        Gizmos.DrawLine(new Vector3(Screen.width * leftThreshold, 0, 0), new Vector3(Screen.width * rightThreshold, Screen.height, 0));
        Gizmos.color = new Color(0, 0, 1, 0.3f);
        Gizmos.DrawLine(new Vector3(Screen.width * rightThreshold, 0, 0), new Vector3(Screen.width, Screen.height, 0));
    }
}