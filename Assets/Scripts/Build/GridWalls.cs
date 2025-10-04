// Author Oxe
// Created at 04.10.2025 19:06

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GridWalls : MonoBehaviour
{
    public EditorGrid grid;
    public float wallHeight = 2.4f;
    public float wallThickness = 0.08f;
    public bool buildOnAwake = true;
    public bool autoDoors = true;
    public float doorWidthM = 0.9f; // ширина проёма
    public ZoneType corridorType = ZoneType.Corridor;

    MeshFilter _mf;

    void Awake(){ _mf = GetComponent<MeshFilter>(); if (buildOnAwake) Rebuild(); }
    public void Rebuild()
    {
        if (!grid) return;
        var mb = new MeshBuilder();

        int W = grid.cols, H = grid.rows;
        float cs = grid.cellSize;
        Vector3 origin = -new Vector3(W*cs, 0, H*cs) * 0.5f;

        bool IsSolid(int x, int y)
        {
            if (x<0||y<0||x>=W||y>=H) return false;
            var t = grid.Tiles[x,y];
            // считаем стену там, где «не коридор» и не пусто
            return t != null && t.Type != ZoneType.None && t.Type != corridorType;
        }

        bool IsCorridor(int x, int y)
        {
            if (x<0||y<0||x>=W||y>=H) return false;
            var t = grid.Tiles[x,y];
            return t != null && t.Type == corridorType;
        }

        // проходим по рёбрам клеток и добавляем стенку, если по разные стороны — solid vs corridor/empty
        for (int x=0; x<W; x++)
        for (int y=0; y<H; y++)
        {
            var t = grid.Tiles[x,y];
            if (t == null) continue;

            // 4 направления: +X, -X, +Y, -Y (в плоскости XZ это восток/запад/север/юг)
            TryEdge(x, y,  1, 0); // +X
            TryEdge(x, y, -1, 0); // -X
            TryEdge(x, y,  0, 1); // +Y
            TryEdge(x, y,  0,-1); // -Y

            void TryEdge(int cx, int cy, int dx, int dy)
            {
                int nx = cx+dx, ny = cy+dy;

                bool aSolid = IsSolid(cx, cy);
                bool bSolid = IsSolid(nx, ny);
                bool aCor = IsCorridor(cx, cy);
                bool bCor = IsCorridor(nx, ny);

                // стена нужна на границе «solid ↔ corridor/empty» (в одну сторону)
                if (aSolid && !bSolid)
                {
                    // координаты ребра в мировых
                    Vector3 c0 = origin + new Vector3((cx+0.0f)*cs, 0, (cy+0.0f)*cs);
                    Vector3 c1 = origin + new Vector3((cx+ (dx!=0?1:0))*cs, 0, (cy+ (dy!=0?1:0))*cs);
                    // центр и направление
                    Vector3 mid = (c0 + c1);
                    Vector3 dir = (c1 - c0).normalized; // вдоль ребра
                    Vector3 nrm = new Vector3(dir.z, 0, -dir.x); // нормаль «наружу»
                    // длина сегмента = cs
                    float length = cs;

                    // авто-проём, если по другую сторону — коридор
                    bool door = autoDoors && (aSolid && (bCor || aCor)); // примыкает к коридору
                    float doorW = Mathf.Clamp(doorWidthM, 0.4f, cs*0.9f);

                    if (door && doorW < length * 0.95f)
                    {
                        // два коротких сегмента слева/справа от проёма
                        float gap = doorW;
                        float half = length * 0.5f;
                        float leftLen  = Mathf.Max(0f, half - gap*0.5f);
                        float rightLen = leftLen;

                        if (leftLen > 0.01f)
                            AddWallSegment(mb, mid - dir * (gap*0.5f + leftLen*0.5f), dir, nrm, leftLen);
                        if (rightLen > 0.01f)
                            AddWallSegment(mb, mid + dir * (gap*0.5f + rightLen*0.5f), dir, nrm, rightLen);
                    }
                    else
                    {
                        AddWallSegment(mb, mid, dir, nrm, length);
                    }
                }
            }
        }

        _mf.sharedMesh = mb.Build();
    }

    void AddWallSegment(MeshBuilder mb, Vector3 center, Vector3 dir, Vector3 nrm, float length)
    {
        float t = wallThickness;
        float h = wallHeight;
        // прямоугольная “экструдированная” полоса: ширина=толщина, длина=length, высота=h
        Vector3 right = dir;
        Vector3 forward = nrm; // перпендикуляр — толщина
        Vector3 p = center + forward * (t*0.5f);

        Vector3 A = p - right*(length*0.5f);
        Vector3 B = p + right*(length*0.5f);
        Vector3 C = A + Vector3.up * h;
        Vector3 D = B + Vector3.up * h;

        // наружная грань
        mb.Quad(A,B,D,C);
        // торцы (сверху/снизу/внутренняя грань) — по желанию
        // внутренняя (в сторону комнаты)
        Vector3 p2 = center - forward * (t*0.5f);
        Vector3 A2 = p2 - right*(length*0.5f);
        Vector3 B2 = p2 + right*(length*0.5f);
        Vector3 C2 = A2 + Vector3.up*h;
        Vector3 D2 = B2 + Vector3.up*h;
        mb.Quad(B2,A2,C2,D2); // внутренняя

        // крышка
        mb.Quad(C,D,D2,C2);
        // нижняя (можно не добавлять)
    }
}
