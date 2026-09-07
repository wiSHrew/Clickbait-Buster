using UnityEngine;
using System.Collections;

public class objectclickable : MonoBehaviour
{
    public SceneLoader sceneLoader; // Reference to the SceneLoader script
    public Camera mainCamera;
    public float zoomSpeed = 5f;
    public float rotationSpeed = 5f;
    public float stopDistance = 2f;

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