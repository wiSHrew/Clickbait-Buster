using UnityEngine;
using TMPro;

public class NightCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nightText;

    private void Update()
    {
        if (GameManager.Instance != null && nightText != null)
        {
            nightText.text = $"Night {GameManager.Instance.currentNight}";
        }
    }
}