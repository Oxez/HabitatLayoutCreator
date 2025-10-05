// Author Oxe
// Created at 04.10.2025 11:16

using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class ReportExporter
{
    [System.Serializable]
    public class Report
    {
        public string                    mission;
        public int                       crew;
        public int                       days;
        public float                     totalArea;
        public Dictionary<string, float> areaByZone;
        public string[]                  issues;

        public Report(string mission, int crew, int days, float totalArea, 
               Dictionary<string, float> areaByZone, string[] issues)
        {
            this.mission = mission;
            this.crew = crew;
            this.days = days;
            this.totalArea = totalArea;
            this.areaByZone = areaByZone;
            this.issues = issues;
        }
        public Report(string json)
        {
            var rep = JsonUtility.FromJson<Report>(json);
            this.mission = rep.mission;
            this.crew = rep.crew;
            this.days = rep.days;
            this.totalArea = rep.totalArea;
            this.areaByZone = rep.areaByZone;
            this.issues = rep.issues;
        }
    }

    public static string SaveJson(string path, string missionName, int crew, int days,
        float totalArea, Dictionary<ZoneType, float> areaByZone,
        List<ValidationIssue> issues)
    {
        var dict = new Dictionary<string, float>();
        foreach (var kv in areaByZone) dict[kv.Key.ToString()] = kv.Value;

        var rep = new Report(
            missionName, crew, days,
            totalArea,
            dict,
            issues.ConvertAll(i => $"{i.Severity}: {i.Message}").ToArray()
        );
        var json = JsonUtility.ToJson(rep, true);
        File.WriteAllText(path, json);
        return path;
    }

    public static Report loadJson(string path)
    {
        if (!File.Exists(path)) return null;
        string json = File.ReadAllText(path);
        return new Report(json);
    }
}
