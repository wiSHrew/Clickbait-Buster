using UnityEngine;
using UnityEngine.UI;

public class TabletManager : MonoBehaviour
{
    [Header("UI Panel")]
    [SerializeField] private GameObject tabletPanel;

    [Header("The 3 Verdict Buttons")]
    [SerializeField] private Button approveButton;
    [SerializeField] private Button condemnButton;
    [SerializeField] private Button waitButton;

    private void Start()
    {
        if (tabletPanel != null) tabletPanel.SetActive(false);

        if (approveButton != null)
            approveButton.onClick.AddListener(() => ProcessVerdict("Legitimate"));
        if (condemnButton != null)
            condemnButton.onClick.AddListener(() => ProcessVerdict("Fraudulent"));
        if (waitButton != null)
            waitButton.onClick.AddListener(() => ProcessVerdict("Wait"));
    }

    public void OpenTablet()
    {
        if (tabletPanel != null)
        {
            if (!tabletPanel.activeSelf) AudioManager.Play(Sfx.TabletOpen); // AUDIO
            tabletPanel.SetActive(true);
        }
    }

    public void CloseTablet()
    {
        if (tabletPanel != null)
        {
            if (tabletPanel.activeSelf) AudioManager.Play(Sfx.TabletClose); // AUDIO
            tabletPanel.SetActive(false);
        }
    }

    // Used after a verdict so the verdict sound isn't layered with the close sound.
    private void CloseTabletSilent()
    {
        if (tabletPanel != null)
            tabletPanel.SetActive(false);
    }

    private void ProcessVerdict(string verdict)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ProcessVerdict(verdict);
        }
        CloseTabletSilent();
    }
}