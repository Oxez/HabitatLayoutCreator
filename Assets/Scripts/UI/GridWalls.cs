// Author Oxe
// Created at 04.10.2025 19:06

using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridWalls : MonoBehaviour
{
    [Header("Sources")]
    public EditorGrid grid;

    [Header("Wall params")]
    public float wallHeight = 1.4f;
    public float    wallThickness = 0.0001f;
    public bool     autoDoors     = true;
    public float    doorWidthM    = 0.9f;
    public ZoneType corridorType  = ZoneType.Corridor;

    [Header("Lifecycle")]
    public                    bool  buildOnStartPlay = true;
    public                    bool  liveUpdate       = true;
    [Range(0f, 0.25f)] public float liveDebounce     = 0.05f;

    [Header("Transform binding")]
    public bool followGridTransform = true;
    public bool parentUnderGrid = true;

    [Header("Rendering mode")]
    public bool thinShell = true;
    [Range(0f, 0.02f)] public float endInset = 0.006f;
    [Range(0f, 0.01f)] public float capInset = 0.0015f;

    [Header("Colors")]
    public Material wallMatBase;
    public Color                                              hullColor = new(0.85f, 0.85f, 0.88f, 1f);
    Transform                                                 _bucketsRoot;
    System.Collections.Generic.Dictionary<Color, MeshBuilder> _bucket;

    [Header("Corner posts")]
    public bool roomPosts = true;
    [Range(0.01f, 0.25f)] public float postSize  = 0.08f;
    [Range(0f, 0.02f)]    public float postInset = 0.001f;


    public bool        autoBind = true;
    public DeckManager deckManager;
    public float       rebindInterval = 1.0f;
    float              _nextRebindTs  = -1f;

    MeshFilter _mf;
    float      _nextBuildTime = -1f;
    bool       _rebuilding;

    void OnEnable()
    {
        if (!_mf) _mf = GetComponent<MeshFilter>();
        if (autoBind)
        {
            if (deckManager == null) deckManager = FindObjectOfType<DeckManager>();
            if (deckManager != null) deckManager.OnActiveDeckChanged += SetGrid;
        }
        SubscribeToGrid(grid);
    }

    void OnDisable()
    {
        if (autoBind && deckManager != null) deckManager.OnActiveDeckChanged -= SetGrid;
        UnsubscribeFromGrid(grid);
    }

    void Start()
    {
        if (Application.isPlaying)
        {
            if (autoBind) TryAutoBind(now: true);
            if (buildOnStartPlay) ScheduleRebuildSoon();
        }
    }

    void Update()
    {
        if (autoBind && Application.isPlaying && Time.time >= _nextRebindTs)
        {
            TryAutoBind(now: true);
            _nextRebindTs = Time.time + rebindInterval;
        }

        if (_nextBuildTime >= 0f && (!Application.isPlaying || Time.time >= _nextBuildTime))
        {
            _nextBuildTime = -1f;
            Rebuild();
        }
    }

    void TryAutoBind(bool now = false)
    {
        if (deckManager == null)
            deckManager = FindObjectOfType<DeckManager>();

        if (deckManager != null && deckManager.Current != null && deckManager.Current != grid)
        {
            SetGrid(deckManager.Current);
            if (now) ScheduleRebuildSoon();
            return;
        }

        var allGrids = FindObjectsOfType<EditorGrid>();
        foreach (var g in allGrids)
        {
            if (!g.isActiveAndEnabled) continue;
            if (g.cols <= 0 || g.rows <= 0 || g.Tiles == null) continue; // ещё не построен
            if (g == grid) return;
            SetGrid(g);
            if (now) ScheduleRebuildSoon();
            return;
        }
    }

    void OnValidate()
    {
        ScheduleRebuildNow();
    }

    public void SetGrid(EditorGrid g)
    {
        if (grid == g)
        {
            ScheduleRebuildSoon();
            return;
        }
        UnsubscribeFromGrid(grid);
        grid = g;

        if (parentUnderGrid && grid != null)
        {
            transform.SetParent(grid.transform, true);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
        ScheduleRebuildSoon();

        SubscribeToGrid(grid);
        ScheduleRebuildSoon();
    }

    void SubscribeToGrid(EditorGrid g)
    {
        if (g != null) g.OnTileChanged += HandleTileChanged;
    }
    void UnsubscribeFromGrid(EditorGrid g)
    {
        if (g != null) g.OnTileChanged -= HandleTileChanged;
    }

    void HandleTileChanged(EditorGrid g, int x, int y, ZoneType oldZ, ZoneType newZ)
    {
        if (!liveUpdate) return;
        ScheduleRebuildSoon();
    }

    void ScheduleRebuildSoon()
    {
        _nextBuildTime = Application.isPlaying ? Time.time + liveDebounce : 0f;
    }
    void ScheduleRebuildNow()
    {
        _nextBuildTime = 0f;
    }

    [ContextMenu("Rebuild Now")]
    public void Rebuild()
    {
        if (_rebuilding) return;
        _rebuilding = true;

        if (!_mf) _mf = GetComponent<MeshFilter>();

        if (!grid || grid.Tiles == null || grid.cols <= 0 || grid.rows <= 0)
        {
            _mf.sharedMesh = null;
            ScheduleRebuildSoon();

            _rebuilding = false;
            return;
        }

        if (followGridTransform && grid && !parentUnderGrid)
        {
            transform.SetPositionAndRotation(grid.transform.position, grid.transform.rotation);
            transform.localScale = grid.transform.lossyScale;
        }
        if (parentUnderGrid && transform.parent != grid.transform)
        {
            transform.SetParent(grid.transform, true);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        EnsureBucketsRoot();
        ClearBuckets();
        _bucket = new System.Collections.Generic.Dictionary<Color, MeshBuilder>();

        int   W  = grid.cols, H = grid.rows;
        float cs = grid.cellSize;

        ZoneType GetTypeAt(int x, int y)
        {
            if (x < 0 || y < 0 || x >= W || y >= H) return ZoneType.None;
            var t = grid.Tiles[x, y];
            return t ? t.Type : ZoneType.None;
        }

        bool IsRoomType(ZoneType z) => z != ZoneType.None && z != corridorType;
        bool IsCorrType(ZoneType z) => z == corridorType;
        bool IsInsideType(ZoneType z) => z != ZoneType.None;
        Vector3 CornerLocal(int x, int y) => new(x * cs - W * cs * 0.5f,0f, y * cs - H * cs * 0.5f);

        void TryEdge(int cx, int cy, int dx, int dy)
        {
            int nx = cx + dx, ny = cy + dy;

            ZoneType aZ = GetTypeAt(cx, cy);
            ZoneType bZ = GetTypeAt(nx, ny);

            bool aIn = IsInsideType(aZ);
            bool bIn = IsInsideType(bZ);

            bool roomVsCorr      = (IsRoomType(aZ) && IsCorrType(bZ)) || (IsCorrType(aZ) && IsRoomType(bZ));
            bool insideVsOutside = aIn ^ bIn;
            if (!roomVsCorr && !insideVsOutside) return; // нет стены

            Vector3 c0, c1;
            if (dx == 1 && dy == 0)
            { // вертикальное ребро
                c0 = CornerLocal(cx + 1, cy);
                c1 = CornerLocal(cx + 1, cy + 1);
            }
            else
            { // горизонтальное ребро
                c0 = CornerLocal(cx, cy + 1);
                c1 = CornerLocal(cx + 1, cy + 1);
            }

            Vector3 mid = (c0 + c1) * 0.5f;
            Vector3 dir = (c1 - c0).normalized;
            Vector3 nrm = new Vector3(dir.z, 0, -dir.x);

            bool outwardFromA = (IsRoomType(aZ) && IsCorrType(bZ)) || (aIn && !bIn);
            if (!outwardFromA) nrm = -nrm;

            float length = cs;
            bool  door = autoDoors && roomVsCorr;
            float gap  = door ? (doorWidthM + (thinShell ? 0f : 2f * endInset)) : 0f;
            float half = length * 0.5f;
            float side = door ? Mathf.Max(0f, half - gap * 0.5f) : half;

            Color colorA = GridTile.GetColorFor(aZ);
            Color colorB = GridTile.GetColorFor(bZ);

            if (door)
            {
                if (side > 0.01f)
                {
                    AddWallSegmentColored(mid - dir * (gap * 0.5f + side * 0.5f), dir, nrm, side, colorA, colorB);
                    AddWallSegmentColored(mid + dir * (gap * 0.5f + side * 0.5f), dir, nrm, side, colorA, colorB);
                }
            }
            else
                AddWallSegmentColored(mid, dir, nrm, length, colorA, colorB);
        }

        for (int x = 0; x < W; x++)
        for (int y = 0; y < H; y++)
        {
            TryEdge(x, y, +1, 0);
            TryEdge(x, y, 0, +1);
        }

        if (roomPosts)
            BuildRoomCornerPostsColored(W, H, cs);

        foreach (var kv in _bucket)
        {
            var go = new GameObject($"Walls_{ColorUtility.ToHtmlStringRGB(kv.Key)}");
            go.transform.SetParent(_bucketsRoot, false);
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mf.sharedMesh = kv.Value.Build();

            var mat = new Material(wallMatBase);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", kv.Key);
            else if (mat.HasProperty("_Color")) mat.SetColor("_Color", kv.Key);
            if (thinShell && mat.HasProperty("_CullMode")) mat.SetFloat("_CullMode", 0f); // Both
            mr.sharedMaterial = mat;
        }

        _mf.sharedMesh = null;
        _rebuilding = false;
    }

    void BuildRoomCornerPostsColored(int W, int H, float cs)
    {
        ZoneType GetTypeAt(int x, int y)
        {
            if (x < 0 || y < 0 || x >= W || y >= H) return ZoneType.None;
            var t = grid.Tiles[x, y];
            return t ? t.Type : ZoneType.None;
        }

        bool IsRoomType(ZoneType z) => z != ZoneType.None && z != corridorType;
        Vector3 CornerLocal(int x, int y) => new(
            x * cs - W * cs * 0.5f,
            0f,
            y * cs - H * cs * 0.5f
        );

        for (int vx = 0; vx <= W; vx++)
        for (int vy = 0; vy <= H; vy++)
        {
            ZoneType z00 = GetTypeAt(vx - 1, vy - 1);
            ZoneType z10 = GetTypeAt(vx, vy - 1);
            ZoneType z01 = GetTypeAt(vx - 1, vy);
            ZoneType z11 = GetTypeAt(vx, vy);

            bool r00 = IsRoomType(z00);
            bool r10 = IsRoomType(z10);
            bool r01 = IsRoomType(z01);
            bool r11 = IsRoomType(z11);

            int roomCount =
                (r00 ? 1 : 0) + (r10 ? 1 : 0) + (r01 ? 1 : 0) + (r11 ? 1 : 0);

            if (roomCount == 0) continue; // нет ни одной комнаты — стойка не нужна

            bool hasBoundary =
                (z00 != z10) || (z10 != z11) || (z11 != z01) || (z01 != z00);
            if (!hasBoundary) continue;

            ZoneType roomZ =
                r00 ? z00 :
                r10 ? z10 :
                r01 ? z01 : z11;
            Color c  = GridTile.GetColorFor(roomZ);
            var   mb = GetB(c);

            Vector3 center = CornerLocal(vx, vy) + new Vector3(0, wallHeight * 0.5f, 0);

            Vector3 nudged = Vector3.zero;
            if (r00) nudged += new Vector3(-1, 0, -1);
            if (r10) nudged += new Vector3(+1, 0, -1);
            if (r01) nudged += new Vector3(-1, 0, +1);
            if (r11) nudged += new Vector3(+1, 0, +1);
            if (nudged.sqrMagnitude > 1e-6f)
                center += nudged.normalized * postInset;

            float s = Mathf.Max(postSize, 0.01f);
            AddBox(mb, center, s, wallHeight, s);
        }
    }


    void BuildCornerPostsColored(int W, int H, float cs, Color postColor)
    {
        if (thinShell) return;

        MeshBuilder mb = GetB(postColor);
        Vector3 CornerLocal(int x, int y) => new(
            x * cs - W * cs * 0.5f,
            0f,
            y * cs - H * cs * 0.5f
        );

        bool IsInsideCell(int x, int y)
        {
            if (x < 0 || y < 0 || x >= W || y >= H) return false;
            var t = grid.Tiles[x, y];
            if (!t) return false;
            return t.Type != ZoneType.None;
        }

        for (int vx = 0; vx <= W; vx++)
        for (int vy = 0; vy <= H; vy++)
        {
            int inside =
                (IsInsideCell(vx - 1, vy - 1) ? 1 : 0) +
                (IsInsideCell(vx, vy - 1) ? 1 : 0) +
                (IsInsideCell(vx - 1, vy) ? 1 : 0) +
                (IsInsideCell(vx, vy) ? 1 : 0);

            if (inside == 0 || inside == 4) continue;

            Vector3 c = CornerLocal(vx, vy) + new Vector3(0, wallHeight * 0.5f, 0);
            float   t = Mathf.Max(0.02f, wallThickness); // чтобы стойка была видна
            AddBox(mb, c, t, wallHeight, t);
        }
    }


    MeshBuilder GetB(Color c)
    {
        if (!_bucket.TryGetValue(c, out var mb))
        {
            mb = new MeshBuilder();
            _bucket[c] = mb;
        }
        return mb;
    }

    void AddWallSegmentColored(Vector3 center, Vector3 dir, Vector3 nrm, float length, Color colorA, Color colorB)
    {
        Vector3 right = dir.normalized;
        Vector3 outN  = nrm.normalized;

        if (thinShell)
        {
            float h   = wallHeight;
            float eps = 0.001f;

            {
                var     mb = GetB(colorA);
                Vector3 A  = center - right * (length * 0.5f) + outN * eps;
                Vector3 B  = center + right * (length * 0.5f) + outN * eps;
                Vector3 C  = A + Vector3.up * h;
                Vector3 D  = B + Vector3.up * h;
                mb.Quad(A, B, D, C);
            }
            {
                var     mb = GetB(colorB);
                Vector3 A  = center + right * (length * 0.5f) - outN * eps;
                Vector3 B  = center - right * (length * 0.5f) - outN * eps;
                Vector3 C  = A + Vector3.up * h;
                Vector3 D  = B + Vector3.up * h;
                mb.Quad(A, B, D, C);
            }
            return;
        }

        float t    = wallThickness;
        float h2   = wallHeight;
        float eps2 = Mathf.Max(0.0005f, capInset);
        float L    = Mathf.Max(0f, length - 2f * endInset);

        {
            var     mb = GetB(colorA);
            Vector3 p  = center + outN * (t * 0.5f);
            Vector3 A  = p - right * (L * 0.5f);
            Vector3 B  = p + right * (L * 0.5f);
            Vector3 C  = A + Vector3.up * h2;
            Vector3 D  = B + Vector3.up * h2;
            mb.Quad(A, B, D, C);
        }
        {
            var     mb = GetB(colorB);
            Vector3 p  = center - outN * (t * 0.5f);
            Vector3 A  = p + right * (L * 0.5f);
            Vector3 B  = p - right * (L * 0.5f);
            Vector3 C  = A + Vector3.up * h2;
            Vector3 D  = B + Vector3.up * h2;
            mb.Quad(A, B, D, C);
        }
        {
            var     mb   = GetB(hullColor);
            Vector3 pOut = center + outN * (t * 0.5f);
            Vector3 pIn  = center - outN * (t * 0.5f);
            Vector3 A    = (pOut - right * (L * 0.5f)) + Vector3.up * h2;
            Vector3 B    = (pOut + right * (L * 0.5f)) + Vector3.up * h2;
            Vector3 C    = (pIn + right * (L * 0.5f)) + Vector3.up * h2;
            Vector3 D    = (pIn - right * (L * 0.5f)) + Vector3.up * h2;
            Vector3 shrink = right * eps2 + outN * eps2;
            mb.Quad(A + shrink, B - shrink, C - shrink, D + shrink);
        }
    }

    void AddBox(MeshBuilder mb, Vector3 center, float sx, float sy, float sz)
    {
        // sx – X (длина), sy – Y (высота), sz – Z (глубина)
        float hx = sx * 0.5f, hy = sy * 0.5f, hz = sz * 0.5f;

        Vector3 A = center + new Vector3(-hx, -hy, -hz);
        Vector3 B = center + new Vector3(+hx, -hy, -hz);
        Vector3 C = center + new Vector3(+hx, +hy, -hz);
        Vector3 D = center + new Vector3(-hx, +hy, -hz);

        Vector3 E = center + new Vector3(-hx, -hy, +hz);
        Vector3 F = center + new Vector3(+hx, -hy, +hz);
        Vector3 G = center + new Vector3(+hx, +hy, +hz);
        Vector3 H = center + new Vector3(-hx, +hy, +hz);

        // фронт/зад
        mb.Quad(A, B, C, D); // -Z
        mb.Quad(F, E, H, G); // +Z
        // лево/право
        mb.Quad(E, A, D, H); // -X
        mb.Quad(B, F, G, C); // +X
        // низ/верх
        mb.Quad(E, F, B, A); // низ
        mb.Quad(D, C, G, H); // верх
    }

    void BuildCornerPosts(MeshBuilder mb, int W, int H, float cs)
    {
        Vector3 CornerLocal(int x, int y) => new Vector3(
            (x * cs) - (W * cs * 0.5f),
            0f,
            (y * cs) - (H * cs * 0.5f)
        );

        bool IsSolidCell(int x, int y)
        {
            if (x < 0 || y < 0 || x >= W || y >= H) return false;
            var t = grid.Tiles[x, y];
            return t != null && t.Type != ZoneType.None && t.Type != corridorType;
        }

        for (int vx = 0; vx <= W; vx++)
        for (int vy = 0; vy <= H; vy++)
        {
            bool s00 = IsSolidCell(vx - 1, vy - 1);
            bool s10 = IsSolidCell(vx, vy - 1);
            bool s01 = IsSolidCell(vx - 1, vy);
            bool s11 = IsSolidCell(vx, vy);

            int solidCount = (s00 ? 1 : 0) + (s10 ? 1 : 0) + (s01 ? 1 : 0) + (s11 ? 1 : 0);
            if (solidCount == 0 || solidCount == 4) continue;

            Vector3 c = CornerLocal(vx, vy) + new Vector3(0, wallHeight * 0.5f, 0);
            float   t = wallThickness;

            AddBox(mb, c, t, wallHeight, t);
        }
    }

    void AddWallSegment(MeshBuilder mb, Vector3 center, Vector3 dir, Vector3 nrm, float length)
    {
        Vector3 right = dir.normalized;
        Vector3 outN  = nrm.normalized;

        if (thinShell)
        {
            Vector3 a = center - right * (length * 0.5f);
            Vector3 b = center + right * (length * 0.5f);
            Vector3 c = a + Vector3.up * wallHeight;
            Vector3 d = b + Vector3.up * wallHeight;
            mb.Quad(a, b, d, c);
            return;
        }

        float t   = wallThickness;
        float h   = wallHeight;
        float eps = Mathf.Max(0.0005f, capInset);

        length = Mathf.Max(0f, length - 2f * endInset);

        Vector3 pOut = center + outN * (t * 0.5f);
        Vector3 A    = pOut - right * (length * 0.5f);
        Vector3 B    = pOut + right * (length * 0.5f);
        Vector3 C    = A + Vector3.up * h;
        Vector3 D    = B + Vector3.up * h;
        mb.Quad(A, B, D, C);

        Vector3 pIn = center - outN * (t * 0.5f);
        Vector3 A2  = pIn - right * (length * 0.5f);
        Vector3 B2  = pIn + right * (length * 0.5f);
        Vector3 C2  = A2 + Vector3.up * h;
        Vector3 D2  = B2 + Vector3.up * h;
        mb.Quad(B2, A2, C2, D2);

        mb.Quad(
            C + right * eps - outN * eps,
            D - right * eps - outN * eps,
            D2 - right * eps + outN * eps,
            C2 + right * eps + outN * eps
        );
    }

    void EnsureBucketsRoot()
    {
        if (_bucketsRoot == null)
        {
            var go = new GameObject("WallBuckets");
            go.transform.SetParent(transform, false);
            _bucketsRoot = go.transform;
        }
    }

    void ClearBuckets()
    {
        if (_bucketsRoot == null) return;
        for (int i = _bucketsRoot.childCount - 1; i >= 0; i--)
            DestroyImmediate(_bucketsRoot.GetChild(i).gameObject);
    }
}
