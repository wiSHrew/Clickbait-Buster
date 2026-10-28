using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class TabletAdDisplay : MonoBehaviour
{
    [Header("Ad Visuals")]
    [SerializeField] private TMP_Text companyNameText; 
    [SerializeField] private Image companyLogoImage;   

    [Header("Tablet Stats UI")]
    [SerializeField] private TMP_Text trustPointsText;
    [SerializeField] private TMP_Text nightText;
    [SerializeField] private TMP_Text counterText;

    private List<Ad> allAds = new List<Ad>();

    void Start()
    {
        LoadAds();
        RefreshDisplay();
    }

    void OnEnable()
    {
        GameManager.OnAdChanged += RefreshDisplay;
    }

    void OnDisable()
    {
        GameManager.OnAdChanged -= RefreshDisplay;
    }

    public void RefreshDisplay()
    {
        if (GameManager.Instance == null) return;

        int currentId = PlayerPrefs.GetInt("CurrentAdId", 1);
        bool isFake = PlayerPrefs.GetInt("CurrentAdIsFake", 0) == 1;
        string variant = isFake ? "fake" : "real";

        // 1. Update Company Name & Logo
        Ad selectedAd = allAds.Find(ad => ad.id == currentId && ad.variant == variant);
        if (selectedAd != null && companyNameText != null)
        {
            companyNameText.text = selectedAd.brand;
        }

        if (companyLogoImage != null)
        {
            Sprite loadedLogo = Resources.Load<Sprite>($"Logos/{currentId}");
            if (loadedLogo != null)
            {
                companyLogoImage.sprite = loadedLogo;
                companyLogoImage.color = Color.white;
            }
            else
            {
                companyLogoImage.sprite = null;
                companyLogoImage.color = new Color(0.2f, 0.5f, 1f);
            }
        }

        // 2. Update Stats
        if (trustPointsText != null)
            trustPointsText.text = $"Trust: {GameManager.Instance.currentTrustPoints}";
            
        if (nightText != null)
            nightText.text = $"Night {GameManager.Instance.currentNight}";
            
        if (counterText != null)
        {
            int currentAdNumber = GameManager.Instance.adsJudgedThisNight + 1;
            int totalAds = GameManager.Instance.totalAdsThisNight;
            counterText.text = $"{currentAdNumber} / {totalAds}";
        }
    }

    private void LoadAds()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "ads.json");
        if (!File.Exists(path)) return;
        string json = File.ReadAllText(path);
        AdList adList = JsonUtility.FromJson<AdList>("{\"ads\":" + json + "}");
        allAds = new List<Ad>(adList.ads);
    }
}