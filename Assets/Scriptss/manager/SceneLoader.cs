using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public SceneFader sceneFader; // Drag your SceneFader object here

    public void GoToMenu()
    {
        if (sceneFader != null)
            sceneFader.FadeToScene("main menu");
        else
            SceneManager.LoadScene("main menu");
    }

    public void GoToSettings()
    {
        if (sceneFader != null)
            sceneFader.FadeToScene("Settings");
        else
            SceneManager.LoadScene("Settings");
    }

    public void GoToGame()
    {
        if (sceneFader != null)
            sceneFader.FadeToScene("game out of pc");
        else
            SceneManager.LoadScene("game out of pc");
    }

    public void GoToPC()
    {
        if (sceneFader != null)
            sceneFader.FadeToScene("game in pc");
        else
            SceneManager.LoadScene("game in pc");
    }
}