using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class valueupdater : MonoBehaviour
{
    public Slider slider;
    private TMP_Text valueText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slider.onValueChanged.AddListener(UpdateValue);
        valueText = gameObject.GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void UpdateValue(float value)
    {
        valueText.text = $"{value:F0}";
    }
}
