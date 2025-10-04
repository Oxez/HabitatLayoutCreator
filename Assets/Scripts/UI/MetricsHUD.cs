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

    public DeckManager        Decks;

    public bool bindToActiveDeck = false;
    public bool sumAllDecks      = true;

    EditorGrid gridForMetrics =>
        (Decks && !sumAllDecks) ? Decks.Current : Grid;

    [Tooltip("частота пересчёта, сек")]
    public float refreshInterval = 0.25f;
    float _timer;

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < refreshInterval) return;
        _timer = 0f;

        if (Context == null || Context.Rules == null) return;

        // ---------- выбор источника метрик для левой колонки ----------
        Dictionary<ZoneType, int> countsTiles;
        float                     cellArea;

        if (sumAllDecks && Decks != null && Decks.Decks.Count > 0)
        {
            // посчитаем суммы тайлов для отображения (переведём из площадей обратно в "тайлы" условно)
            var areas = Decks.AreasAllDecksM2();
            cellArea = Decks.Decks[0].cellSize * Decks.Decks[0].cellSize;

            countsTiles = new Dictionary<ZoneType, int>();
            foreach (var kv in areas)
                countsTiles[kv.Key] = Mathf.RoundToInt(kv.Value / cellArea);
        }
        else
        {
            var src = (bindToActiveDeck && Decks != null) ? Decks.Current : Grid;
            if (src == null) return;

            cellArea = src.cellSize * src.cellSize;
            countsTiles = MetricsCalculator.CountTiles(src);
        }

        // ---------- METRICS (левая колонка) ----------
        float totalArea = countsTiles.Values.Sum() * cellArea;
        var   sb        = new System.Text.StringBuilder();
        sb.AppendLine($"Crew: {Context.CrewSize}, Days: {Context.MissionDays}");
        sb.AppendLine($"Total Area: {totalArea:0.00} m²  |  per crew: {(Context.CrewSize > 0 ? totalArea / Context.CrewSize : 0):0.00} m²");
        foreach (var z in countsTiles.OrderBy(k => k.Key))
            sb.AppendLine($"{z.Key,-12}: {(z.Value * cellArea):0.00} m²");

        var fair = FairingChecker.Check(Context.Mission, ShapePreset);
        sb.AppendLine(fair.message);
        if (TxtMetrics) TxtMetrics.text = sb.ToString();

        // ---------- ISSUES (правая колонка) ----------
        List<ValidationIssue> issues = new List<ValidationIssue>();
        if (sumAllDecks && Decks != null)
        {
            var areas = Decks.AreasAllDecksM2(); // ← суммарная проверка
            issues = new List<ValidationIssue>(LayoutValidator.ValidateAreas(
                areas, Context.Rules, Context.CrewSize, Context.MissionDays));
        }

        var pathSrc = (Decks != null) ? Decks.Current : Grid;
        if (pathSrc != null)
        {
            issues = new List<ValidationIssue>(LayoutValidator.Validate(
                pathSrc, Context.Rules, Context.CrewSize, Context.MissionDays));
        }

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
