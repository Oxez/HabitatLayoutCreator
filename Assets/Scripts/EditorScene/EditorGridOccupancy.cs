// Author Oxe
// Created at 05.10.2025 12:07

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(EditorGrid))]
public class EditorGridOccupancy : MonoBehaviour
{
    public EditorGrid Grid { get; private set; }
    // занятость по клеткам (true – заблокировано предметом)
    public bool[,] Blocked { get; private set; }
    // словарь: какие клетки занимает конкретный объект
    Dictionary<PlaceableObject, List<Vector2Int>> _byObject = new();

    void Awake()
    {
        Grid = GetComponent<EditorGrid>();
        Realloc();
    }

    public void Realloc()
    {
        Blocked = new bool[Grid.cols, Grid.rows];
        _byObject.Clear();
    }

    public bool CanPlace(List<Vector2Int> cells)
    {
        foreach (var c in cells)
        {
            if (c.x < 0 || c.y < 0 || c.x >= Grid.cols || c.y >= Grid.rows) return false;
            // нельзя ставить вне корпуса и на None
            var t = Grid.Tiles[c.x, c.y];
            if (t == null || t.Type == ZoneType.None) return false;
            if (Blocked[c.x, c.y]) return false;
        }
        return true;
    }

    public void Mark(PlaceableObject obj, List<Vector2Int> cells, bool value)
    {
        foreach (var c in cells)
        {
            if (c.x < 0 || c.y < 0 || c.x >= Grid.cols || c.y >= Grid.rows) continue;
            Blocked[c.x, c.y] = value;
        }
        if (value) _byObject[obj] = new List<Vector2Int>(cells);
        else _byObject.Remove(obj);
    }

    public void UnmarkObject(PlaceableObject obj)
    {
        if (!_byObject.TryGetValue(obj, out var cells)) return;
        Mark(obj, cells, false);
    }
}
