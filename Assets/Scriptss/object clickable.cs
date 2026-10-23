using UnityEngine;
using System.Collections;

public class objectclickable : MonoBehaviour
{
    public enum TargetScene { Monitor1, Monitor2 }

    public SceneLoader sceneLoader; // Reference to the SceneLoader script
    public Camera mainCamera;
    public float zoomSpeed = 5f;
    public float rotationSpeed = 5f;
    public float stopDistance = 2f;

    [SerializeField] private TargetScene targetScene = TargetScene.Monitor1;

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
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            mat = rend.material;
            originalColor = mat.color;
            originalEmissionEnabled = mat.IsKeywordEnabled("_EMISSION");
            if (originalEmissionEnabled)
                originalEmissionColor = mat.GetColor("_EmissionColor");
        }
    }

    void OnMouseEnter()
    {
        if (rend != null)
        {
            mat.color = hoverColor;
            if (mat.IsKeywordEnabled("_EMISSION"))
                mat.SetColor("_EmissionColor", hoverColor * hoverEmissionIntensity);
        }
    }

    void OnMouseExit()
    {
        if (rend != null)
        {
            mat.color = originalColor;
            if (mat.IsKeywordEnabled("_EMISSION"))
                mat.SetColor("_EmissionColor", originalEmissionColor);
        }
    }

    public void Click()
    {
        if (targetScene == TargetScene.Monitor1)
            sceneLoader.GoToMonitor1();
        else
            sceneLoader.GoToMonitor2();

        StartCoroutine(ZoomToObject());
    }

    IEnumerator ZoomToObject()
    {
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

        direction = (transform.position - mainCamera.transform.position).normalized;
        Vector3 targetPosition = transform.position - direction * stopDistance;

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