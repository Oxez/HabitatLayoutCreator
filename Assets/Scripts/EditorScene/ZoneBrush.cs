// Author Oxe
// Created at 04.10.2025 09:28

using UnityEngine;
using UnityEngine.EventSystems;

public class ZoneBrush : MonoBehaviour
{
    public EditorGrid grid;
    public ZoneType   current  = ZoneType.Sleep;
    public KeyCode    eraseKey = KeyCode.Mouse1;

    GridTile _hover;

    public void SetCurrentBrush(ZoneType type)
    {
        current = type;
    }

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

            if (painting && grid.Mask[t.X, t.Y] && !(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())) t.SetZone(current);
            if (erasing) t.SetZone(ZoneType.None);
        }
        else if (_hover)
        {
            _hover.SetHover(false);
            _hover = null;
        }
    }
}
