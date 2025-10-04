// Author Oxe
// Created at 04.10.2025 09:22

using UnityEngine;

public class GridTile : MonoBehaviour
{
    public int X, Y;
    public ZoneType Type { get; private set; } = ZoneType.None;

    Renderer _renderer;
    MaterialPropertyBlock _mpb;
    bool _hover;

    static readonly Color kNeutral = new Color(0.90f, 0.90f, 0.92f, 1f);

    public System.Action<GridTile, ZoneType, ZoneType> OnZoneChanged; // (tile, oldType, newType)

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();
        ApplyVisual();
    }

    public void SetZone(ZoneType z)
    {
        if (z == Type) return;
        var old = Type;
        Type = z;
        ApplyVisual();
        OnZoneChanged?.Invoke(this, old, z);
    }

    public void SetHover(bool on)
    {
        if (_hover == on) return;
        _hover = on;
        ApplyVisual();
    }

    void ApplyVisual()
    {
        Color baseCol = (Type == ZoneType.None) ? kNeutral : GetColorFor(Type);
        if (_hover)
            baseCol = Color.Lerp(baseCol, Color.gray, 0.25f);

        _renderer.GetPropertyBlock(_mpb);
        if (_renderer.sharedMaterial != null)
        {
            if (_renderer.sharedMaterial.HasProperty("_BaseColor"))
                _mpb.SetColor("_BaseColor", baseCol);
            else if (_renderer.sharedMaterial.HasProperty("_Color"))
                _mpb.SetColor("_Color", baseCol);
            else
                _mpb.SetColor("_BaseColor", baseCol);
        }
        else
            _mpb.SetColor("_BaseColor", baseCol);

        _renderer.SetPropertyBlock(_mpb);
    }

    public static Color GetColorFor(ZoneType z) => z switch
    {
        ZoneType.None        => kNeutral,
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
