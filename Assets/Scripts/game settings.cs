using UnityEngine;

public class gamesettings : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerPrefs.SetInt("Volume", 100);
        PlayerPrefs.SetInt("Sensitivity", 100);

        PlayerPrefs.Save();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
