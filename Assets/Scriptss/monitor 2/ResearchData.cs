using System;
using UnityEngine;
using System.IO;

[Serializable]
public class ResearchPointData
{
    public int companyId;
    public string databaseDetail;
}

[Serializable]
public class ResearchPointList
{
    public ResearchPointData[] points;
}

public static class ResearchDataLoader
{
    public static ResearchPointData GetData(string jsonFileName, int companyId)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "ResearchData", jsonFileName);
        
        if (!File.Exists(path))
        {
            Debug.LogError($"JSON file not found: {path}");
            return null;
        }

        string json = File.ReadAllText(path);
        ResearchPointList list = JsonUtility.FromJson<ResearchPointList>("{\"points\":" + json + "}");

        if (list == null || list.points == null) return null;

        foreach (var point in list.points)
        {
            if (point.companyId == companyId)
                return point;
        }

        return null; 
    }
}