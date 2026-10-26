using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class Monitor2_Manager : MonoBehaviour
{
    [Header("Data Source")]
    public string mainJsonFile = "ads.json";

    [Header("UI Panels")]
    [SerializeField] private GameObject companySelectionPanel; 
    [SerializeField] private GameObject companyInfoPanel;      

    [Header("List Screen UI")]
    [SerializeField] private Transform listContentParent; 
    [SerializeField] private GameObject companyButtonPrefab;

    [Header("Info Screen UI")]
    [SerializeField] private TMP_Text nowViewingText;
    [SerializeField] private TMP_Text databaseText;
    [SerializeField] private Image companyLogo; 
    [SerializeField] private Button btnBackToList; 

    [Header("The 6 Research Buttons")]
    [SerializeField] private Button btnRegistration;
    [SerializeField] private Button btnAddress;
    [SerializeField] private Button btnReturn;
    [SerializeField] private Button btnTerms;
    [SerializeField] private Button btnReviews;
    [SerializeField] private Button btnLeadership;

    private int selectedCompanyId = -1;
    private Ad[] allAds;

    private void Start()
    {
        LoadAdDatabase();
        SpawnCompanyButtons();

        if (btnBackToList != null)
            btnBackToList.onClick.AddListener(ShowListScreen);

        ShowListScreen();
    }

    private void LoadAdDatabase()
    {
        string path = Path.Combine(Application.streamingAssetsPath, mainJsonFile);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            AdList list = JsonUtility.FromJson<AdList>("{\"ads\":" + json + "}");
            allAds = list.ads;
        }
    }

    private void SpawnCompanyButtons()
    {
        foreach (Transform child in listContentParent) Destroy(child.gameObject);

        foreach (Ad ad in allAds)
        {
            GameObject btnObj = Instantiate(companyButtonPrefab, listContentParent);
            CompanyListItem item = btnObj.GetComponent<CompanyListItem>();
            
            if (item == null) item = btnObj.AddComponent<CompanyListItem>();
            item.Setup(ad.id, ad.brand, this);
        }
    }

    private void ShowListScreen()
    {
        companySelectionPanel.SetActive(true);
        companyInfoPanel.SetActive(false);
    }

    public void ShowCompanyDetails(int id, string brandName)
    {
        selectedCompanyId = id;

        companySelectionPanel.SetActive(false);
        companyInfoPanel.SetActive(true);

        nowViewingText.text = $"{brandName}";
        if (companyLogo != null) companyLogo.color = new Color(0.2f, 0.5f, 1f); 

        databaseText.text = "<i>Select a research point on the right to begin cross-checking.</i>";
        databaseText.color = Color.black;

        btnRegistration.onClick.RemoveAllListeners();
        btnRegistration.onClick.AddListener(() => DisplayData("Registration.json", "Registration"));

        btnAddress.onClick.RemoveAllListeners();
        btnAddress.onClick.AddListener(() => DisplayData("PhysicalAddress.json", "Physical Address"));

        btnReturn.onClick.RemoveAllListeners();
        btnReturn.onClick.AddListener(() => DisplayData("ReturnComparison.json", "Return Comparison"));

        btnTerms.onClick.RemoveAllListeners();
        btnTerms.onClick.AddListener(() => DisplayData("TermsAndConditions.json", "Terms & Conditions"));

        btnReviews.onClick.RemoveAllListeners();
        btnReviews.onClick.AddListener(() => DisplayData("ReviewsAndComplaints.json", "Reviews & Complaints"));

        btnLeadership.onClick.RemoveAllListeners();
        btnLeadership.onClick.AddListener(() => DisplayData("LeadershipBackground.json", "Leadership Background"));
        
        DisplayData("Registration.json", "Registration");
    }

    private void DisplayData(string fileName, string displayTitle)
    {
        if (selectedCompanyId == -1) return;

        ResearchPointData data = ResearchDataLoader.GetData(fileName, selectedCompanyId);

        if (data != null)
        {
            databaseText.text = $"<b>DATABASE RECORD:</b> {displayTitle.ToUpper()}\n" +
                                $"<b>COMPANY ID:</b> #{selectedCompanyId:D3}\n" +
                                $"--------------------------------------------------\n\n" +
                                $"{data.databaseDetail}";

            // Text remains a neutral color because the database is just presenting facts
            databaseText.color = Color.black;
        }
        else
        {
            databaseText.text = $"<i>No database records found for {displayTitle}.</i>";
            databaseText.color = Color.gray;
        }
    }
}