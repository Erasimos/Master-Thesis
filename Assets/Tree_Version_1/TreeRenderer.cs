using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TreeRenderer : MonoBehaviour
{
    public Material treeMaterial;
    public Tree tree;
    private Mesh mesh;
    private float shoot_lenght_percentage;
    private float elapsed_time;
    public float GROWTH_RATE;
    private float SCALE = 0.5f;
    private bool noLeafs;
    List<Vector3> vertices;
    List<int> triangles;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = treeMaterial;
        elapsed_time = 0f;
        noLeafs = true;
    }

    // Update is called once per frame
    void Update()
    {
        elapsed_time += Time.deltaTime;
        if (elapsed_time >= GROWTH_RATE) elapsed_time = 0f;
        shoot_lenght_percentage = elapsed_time / GROWTH_RATE;

        if (!tree.stop) //tree.grown)
        {
            tree.grown = false;
            vertices = new List<Vector3>();
            triangles = new List<int>();
            tree.CalculateBranchDiameters();
            generateMesh(tree.root);
            UpdateMesh();
        }
        if (noLeafs & tree.stop)
        {
            noLeafs = false;
            generateLeafMeshes(tree.root);
            UpdateMesh();
        }
    }

    void generateLeafMeshes(Tree.Metamer metamer)
    {
        if (metamer == null) return;

        if (metamer.isTerminal) generateLeafMesh(metamer.top);
        else
        {
            generateLeafMeshes(metamer.main);
            generateLeafMeshes(metamer.lateral);
        }
    }

    void generateLeafMesh(Vector3 position)
    {
        MeshHelper.square(vertices, triangles, position, 1);
    }

    void generateMesh(Tree.Metamer metamer)
    {
        if (metamer == null) return;
        else
        {

            float bottomDiameter = metamer.diameter;
            float mainTopDiameter = (metamer.main != null) ? metamer.main.diameter : tree.dna.MIN_DIAMETER;
            float lateralTopDiameter = (metamer.lateral != null) ? metamer.lateral.diameter : tree.dna.MIN_DIAMETER;
            float topDiamter = Mathf.Max(mainTopDiameter, lateralTopDiameter);

            Vector3 top;
            if (metamer.isTerminal)
            {
                float metamer_length = (metamer.top - metamer.bottom).magnitude;
                top = metamer.bottom + (metamer.direction * metamer_length * shoot_lenght_percentage); 
            }
            else
            {
                top = metamer.top;
            }


            MeshHelper.cylinder(vertices, triangles, metamer.bottom, top, bottomDiameter, topDiamter, 5);
            generateMesh(metamer.main);
            generateMesh(metamer.lateral);

            // STOP IF MESH TOO BIG
            if (vertices.Count >= 20000)
            {
                tree.stop = true;
                return;
            }
        }
    }


    void UpdateMesh()
    {
        mesh.Clear();

        Vector3[] meshVertices = new Vector3[vertices.Count];
        vertices.CopyTo(meshVertices);

        int[] meshTriangles = new int[triangles.Count];
        triangles.CopyTo(meshTriangles);

        mesh.vertices = meshVertices;
        mesh.triangles = meshTriangles;
        mesh.RecalculateNormals();
    }
}
