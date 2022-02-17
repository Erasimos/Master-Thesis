using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeSpheres : MonoBehaviour
{

    public int SphereAmount = 70;
    public ComputeShader Shader;

    ComputeBuffer resultBuffer;
    int kernel;
    uint threadGroupSize;
    Vector3[] output;

    public GameObject computeParticle;
    public GameObject[] computeParticles;

    // Start is called before the first frame update
    void Start()
    {
        //program we're executing
        kernel = Shader.FindKernel("Spheres");
        Shader.GetKernelThreadGroupSizes(kernel, out threadGroupSize, out _, out _);

        //buffer on the gpu in the ram
        resultBuffer = new ComputeBuffer(SphereAmount, sizeof(float) * 3);
        output = new Vector3[SphereAmount];

        computeParticles = new GameObject[SphereAmount];
        for(int i = 0; i < SphereAmount; i++)
        {
            computeParticles[i] = Instantiate(computeParticle, new Vector3(0, 0, 0), Quaternion.identity);
        }

    }

    // Update is called once per frame
    void Update()
    {
        Shader.SetFloat("Time", Time.time);
        Shader.SetBuffer(kernel, "Result", resultBuffer);
        int threadGroups = (int)((SphereAmount + (threadGroupSize - 1)) / threadGroupSize);
        Shader.Dispatch(kernel, threadGroups, 1, 1);
        resultBuffer.GetData(output);

        for (int i = 0; i < computeParticles.Length; i++)
        {
            computeParticles[i].transform.localPosition = output[i];
        }
    }

    private void OnDestroy()
    {
        resultBuffer.Dispose();
    }
}
