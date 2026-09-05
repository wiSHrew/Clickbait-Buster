using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void Quit()
    {
        // Logs in editor for testing
        Debug.Log("Game is quitting...");
        
        // Quits the game in build
        Application.Quit();
    }
}