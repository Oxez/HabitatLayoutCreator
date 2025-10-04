// Author Oxe
// Created at 04.10.2025 09:22

using UnityEngine;

public class GridTile : MonoBehaviour
{
    public int X, Y;
    public ZoneType Type { get; private set; } = ZoneType.None;

    Renderer _renderer;
    Color    _baseColor;

    public System.Action<GridTile, ZoneType, ZoneType> OnZoneChanged; // (tile, oldType, newType)

    public void SetZone(ZoneType z)
    {
        if (z == Type) return;
        var old = Type;
        Type = z;
        SetColor(GetColorFor(z));
        OnZoneChanged?.Invoke(this, old, z);
    }

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _baseColor = new Color(0.9f,0.9f,0.9f,1f);
        SetZone(ZoneType.None);
    }

    public void SetHover(bool on)
    {
        var baseCol = GetColorFor(Type);
        SetColor(on ? baseCol * 1.25f : baseCol);
    }

    void SetColor(Color c)
    {
        var mat = _renderer.material;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
        else if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
        else _renderer.material.color = c;
    }

    static Color GetColorFor(ZoneType z) => z switch
    {
        ZoneType.None        => new Color(0.90f, 0.90f, 0.92f, 1f),
        ZoneType.Sleep       => new Color(0.55f, 0.78f, 1.00f, 1f),
        ZoneType.Hygiene     => new Color(0.40f, 0.90f, 0.85f, 1f),
        ZoneType.ECLSS       => new Color(0.95f, 0.70f, 0.30f, 1f),
        ZoneType.Galley      => new Color(0.95f, 0.85f, 0.45f, 1f),
        ZoneType.Storage     => new Color(0.75f, 0.65f, 0.55f, 1f),
        ZoneType.Exercise    => new Color(0.95f, 0.45f, 0.45f, 1f),
        ZoneType.Medical     => new Color(0.80f, 0.60f, 1.00f, 1f),
        ZoneType.Maintenance => new Color(0.70f, 0.80f, 0.55f, 1f),
        ZoneType.Recreation  => new Color(1.00f, 0.65f, 0.85f, 1f),
        ZoneType.Airlock     => new Color(0.55f, 0.55f, 0.55f, 1f),
        ZoneType.Corridor    => new Color(0.80f, 0.90f, 1.00f, 1f),
        _                    => Color.white
    };
}
