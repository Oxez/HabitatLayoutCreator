// Author Oxe
// Created at 04.10.2025 11:15

using System.Collections.Generic;
using UnityEngine;

public enum Severity { Info, Warning, Error }

public struct ValidationIssue
{
    public Severity Severity;
    public string Message;
}

public static class LayoutValidator
{
    public static IEnumerable<ValidationIssue> Validate(EditorGrid map, RuleSet rules, int crew, int missionDays)
    {
        float cellArea = map.cellSize * map.cellSize;

        // 1) Минимумы по площади
        foreach (var spec in rules.ZoneSpecs)
        {
            float required = spec.MinAreaPerCrew * crew + spec.AreaPer100Days * (missionDays / 100f);
            float actual   = AreaOf(map, spec.Type, cellArea);
            if (actual + 0.001f < required)
            {
                yield return new ValidationIssue {
                    Severity = actual <= 0.0001f ? Severity.Error : Severity.Warning,
                    Message  = $"[{spec.Type}] {actual:0.0} м² < требуемых {required:0.0} м²"
                };
            }
        }

        // 2) Соседства (упрощение: центры масс зон)
        var centers = ComputeZoneCenters(map);
        foreach (var rule in rules.AdjacencyRules)
        {
            if (!centers.TryGetValue(rule.A, out var a)) continue;
            if (!centers.TryGetValue(rule.B, out var b)) continue;

            float dist = Vector2.Distance(a, b) * map.cellSize;
            if (rule.ShouldSeparate && dist < rule.DistanceMeters)
                yield return new ValidationIssue {
                    Severity = Severity.Warning,
                    Message  = $"{rule.A} и {rule.B} слишком близко ({dist:0.0} м < {rule.DistanceMeters:0.0} м)"
                };
            if (!rule.ShouldSeparate && dist > rule.DistanceMeters)
                yield return new ValidationIssue {
                    Severity = Severity.Info,
                    Message  = $"{rule.A} и {rule.B} лучше расположить ближе ({dist:0.0} м > {rule.DistanceMeters:0.0} м)"
                };
        }
    }

    static float AreaOf(EditorGrid map, ZoneType type, float cellArea)
    {
        int count = 0;
        for (int x=0; x<map.cols; x++)
        for (int y=0; y<map.rows; y++)
        {
            var t = map.Tiles[x,y];
            if (t != null && t.Type == type) count++;
        }
        return count * cellArea;
    }

    static Dictionary<ZoneType, Vector2> ComputeZoneCenters(EditorGrid map)
    {
        var sums = new Dictionary<ZoneType, Vector2>();
        var cnts = new Dictionary<ZoneType, int>();
        for (int x=0; x<map.cols; x++)
        for (int y=0; y<map.rows; y++)
        {
            var t = map.Tiles[x,y];
            if (t == null) continue;
            var z = t.Type;
            if (z == ZoneType.None) continue;

            if (!sums.ContainsKey(z)) { sums[z] = Vector2.zero; cnts[z] = 0; }
            sums[z] += new Vector2(x, y);
            cnts[z]++;
        }
        var centers = new Dictionary<ZoneType, Vector2>();
        foreach (var kv in sums) centers[kv.Key] = kv.Value / cnts[kv.Key];
        return centers;
    }
}
