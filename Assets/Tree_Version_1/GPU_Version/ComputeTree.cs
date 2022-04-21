using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ComputeTree : MonoBehaviour
{
    // Tree Parameters
    static int NUM_TREES = 50;
    static int batch_size = 1024;
    static int batches = 100;
    int MAX_BRANCHES = batches * batch_size;
    float GROWTH_RATE = 1f;
    float last_growth = 0;
    static int MAX_GENERATIONS = 20;
    int generation;
    int MAX_DEPTH = 1;
    int NUM_BRANCHES = NUM_TREES;

    // Growth Parameters
    static float ENERGY_LAMBDA = 0.5f;
    static float GROWTH_TRESHOLD = 0.5f;

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

    // Rendering
    public Material ComputeTreeMaterial;
    Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 10000);
    ComputeBuffer triangles;
    ComputeBuffer vertices;
    ComputeBuffer branch_TRS_matrices;
    public Mesh branch_mesh;

    // Buffers
    ComputeBuffer branch_parent_main_lateral;
    ComputeBuffer branch_bottom;
    ComputeBuffer branch_top;
    ComputeBuffer branch_gath_energy;
    ComputeBuffer branch_dist_energy;
    ComputeBuffer free_idxs;
    ComputeBuffer tree_variables; // 0: NUM_BRANCHES, 1: MAX_DEPTH

    void initBuffers()
    {
        branch_parent_main_lateral = new ComputeBuffer(MAX_BRANCHES, sizeof(int) * 3);
        branch_bottom = new ComputeBuffer(MAX_BRANCHES, sizeof(float) * 3);
        branch_top = new ComputeBuffer(MAX_BRANCHES, sizeof(float) * 3);
        branch_gath_energy = new ComputeBuffer(MAX_BRANCHES, sizeof(float));
        branch_dist_energy = new ComputeBuffer(MAX_BRANCHES, sizeof(float));
        free_idxs = new ComputeBuffer(MAX_BRANCHES, sizeof(int));
        int[] free_idxs_arr = Enumerable.Range(0, MAX_BRANCHES).ToArray();
        free_idxs_arr[0] = NUM_TREES;
        free_idxs.SetData(free_idxs_arr);
        branch_TRS_matrices = new ComputeBuffer(MAX_BRANCHES, sizeof(float) * 4 * 4);
        tree_variables = new ComputeBuffer(2, sizeof(int));
        int[] tree_variables_arr = new int[2];
        tree_variables_arr[0] = NUM_TREES;
        tree_variables_arr[1] = 0;
        tree_variables.SetData(tree_variables_arr);

        Shader.SetGlobalBuffer("branch_parent_main_lateral", branch_parent_main_lateral);
        Shader.SetGlobalBuffer("branch_bottom", branch_bottom);
        Shader.SetGlobalBuffer("branch_top", branch_top);
        Shader.SetGlobalBuffer("branch_gath_energy", branch_gath_energy);
        Shader.SetGlobalBuffer("branch_dist_energy", branch_dist_energy);
        Shader.SetGlobalBuffer("free_idxs", free_idxs);
        Shader.SetGlobalBuffer("branch_TRS_matrices", branch_TRS_matrices);
        Shader.SetGlobalBuffer("tree_variables", tree_variables);
        
        vertices = new ComputeBuffer(branch_mesh.vertexCount, sizeof(float) * 3);
        vertices.SetData(branch_mesh.vertices.ToArray());
        triangles = new ComputeBuffer(branch_mesh.triangles.Length, sizeof(int));
        triangles.SetData(branch_mesh.triangles);
        GetComponent<MeshRenderer>().material = ComputeTreeMaterial;
        GetComponent<MeshFilter>().mesh = branch_mesh;
        ComputeTreeMaterial.SetBuffer("branch_TRS_matrices", branch_TRS_matrices);
        ComputeTreeMaterial.SetBuffer("vertices", vertices);
        ComputeTreeMaterial.SetBuffer("triangles", triangles);

        DistributeEnergy.SetFloat("energy_lambda", ENERGY_LAMBDA);
        Grow.SetFloat("growth_treshold", GROWTH_TRESHOLD);
    }

    void Start()
    {
        kernelReceiveLight = ReceiveLight.FindKernel("ReceiveLight");
        kernelGatherEnergy = GatherEnergy.FindKernel("GatherEnergy");
        kernelDistributeEnergy = DistributeEnergy.FindKernel("DistributeEnergy");
        kernelGrow = Grow.FindKernel("Grow");
        kernelInitTrees = InitTrees.FindKernel("InitTrees");

        initBuffers();

        for (int i = 0; i < batches; i ++)
        {
            InitTrees.SetInt("batch", i);
            InitTrees.Dispatch(kernelInitTrees, batch_size, 1, 1);
        }

        generation = 0;
    }

    void RetrieveTreeVariables()
    {
        // Retrive tree depth and number of branches
        int[] tree_variables_arr = new int[2];
        tree_variables.GetData(tree_variables_arr);
        NUM_BRANCHES = tree_variables_arr[0];
        MAX_DEPTH = tree_variables_arr[1];
    }

    void DispatchComputeShaders()
    {
        

        //// RecieveLight
        for (int i = 0; i < batches; i++)
        {
            ReceiveLight.SetInt("batch", i);
            ReceiveLight.Dispatch(kernelReceiveLight, batch_size, 1, 1);
        }

        //// GatherEnergy
        for (int i = 0; i < MAX_DEPTH; i++)
        {
            for (int j = 0; j < batches; j++)
            {
                GatherEnergy.SetInt("batch", j);
                GatherEnergy.Dispatch(kernelGatherEnergy, batch_size, 1, 1);
            }
        }

        //// DistributeEnergy
        for (int i = 0; i < MAX_DEPTH; i++)
        {
            for (int j = 0; j < batches; j++)
            {
                DistributeEnergy.SetInt("batch", j);
                DistributeEnergy.Dispatch(kernelDistributeEnergy, batch_size, 1, 1);
            }
        }

        // Grow
        for (int i = 0; i < batches; i ++)
        {
            Grow.SetInt("batch", i);
            Grow.SetFloat("time", Time.time);
            Grow.Dispatch(kernelGrow, batch_size, 1, 1);
        }

        RetrieveTreeVariables();
    }

    void CleanUp()
    {
        branch_parent_main_lateral.Dispose();
        branch_dist_energy.Dispose();
        branch_gath_energy.Dispose();
        branch_bottom.Dispose();
        branch_top.Dispose();
        free_idxs.Dispose();
        branch_TRS_matrices.Dispose();
        tree_variables.Dispose();
    }

    void Render()
    {
        Graphics.DrawProcedural(ComputeTreeMaterial, bounds, MeshTopology.Triangles, branch_mesh.triangles.Length, NUM_BRANCHES);
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
