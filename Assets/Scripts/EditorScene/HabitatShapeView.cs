// Author Oxe
// Created at 04.10.2025 13:27

using UnityEngine;

public class HabitatShapeView : MonoBehaviour
{
    public HabitatShapePreset preset;

    [Header("Solid (старый)")]
    public Material shellMaterial;

    [Header("Wireframe")]
    public Material lineMaterial;   // Unlit/Color или URP/Unlit
    public float lineWidth = 0.05f;
    [Range(8,256)] public int circleSegments = 96;
    public Color lineColor = new Color(1,1,1,0.9f);

    public float deckHeight = 2.5f;

    public enum ShellRenderMode { Solid, Wireframe }
    public ShellRenderMode mode = ShellRenderMode.Wireframe;

    Transform _shellRoot;

    public void BuildShell()
    {
        if (_shellRoot != null) DestroyImmediate(_shellRoot.gameObject);
        _shellRoot = new GameObject("ShellRoot").transform;
        _shellRoot.SetParent(transform, false);

        if (mode == ShellRenderMode.Solid)
        {
            BuildSolid();
        }
        else
        {
            BuildWire();
        }

        for (int d=0; d<preset.Decks; d++)
        {
            float y = d * deckHeight;
            if (preset.Shape == HabitatShape.Prism)
                CreateRectXZ(new Vector3(0,y,0), preset.Length, preset.Diameter, $"Deck_{d}_Rect");
            else
                CreateCircleXZ(new Vector3(0,y,0), preset.Diameter * 0.5f * 0.98f).name = $"Deck_{d}_Ring";
        }
    }

    void BuildSolid()
    {
        switch (preset.Shape)
        {
            case HabitatShape.Cylinder:
            case HabitatShape.Inflatable:
                var cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cyl.name = "Hull_Cylinder";
                cyl.transform.SetParent(_shellRoot, false);
                // У примитива height по Y = 2 → половина: L/2
                cyl.transform.localScale = new Vector3(preset.Diameter, preset.Length * 0.5f, preset.Diameter);
                var mr = cyl.GetComponent<MeshRenderer>();
                mr.sharedMaterial = shellMaterial;
                var col = cyl.GetComponent<Collider>(); if (col) Destroy(col);
                break;

            case HabitatShape.Prism:
                var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                box.name = "Hull_Prism";
                box.transform.SetParent(_shellRoot, false);
                box.transform.localScale = new Vector3(preset.Length, preset.Decks * deckHeight, preset.Diameter);
                var mr2 = box.GetComponent<MeshRenderer>();
                mr2.sharedMaterial = shellMaterial;
                var c2 = box.GetComponent<Collider>(); if (c2) Destroy(c2);
                break;
        }
    }

    void BuildWire()
    {
        float R = preset.Diameter * 0.5f;
        float L = preset.Length;

        if (preset.Shape == HabitatShape.Prism)
        {
            // Экватор (прямоугольник в XZ) и два торца (прямоугольники в YZ)
            CreateRectXZ(Vector3.zero, L, R * 2, "Equator_Rect");
            CreateRectYZ(new Vector3( L*0.5f, 0, 0), R * 2, preset.Decks * deckHeight, "Cap_Right");
            CreateRectYZ(new Vector3(-L*0.5f, 0, 0), R * 2, preset.Decks * deckHeight, "Cap_Left");
            return;
        }

        // Cylinder/Inflatable:
        // 1) экватор — окружность в XZ
        var eq = CreateCircleXZ(Vector3.zero, R);
        eq.name = "Equator";

        // 2) торцы — окружности в плоскости YZ, смещённые по X
        var capR = CreateCircleYZ(new Vector3( L*0.5f, 0, 0), R);
        capR.name = "Cap_Right";
        var capL = CreateCircleYZ(new Vector3(-L*0.5f, 0, 0), R);
        capL.name = "Cap_Left";
    }

    // ---------- helpers ----------
    LineRenderer CreateLR(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.widthMultiplier = lineWidth;
        lr.material = lineMaterial;
        lr.positionCount = circleSegments;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.startColor = lr.endColor = lineColor;
        return lr;
    }

    Transform CreateCircleXZ(Vector3 center, float radius)
    {
        var lr = CreateLR("CircleXZ", _shellRoot);
        for (int i = 0; i < circleSegments; i++)
        {
            float t = (float)i / circleSegments * Mathf.PI * 2f;
            float x = Mathf.Cos(t) * radius;
            float z = Mathf.Sin(t) * radius;
            lr.SetPosition(i, center + new Vector3(x, 0f, z));
        }
        return lr.transform;
    }

    Transform CreateCircleYZ(Vector3 center, float radius)
    {
        var lr = CreateLR("CircleYZ", _shellRoot);
        for (int i = 0; i < circleSegments; i++)
        {
            float t = (float)i / circleSegments * Mathf.PI * 2f;
            float y = Mathf.Cos(t) * radius;
            float z = Mathf.Sin(t) * radius;
            lr.SetPosition(i, center + new Vector3(0f, y, z));
        }
        return lr.transform;
    }

    void CreateRectXZ(Vector3 center, float length, float width, string name)
    {
        var lr = CreateLR(name, _shellRoot);
        lr.positionCount = 4;
        lr.loop = true;
        float hx = length * 0.5f;
        float hz = width  * 0.5f;
        lr.SetPosition(0, center + new Vector3(-hx, 0f, -hz));
        lr.SetPosition(1, center + new Vector3( hx, 0f, -hz));
        lr.SetPosition(2, center + new Vector3( hx, 0f,  hz));
        lr.SetPosition(3, center + new Vector3(-hx, 0f,  hz));
    }

    void CreateRectYZ(Vector3 center, float width, float height, string name)
    {
        var lr = CreateLR(name, _shellRoot);
        lr.positionCount = 4;
        lr.loop = true;
        float hy = height * 0.5f;
        float hz = width  * 0.5f;
        lr.SetPosition(0, center + new Vector3(0f, -hy, -hz));
        lr.SetPosition(1, center + new Vector3(0f,  hy, -hz));
        lr.SetPosition(2, center + new Vector3(0f,  hy,  hz));
        lr.SetPosition(3, center + new Vector3(0f, -hy,  hz));
    }

    public void SetVisible(bool on)
    {
        if (_shellRoot) _shellRoot.gameObject.SetActive(on);
    }
}
