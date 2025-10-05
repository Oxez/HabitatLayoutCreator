// Author Oxe
// Created at 05.10.2025 12:08

using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer))]
public class PlaceableObject : MonoBehaviour
{
    [Header("Footprint (cells)")]
    public int sizeX = 2;
    public int sizeY = 1;
    public float heightM = 1;

    [Header("Snapping")]
    public bool snapToGrid = true;
    public bool occupyCells = true;

    [HideInInspector] public EditorGrid grid;
    [HideInInspector] public EditorGridOccupancy occupancy;

    public List<Vector2Int> GetCells(Vector2Int centerCell, int rot90)
    {
        int w = sizeX, h = sizeY;
        rot90 = (rot90 % 4 + 4) % 4;
        if ((rot90 & 1) == 1) { var t = w; w = h; h = t; }

        int halfW  = w / 2;
        int halfH  = h / 2;
        int startX = centerCell.x - halfW + (w % 2 == 0 ? 1 : 0);
        int endX   = centerCell.x + halfW;
        int startY = centerCell.y - halfH + (h % 2 == 0 ? 1 : 0);
        int endY   = centerCell.y + halfH;

        var list = new List<Vector2Int>(w * h);
        for (int x = startX; x <= endX; x++)
        for (int y = startY; y <= endY; y++)
            list.Add(new Vector2Int(x, y));

        return list;
    }

    public void ApplyOccupancy(Vector2Int centerCell, int rot90)
    {
        if (!occupyCells || occupancy == null) return;
        occupancy.UnmarkObject(this);
        var cells = GetCells(centerCell, rot90);
        occupancy.Mark(this, cells, true);
    }

    public void ClearOccupancy() => occupancy?.UnmarkObject(this);

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (grid == null) return;
        float cs = grid.cellSize;
        Gizmos.matrix = grid.transform.localToWorldMatrix;
        Gizmos.color = new Color(1, 1, 0, 0.2f);
        var cell = grid.WorldToCell(transform.position);
        var cells = GetCells(new Vector2Int(cell.x, cell.y), Mathf.RoundToInt(transform.eulerAngles.y / 90f));
        foreach (var c in cells)
        {
            var p = grid.CellToLocalCenter(c.x, c.y);
            Gizmos.DrawCube(new Vector3(p.x, 0.01f, p.z), new Vector3(cs, 0.02f, cs));
        }
    }
#endif
}
