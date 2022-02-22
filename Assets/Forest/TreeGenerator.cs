using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TreeGenerator : MonoBehaviour
{
    
    Mesh mesh;
    public Vector3 treePosition;
    public Material treeMaterial;

    // ENVIRONMENTAL PARAMETERS
    float GRAVITROPISM_STRENGTH = 0.05f;
    Vector3 SUN_DIRECTION;
    float PHOTOTROPISM_STRNEGTH = 0.05f;

    // TREE PARAMETERS
    int STEM_SIDES = 8;
    int MAX_CHILDREN = 5;
    int MIN_CHILDREN = 2;
    float MIN_START_THICNKESS = 20f;
    float MAX_START_THICKNESS = 40f;
    float MIN_START_LENGTH = 70f;
    float MAX_START_LENGTH = 150f;
    float BRANCH_THICKNESS_DECLINE = 0.6f;
    float BRANCH_THICKNESS_VARIATION = 0.2f;
    float BRANCH_LENGTH_VARIATION = 0.3f;
    float BRANCH_LENGTH_DECLINE = 0.7f;
    float MIN_BRANCHING_DISTANCE = 0.6f;
    float MIN_BRANCH_THICKNESS = 1f;
    int MAX_NUMBER_OF_VERTICES = 65535;
    float TREE_SCALE = 0.1f;
    int MAX_DEPTH = 6;

    float GROWTH_RATE = 0; // SECONDS
    float last_growth = 0.0f;
    int depth = 0;

    bool noLeafs = true;

    public struct Bud
    {
        public Bud(Vector3 direction, Vector3 position, float size, float length)
        {
            Direction = direction;
            Position = position;
            Size = size;
            Length = length;
        }

        public Vector3 Direction { get; }
        public Vector3 Position { get; }
        public float Size { get; }
        public float Length { get; }

    }

    List<Bud> buds = new List<Bud>();
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    int triangleIndex = 0;

    // LEAFS
    List<Vector3> leafPositions = new List<Vector3>();
    public GameObject leafs;

    void Start()
    {
        noLeafs = true;
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = treeMaterial; 
        //GetComponent<MeshRenderer>().material = new Material(Shader.Find("Unlit/Color"));
        //GetComponent<MeshRenderer>().material.SetColor("_Color", new Vector4(0.0f, 0.0f, 0.0f, 1));

        SUN_DIRECTION = -GameObject.FindObjectOfType<Light>().transform.forward;

        Vector3 startPos = new Vector3(0, 0, 0);
        
        Vector3 branchDirection = new Vector3(0, 1, 0);
        
        float branchThickness = GetRandom(MIN_START_THICNKESS, MAX_START_THICKNESS);
        float branchLength = GetRandom(MIN_START_LENGTH, MAX_START_LENGTH);
        Vector3 endPos = startPos + branchDirection * branchLength;
        Bud startingBud = new Bud(branchDirection, startPos, branchThickness, branchLength);
        buds.Add(startingBud);
    }

    private void Update()
    {

        if (!noLeafs) return;

        if (last_growth > 0)
        {
            last_growth -= Time.deltaTime;
            return;
        }

        last_growth = GROWTH_RATE;


        List<Bud> currentBuds = new List<Bud>(buds);
        buds = new List<Bud>();

        if(currentBuds.Count == 0 && noLeafs)
        {
            noLeafs = false;
            leafs.GetComponent<LeafGenerator>().leafPositions = leafPositions;
            Instantiate(leafs, treePosition, Quaternion.identity);
        }

        foreach (Bud bud in currentBuds)
        {
            Grow(bud);
        }
        depth += 1;

        UpdateMesh();
    }

    void DisableRender()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }

    void EnableRender() 
    {
        GetComponent<MeshRenderer>().enabled = true;
    }

    int GetRandom(int min, int max)
    {
        return Random.Range(min, max);
    }

    float GetRandom(float min, float max)
    {
        return Random.Range(min, max);
    }

    Vector3 ApplyGravitropism(Vector3 branchDirection)
    {
        Vector3 newBranchDirection = branchDirection;

        // X
        if (newBranchDirection.x > GRAVITROPISM_STRENGTH)
        {
            newBranchDirection.x -= GRAVITROPISM_STRENGTH;
        }
        else if (newBranchDirection.x < -GRAVITROPISM_STRENGTH)
        {
            newBranchDirection.x += GRAVITROPISM_STRENGTH;
        }
        else
        {
            newBranchDirection.x = 0;
        }

        // Y
        if (newBranchDirection.y > 1 + GRAVITROPISM_STRENGTH)
        {
            newBranchDirection.y -= GRAVITROPISM_STRENGTH;
        }
        else if (newBranchDirection.y < 1 - GRAVITROPISM_STRENGTH)
        {
            newBranchDirection.y += GRAVITROPISM_STRENGTH;
        }
        else
        {
            newBranchDirection.y = 1;
        }

        // Z
        if (newBranchDirection.z > GRAVITROPISM_STRENGTH)
        {
            newBranchDirection.z -= GRAVITROPISM_STRENGTH;
        }
        else if (newBranchDirection.z < -GRAVITROPISM_STRENGTH)
        {
            newBranchDirection.z += GRAVITROPISM_STRENGTH;
        }
        else
        {
            newBranchDirection.z = 0;
        }

        return newBranchDirection;
    }

    Vector3 ApplyPhototropism(Vector3 branchDirection)
    {
        Vector3 newBranchDirection = branchDirection;

        // X
        if (newBranchDirection.x < SUN_DIRECTION.x - PHOTOTROPISM_STRNEGTH)
        {
            newBranchDirection.x += PHOTOTROPISM_STRNEGTH;
        }
        else if (newBranchDirection.x > SUN_DIRECTION.x + PHOTOTROPISM_STRNEGTH)
        {
            newBranchDirection.x -= PHOTOTROPISM_STRNEGTH;
        }
        else
        {
            newBranchDirection.x = SUN_DIRECTION.x;
        }

        // Y
        if (newBranchDirection.y < SUN_DIRECTION.y - PHOTOTROPISM_STRNEGTH)
        {
            newBranchDirection.y += PHOTOTROPISM_STRNEGTH;
        }
        else if (newBranchDirection.y > SUN_DIRECTION.y + PHOTOTROPISM_STRNEGTH)
        {
            newBranchDirection.y -= PHOTOTROPISM_STRNEGTH;
        }
        else
        {
            newBranchDirection.y = SUN_DIRECTION.y;
        }

        // Z
        if (newBranchDirection.z < SUN_DIRECTION.z - PHOTOTROPISM_STRNEGTH)
        {
            newBranchDirection.z += PHOTOTROPISM_STRNEGTH;
        }
        else if (newBranchDirection.z > SUN_DIRECTION.z + PHOTOTROPISM_STRNEGTH)
        {
            newBranchDirection.z -= PHOTOTROPISM_STRNEGTH;
        }
        else
        {
            newBranchDirection.z = SUN_DIRECTION.z;
        }

        return newBranchDirection;
    }

    Vector3 GetPerpendicularVector(Vector3 v)
    {
        return Vector3.Normalize(new Vector3(v.y, -v.x, 0));
    }

    Vector3 GetPerpendicularVector(Vector3 v1, Vector3 v2)
    {
        return Vector3.Cross(v1, v2).normalized;
    }

    Vector3 RotatePointAroundAxis(Vector3 axis, Vector3 point, float angle)
    {
        return (Quaternion.AngleAxis(angle, axis) * point);
    }

    void Grow(Bud bud)
    {
        // Break if too thin or too many vertices
        if(vertices.Count >= MAX_NUMBER_OF_VERTICES || bud.Size < MIN_BRANCH_THICKNESS || depth >= MAX_DEPTH)
        {
            leafPositions.Add(bud.Position * TREE_SCALE);
            return;
        }

        // Update branch thickness and length 
        float newBranchThickness = bud.Size * GetRandom(BRANCH_THICKNESS_DECLINE - BRANCH_THICKNESS_VARIATION, BRANCH_THICKNESS_DECLINE + BRANCH_THICKNESS_VARIATION);
        float newBranchLength = bud.Length * GetRandom(BRANCH_LENGTH_DECLINE - BRANCH_LENGTH_VARIATION, BRANCH_LENGTH_DECLINE + BRANCH_LENGTH_VARIATION);
        Vector3 branchDirection = bud.Direction;
        // Gravitropism
        branchDirection = ApplyGravitropism(branchDirection);
        // Phototropism
        branchDirection = ApplyPhototropism(branchDirection);

        Vector3 branchTangent1 = GetPerpendicularVector(branchDirection);
        Vector3 branchTangent2 = GetPerpendicularVector(branchDirection, branchTangent1);

        // Gravitropism
        branchDirection = ApplyGravitropism(branchDirection);
        // Phototropism
        branchDirection = ApplyPhototropism(branchDirection);

        // Create current branch
        Vector3 endPos = bud.Position + bud.Length * branchDirection;
        //CreateBlock(bud.Size, bud.Size * 0.7f, bud.Position, endPos);
        CreateCylinder(bud.Position, endPos, branchDirection, bud.Size, newBranchThickness);

        // Branch Children
        int numberOfChildren = GetRandom(MIN_CHILDREN, MAX_CHILDREN);
        for (int i = 0; i < numberOfChildren; i++)
        {
            Vector3 newStartPos;
            if (i == 0)
            {
                newStartPos = endPos;
            }
            else
            {
                newStartPos = (endPos - bud.Position) * GetRandom(MIN_BRANCHING_DISTANCE, 1) + bud.Position;
            }

            float r1 = GetRandom(-1.0f, 1.0f);
            float r2;
            if (r1 <= 0) r2 = -1 - r1;
            else r2 = 1 - r1;

            Vector3 offset1 = r1 * branchTangent1 * newBranchLength;
            Vector3 offset2 = r2 * branchTangent2 * newBranchLength;
            Vector3 newBranchDirection = Vector3.Normalize((endPos + offset1 + offset2 + branchDirection * newBranchLength) - endPos);
            
            Bud newBud = new Bud(newBranchDirection, newStartPos, newBranchThickness, newBranchLength);
            buds.Add(newBud);

        }
    }


    void CreateBlock(float branchThicknessBottom, float branchThicknessTop, Vector3 startPos, Vector3 endPos)
    {

        int offset = triangleIndex * 8;

        // FRONT
        vertices.Add(new Vector3(0, 0, 0) + startPos);
        vertices.Add(new Vector3(0, 0, 0) + endPos);
        vertices.Add(new Vector3(branchThicknessBottom, 0, 0) + startPos);
        vertices.Add(new Vector3(branchThicknessTop, 0, 0) + endPos);

        // Triangle 1
        triangles.Add(offset + 0);
        triangles.Add(offset + 1);
        triangles.Add(offset + 2);
        // Triangle 2
        triangles.Add(offset + 1);
        triangles.Add(offset + 3);
        triangles.Add(offset + 2);

        // BACK
        vertices.Add(new Vector3(0, 0, branchThicknessBottom) + startPos);
        vertices.Add(new Vector3(0, 0, branchThicknessTop) + endPos);
        vertices.Add(new Vector3(branchThicknessBottom, 0, branchThicknessBottom) + startPos);
        vertices.Add(new Vector3(branchThicknessTop, 0, branchThicknessTop) + endPos);

        // Triangle 1
        triangles.Add(offset + 4);
        triangles.Add(offset + 6);
        triangles.Add(offset + 5);
        // Triangle 2
        triangles.Add(offset + 5);
        triangles.Add(offset + 6);
        triangles.Add(offset + 7);

        //LEFT

        // Triangle 1
        triangles.Add(offset + 1);
        triangles.Add(offset + 0);
        triangles.Add(offset + 4);
        // Triangle 2
        triangles.Add(offset + 1);
        triangles.Add(offset + 4);
        triangles.Add(offset + 5);

        //RIGHT

        // Triangle 1
        triangles.Add(offset + 2);
        triangles.Add(offset + 3);
        triangles.Add(offset + 6);
        // Triangle 2
        triangles.Add(offset + 3);
        triangles.Add(offset + 7);
        triangles.Add(offset + 6);

        triangleIndex++;
    }

    void CreateCylinder(Vector3 start, Vector3 end, Vector3 dir, float startThickness, float endThickness)
    {

        Vector3 outDir = GetPerpendicularVector(dir);
        float angleStep = 360f / STEM_SIDES;
        int triIndex = vertices.Count;

        // BOTTOM
        vertices.Add(start);
        uvs.Add(new Vector2(0, 1));
        for (int i = 0; i < STEM_SIDES; i ++)
        {
            Vector3 newPoint = start +  RotatePointAroundAxis(dir, outDir, i * angleStep).normalized * (startThickness/2);
            vertices.Add(newPoint);

            if (i % 2 == 0) uvs.Add(new Vector2(0, 1)); // top-left
            else uvs.Add(new Vector2(1, 1)); // top-right
        }

        // TOP
        vertices.Add(end);
        uvs.Add(new Vector2(0, 1));
        for (int i = 0; i < STEM_SIDES; i++)
        {
            Vector3 newPoint = end + RotatePointAroundAxis(dir, outDir, i * angleStep).normalized * (endThickness / 2);
            vertices.Add(newPoint);

            if(i % 2 == 0) uvs.Add(new Vector2(0, 0)); // bottom-left
            else uvs.Add(new Vector2(1, 0)); // bottom-right
        }

        for (int i = 0; i < STEM_SIDES; i++)
        {
            
            triangles.Add(triIndex + 1 + (i % STEM_SIDES));
            triangles.Add(triIndex);
            triangles.Add(triIndex + 1 + ((i + 1) % STEM_SIDES));

            triangles.Add(triIndex + STEM_SIDES + 1);
            triangles.Add((triIndex + STEM_SIDES + 1) + 1 + (i % STEM_SIDES));
            triangles.Add((triIndex + STEM_SIDES + 1) + 1 + ((i + 1) % STEM_SIDES));

            
        }
        
        // SIDES
        for(int i = 0; i < STEM_SIDES; i++)
        {

            triangles.Add(triIndex + 1 + (i % STEM_SIDES));
            triangles.Add(triIndex + 1 + ((i + 1) % STEM_SIDES));
            triangles.Add((triIndex + STEM_SIDES + 1) + 1 + (i % STEM_SIDES));

            triangles.Add((triIndex + STEM_SIDES + 1) + 1 + (i % STEM_SIDES));
            triangles.Add(triIndex + 1 + ((i + 1) % STEM_SIDES));
            triangles.Add((triIndex + STEM_SIDES + 1) + 1 + ((i + 1) % STEM_SIDES));
            

        }
    }

    void UpdateMesh()
    {
        mesh.Clear();

        Vector3[] meshVertices = new Vector3[vertices.Count];

        // Copy vertices from vertices list
        int index = 0;
        foreach (Vector3 vertex in vertices)
        {
            meshVertices[index] = vertex * TREE_SCALE;
            index++;
        }

        // Copy triangle indexes
        int[] meshTriangles = new int[triangles.Count];
        index = 0;
        foreach (int triangle in triangles)
        {
            meshTriangles[index] = triangle;
            index++;
        }

        // Copy uv voordinates
        Vector2[] meshUvs = new Vector2[uvs.Count];
        index = 0;
        foreach(Vector2 uv in uvs)
        {
            meshUvs[index] = uv;
            index++;
        }

        mesh.vertices = meshVertices;
        mesh.triangles = meshTriangles;
        mesh.uv = meshUvs;
        mesh.RecalculateNormals();
    }
}
