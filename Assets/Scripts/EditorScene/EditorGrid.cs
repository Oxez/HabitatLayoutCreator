// Author Oxe
// Created at 04.10.2025 09:24

using UnityEngine;

public enum HabitatMask { Rectangle, Cylinder }

public class EditorGrid : MonoBehaviour
{
    [Header("Получаем из ShapePreset")]
    public float cellSize = 0.5f;
    public float lengthM  = 10f;
    public float diameterM = 6f;
    public HabitatMask mask = HabitatMask.Cylinder;

    [Header("Runtime")]
    public GameObject tilePrefab;

    public int cols { get; private set; }
    public int rows { get; private set; }
    public GridTile[,] Tiles { get; private set; }
    public bool[,] Mask { get; private set; }

    Transform _tilesRoot;

    public void Rebuild()
    {
        cols = Mathf.Max(1, Mathf.RoundToInt(lengthM / cellSize));
        rows = Mathf.Max(1, Mathf.RoundToInt(diameterM / cellSize));

        if (_tilesRoot != null) DestroyImmediate(_tilesRoot.gameObject);
        _tilesRoot = new GameObject("TilesRoot").transform;
        _tilesRoot.SetParent(transform, false);

        Tiles = new GridTile[cols, rows];
        Mask  = new bool[cols, rows];

        var origin = -new Vector3(cols * cellSize, 0, rows * cellSize) * 0.5f;
        float radius = diameterM * 0.5f;

        for (int x = 0; x < cols; x++)
        for (int y = 0; y < rows; y++)
        {
            var pos = origin + new Vector3((x + 0.5f) * cellSize, 0f, (y + 0.5f) * cellSize);

            bool inside = true;
            if (mask == HabitatMask.Cylinder)
            {
                float dz = pos.z;
                inside = (dz*dz) <= radius*radius;
            }

            Mask[x,y] = inside;

            var tileGO = Instantiate(tilePrefab, _tilesRoot);
            tileGO.transform.localPosition = pos;
            tileGO.transform.localScale = new Vector3(cellSize, cellSize, 1f);

            var tile = tileGO.AddComponent<GridTile>();
            tile.X = x; tile.Y = y;

            tileGO.GetComponent<Collider>().enabled = inside;
            tileGO.GetComponent<Renderer>().material.color *= inside ? 1f : 0.45f;

            Tiles[x, y] = tile;
        }
    }

    public bool TryRaycastTile(Ray ray, out GridTile hitTile)
    {
        hitTile = null;
        var hits = Physics.RaycastAll(ray, 1000f, ~0, QueryTriggerInteraction.Ignore);
        if (hits.Length == 0) return false;
        System.Array.Sort(hits, (a,b) => a.distance.CompareTo(b.distance));
        foreach (var h in hits)
        {
            var t = h.collider.GetComponent<GridTile>();
            if (t != null) { hitTile = t; return true; }
        }
        return false;
    }
}
