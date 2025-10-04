// Author Oxe
// Created at 04.10.2025 09:35

using UnityEngine;
using System.Linq;

public class SimpleMetrics : MonoBehaviour
{
    public EditorGrid grid;
    public float cellArea => grid.cellSize * grid.cellSize;

    [ContextMenu("Print Areas")]
    public void PrintAreas()
    {
        var tiles = grid.Tiles;
        int cols  = grid.cols, rows = grid.rows;

        var totals = new System.Collections.Generic.Dictionary<ZoneType, int>();
        for (int x=0;x<cols;x++)
        for (int y=0;y<rows;y++)
        {
            var t = tiles[x,y];
            if (t == null) continue;
            var z = t.Type;
            if (z == ZoneType.None) continue;
            if (!totals.ContainsKey(z)) totals[z] = 0;
            totals[z]++;
        }

        float totalArea = totals.Values.Sum()*cellArea;
        Debug.Log($"Total area: {totalArea:0.00} m²");
        foreach (var kv in totals.OrderBy(k=>k.Key))
            Debug.Log($"{kv.Key}: {(kv.Value*cellArea):0.00} m²");
    }
}
