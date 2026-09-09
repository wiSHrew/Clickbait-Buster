using UnityEngine;
using UnityEngine.InputSystem;

public class keyboardcontrols : MonoBehaviour
{
    private SceneLoader sceneLoader;
    // Update is called once per frame
    void Start()
    {
        sceneLoader = GetComponent<SceneLoader>();
    }
    void Update()
    {
        if (Keyboard.current.spaceKey.isPressed)
        {
            sceneLoader.GoToGame();
            this.enabled = false; // Disable this script to prevent further input
        }
        if (Keyboard.current.escapeKey.isPressed)
        {
            sceneLoader.GoToGame();
            this.enabled = false; // Disable this script to prevent further input
        }
    }
}
