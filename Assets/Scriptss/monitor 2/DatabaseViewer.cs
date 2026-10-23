
using System;
using System.IO;
using UnityEngine;
using TMPro;

public class DatabaseViewer : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private string jsonFileName = "ads.json";

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
        if (content == null)
        {
            Debug.LogError("DatabaseViewer content reference is not assigned.");
            return;
        }

        if (rowPrefab == null)
        {
            Debug.LogError("DatabaseViewer row prefab reference is not assigned.");
            return;
        }

        string path = Path.IsPathRooted(jsonFileName)
            ? jsonFileName
            : Path.Combine(Application.streamingAssetsPath, jsonFileName);

        if (!File.Exists(path))
        {
            Debug.LogError("Database file not found: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        DatabaseData database = JsonUtility.FromJson<DatabaseData>(
            "{\"records\":" + json + "}"
        );

        if (database == null || database.records == null)
        {
            Debug.LogError("Invalid JSON database format.");
            return;
        }

        // Clear old rows before reloading.
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        foreach (Ad ad in database.records)
        {
            CreateRow(ad);
        }

        Debug.Log("Loaded " + database.records.Length +
                  " database records.");
    }

    private void CreateRow(Ad ad)
    {
        GameObject row = Instantiate(rowPrefab, content, false);

        RectTransform rectTransform = row.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localPosition = new Vector3(
                rectTransform.localPosition.x,
                rectTransform.localPosition.y,
                0f
            );
        }

        SetText(row.transform, "id", ad.id.ToString());
        SetText(row.transform, "title", ad.title);
        SetText(row.transform, "brand", ad.brand);
        SetText(row.transform, "desc", ad.description);
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

        TMP_Text text = child.GetComponent<TMP_Text>();
        if (text == null)
        {
            text = child.GetComponentInChildren<TMP_Text>();
        }

        if (text == null)
        {
            Debug.LogError(
                "Text object has no TextMeshPro component: " + childName,
                child
            );
            return;
        }

        text.text = value ?? string.Empty;
    }
}
