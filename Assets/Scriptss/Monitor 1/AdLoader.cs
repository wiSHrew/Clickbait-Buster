using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AdLoader : MonoBehaviour
{
    [Header("Main UI References")]
    [SerializeField] private TMP_Text companyNameText;
    [SerializeField] private TMP_Text captionText;
    [SerializeField] private TMP_Text mainDescriptionText;
    [SerializeField] private Image companyLogoImage;

    [Header("Corner References")]
    [SerializeField] private TMP_Text cornerTopLeft;
    [SerializeField] private TMP_Text cornerTopRight;
    [SerializeField] private TMP_Text cornerBottomLeft;
    [SerializeField] private TMP_Text cornerBottomRight;

    [Header("Header HUD References")]
    [SerializeField] private TMP_Text nightText;
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private TMP_Text trustPointsText;

    private List<Ad> allAds = new List<Ad>();

    void Start()
    {
        LoadAds();
        DisplayCurrentAd();
        UpdateHUD();
    }

    void OnEnable() => GameManager.OnAdChanged += HandleAdChanged;
    void OnDisable() => GameManager.OnAdChanged -= HandleAdChanged;

    private void HandleAdChanged()
    {
        DisplayCurrentAd();
        UpdateHUD();
    }

    private void LoadAds()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "ads.json");
        if (!File.Exists(path)) return;
        string json = File.ReadAllText(path);
        AdList adList = JsonUtility.FromJson<AdList>("{\"ads\":" + json + "}");
        allAds = new List<Ad>(adList.ads);
    }

    private void DisplayCurrentAd()
    {
        int currentId = PlayerPrefs.GetInt("CurrentAdId", 1);
        bool isFake = PlayerPrefs.GetInt("CurrentAdIsFake", 0) == 1;

        string variant = isFake ? "fake" : "real";
        Ad selectedAd = allAds.Find(ad => ad.id == currentId && ad.variant == variant);
        if (selectedAd == null) return;

        companyNameText.text = selectedAd.brand;
        captionText.text = selectedAd.caption;
        mainDescriptionText.text = selectedAd.maincontext;

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

        PopulateCorners(selectedAd.id, isFake);
    }

    private void PopulateCorners(int companyId, bool isFake)
    {
        // Read the fixed list from GameManager
        List<string> categories = GameManager.Instance.currentCornerCategories;

        ClearCorners();
        
        if (categories.Count >= 1) cornerTopLeft.text = GetCornerText(categories[0], isFake, companyId);
        if (categories.Count >= 2) cornerTopRight.text = GetCornerText(categories[1], isFake, companyId);
        if (categories.Count >= 3) cornerBottomLeft.text = GetCornerText(categories[2], isFake, companyId);
        if (categories.Count >= 4) cornerBottomRight.text = GetCornerText(categories[3], isFake, companyId);
    }

    private string GetCornerText(string categoryName, bool isFake, int companyId)
    {
        string baseFileName = categoryName.Replace(" ", ""); 
        string fileName = isFake ? $"{baseFileName}_FAKE.json" : $"{baseFileName}.json";
        ResearchPointData data = ResearchDataLoader.GetData(fileName, companyId);
        
        if (data == null) return $"<b>{categoryName}</b>";
        
        // Increased the limit to 150 so it shows more detail
        string shortDetail = data.databaseDetail;
        if (shortDetail.Length > 200) 
            shortDetail = shortDetail.Substring(0, 200) + "...";
            
        return $"<b>{categoryName}</b>\n<size=80%>{shortDetail}</size>";
    }

    private void ClearCorners()
    {
        cornerTopLeft.text = "";
        cornerTopRight.text = "";
        cornerBottomLeft.text = "";
        cornerBottomRight.text = "";
    }

    private void UpdateHUD()
    {
        if (GameManager.Instance != null)
        {
            if (nightText != null) nightText.text = $"Night {GameManager.Instance.currentNight}";
            if (counterText != null)
            {
                int currentAdNumber = GameManager.Instance.adsJudgedThisNight + 1;
                int totalAds = GameManager.Instance.totalAdsThisNight;
                counterText.text = $"{currentAdNumber} / {totalAds}";
            }
            if (trustPointsText != null) trustPointsText.text = $"{GameManager.Instance.displayedTrustPoints} Trust Points";
        }
    }

    public void ReturnToOffice() => SceneManager.LoadScene("Office");
}