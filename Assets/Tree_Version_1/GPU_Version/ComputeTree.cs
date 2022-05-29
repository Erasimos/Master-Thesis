using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ComputeTree : MonoBehaviour
{
    // Tree Parameters
    static int NUM_TREES = 250;
    static int MAX_BRANCHES = 10000000;
    
    float GROWTH_RATE = 0.0f;
    float last_growth = 0f;
    static int MAX_GENERATIONS = 1000;
    int generation;
    float last_generation = 0;
    int MAX_DEPTH = 1;
    int NUM_BRANCHES = NUM_TREES;
    float simulation_time;
    float simulation_time_avg = 0;
    int frames = 0;
    float elapsed_time;

    float TREE_SPACING = 5f;
    Vector2 FOREST_SIZE = new Vector2(1000, 1000);

    // Growth Parameters
    static float ENERGY_LAMBDA = 0.3f;

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

    // Shadow Map
    public Light sun;
    public Camera cam;
    public GameObject textureCube;
    private RenderTexture shadowmap;

    // Rendering
    public Material ComputeTreeMaterial;
    Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 10000);
    ComputeBuffer triangles;
    ComputeBuffer vertices;
    ComputeBuffer branch_TRS_matrices;
    public Mesh branch_mesh;

    // Buffers
    ComputeBuffer branch_parent_main_lateral;
    ComputeBuffer branch_bottom_top;
    ComputeBuffer branch_gath_energy;
    ComputeBuffer branch_dist_energy;
    ComputeBuffer free_idxs;
    ComputeBuffer tree_variables; // 0: NUM_BRANCHES, 1: MAX_DEPTH
    ComputeBuffer tree_spawn_positions;

    void initBuffers()
    {
        branch_parent_main_lateral = new ComputeBuffer(MAX_BRANCHES, sizeof(int) * 3);
        branch_bottom_top = new ComputeBuffer(MAX_BRANCHES, sizeof(float) * 3 * 2);
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

        tree_spawn_positions = new ComputeBuffer(NUM_TREES, sizeof(float) * 2);
        List<Vector2> tree_spawn_positions_list = PossoinDiscSampling.GeneratePoints(TREE_SPACING, FOREST_SIZE, NUM_TREES);
        tree_spawn_positions.SetData(tree_spawn_positions_list);

        Shader.SetGlobalBuffer("tree_spawn_positions", tree_spawn_positions);
        Shader.SetGlobalBuffer("branch_parent_main_lateral", branch_parent_main_lateral);
        Shader.SetGlobalBuffer("branch_bottom_top", branch_bottom_top);
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

        // Shadow map
        if (shadowmap == null)
        {
            cam.depth = -1000;
            cam.depthTextureMode |= DepthTextureMode.Depth;
            shadowmap = new RenderTexture(1024, 1024, 16, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
            shadowmap.wrapMode = TextureWrapMode.Clamp;
            shadowmap.filterMode = FilterMode.Bilinear;
            shadowmap.autoGenerateMips = false;
            shadowmap.useMipMap = false;
            cam.targetTexture = shadowmap;

        }

    }

    void Start()
    {
        kernelReceiveLight = ReceiveLight.FindKernel("ReceiveLight");
        kernelGatherEnergy = GatherEnergy.FindKernel("GatherEnergy");
        kernelDistributeEnergy = DistributeEnergy.FindKernel("DistributeEnergy");
        kernelGrow = Grow.FindKernel("Grow");
        kernelInitTrees = InitTrees.FindKernel("InitTrees");

        initBuffers();

        InitTrees.Dispatch(kernelInitTrees, MAX_BRANCHES/256, 1, 1);

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
        ReceiveLight.Dispatch(kernelReceiveLight, MAX_BRANCHES/256, 1, 1);

        //// GatherEnergy
        for (int i = 0; i < MAX_DEPTH; i++)
        {
            GatherEnergy.Dispatch(kernelGatherEnergy, MAX_BRANCHES/256, 1, 1);
        }

        //// DistributeEnergy
        for (int i = 0; i < MAX_DEPTH; i++)
        {
            DistributeEnergy.Dispatch(kernelDistributeEnergy, MAX_BRANCHES/256, 1, 1);
        }

        // Grow
        Grow.SetFloat("time", Time.time);
        Grow.Dispatch(kernelGrow, MAX_BRANCHES/256, 1, 1);
        

        RetrieveTreeVariables();
    }

    void CleanUp()
    {
        branch_parent_main_lateral.Dispose();
        branch_dist_energy.Dispose();
        branch_gath_energy.Dispose();
        branch_bottom_top.Dispose();
        free_idxs.Dispose();
        branch_TRS_matrices.Dispose();
        tree_variables.Dispose();
        tree_spawn_positions.Dispose();
        vertices.Dispose();
        triangles.Dispose();
        shadowmap.Release();
    }

    void Render()
    {
        Graphics.DrawProcedural(ComputeTreeMaterial, bounds, MeshTopology.Triangles, branch_mesh.triangles.Length, NUM_BRANCHES);

        textureCube.GetComponent<MeshRenderer>().material.mainTexture = shadowmap;
        Shader.SetGlobalTexture("shadowmap", shadowmap);
        Shader.SetGlobalMatrix("shadow_matrix", cam.previousViewProjectionMatrix);
        Shader.SetGlobalMatrix("shadow_view_matrix", cam.worldToCameraMatrix);
    }

    // Update is called once per frame
    void Update()
    {
        if (frames == 0)
        {
            elapsed_time = 0;
        }

        if (frames < MAX_GENERATIONS)
        {
            frames++;
            elapsed_time += Time.deltaTime;
            Render();
        }
        else
        {
            print("FPS: " + frames / elapsed_time);
            print("Render Time: " + elapsed_time / frames);
        }

        return;

        last_growth += Time.deltaTime;
        if(last_growth >= GROWTH_RATE & generation < MAX_GENERATIONS & NUM_BRANCHES < 10000000)
        {
            
            print("--------------------------------------------------------------");
            generation += 1;
            //DispatchComputeShaders();
            simulation_time = Time.unscaledTime - last_generation - GROWTH_RATE;
            print("Branches: " + NUM_BRANCHES);
            print("Simulation Time:" + simulation_time);
            last_generation = Time.unscaledTime;
            last_growth = 0;

            if (generation > 1)
            {
                simulation_time_avg += simulation_time;
            }
        }

        else if(generation >= MAX_GENERATIONS)
        {
            print("average simulation time:" + simulation_time_avg / generation);
        }
    }

    private void OnDestroy()
    {
        CleanUp();
    }

}
