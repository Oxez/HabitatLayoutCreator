// Author Oxe
// Created at 04.10.2025 17:35

using System.Collections.Generic;
using UnityEngine;

public class PathOverlay : MonoBehaviour
{
    public EditorGrid grid;
    public ZoneType   fromType = ZoneType.Sleep;
    public ZoneType   toType   = ZoneType.Hygiene;
    public float      minAisle = 0.9f;
    public Material   lineMat;
    public float      width = 0.05f;

    LineRenderer lr;

    void LateUpdate()
    {
        if (!grid) return;
        var walkable = new bool[grid.cols, grid.rows];
        for (int x=0;x<grid.cols;x++)
        for (int y=0;y<grid.rows;y++)
            walkable[x,y] = grid.Tiles[x,y] && grid.Tiles[x,y].Type == ZoneType.Corridor;

        if (!PathAnalyzer.TryGetNearestCorridor(grid, fromType, out var s) ||
            !PathAnalyzer.TryGetNearestCorridor(grid, toType, out var t))
        { Clear(); return; }

        var path = Pathfinding.AStar(grid, walkable, s, t);
        if (path == null) { Clear(); return; }

        if (!lr)
        {
            lr = new GameObject("PathLR").AddComponent<LineRenderer>();
            lr.transform.SetParent(transform, false);
            lr.material = lineMat;
            lr.widthMultiplier = width;
            lr.loop = false;
            lr.useWorldSpace = true;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
        }

        lr.positionCount = path.Count;
        for (int i=0;i<path.Count;i++)
        {
            var c = path[i];
            var world = grid.transform.TransformPoint(
                new Vector3((c.x+0.5f)*grid.cellSize - grid.cols*grid.cellSize*0.5f,
                    0.02f,
                    (c.y+0.5f)*grid.cellSize - grid.rows*grid.cellSize*0.5f));
            lr.SetPosition(i, world);
        }
    }

    void Clear(){ if (lr) lr.positionCount = 0; }
}
