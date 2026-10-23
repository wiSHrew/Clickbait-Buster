using System;
using System.IO;
using UnityEngine;
using TMPro;

public class DatabaseViewer : MonoBehaviour
{
    [Header("Database")]
    [Tooltip("Relative to Assets/StreamingAssets/. Example: ads.json")]
    [SerializeField] private string jsonRelativePath = "ads.json";

    [Header("UI References")]
    [SerializeField] private Transform content;
    [SerializeField] private GameObject rowPrefab;

    [Serializable]
    public class DatabaseData
    {
        public Ad[] records;
    }

    private void Start()
    {
        LoadDatabase();
    }

    public void LoadDatabase()
    {
        if (content == null)   { Debug.LogError("content is not assigned.");   return; }
        if (rowPrefab == null) { Debug.LogError("rowPrefab is not assigned."); return; }

        string path = Path.Combine(Application.streamingAssetsPath, jsonRelativePath);

        if (!File.Exists(path))
        {
            Debug.LogError("JSON file not found at: " + path);
            return;
        }

        string json = File.ReadAllText(path);

        DatabaseData database = JsonUtility.FromJson<DatabaseData>(
            "{\"records\":" + json + "}"
        );

        if (database == null || database.records == null)
        {
            Debug.LogError("Invalid JSON format. Check the file contents.");
            return;
        }

        foreach (Transform child in content)
            Destroy(child.gameObject);

        foreach (Ad ad in database.records)
            CreateRow(ad);

        Debug.Log("Loaded " + database.records.Length + " database records from " + path);
    }

    private void CreateRow(Ad ad)
    {
        GameObject row = Instantiate(rowPrefab, content, false);

        RectTransform rt = row.GetComponent<RectTransform>();
        if (rt != null)
            rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0f);

        SetText(row.transform, "id",     ad.id.ToString());
        SetText(row.transform, "title",  ad.title);
        SetText(row.transform, "brand",  ad.brand);
        SetText(row.transform, "desc",   ad.description);
        SetText(row.transform, "isFake", ad.isFake.ToString());
    }

    private void SetText(Transform parent, string childName, string value)
    {
        Transform child = parent.Find(childName);
        if (child == null)
        {
            Debug.LogError("Missing text object: " + childName, parent);
            return;
        }

        TMP_Text text = child.GetComponent<TMP_Text>() ?? child.GetComponentInChildren<TMP_Text>();
        if (text == null)
        {
            Debug.LogError("No TMP_Text on: " + childName, child);
            return;
        }

        text.text = value ?? string.Empty;
    }
}