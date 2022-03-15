using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class TreeMesh : MonoBehaviour
{
    public List<Vector3> vertices;
    public List<int> triangles;
    private Mesh mesh;
    public Material treeMaterial;

    public void Start()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        mesh = GetComponent<MeshFilter>().mesh;
        mesh = new Mesh();
        GetComponent<MeshRenderer>().material = treeMaterial;
    }

    public void UpdateMesh()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        GetComponent<MeshFilter>().mesh.Clear();

        Vector3[] meshVertices = new Vector3[vertices.Count];
        vertices.CopyTo(meshVertices);

        int[] meshTriangles = new int[triangles.Count];
        triangles.CopyTo(meshTriangles);

        mesh.vertices = meshVertices;
        mesh.triangles = meshTriangles;
        mesh.RecalculateNormals();
    }

}
 