// Author Oxe
// Created at 04.10.2025 17:06


using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
    struct Node
    {
        public int   x,  y;
        public float g,  h;
        public int   px, py;
    }

    static readonly (int dx, int dy)[] N4 = { (1, 0), (-1, 0), (0, 1), (0, -1) };

    static float Heu(int x1, int y1, int x2, int y2) => Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2);

    public static List<Vector2Int> AStar(EditorGrid g, bool[,] walkable, Vector2Int start, Vector2Int goal)
    {
        int W=g.cols, H=g.rows;
        var open = new SortedSet<(float f,int id)>(); // (f, id)
        var idmap = new Dictionary<(int,int),int>();
        var nodes = new Dictionary<int,Node>();
        var inOpen = new HashSet<(int,int)>();
        var closed = new bool[W,H];
        int nextId=1;

        int sid = nextId++;
        nodes[sid] = new Node{ x=start.x,y=start.y, g=0f, h=Heu(start.x,start.y,goal.x,goal.y), px=-1,py=-1};
        open.Add((nodes[sid].g+nodes[sid].h,sid));
        inOpen.Add((start.x,start.y));
        idmap[(start.x,start.y)]=sid;

        while(open.Count>0)
        {
            var cur = open.Min; open.Remove(cur);
            var n = nodes[cur.id];
            inOpen.Remove((n.x,n.y));
            if (n.x==goal.x && n.y==goal.y)
            {
                // reconstruct
                var path=new List<Vector2Int>();
                var k=cur.id;
                while(k!=0){
                    var nd=nodes[k];
                    path.Add(new Vector2Int(nd.x,nd.y));
                    if (nd.px<0) break;
                    k = idmap[(nd.px,nd.py)];
                }
                path.Reverse();
                return path;
            }
            closed[n.x,n.y]=true;

            foreach (var (dx,dy) in N4)
            {
                int nx=n.x+dx, ny=n.y+dy;
                if (nx<0||ny<0||nx>=W||ny>=H) continue;
                if (!walkable[nx,ny] || closed[nx,ny]) continue;

                float gScore = n.g + 1f;
                bool wasOpen = inOpen.Contains((nx,ny));
                int nid;
                if (!wasOpen)
                {
                    nid = nextId++;
                    nodes[nid]= new Node{ x=nx,y=ny, g=gScore, h=Heu(nx,ny,goal.x,goal.y), px=n.x,py=n.y };
                    idmap[(nx,ny)]=nid;
                    open.Add((nodes[nid].g+nodes[nid].h, nid));
                    inOpen.Add((nx,ny));
                }
                else
                {
                    nid = idmap[(nx,ny)];
                    if (gScore < nodes[nid].g)
                    {
                        open.RemoveWhere(t => t.id==nid);
                        nodes[nid] = new Node{ x=nx,y=ny, g=gScore, h=nodes[nid].h, px=n.x,py=n.y };
                        open.Add((nodes[nid].g+nodes[nid].h, nid));
                    }
                }
            }
        }
        return null;
    }

    public static int[,] Clearance(EditorGrid g, bool[,] walkable)
    {
        int W=g.cols, H=g.rows;
        var dist = new int[W,H];
        var q = new Queue<Vector2Int>();

        for (int x=0;x<W;x++)
        for (int y=0;y<H;y++)
        {
            if (walkable[x,y]) dist[x,y]=int.MaxValue/4;
            else { dist[x,y]=0; q.Enqueue(new Vector2Int(x,y)); }
        }

        var dirs = N4;
        while(q.Count>0)
        {
            var p=q.Dequeue();
            int d=dist[p.x,p.y];
            foreach(var (dx,dy) in dirs)
            {
                int nx=p.x+dx, ny=p.y+dy;
                if (nx<0||ny<0||nx>=W||ny>=H) continue;
                if (dist[nx,ny] > d+1)
                {
                    dist[nx,ny]=d+1;
                    q.Enqueue(new Vector2Int(nx,ny));
                }
            }
        }
        return dist; // значение = клеток до ближайшей преграды
    }
}
