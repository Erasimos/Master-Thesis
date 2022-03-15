using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ObjData
{
    public Vector3 pos;
    public Vector3 scale;
    public Quaternion rot;

    public Matrix4x4 matrix
    {
        get
        {
            return Matrix4x4.TRS(pos, rot, scale);
        }
    } 

    public ObjData(Vector3 pos, Vector3 scale, Quaternion rot)
    {
        this.pos = pos;
        this.scale = scale;
        this.rot = rot;
    }
}

public class TreeRenderer_GPU_INSTANCING : MonoBehaviour
{
    public Tree tree;
    public Mesh objMesh;
    public Material objMat;

    private List<List<ObjData>> batches = new List<List<ObjData>>();
    private List<ObjData> currentBatch = new List<ObjData>();
    private int batchIndex = 0;
    public bool started = false;

    // Start is called before the first frame update
    void Start()
    {
        batches.Add(currentBatch);
    }

    void AddBranch(Tree.Branch branch)
    {
        float branchLength = (branch.top - branch.bottom).magnitude;
        Vector3 pos = transform.position + branch.bottom +  branchLength * branch.direction * 0.5f;// + branch.direction * (branch.top - branch.bottom).magnitude * 0.5f; // + (branch.direction * (branch.top - branch.bottom).magnitude);
        
        Vector3 scale = new Vector3(0.25f/branch.depth, (branch.top - branch.bottom).magnitude/2f, 0.25f /branch.depth);
        
        Quaternion rot = Quaternion.FromToRotation(new Vector3(0, 1, 0), branch.direction);

        AddObj(pos, scale, rot);
    }

    void UpdateBatches()
    {
        foreach (Tree.Branch branch in tree.newBranches)
        {
            AddBranch(branch);
            batchIndex++; 
            if(batchIndex >= 1000)
            {
                batches.Add(currentBatch);
                currentBatch = new List<ObjData>();
                batchIndex = 0;
                print("NUMBER OF BATCHES: " + batches.Count.ToString());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (tree.grown)
        {
            UpdateBatches();
            tree.grown = false;
            
        }
        RenderBatches();
    }

    private void AddObj(Vector3 pos, Vector3 scale, Quaternion rot)
    {
        currentBatch.Add(new ObjData(pos, scale, rot));
    }

    private void RenderBatches()
    {
        foreach (var batch in batches)
        {
            Graphics.DrawMeshInstanced(objMesh, 0, objMat, batch.Select((a) => a.matrix).ToList());
            //Graphics.Dr
        }
    }
}
