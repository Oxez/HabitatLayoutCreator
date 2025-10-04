// Author Oxe
// Created at 04.10.2025 09:24

using UnityEngine;

public enum HabitatShape { Rectangle, Cylinder }

public class EditorGrid : MonoBehaviour
{
    [Header("Grid")]
    public int cols = 20;
    public int rows = 12;
    public float cellSize = 0.5f;
    public HabitatShape shape = HabitatShape.Cylinder;
    [Tooltip("Для Cylinder — диаметр в метрах (по оси Z)")]
    public float diameterM = 6f;

    [Header("Refs")]
    public GameObject tilePrefab;

    public GridTile[,] Tiles { get; private set; }
    public bool[,] Mask { get; private set; } // куда можно ставить зоны

    Transform _tilesRoot;

    void Start()
    {
        Build();
    }

    public void Build()
    {
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

            if (shape == HabitatShape.Cylinder)
            {
                float dz = (pos.z) - 0f;
                float dx = 0f;
                inside = (dz*dz) <= radius*radius;
            }

            Mask[x, y] = inside;

            var tileGO = Instantiate(tilePrefab, _tilesRoot);
            tileGO.transform.localPosition = pos;
            tileGO.transform.localScale = new Vector3(cellSize, cellSize, 1f); // Quad лежит, масштаб Z влияет на ширину
            var tile = tileGO.AddComponent<GridTile>();
            tile.X = x; tile.Y = y;

            // Полутон для недоступных клеток
            if (!inside) tile.SetZone(ZoneType.Airlock);
            if (!inside) tileGO.GetComponent<Renderer>().material.SetFloat("_Surface", 1); // opaque
            tileGO.GetComponent<Renderer>().material.color *= inside ? 1f : 0.45f;
            tileGO.GetComponent<Collider>().enabled = true;
            if (!inside) tileGO.GetComponent<Collider>().enabled = false;

            Tiles[x, y] = tile;
        }
    }

    public bool TryRaycastTile(Ray ray, out GridTile hitTile)
    {
        hitTile = null;
        var hits = Physics.RaycastAll(ray, 1000f, ~0, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0) return false;

        System.Array.Sort(hits, (a,b) => a.distance.CompareTo(b.distance));
        foreach (var h in hits)
        {
            var t = h.collider.GetComponent<GridTile>();
            if (t != null) { hitTile = t; return true; }
        }
        return false;
    }
}
