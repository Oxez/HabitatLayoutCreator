// Author Oxe
// Created at 04.10.2025 11:15

using UnityEngine;
using TMPro;
using System.Linq;
using System.Text;
using System.Collections.Generic;

public class MetricsHUD : MonoBehaviour
{
    public EditorGrid         Grid;
    public HabitatContext     Context;
    public TextMeshProUGUI    TxtMetrics;
    public TextMeshProUGUI    TxtIssues;
    public HabitatShapePreset ShapePreset;

    [Tooltip("частота пересчёта, сек")]
    public float refreshInterval = 0.25f;
    float _timer;

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < refreshInterval) return;
        _timer = 0f;

        if (Grid == null || Context == null || Context.Rules == null) return;

        var   counts    = MetricsCalculator.CountTiles(Grid);
        float cellArea  = MetricsCalculator.CellArea(Grid.cellSize);
        float totalArea = counts.Values.Sum() * cellArea;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Crew: {Context.CrewSize}, Days: {Context.MissionDays}");
        sb.AppendLine($"Total Area: {totalArea:0.00} m² | per crew: {(Context.CrewSize>0? totalArea/Context.CrewSize:0):0.00} m²");
        foreach (var z in counts.OrderBy(k=>k.Key))
            sb.AppendLine($"{z.Key,-12}: {(z.Value * cellArea):0.00} m²");

        // Fairing
        var fair = FairingChecker.Check(Context.Mission, ShapePreset);
        sb.AppendLine(fair.message);

        if (TxtMetrics) TxtMetrics.text = sb.ToString();

        // Issues
        var issues = new List<ValidationIssue>(LayoutValidator.Validate(Grid, Context.Rules, Context.CrewSize, Context.MissionDays));
        if (TxtIssues)
        {
            if (issues.Count == 0) TxtIssues.text = "<color=#7CFC00>All good ✔</color>";
            else
            {
                var si = new System.Text.StringBuilder();
                foreach (var it in issues)
                {
                    string color = it.Severity switch
                    {
                        Severity.Error   => "#FF5555",
                        Severity.Warning => "#FFCC00",
                        _                => "#AAAAAA"
                    };
                    si.AppendLine($"<color={color}>• {it.Message}</color>");
                }
                TxtIssues.text = si.ToString();
            }
        }
    }
}
