using UnityEngine;
using System.IO;
using TMPro;

public class AdLoader : MonoBehaviour
{
    public string fileName = "ads.json";
    GameObject title;
    GameObject contents;

    private AdList adList;

    void Start()
    {
        title = transform.GetChild(0).gameObject;
        contents = transform.GetChild(1).gameObject;

        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        string json = File.ReadAllText(path);
        json = "{\"ads\":" + json + "}";

        adList = JsonUtility.FromJson<AdList>(json);

        int numberOfAds = adList.ads.Length;

        Debug.Log("Number of ads: " + numberOfAds);

        int randomIndex = Random.Range(0, numberOfAds);

        Debug.Log("Selected index: " + randomIndex);

        Ad selectedAd = adList.ads[randomIndex];

        Debug.Log("Selected ad: " + selectedAd.title);

        title.GetComponent<TMP_Text>().text = selectedAd.title;
        contents.GetComponent<TMP_Text>().text = selectedAd.description;
    }
}
