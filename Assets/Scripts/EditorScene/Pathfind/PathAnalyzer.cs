// Author Oxe
// Created at 04.10.2025 17:15

using System.Collections.Generic;
using UnityEngine;

public static class PathAnalyzer
{
    static readonly (int dx, int dy)[] N4 = { (1, 0), (-1, 0), (0, 1), (0, -1) };

    static bool[,] BuildWalkable(EditorGrid g)
    {
        int W = g.cols, H = g.rows;
        var w = new bool[W, H];
        for (int x = 0; x < W; x++)
        for (int y = 0; y < H; y++)
        {
            var t = g.Tiles[x, y];
            if (t == null) continue;
            // ходим по коридорам; старт/финиш примыкают к коридорам
            w[x, y] = (t.Type == ZoneType.Corridor);
        }
        return w;
    }

    public static bool TryGetNearestCorridor(EditorGrid g, ZoneType z, out Vector2Int cell)
    {
        int W    = g.cols, H = g.rows;
        var q    = new Queue<Vector2Int>();
        var seen = new bool[W, H];
        // стартуем со всех клеток зоны
        for (int x = 0; x < W; x++)
        for (int y = 0; y < H; y++)
        {
            var t = g.Tiles[x, y];
            if (t != null && t.Type == z)
            {
                q.Enqueue(new Vector2Int(x, y));
                seen[x, y] = true;
            }
        }
        while (q.Count > 0)
        {
            var p = q.Dequeue();
            foreach (var (dx, dy) in N4)
            {
                int nx = p.x + dx, ny = p.y + dy;
                if (nx < 0 || ny < 0 || nx >= W || ny >= H || seen[nx, ny]) continue;
                seen[nx, ny] = true;
                var t = g.Tiles[nx, ny];
                if (t == null) continue;
                if (t.Type == ZoneType.Corridor)
                {
                    cell = new Vector2Int(nx, ny);
                    return true;
                }
                if (t.Type == ZoneType.None || t.Type == z) q.Enqueue(new Vector2Int(nx, ny));
            }
        }
        cell = default;
        return false;
    }

    public struct PathReport
    {
        public bool   hasPath;
        public float  lengthMeters;
        public float  minWidthMeters;
        public string why;
    }

    public static PathReport Analyze(EditorGrid g, ZoneType a, ZoneType b, float minWidthM)
    {
        var walkable = BuildWalkable(g);

        if (!TryGetNearestCorridor(g, a, out var start))
            return new PathReport{ hasPath=false, why=$"no corridor nearby {a}" };
        if (!TryGetNearestCorridor(g, b, out var goal))
            return new PathReport{ hasPath=false, why=$"no corridor nearby {b}" };

        // Можно оставить эрозию для “жёсткой” проверки требуемой ширины:
        var   dist      = Pathfinding.Clearance(g, walkable);
        float cs        = g.cellSize;
        int   needCells = Mathf.CeilToInt(minWidthM / cs);
        int   kNeeded   = Mathf.Max(1, Mathf.CeilToInt((needCells + 1) / 2f));

        bool[,] walkEroded = new bool[g.cols,g.rows];
        for (int x=0;x<g.cols;x++)
        for (int y=0;y<g.rows;y++)
            walkEroded[x,y] = walkable[x,y] && dist[x,y] >= kNeeded;

        // путь — сначала пробуем в “эрозированной” сетке (если нет — всё равно посчитаем бутылочное место)
        var path = Pathfinding.AStar(g, walkEroded, start, goal) ?? Pathfinding.AStar(g, walkable, start, goal);
        if (path == null)
            return new PathReport{ hasPath=false, why=$"no path {a} -> {b}" };

        // ТОЧНАЯ ширина: минимальный локальный размах вдоль пути
        int minCells = int.MaxValue;
        foreach (var c in path)
            minCells = Mathf.Min(minCells, LocalWidthCells(g, walkable, c.x, c.y));

        float length = (path.Count>0 ? (path.Count-1)*cs : 0f);
        float widthM = minCells * cs; // без -1 и прочих хаков

        return new PathReport {
            hasPath = (path != null),
            lengthMeters = length,
            minWidthMeters = widthM,
            why = (widthM + 1e-3f < minWidthM) ? $"Width {widthM:0.00} м < {minWidthM:0.00} м" : ""
        };
    }

    static int LocalWidthCells(EditorGrid g, bool[,] walkable, int x, int y)
    {
        int W=g.cols, H=g.rows;

        // скан по 4 направлениям до первого препятствия
        int left  =0;  for (int i=x-1; i>=0   && walkable[i,y]; i--) left++;
        int right =0; for (int i=x+1; i< W   && walkable[i,y]; i++) right++;
        int down  =0;  for (int j=y-1; j>=0   && walkable[x,j]; j--) down++;
        int up    =0;    for (int j=y+1; j< H   && walkable[x,j]; j++) up++;

        int widthH = 1 + left + right;    // количество клеток по горизонтали
        int widthV = 1 + down + up;       // количество клеток по вертикали
        return Mathf.Min(widthH, widthV); // эффективная “узкая” ширина
    }
}
