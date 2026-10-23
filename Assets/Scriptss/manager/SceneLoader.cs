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
            sceneFader.FadeToScene("office");
        else
            SceneManager.LoadScene("office");
    }

    public void GoToPC()
    {
        if (sceneFader != null)
            sceneFader.FadeToScene("monitor 1");
        else
            SceneManager.LoadScene("monitor 1");
    }

    public void GoToDB()
    {
        if (sceneFader != null)
            sceneFader.FadeToScene("monitor 2");
        else
            SceneManager.LoadScene("monitor 2");
    }
}