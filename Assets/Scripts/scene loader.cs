using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{

    public void GoToMenu()
    {
        SceneManager.LoadScene("main menu");
    }
    public void GoToSettings()
    {
        SceneManager.LoadScene("Settings");
    }

    public void GoToGame()
    {
        SceneManager.LoadScene("game out of pc");
    }

    public void GoToPC()
    {
        SceneManager.LoadScene("game in pc");
    }
}
