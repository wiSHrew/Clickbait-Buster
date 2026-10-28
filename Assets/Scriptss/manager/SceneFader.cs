using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public Image image;
    public float fadeDuration = 1f;
    public float holdBlackDuration = 0.25f;

    [Header("Camera Zoom")]
    public Camera targetCamera;          // Assign your main camera
    public float zoomStartSize = 5f;     // Orthographic size before zoom
    public float zoomEndSize = 2f;       // Orthographic size after zoom
    public float zoomDuration = 0.5f;    // How fast the zoom happens
    public AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeToSceneCoroutine(sceneName));
    }

    IEnumerator FadeToSceneCoroutine(string sceneName)
    {
        // 1. Fade to black AND zoom at the same time
        Coroutine fadeCo = StartCoroutine(Fade(1f));
        Coroutine zoomCo = StartCoroutine(ZoomCamera(zoomStartSize, zoomEndSize));
        yield return fadeCo;
        yield return zoomCo;

        // 2. Hold at black
        yield return new WaitForSeconds(holdBlackDuration);

        // 3. Load scene additively
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
            yield return null;

        // 4. Unload the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        yield return SceneManager.UnloadSceneAsync(currentScene);

        // 5. Set the new scene as active
        Scene newScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(newScene);

        // 6. Reset zoom, then fade back in
        yield return StartCoroutine(ZoomCamera(zoomEndSize, zoomStartSize));
        yield return StartCoroutine(Fade(0f));
    }

    IEnumerator ZoomCamera(float from, float to)
    {
        if (targetCamera == null || !targetCamera.orthographic) yield break;

        float t = 0f;
        while (t < zoomDuration)
        {
            t += Time.deltaTime;
            float p = zoomCurve.Evaluate(Mathf.Clamp01(t / zoomDuration));
            targetCamera.orthographicSize = Mathf.Lerp(from, to, p);
            yield return null;
        }
        targetCamera.orthographicSize = to;
    }

    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = image.color.a;
        float t = 0;
        Color c = image.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            image.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        image.color = c;
    }

    IEnumerator FadeIn()
    {
        Color c = image.color;
        c.a = 1f;
        image.color = c;
        yield return StartCoroutine(Fade(0f));
    }
}