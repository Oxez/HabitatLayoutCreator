// Author Oxe
// Created at 04.10.2025 19:06

using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridWalls : MonoBehaviour
{
    [Header("Sources")]
    public EditorGrid grid;                  // активный грид палубы

    [Header("Wall params")]
    public float wallHeight    = 2.4f;       // м
    public float wallThickness = 0.08f;      // м
    public bool  autoDoors     = true;       // делать проёмы у коридоров
    public float doorWidthM    = 0.9f;       // ширина проёма
    public ZoneType corridorType = ZoneType.Corridor;

    [Header("Lifecycle")]
    public bool buildOnAwake        = true;  // строить при загрузке сцены (в редакторе)
    public bool autoRebuildEachPlay = true;  // строить в Start() (в Play, когда грид уже готов)
    public bool liveUpdate          = true;  // перестраивать при каждом изменении тайла
    [Range(0f, 0.25f)] public float liveDebounce = 0.05f;

    MeshFilter _mf;
    float _nextBuildTime = -1f;
    int   _builtSegments;

    // ---------- Lifecycle ----------

    void Awake()
    {
        _mf = GetComponent<MeshFilter>();
#if UNITY_EDITOR
        if (!Application.isPlaying && buildOnAwake) Rebuild();
#endif
    }

    void OnEnable()
    {
        if (grid) grid.OnTileChanged += HandleTileChanged;
    }

    void OnDisable()
    {
        if (grid) grid.OnTileChanged -= HandleTileChanged;
    }

    void Start()
    {
        if (Application.isPlaying && autoRebuildEachPlay) Rebuild();
    }

    void OnValidate()
    {
        // легкий ребилд при смене параметров в инспекторе
        if (!Application.isPlaying && enabled) Rebuild();
    }

    void Update()
    {
        if (_nextBuildTime < 0f) return;
        if (!Application.isPlaying || Time.time >= _nextBuildTime)
        {
            _nextBuildTime = -1f;
            Rebuild();
        }
    }

    // Позвать при смене палубы
    public void SetGrid(EditorGrid g)
    {
        if (grid) grid.OnTileChanged -= HandleTileChanged;
        grid = g;
        if (grid) grid.OnTileChanged += HandleTileChanged;
        ScheduleRebuild();
    }

    void HandleTileChanged(EditorGrid g, int x, int y, ZoneType oldZ, ZoneType newZ)
    {
        if (!liveUpdate) return;
        ScheduleRebuild();
    }

    void ScheduleRebuild()
    {
        _nextBuildTime = Application.isPlaying ? Time.time + liveDebounce : 0f;
    }

    // ---------- Build ----------

    [ContextMenu("Rebuild Now")]
    public void Rebuild()
    {
        if (_mf == null) _mf = GetComponent<MeshFilter>();

        if (!grid)
        {
            _mf.sharedMesh = null;
            Debug.LogWarning("[GridWalls] No grid assigned");
            return;
        }
        if (grid.cols == 0 || grid.rows == 0 || grid.Tiles == null)
        {
            _mf.sharedMesh = null;
            Debug.LogWarning("[GridWalls] Grid is not built yet");
            return;
        }

        _builtSegments = 0;
        var mb = new MeshBuilder();

        int W = grid.cols, H = grid.rows;
        float cs = grid.cellSize;
        Vector3 origin = -new Vector3(W * cs, 0, H * cs) * 0.5f;

        bool IsSolid(int x, int y)
        {
            if (x < 0 || y < 0 || x >= W || y >= H) return false;
            var t = grid.Tiles[x, y];
            return t != null && t.Type != ZoneType.None && t.Type != corridorType;
        }

        bool IsCorridor(int x, int y)
        {
            if (x < 0 || y < 0 || x >= W || y >= H) return false;
            var t = grid.Tiles[x, y];
            return t != null && t.Type == corridorType;
        }

        // Обходим каждую ячейку и проверяем рёбра только в +X и +Y (без дублей)
        for (int x = 0; x < W; x++)
        for (int y = 0; y < H; y++)
        {
            TryEdge(x, y, +1, 0); // ребро справа
            TryEdge(x, y, 0, +1); // ребро сверху
        }

        void TryEdge(int cx, int cy, int dx, int dy)
        {
            int nx = cx + dx, ny = cy + dy;

            bool aSolid = IsSolid(cx, cy);
            bool bSolid = IsSolid(nx, ny);

            // нужна стена на границе "solid ↔ не solid"
            if (aSolid == bSolid) return;
            // определим, с какой стороны solid (для нормали наружу комнаты)
            bool solidOnA = aSolid && !bSolid;

            // мировые точки начала/конца ребра
            Vector3 c0, c1;
            if (dx == 1 && dy == 0)
            {
                // вертикальное ребро между (x+1) столбцами
                c0 = origin + new Vector3((cx + 1) * cs, 0, cy * cs);
                c1 = origin + new Vector3((cx + 1) * cs, 0, (cy + 1) * cs);
            }
            else // (dx==0, dy==1)
            {
                // горизонтальное ребро между (y+1) строками
                c0 = origin + new Vector3(cx * cs, 0, (cy + 1) * cs);
                c1 = origin + new Vector3((cx + 1) * cs, 0, (cy + 1) * cs);
            }

            Vector3 mid = (c0 + c1) * 0.5f;
            Vector3 dir = (c1 - c0).normalized;              // вдоль ребра
            Vector3 nrm = new Vector3(dir.z, 0, -dir.x);     // нормаль (по правилу руки)
            if (!solidOnA) nrm = -nrm;                       // наружу от solid
            float length = cs;

            // Авто-проём, если к стене примыкает коридор (с любой стороны)
            bool door = autoDoors && (IsCorridor(cx, cy) || IsCorridor(nx, ny));
            float doorW = Mathf.Clamp(doorWidthM, 0.4f, cs * 0.95f);

            if (door)
            {
                float half = length * 0.5f;
                float side = Mathf.Max(0f, half - doorW * 0.5f); // длина боковых сегментов
                if (side > 0.01f)
                {
                    AddWallSegment(mb, mid - dir * (doorW * 0.5f + side * 0.5f), dir, nrm, side);
                    AddWallSegment(mb, mid + dir * (doorW * 0.5f + side * 0.5f), dir, nrm, side);
                }
                // если side≈0 — проём съел весь сегмент, и это нормально
            }
            else
            {
                AddWallSegment(mb, mid, dir, nrm, length);
            }
        }

        _mf.sharedMesh = mb.Build();
#if UNITY_EDITOR
        Debug.Log($"[GridWalls] Rebuilt: segments={_builtSegments}, grid={grid.cols}x{grid.rows}, cell={grid.cellSize:0.###}");
#endif
    }

    void AddWallSegment(MeshBuilder mb, Vector3 center, Vector3 dir, Vector3 nrm, float length)
    {
        _builtSegments++;
        float t = wallThickness;
        float h = wallHeight;

        Vector3 right = dir;
        Vector3 outN  = nrm.normalized;
        Vector3 p = center + outN * (t * 0.5f); // наружная грань

        // Наружная
        Vector3 A = p - right * (length * 0.5f);
        Vector3 B = p + right * (length * 0.5f);
        Vector3 C = A + Vector3.up * h;
        Vector3 D = B + Vector3.up * h;
        mb.Quad(A, B, D, C);

        // Внутренняя
        Vector3 p2 = center - outN * (t * 0.5f);
        Vector3 A2 = p2 - right * (length * 0.5f);
        Vector3 B2 = p2 + right * (length * 0.5f);
        Vector3 C2 = A2 + Vector3.up * h;
        Vector3 D2 = B2 + Vector3.up * h;
        mb.Quad(B2, A2, C2, D2);

        // Верх
        mb.Quad(C, D, D2, C2);
    }
}
