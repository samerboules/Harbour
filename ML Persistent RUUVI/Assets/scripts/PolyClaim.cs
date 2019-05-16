using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PolyClaim : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        //TestMesh();
    }

    public void SetClaim(List<Vector3> points)
    {
        Mesh mesh = CreateMesh(CreateClaim(points));
        transform.GetComponent<MeshFilter>().mesh = mesh;
    }

    private List<Vector3> CreateClaim(List<Vector3> points)
    {
        var result = new List<Vector3>();
        result.AddRange(points.Select(p => new Vector3(p.x, 0.02f, p.z)));
        return result;
    }

    private Mesh CreateMesh(List<Vector3> vertex)
    {
        //Create a new mesh
        var mesh = new Mesh();
        List<int> indices = null;
        List<Vector3> vertices = null;

        Triangulator.Triangulate(vertex, out indices, out vertices);

        //Assign data to mesh
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, GetUvs(vertices));
        mesh.SetTriangles(indices, 0);

        //Recalculations
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        //Name the mesh
        mesh.name = "ClaimMesh";

        //Return the mesh
        return mesh;
    }

    private List<Vector2> GetUvs(List<Vector3> vertex)
    {
        var uvs = new List<Vector2>();
        for (int x = 0; x < vertex.Count; x++)
        {
            uvs.Add(new Vector2(x % 2, x % 2));
        }

        return uvs;
    }

    private void TestMesh()
    {
        var testPolygon = new List<Vector3>()
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 5.11231f),
            new Vector3(5.11231f, 0, 5.11231f),
            new Vector3(5.11231f, 0, 0),
        };

        SetClaim(testPolygon);
    }
}
