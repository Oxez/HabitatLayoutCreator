// Author Oxe
// Created at 04.10.2025 11:16

using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class ReportExporter
{
    [System.Serializable]
    class Report
    {
        public string                    mission;
        public int                       crew;
        public int                       days;
        public float                     totalArea;
        public Dictionary<string, float> areaByZone;
        public string[]                  issues;
    }

    public static string SaveJson(string path, string missionName, int crew, int days,
        float totalArea, Dictionary<ZoneType, float> areaByZone,
        List<ValidationIssue> issues)
    {
        var dict = new Dictionary<string, float>();
        foreach (var kv in areaByZone) dict[kv.Key.ToString()] = kv.Value;

        var rep = new Report {
            mission = missionName, crew = crew, days = days,
            totalArea = totalArea,
            areaByZone = dict,
            issues = issues.ConvertAll(i => $"{i.Severity}: {i.Message}").ToArray()
        };
        var json = JsonUtility.ToJson(rep, true);
        File.WriteAllText(path, json);
        return path;
    }
}
