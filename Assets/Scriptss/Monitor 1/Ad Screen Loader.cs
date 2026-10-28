using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using TMPro;

public class AdScreenLoader : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    public string fileName = "ads.json";
    private AdList adList;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (PlayerPrefs.HasKey("CurrentAdId"))
        {
            int currentAdId = PlayerPrefs.GetInt("CurrentAdId");

            string path = Path.Combine(Application.streamingAssetsPath, fileName);
            string json = File.ReadAllText(path);
            json = "{\"ads\":" + json + "}";

            adList = JsonUtility.FromJson<AdList>(json);
            
            Ad selectedAd = System.Array.Find(adList.ads, ad => ad.id == currentAdId);
            if (selectedAd == null)
            {
                Debug.LogError($"No ad found with ID {currentAdId}.");
                return;
            }

            label.text = selectedAd.title;
        }
        else
        {
            Debug.Log("No Current Ad ID found.");
        }
    }
}
