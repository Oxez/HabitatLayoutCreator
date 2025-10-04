// Author Oxe
// Created at 04.10.2025 11:14

using System.Collections.Generic;

public static class MetricsCalculator
{
    public static float CellArea(float cellSize) => cellSize * cellSize;

    public static Dictionary<ZoneType, int> CountTiles(EditorGrid grid)
    {
        var dict = new Dictionary<ZoneType, int>();
        for (int x=0; x<grid.cols; x++)
        for (int y=0; y<grid.rows; y++)
        {
            var t = grid.Tiles[x,y];
            if (t == null) continue;
            var z = t.Type;
            if (z == ZoneType.None) continue;
            if (!dict.ContainsKey(z)) dict[z] = 0;
            dict[z]++;
        }
        return dict;
    }
}
