using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ComputeTree : MonoBehaviour
{
    // Tree Parameters
    int NUM_TREES = 100;
    static int batch_size = 1024;
    static int batches = 100;
    int MAX_BRANCHES = batches * batch_size;
    float GROWTH_RATE = 5f;
    float last_growth = 0;
    static int MAX_GENERATIONS = 8;
    int generation;

    // Compute Shaders
    public ComputeShader ReceiveLight;
    private int kernelReceiveLight;
    public ComputeShader GatherEnergy;
    private int kernelGatherEnergy;
    public ComputeShader DistributeEnergy;
    private int kernelDistributeEnergy;
    public ComputeShader Grow;
    private int kernelGrow;
    public ComputeShader InitTrees;
    private int kernelInitTrees;
    public ComputeShader GenerateMesh;
    private int kernelGenerateMesh;

    // Rendering
    public Material ComputeTreeMaterial;
    Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 10000);
    ComputeBuffer mesh_triangles;

    // Buffers
    ComputeBuffer branch_main;
    ComputeBuffer branch_lateral;
    ComputeBuffer branch_parent;
    ComputeBuffer branch_bottom;
    ComputeBuffer branch_top;
    ComputeBuffer branch_gath_energy;
    ComputeBuffer branch_dist_energy;
    ComputeBuffer free_idxs;

    void initBuffers()
    {
        branch_main = new ComputeBuffer(MAX_BRANCHES, sizeof(int));
        branch_lateral = new ComputeBuffer(MAX_BRANCHES, sizeof(int));
        branch_parent = new ComputeBuffer(MAX_BRANCHES, sizeof(int));
        branch_bottom = new ComputeBuffer(MAX_BRANCHES, sizeof(float) * 3);
        branch_top = new ComputeBuffer(MAX_BRANCHES, sizeof(float) * 3);
        branch_gath_energy = new ComputeBuffer(MAX_BRANCHES, sizeof(float));
        branch_dist_energy = new ComputeBuffer(MAX_BRANCHES, sizeof(float));
        free_idxs = new ComputeBuffer(MAX_BRANCHES, sizeof(int));
        int[] free_idxs_arr = Enumerable.Range(0, MAX_BRANCHES).ToArray();
        free_idxs_arr[0] = NUM_TREES;
        free_idxs.SetData(free_idxs_arr);

        mesh_triangles = new ComputeBuffer(MAX_BRANCHES * 3 * 2, sizeof(float) * 3);

        Shader.SetGlobalBuffer("branch_main", branch_main);
        Shader.SetGlobalBuffer("branch_lateral", branch_lateral);
        Shader.SetGlobalBuffer("branch_parent", branch_parent);
        Shader.SetGlobalBuffer("branch_bottom", branch_bottom);
        Shader.SetGlobalBuffer("branch_top", branch_top);
        Shader.SetGlobalBuffer("branch_gath_energy", branch_gath_energy);
        Shader.SetGlobalBuffer("branch_dist_energy", branch_dist_energy);
        Shader.SetGlobalBuffer("free_idxs", free_idxs);
        
        GetComponent<MeshRenderer>().material = ComputeTreeMaterial;
        Shader.SetGlobalBuffer("mesh_triangles", mesh_triangles);
        ComputeTreeMaterial.SetBuffer("mesh_triangles", mesh_triangles);
    }

    void Start()
    {
        kernelReceiveLight = ReceiveLight.FindKernel("ReceiveLight");
        kernelGatherEnergy = GatherEnergy.FindKernel("GatherEnergy");
        kernelDistributeEnergy = DistributeEnergy.FindKernel("DistributeEnergy");
        kernelGrow = Grow.FindKernel("Grow");
        kernelInitTrees = InitTrees.FindKernel("InitTrees");
        kernelGenerateMesh = GenerateMesh.FindKernel("GenerateMesh");

        initBuffers();

        for (int i = 0; i < batches; i ++)
        {
            InitTrees.SetInt("batch", i);
            InitTrees.Dispatch(kernelInitTrees, batch_size, 1, 1);
        }

        generation = 0;
    }

    void DispatchComputeShaders()
    {
        //// RecieveLight
        //for (int i = 0; i < batches; i ++)
        //{
        //    ReceiveLight.SetInt("batch", i);
        //    ReceiveLight.Dispatch(kernelReceiveLight, batch_size, 1, 1);
        //}

        //// GatherEnergy
        //for (int i = 0; i < MAX_DEPTH; i ++)
        //{
        //    for (int j = 0; j < batches; j ++)
        //    {
        //        GatherEnergy.SetInt("batch", j);
        //        GatherEnergy.Dispatch(kernelGatherEnergy, batch_size, 1, 1);
        //    }
        //}

        //// DistributeEnergy
        //for (int i = 0; i < MAX_DEPTH; i ++)
        //{
        //    for (int j = 0; j < batches; j ++)
        //    {
        //        DistributeEnergy.SetInt("batch", j);
        //        DistributeEnergy.Dispatch(kernelDistributeEnergy, batch_size, 1, 1);
        //    }
        //}

        // Grow
        for (int i = 0; i < batches; i ++)
        {
            Grow.SetInt("batch", i);
            Grow.SetFloat("time", Time.time);
            Grow.Dispatch(kernelGrow, batch_size, 1, 1);
        }

        // Generate Mesh
        for (int i = 0; i < batches; i ++)
        {
            GenerateMesh.SetInt("batch", i);
            GenerateMesh.Dispatch(kernelGenerateMesh, batch_size, 1, 1);
        }
    }

    void CleanUp()
    {
        branch_main.Dispose();
        branch_lateral.Dispose();
        branch_parent.Dispose();
        branch_dist_energy.Dispose();
        branch_gath_energy.Dispose();
        branch_bottom.Dispose();
        branch_top.Dispose();
        free_idxs.Dispose();
        mesh_triangles.Dispose();
    }

    void Render()
    {
        Graphics.DrawProcedural(ComputeTreeMaterial, bounds, MeshTopology.Triangles, MAX_BRANCHES * 2, 1);
    }

    // Update is called once per frame
    void Update()
    {
        Render();

        last_growth += Time.deltaTime;
        if(last_growth >= GROWTH_RATE)
        {
            print(" New season");
            DispatchComputeShaders();
            last_growth = 0;

            generation += 1;
            if (generation >= MAX_GENERATIONS)
            {
                enabled = false;
                CleanUp();  
            }
        }
    }

    private void OnDestroy()
    {
        CleanUp();
    }
}
