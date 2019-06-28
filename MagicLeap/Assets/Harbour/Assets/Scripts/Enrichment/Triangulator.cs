using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;

public class Triangulator {

    public static bool Triangulate(List<Vector3> points, out List<int> indicies, out List<Vector3> vertices)
    {
        Polygon poly = new Polygon();
        indicies = new List<int>();
        vertices = new List<Vector3>();

        for (int i = 0; i < points.Count; i++)
        {
            var p = points[i];
            var vertex = new Vertex(p.x, p.z);

            poly.Add(vertex);

            if (i == points.Count - 1)
            {
                poly.Add(new Segment(vertex, new Vertex(points[0].x, points[0].z)));
            }
            else
            {
                poly.Add(new Segment(vertex, new Vertex(points[i + 1].x, points[i + 1].z)));
            }
        }

        var mesh = poly.Triangulate();

        foreach (var t in mesh.Triangles)
        {
            for (int j = 2; j >= 0; j--)
            {
                bool found = false;
                for (int k = 0; k < vertices.Count; k++)
                {
                    if ((vertices[k].x == (float)t.GetVertex(j).X) && (vertices[k].z == (float)t.GetVertex(j).Y))
                    {
                        indicies.Add(k);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    vertices.Add(new Vector3((float)t.GetVertex(j).X, points[0].y, (float)t.GetVertex(j).Y));
                    indicies.Add(vertices.Count - 1);
                }
            }
        }
        return true;
    }

}
