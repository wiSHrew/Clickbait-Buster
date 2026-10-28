using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class OfficeManager : MonoBehaviour
{
    [Header("Night Transition UI")]
    [SerializeField] private GameObject nightTransitionPanel;
    [SerializeField] private TMP_Text waitBonusText;
    [SerializeField] private TMP_Text nightHeaderText;
    [SerializeField] private TMP_Text trustChangeText;
    [SerializeField] private CanvasGroup transitionCanvasGroup;
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text continueButtonText;

    [Header("Ending UI (Post Night 5)")]
    [SerializeField] private GameObject endingPanel;
    [SerializeField] private TMP_Text endingTitleText;
    [SerializeField] private TMP_Text endingDescriptionText;

    [SerializeField] private float fadeDuration = 0.5f;
    private bool isTransitioning = false;
    private bool isGameComplete = false;

    public void ShowNightTransition()
    {
        if (nightTransitionPanel == null) return;

        isTransitioning = true;
        nightTransitionPanel.SetActive(true);
        AudioManager.Play(Sfx.NightComplete); // AUDIO

        var data = GameManager.Instance.pendingTransition;
        isGameComplete = data.IsGameComplete;

        // 1. WAIT verdict bonus
        if (data.WaitedAdsCarried > 0 && !isGameComplete)
        {
            waitBonusText.gameObject.SetActive(true);
            waitBonusText.text = $"WAIT VERDICT: +{data.WaitedAdsCarried} AD";
        }
        else
        {
            waitBonusText.gameObject.SetActive(false);
        }

        // 2. Header (Night X vs Game Complete)
        if (isGameComplete)
        {
            nightHeaderText.text = "ALL NIGHTS COMPLETE";
        }
        else
        {
            nightHeaderText.text = $"NIGHT {data.NextNightNumber}";
        }

        // 3. Trust points change
        int change = data.TrustPointsChange;
        if (change > 0)
        {
            trustChangeText.text = $"Trust Points: +{change}";
            trustChangeText.color = new Color(0.2f, 0.8f, 0.2f); // Green
        }
        else if (change < 0)
        {
            trustChangeText.text = $"Trust Points: {change}";
            trustChangeText.color = new Color(0.9f, 0.2f, 0.2f); // Red
        }
        else
        {
            trustChangeText.text = "Trust Points: no change";
            trustChangeText.color = Color.white;
        }

        // 4. Continue Button Text
        if (continueButtonText != null)
        {
            continueButtonText.text = isGameComplete ? "SEE RESULTS" : "CONTINUE";
        }

        StartCoroutine(FadeIn());
    }

    private System.Collections.IEnumerator FadeIn()
    {
        transitionCanvasGroup.alpha = 0f;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            transitionCanvasGroup.alpha = t / fadeDuration;
            yield return null;
        }
        transitionCanvasGroup.alpha = 1f;
    }

    public void ContinueToNextNight()
    {
        if (!isTransitioning) return;

        nightTransitionPanel.SetActive(false);
        isTransitioning = false;

        if (isGameComplete)
        {
            ShowEnding();
        }
        else
        {
            GameManager.Instance.StartNextNightNow();
        }
    }

    private void ShowEnding()
    {
        if (endingPanel == null) return;

        endingPanel.SetActive(true);
        int finalScore = GameManager.Instance.currentTrustPoints;

        // Determine Ending based on final score
        if (finalScore >= 60)
        {
            endingTitleText.text = "TRUE CYBERSECURITY VIGILANTE";
            endingDescriptionText.text = $"Final Score: {finalScore}\n\nThe internet is a safer place because of you. You successfully uncovered scams and protected the community.";
            endingTitleText.color = new Color(0.2f, 0.8f, 0.2f); // Green
            AudioManager.Play(Sfx.EndingGood); // AUDIO
        }
        else if (finalScore >= 1)
        {
            endingTitleText.text = "CURIOUS INVESTIGATOR";
            endingDescriptionText.text = $"Final Score: {finalScore}\n\nYou did an okay job, but some scams slipped through the cracks. The community needs more from you.";
            endingTitleText.color = new Color(1f, 0.8f, 0.2f); // Yellow/Orange
            AudioManager.Play(Sfx.EndingNeutral); // AUDIO
        }
        else
        {
            endingTitleText.text = "SCAMMER'S PARADISE";
            endingDescriptionText.text = $"Final Score: {finalScore}\n\nYour reputation is in ruins. The scammers won, and the community lost all trust in you.";
            endingTitleText.color = new Color(0.9f, 0.2f, 0.2f); // Red
            AudioManager.Play(Sfx.EndingBad); // AUDIO
        }
    }
}