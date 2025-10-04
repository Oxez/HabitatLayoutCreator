// Author Oxe
// Created at 04.10.2025 09:28

using UnityEngine;
using UnityEngine.InputSystem;

public class ZoneBrush : MonoBehaviour
{
    public EditorGrid grid;
    public ZoneType   current  = ZoneType.Sleep;
    public KeyCode    eraseKey = KeyCode.Mouse1;

    GridTile _hover;

    void Update()
    {
        var cam = Camera.main;
        if (cam == null) return;
        var mp = Input.mousePosition;
        Debug.DrawRay(cam.ScreenPointToRay(mp).origin, cam.ScreenPointToRay(mp).direction * 100f, Color.cyan);

        // hover
        if (grid.TryRaycastTile(Camera.main.ScreenPointToRay(Input.mousePosition), out var t))
        {
            if (_hover && _hover != t) _hover.SetHover(false);
            _hover = t;
            _hover.SetHover(true);

            bool painting = Input.GetMouseButton(0);
            bool erasing  = Input.GetKey(eraseKey);

            if (painting && grid.Mask[t.X, t.Y]) t.SetZone(current);
            if (erasing) t.SetZone(ZoneType.None);
        }
        else if (_hover)
        {
            _hover.SetHover(false);
            _hover = null;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) current = ZoneType.Sleep;
        if (Input.GetKeyDown(KeyCode.Alpha2)) current = ZoneType.Hygiene;
        if (Input.GetKeyDown(KeyCode.Alpha3)) current = ZoneType.ECLSS;
        if (Input.GetKeyDown(KeyCode.Alpha4)) current = ZoneType.Galley;
        if (Input.GetKeyDown(KeyCode.Alpha5)) current = ZoneType.Storage;
        if (Input.GetKeyDown(KeyCode.Alpha6)) current = ZoneType.Exercise;
        if (Input.GetKeyDown(KeyCode.Alpha7)) current = ZoneType.Medical;
        if (Input.GetKeyDown(KeyCode.Alpha8)) current = ZoneType.Maintenance;
        if (Input.GetKeyDown(KeyCode.Alpha9)) current = ZoneType.Recreation;
        if (Input.GetKeyDown(KeyCode.Alpha0)) current = ZoneType.Airlock;

        // test
        if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
        {
            for (int x = 2; x < 8; x++)
            for (int y = 3; y < 6; y++)
            {
                var tile = grid.Tiles[x,y];
                if (tile != null && grid.Mask[x,y]) tile.SetZone(current);
            }
        }
    }
}
