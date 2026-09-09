using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public Image image;
    public float fadeDuration = 1f;
    public float holdBlackDuration = 0.25f;

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
        // 1. Fade to black
        yield return StartCoroutine(Fade(1f));
        
        // 2. Hold at black
        yield return new WaitForSeconds(holdBlackDuration);
        
        // 3. ✅ FIX: Load scene additively first
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        
        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // 4. Unload the current scene (this happens instantly now)
        Scene currentScene = SceneManager.GetActiveScene();
        yield return SceneManager.UnloadSceneAsync(currentScene);
        
        // 5. Set the new scene as active
        Scene newScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(newScene);
        
        // 6. Fade back in
        yield return StartCoroutine(Fade(0f));
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