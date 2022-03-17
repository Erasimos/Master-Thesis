using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct ComputeBranch
{
    Vector3 top;
    Vector3 bottom;
    float diameter;
    int main_index;
    int lateral_index;

    // Energy
    float Q;
    float V;
};

struct ComputeTree
{
    ComputeBranch [] branches;
    int NUMBER_OF_BRANCHES;
};

public class TreeComputeShader : MonoBehaviour
{
    public ComputeShader treeComputeShader;
    public float GROWTH_RATE = 1f; // seconds
    public float LAST_GROWTH = 0;
    private ComputeBuffer treeBuffer;
    private ComputeTree[] trees;
    private int MAX_BRANCHES = 40;
    int kernel;
    uint threadGroupSize;

    // Start is called before the first frame update
    void Start()
    {
        kernel = treeComputeShader.FindKernel("CSMain");
        treeBuffer = new ComputeBuffer(1, (MAX_BRANCHES * ((sizeof(float) * 3 * 2)  + (sizeof(int) * 2) + (sizeof(float) * 3) + sizeof(int))));
        trees = new ComputeTree[1];
        // init trees
        treeComputeShader.SetBool("init", true);
        treeComputeShader.SetBuffer(kernel, "TreeBuffer", treeBuffer);
        treeComputeShader.Dispatch(kernel, 1, 1, 1);
        treeComputeShader.SetBool("init", false);
    }

    // Update is called once per frame
    void Update()
    {
        LAST_GROWTH += Time.deltaTime;
        if(LAST_GROWTH >= GROWTH_RATE)
        {
            LAST_GROWTH = 0;
            treeComputeShader.SetFloat("DeltaTime", Time.deltaTime);
            treeComputeShader.Dispatch(kernel, 1, 1, 1);
            treeBuffer.GetData(trees);
            print(trees);
        }
        
    }
}
