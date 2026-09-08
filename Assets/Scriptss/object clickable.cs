using UnityEngine;
using System.Collections;

public class objectclickable : MonoBehaviour
{
    public SceneLoader sceneLoader; // Reference to the SceneLoader script
    public Camera mainCamera;
    public float zoomSpeed = 5f;
    public float rotationSpeed = 5f;
    public float stopDistance = 2f;

    // Hover effect properties
    public Color hoverColor = new Color(1f, 1f, 1f, 0.5f); // semi-transparent white
    public float hoverEmissionIntensity = 0.5f;           // optional glow

    private Renderer rend;
    private Material mat;
    private Color originalColor;
    private bool originalEmissionEnabled;
    private Color originalEmissionColor;

    void Start()
    {
        // Get the renderer and material (assuming a single material)
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            mat = rend.material;
            originalColor = mat.color;
            // Store emission settings if you want to use emission glow
            originalEmissionEnabled = mat.IsKeywordEnabled("_EMISSION");
            if (originalEmissionEnabled)
                originalEmissionColor = mat.GetColor("_EmissionColor");
        }
    }

    void OnMouseEnter()
    {
        if (rend != null)
        {
            // Change to hover color (semi-transparent white)
            mat.color = hoverColor;

            // Optional: add a glow by setting emission
            if (mat.IsKeywordEnabled("_EMISSION"))
            {
                mat.SetColor("_EmissionColor", hoverColor * hoverEmissionIntensity);
            }
        }
    }

    void OnMouseExit()
    {
        if (rend != null)
        {
            // Restore original color
            mat.color = originalColor;

            // Restore original emission
            if (mat.IsKeywordEnabled("_EMISSION"))
            {
                mat.SetColor("_EmissionColor", originalEmissionColor);
            }
        }
    }

    public void Click()
    {
        sceneLoader.GoToPC();
        StartCoroutine(ZoomToObject());
    }

    IEnumerator ZoomToObject()
    {
        // Rotate camera toward object
        Vector3 direction = transform.position - mainCamera.transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        while (Quaternion.Angle(mainCamera.transform.rotation, targetRotation) > 0.5f)
        {
            mainCamera.transform.rotation = Quaternion.Slerp(
                mainCamera.transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            yield return null;
        }

        // Calculate direction from camera to object
        direction = (transform.position - mainCamera.transform.position).normalized;

        // Calculate target position
        Vector3 targetPosition = transform.position - direction * stopDistance;

        // Zoom toward object
        while (Vector3.Distance(mainCamera.transform.position, targetPosition) > 0.05f)
        {
            mainCamera.transform.position = Vector3.MoveTowards(
                mainCamera.transform.position,
                targetPosition,
                zoomSpeed * Time.deltaTime
            );

            yield return null;
        }
    }
}