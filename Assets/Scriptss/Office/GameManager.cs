using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class NightTransitionData
{
    public int NextNightNumber;
    public int WaitedAdsCarried;
    public int TrustPointsChange;
    public bool IsGameComplete;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static event System.Action OnAdChanged;

    [Header("Game State")]
    public int currentNight = 1;
    public int currentTrustPoints = 25;
    public int displayedTrustPoints = 25; 
    public int adsJudgedThisNight = 0;
    public int totalAdsThisNight = 1;

    [Header("Ad Queue System")]
    public List<Ad> currentNightQueue = new List<Ad>();
    public List<int> waitList = new List<int>();
    
    // NEW: Stores the categories chosen for the current ad
    public List<string> currentCornerCategories = new List<string>();

    public NightTransitionData pendingTransition;
    private int currentNightTrustChange = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (currentNightQueue.Count == 0)
        {
            StartNewNight();
        }
    }

    public void StartNewNight()
    {
        adsJudgedThisNight = 0;
        currentNightTrustChange = 0;
        totalAdsThisNight = currentNight;
        currentNightQueue.Clear();

        int bonus = PlayerPrefs.GetInt("PendingTrustBonus", 0);
        currentTrustPoints += bonus;
        PlayerPrefs.SetInt("PendingTrustBonus", 0); 
        displayedTrustPoints = currentTrustPoints; 

        string path = Path.Combine(Application.streamingAssetsPath, "ads.json");
        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        AdList adList = JsonUtility.FromJson<AdList>("{\"ads\":" + json + "}");
        List<Ad> allAds = new List<Ad>(adList.ads);

        for (int i = 0; i < totalAdsThisNight; i++)
        {
            int randomCompanyId = Random.Range(1, 101);
            bool isFake = (Random.value > 0.5f);
            string targetVariant = isFake ? "fake" : "real";
            Ad selectedAd = allAds.Find(ad => ad.id == randomCompanyId && ad.variant == targetVariant);
            if (selectedAd != null) currentNightQueue.Add(selectedAd);
        }

        foreach (int waitedId in waitList)
        {
            Ad waitedAd = allAds.Find(ad => ad.id == waitedId);
            if (waitedAd != null)
            {
                currentNightQueue.Add(waitedAd);
                totalAdsThisNight++;
            }
        }
        waitList.Clear();

        if (currentNightQueue.Count > 0)
        {
            PlayerPrefs.SetInt("CurrentAdId", currentNightQueue[0].id);
            PlayerPrefs.SetInt("CurrentAdIsFake", currentNightQueue[0].isFake ? 1 : 0);
            PlayerPrefs.Save();
            GenerateNewCorners(); // NEW: Pick corners for the first ad
        }

        OnAdChanged?.Invoke();
        Debug.Log($"<color=cyan>--- NIGHT {currentNight} BEGINS (Total Ads: {totalAdsThisNight}) ---</color>");
    }

    public void ProcessVerdict(string verdict)
    {
        if (currentNightQueue.Count == 0) return;

        Ad currentAd = currentNightQueue[0];
        bool isAdActuallyFake = currentAd.isFake;
        bool isPlayerRight = false;

        if (verdict == "Fraudulent" && isAdActuallyFake) isPlayerRight = true;
        if (verdict == "Legitimate" && !isAdActuallyFake) isPlayerRight = true;

        int change = 0;

        if (verdict == "Wait")
        {
            change = -3;
            waitList.Add(currentAd.id);
        }
        else if (isPlayerRight)
        {
            PlayerPrefs.SetInt("PendingTrustBonus", PlayerPrefs.GetInt("PendingTrustBonus", 0) + 15);
            change = 0; 
        }
        else
        {
            change = -15;
        }

        if (change != 0) currentTrustPoints += change;
        currentNightTrustChange += (isPlayerRight ? 15 : change); 

        currentNightQueue.RemoveAt(0);
        adsJudgedThisNight++;

        if (currentNightQueue.Count > 0)
        {
            PlayerPrefs.SetInt("CurrentAdId", currentNightQueue[0].id);
            PlayerPrefs.SetInt("CurrentAdIsFake", currentNightQueue[0].isFake ? 1 : 0);
            PlayerPrefs.Save();
            GenerateNewCorners(); // NEW: Pick corners for the next ad
            OnAdChanged?.Invoke(); 
        }
        else
        {
            TriggerNightTransition();
        }
    }

    // NEW: Method to pick 2-4 random categories
    private void GenerateNewCorners()
    {
        currentCornerCategories.Clear();
        List<string> allCategories = new List<string> { 
            "Registration", "PhysicalAddress", "ReturnComparison", 
            "TermsAndConditions", "ReviewsAndComplaints", "LeadershipBackground" 
        };

        // Shuffle the list
        for (int i = 0; i < allCategories.Count; i++)
        {
            string temp = allCategories[i];
            int r = Random.Range(i, allCategories.Count);
            allCategories[i] = allCategories[r];
            allCategories[r] = temp;
        }

        // Pick 2 to 4
        int numToShow = Random.Range(2, 5);
        for (int i = 0; i < numToShow; i++)
        {
            currentCornerCategories.Add(allCategories[i]);
        }
    }

    private void TriggerNightTransition()
    {
        bool isGameComplete = (currentNight >= 5);

        pendingTransition = new NightTransitionData
        {
            NextNightNumber = isGameComplete ? 5 : currentNight + 1,
            WaitedAdsCarried = waitList.Count,
            TrustPointsChange = currentNightTrustChange,
            IsGameComplete = isGameComplete
        };

        var officeManager = FindAnyObjectByType<OfficeManager>();
        if (officeManager != null) officeManager.ShowNightTransition();
        else if (isGameComplete) Debug.Log("GAME COMPLETE. Final Score: " + currentTrustPoints);
        else StartNextNightNow();
    }

    public void StartNextNightNow()
    {
        currentNight++;
        adsJudgedThisNight = 0;
        StartNewNight();
        PlayerPrefs.Save();
    }
}