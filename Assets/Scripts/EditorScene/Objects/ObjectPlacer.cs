// Author Oxe
// Created at 05.10.2025 12:09
// Adds bottom-alignment to floor (no half-under-floor), tool toggle, safe ghost

using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectPlacer : MonoBehaviour
{
    public Camera cam;

    [Header("Layers")]
    public LayerMask tileMask;
    public LayerMask removeMask;

    public EditorGrid grid;
    public EditorGridOccupancy occupancy;

    [Header("Prefabs")]
    public PlaceableObject bunkPrefab;
    public PlaceableObject rackPrefab;
    public PlaceableObject lockerPrefab;
    public PlaceableObject tablePrefab;
    public PlaceableObject treadmillPrefab;

    [Header("Ghost")]
    public Material ghostMat;
    PlaceableObject _ghost;
    PlaceableObject _activePrefab;
    int _rot90;

    [Header("Floor")]
    public float floorYOffset = 0f;

    [Header("Input")]
    public InputAction place;
    public InputAction remove;
    public InputAction rotate;
    public InputAction select1;
    public InputAction select2;
    public InputAction select3;
    public InputAction select4;
    public InputAction select5;

    bool       _placing;
    Vector2Int _lastPlacedCell = new(-9999, -9999);

    void OnEnable()
    {
        place.Enable(); remove.Enable(); rotate.Enable();
        select1.Enable(); select2.Enable(); select3.Enable(); select4.Enable(); select5.Enable();

        place.started   += OnPlaceStarted;
        place.performed += OnPlacePerformed;
        place.canceled  += OnPlaceCanceled;

        remove.performed += OnRemovePerformed;
        rotate.performed += OnRotatePerformed;

        select1.performed += OnSelect1;
        select2.performed += OnSelect2;
        select3.performed += OnSelect3;
        select4.performed += OnSelect4;
        select5.performed += OnSelect5;
    }

    void OnDisable()
    {
        place.started   -= OnPlaceStarted;
        place.performed -= OnPlacePerformed;
        place.canceled  -= OnPlaceCanceled;

        remove.performed -= OnRemovePerformed;
        rotate.performed -= OnRotatePerformed;

        select1.performed -= OnSelect1;
        select2.performed -= OnSelect2;
        select3.performed -= OnSelect3;
        select4.performed -= OnSelect4;
        select5.performed -= OnSelect5;

        place.Disable(); remove.Disable(); rotate.Disable();
        select1.Disable(); select2.Disable(); select3.Disable(); select4.Disable(); select5.Disable();
    }

    void Start()
    {
        if (!grid) grid = FindObjectOfType<EditorGrid>();
        EnsureOccupancyFor(grid);
        SetActive(bunkPrefab);
    }

    void Update()
    {
        if (_activePrefab == null)
        {
            if (_ghost != null) _ghost.gameObject.SetActive(false);
            return;
        }

        if (!RaycastTiles(out var hit, out var hitGrid, out var cell))
        {
            if (_ghost != null) _ghost.gameObject.SetActive(false);
            return;
        }

        if (_ghost == null) CreateGhost();
        if (_ghost == null) return;

        if (!_ghost.gameObject.activeSelf) _ghost.gameObject.SetActive(true);

        if (grid != hitGrid)
        {
            grid = hitGrid;
            EnsureOccupancyFor(grid);
        }

        Vector3 posWorld = grid.CellToWorldCenter(cell.x, cell.y);
        float floorY = grid.transform.position.y + floorYOffset;
        posWorld.y = floorY;

        _ghost.transform.SetPositionAndRotation(posWorld, Quaternion.Euler(0, _rot90 * 90f, 0));
        AlignBottomToFloor(_ghost, floorY);

        var footprint = _activePrefab.GetComponent<PlaceableObject>().GetCells(cell, _rot90);
        bool ok = occupancy != null && occupancy.CanPlace(footprint);

        Color ghostColor = ok ? new Color(0, 1, 0, 0.35f) : new Color(1, 0, 0, 0.35f);
        foreach (var r in _ghost.GetComponentsInChildren<Renderer>())
        {
            if (r == null) continue;
            var mpb = new MaterialPropertyBlock();
            r.GetPropertyBlock(mpb);
            mpb.SetColor("_BaseColor", ghostColor);
            r.SetPropertyBlock(mpb);
        }

        if (_placing && cell != _lastPlacedCell)
            TryPlaceAtCursor(force: false);
    }

    void OnPlaceStarted(InputAction.CallbackContext _)
    {
        if (_activePrefab == null) return;
        _placing = true;
        TryPlaceAtCursor(force: true);
    }
    void OnPlacePerformed(InputAction.CallbackContext _)
    {
        if (_activePrefab == null) return;
        TryPlaceAtCursor(force: false);
    }
    void OnPlaceCanceled(InputAction.CallbackContext _)
    {
        _placing = false;
        _lastPlacedCell = new(-9999, -9999);
    }

    void OnRemovePerformed(InputAction.CallbackContext _)
    {
        TryRemoveAtCursor();
        _lastPlacedCell = new(-9999, -9999);
    }

    void OnRotatePerformed(InputAction.CallbackContext _)
    {
        _rot90 = (_rot90 + 1) & 3;
        _lastPlacedCell = new(-9999, -9999);
    }

    void OnSelect1(InputAction.CallbackContext _) => ToggleActive(bunkPrefab);
    void OnSelect2(InputAction.CallbackContext _) => ToggleActive(rackPrefab);
    void OnSelect3(InputAction.CallbackContext _) => ToggleActive(lockerPrefab);
    void OnSelect4(InputAction.CallbackContext _) => ToggleActive(tablePrefab);
    void OnSelect5(InputAction.CallbackContext _) => ToggleActive(treadmillPrefab);

    void ToggleActive(PlaceableObject prefab)
    {
        if (_activePrefab == prefab) ClearActive();
        else SetActive(prefab);
    }

    void SetActive(PlaceableObject prefab)
    {
        _activePrefab = prefab;
        _rot90 = 0;
        _lastPlacedCell = new(-9999, -9999);
        CreateGhost();
    }

    void ClearActive()
    {
        _activePrefab = null;
        _lastPlacedCell = new(-9999, -9999);
        if (_ghost != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(_ghost.gameObject);
            else Destroy(_ghost.gameObject);
#else
            Destroy(_ghost.gameObject);
#endif
            _ghost = null;
        }
    }

    void CreateGhost()
    {
        if (_ghost != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(_ghost.gameObject);
            else Destroy(_ghost.gameObject);
#else
            Destroy(_ghost.gameObject);
#endif
            _ghost = null;
        }
        if (_activePrefab == null) return;

        _ghost = Instantiate(_activePrefab);
        _ghost.name = "_GHOST_" + _activePrefab.name;

        _ghost.occupancy = null;
        _ghost.grid = grid;

        foreach (var r in _ghost.GetComponentsInChildren<Renderer>())
        {
            if (r == null) continue;
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++) mats[i] = ghostMat;
            r.sharedMaterials = mats;
        }
    }

    bool RaycastTiles(out RaycastHit hit, out EditorGrid hitGrid, out Vector2Int cell)
    {
        hitGrid = null; cell = default;
        var pos = Mouse.current?.position.ReadValue() ?? Vector2.zero;
        if (!Physics.Raycast(cam.ScreenPointToRay(pos), out hit, 500f, tileMask))
            return false;

        var tile = hit.collider.GetComponent<GridTile>();
        if (tile != null)
        {
            hitGrid = tile.GetComponentInParent<EditorGrid>();
            if (hitGrid == null) return false;
            cell = new Vector2Int(tile.X, tile.Y);
            return true;
        }

        hitGrid = FindObjectOfType<EditorGrid>();
        if (hitGrid == null) return false;
        cell = hitGrid.WorldToCell(hit.point);
        return true;
    }

    void EnsureOccupancyFor(EditorGrid g)
    {
        if (g == null) { occupancy = null; return; }
        occupancy = g.GetComponent<EditorGridOccupancy>();
        if (!occupancy) occupancy = g.gameObject.AddComponent<EditorGridOccupancy>();
        if (_ghost != null) _ghost.grid = g;
    }

    void TryPlaceAtCursor(bool force)
    {
        if (_activePrefab == null) return;
        if (!RaycastTiles(out var hit, out var hitGrid, out var cell)) return;

        if (grid != hitGrid)
        {
            grid = hitGrid;
            EnsureOccupancyFor(grid);
        }

        var footprint = _activePrefab.GetComponent<PlaceableObject>().GetCells(cell, _rot90);
        if (!force && (occupancy == null || !occupancy.CanPlace(footprint)))
        {
            _lastPlacedCell = cell;
            return;
        }

        var go = Instantiate(_activePrefab);
        go.name = _activePrefab.name;
        go.grid = grid;
        go.occupancy = occupancy;

        float floorY = grid.transform.position.y + floorYOffset;

        Vector3 posWorld = grid.CellToWorldCenter(cell.x, cell.y);
        posWorld.y = floorY;
        go.transform.SetPositionAndRotation(posWorld, Quaternion.Euler(0, _rot90 * 90f, 0));

        AlignBottomToFloor(go, floorY);

        go.ApplyOccupancy(cell, _rot90);

        _lastPlacedCell = cell;
    }

    void TryRemoveAtCursor()
    {
        var pos = Mouse.current?.position.ReadValue() ?? Vector2.zero;
        if (!Physics.Raycast(cam.ScreenPointToRay(pos), out var hit, 500f, removeMask)) return;

        var obj = hit.collider.GetComponentInParent<PlaceableObject>();
        if (obj != null)
        {
            obj.ClearOccupancy();
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(obj.gameObject);
            else Destroy(obj.gameObject);
#else
            Destroy(obj.gameObject);
#endif
        }
        _lastPlacedCell = new(-9999, -9999);
    }

    static void AlignBottomToFloor(PlaceableObject obj, float floorY)
    {
        if (obj == null) return;
        var renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers == null || renderers.Length == 0) return;

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r != null) b.Encapsulate(r.bounds);
        }

        float deltaY = floorY - b.min.y;
        if (Mathf.Abs(deltaY) > 1e-5f)
            obj.transform.position += new Vector3(0f, deltaY, 0f);
    }
}
