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
        if (map == null || rules == null) yield break;

        float cellArea = map.cellSize * map.cellSize;

        // 1) Минимумы по площади
        foreach (var spec in rules.ZoneSpecs)
        {
            float required = spec.MinAreaPerCrew * crew + spec.AreaPer100Days * (missionDays / 100f);
            float actual   = AreaOf(map, spec.Type, cellArea);
            if (actual + 0.001f < required)
            {
                yield return new ValidationIssue
                {
                    Severity = actual <= 0.0001f ? Severity.Error : Severity.Warning,
                    Message = $"[{spec.Type}] {actual:0.0} м² < требуемых {required:0.0} м²"
                };
            }
        }

        // 2) Соседства (центры масс зон)
        var centers = ComputeZoneCenters(map);
        foreach (var rule in rules.AdjacencyRules)
        {
            if (!centers.TryGetValue(rule.A, out var a)) continue;
            if (!centers.TryGetValue(rule.B, out var b)) continue;

            float dist = Vector2.Distance(a, b) * map.cellSize;
            if (rule.ShouldSeparate && dist < rule.DistanceMeters)
                yield return new ValidationIssue
                {
                    Severity = Severity.Warning,
                    Message = $"{rule.A} и {rule.B} слишком близко ({dist:0.0} м < {rule.DistanceMeters:0.0} м)"
                };
            if (!rule.ShouldSeparate && dist > rule.DistanceMeters)
                yield return new ValidationIssue
                {
                    Severity = Severity.Info,
                    Message = $"{rule.A} и {rule.B} лучше расположить ближе ({dist:0.0} м > {rule.DistanceMeters:0.0} м)"
                };
        }

        // 3) Доступность через коридоры - считаем только если обе зоны есть на карте
        float minAisle = 0.9f;
        foreach (var z in rules.ZoneSpecs)
            minAisle = Mathf.Max(minAisle, z.MinAisleWidth); // консервативно

        if (HasZone(map, ZoneType.Sleep) && HasZone(map, ZoneType.Hygiene))
            yield return CheckPath(map, ZoneType.Sleep, ZoneType.Hygiene, minAisle);

        if (HasZone(map, ZoneType.Sleep) && HasZone(map, ZoneType.Airlock))
            yield return CheckPath(map, ZoneType.Sleep, ZoneType.Airlock, minAisle);
    }

    // helper
    static bool HasZone(EditorGrid map, ZoneType t)
    {
        for (int x = 0; x < map.cols; x++)
        for (int y = 0; y < map.rows; y++)
            if (map.Tiles[x, y] != null && map.Tiles[x, y].Type == t)
                return true;
        return false;
    }

    static ValidationIssue CheckPath(EditorGrid map, ZoneType a, ZoneType b, float minAisle)
    {
        var rep = PathAnalyzer.Analyze(map, a, b, minAisle);
        if (!rep.hasPath)
            return new ValidationIssue
            {
                Severity = Severity.Error,
                Message = $"{a} → {b}: нет пути ({rep.why})"
            };
        if (rep.minWidthMeters + 1e-3f < minAisle)
            return new ValidationIssue
            {
                Severity = Severity.Warning,
                Message = $"{a} → {b}: узко {rep.minWidthMeters:0.00} м < {minAisle:0.00} м (L={rep.lengthMeters:0.0} м)"
            };
        return new ValidationIssue
        {
            Severity = Severity.Info,
            Message = $"{a} → {b}: ок (L={rep.lengthMeters:0.0} м, min {rep.minWidthMeters:0.00} м)"
        };
    }

    static float AreaOf(EditorGrid map, ZoneType type, float cellArea)
    {
        int count = 0;
        for (int x = 0; x < map.cols; x++)
        for (int y = 0; y < map.rows; y++)
        {
            var t = map.Tiles[x, y];
            if (t != null && t.Type == type) count++;
        }
        return count * cellArea;
    }

    static Dictionary<ZoneType, Vector2> ComputeZoneCenters(EditorGrid map)
    {
        var sums = new Dictionary<ZoneType, Vector2>();
        var cnts = new Dictionary<ZoneType, int>();
        for (int x = 0; x < map.cols; x++)
        for (int y = 0; y < map.rows; y++)
        {
            var t = map.Tiles[x, y];
            if (t == null) continue;
            var z = t.Type;
            if (z == ZoneType.None) continue;

            if (!sums.ContainsKey(z))
            {
                sums[z] = Vector2.zero;
                cnts[z] = 0;
            }
            sums[z] += new Vector2(x, y);
            cnts[z]++;
        }
        var centers = new Dictionary<ZoneType, Vector2>();
        foreach (var kv in sums) centers[kv.Key] = kv.Value / cnts[kv.Key];
        return centers;
    }

    public static IEnumerable<ValidationIssue> ValidateAreas(
        Dictionary<ZoneType, float> actualAreasM2, RuleSet rules, int crew, int missionDays)
    {
        foreach (var spec in rules.ZoneSpecs)
        {
            float required = spec.MinAreaPerCrew * crew + spec.AreaPer100Days * (missionDays / 100f);
            float actual   = actualAreasM2.TryGetValue(spec.Type, out var a) ? a : 0f;

            if (actual + 0.001f < required)
            {
                yield return new ValidationIssue
                {
                    Severity = actual <= 0.0001f ? Severity.Error : Severity.Warning,
                    Message = $"[{spec.Type}] {actual:0.0} м² < требуемых {required:0.0} м²"
                };
            }
        }

        // todo: adjacency по всем палубам
    }
}
