// Author Oxe
// Created at 04.10.2025 19:07

using System.Collections.Generic;
using UnityEngine;

public class MeshBuilder
{
    readonly List<Vector3> v   = new();
    readonly List<Vector3> n   = new();
    readonly List<Vector2> uv  = new();
    readonly List<int>     idx = new();

    public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        int i = v.Count;
        v.Add(a); v.Add(b); v.Add(c); v.Add(d);
        Vector3 normal = Vector3.Cross(b-a, c-a).normalized;
        n.Add(normal); n.Add(normal); n.Add(normal); n.Add(normal);
        uv.Add(new Vector2(0,0)); uv.Add(new Vector2(1,0)); uv.Add(new Vector2(1,1)); uv.Add(new Vector2(0,1));
        idx.Add(i+0); idx.Add(i+1); idx.Add(i+2);
        idx.Add(i+0); idx.Add(i+2); idx.Add(i+3);
    }

    public Mesh Build()
    {
        var m = new Mesh();
        m.SetVertices(v);
        m.SetNormals(n);
        m.SetUVs(0, uv);
        m.SetTriangles(idx, 0);
        m.RecalculateBounds();
        return m;
    }
}
