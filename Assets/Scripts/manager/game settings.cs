using UnityEngine;
using UnityEngine.UI;

public class gamesettings : MonoBehaviour
{
    public Slider volumeSlider;
    public Slider sensitivitySlider;

    public void Save()
    {
        PlayerPrefs.SetFloat("Volume", volumeSlider.value);
        PlayerPrefs.SetFloat("Sensitivity", sensitivitySlider.value);
        PlayerPrefs.Save();
    }
}
