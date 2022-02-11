using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TreeGenerator : MonoBehaviour
{
    
    Mesh mesh;
    public Vector3 treePosition;

    // ENVIRONMENTAL PARAMETERS
    Vector3 GRAVITY = new Vector3(0, -1, 0);
    float GRAVITROPISM_STRENGTH = 0.05f;
    Vector3 SUN_DIRECTION;
    float PHOTOTROPISM_STRNEGTH = 0.05f;

    // TREE PARAMETERS
    int MAX_DEPTH = 5;
    int MAX_CHILDREN = 5;
    int MIN_CHILDREN = 2;
    float MIN_START_THICNKESS = 10f;
    float MAX_START_THICKNESS = 30f;
    float MIN_START_LENGTH = 50f;
    float MAX_START_LENGTH = 100f;
    float BRANCH_THICKNESS_DECLINE = 0.6f;
    float BRANCH_THICKNESS_VARIATION = 0.2f;
    float BRANCH_LENGTH_VARIATION = 0.3f;
    float BRANCH_LENGTH_DECLINE = 0.7f;
    float MIN_BRANCHING_DISTANCE = 0.6f;
    float MIN_BRANCH_THICKNESS = 1f;
    int MAX_NUMBER_OF_VERTICES = 65535;
    float TREE_SCALE = 0.1f;
    float DEATH_CHANCE = 0.0f;

    float GROWTH_RATE = 3; // SECONDS
    float last_growth = 0.0f;

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
    int triangleIndex = 0;

    // LEAFS
    List<Vector3> leafPositions = new List<Vector3>();
    public GameObject leafs;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = new Material(Shader.Find("Specular"));
        GetComponent<MeshRenderer>().material.SetColor("_Color", new Vector4(0.6f, 0.3f, 0.1f, 1));

        SUN_DIRECTION = -GameObject.FindObjectOfType<Light>().transform.forward;

        Vector3 startPos = new Vector3(0, 0, 0);
        Vector3 branchDirection = new Vector3(0, 1, 0);
        float branchThickness = GetRandom(MIN_START_THICNKESS, MAX_START_THICKNESS);
        float branchLength = GetRandom(MIN_START_LENGTH, MAX_START_LENGTH);
        Bud startingBud = new Bud(branchDirection, startPos, branchThickness, branchLength);
        buds.Add(startingBud);
        //GrowTree(branchThickness, branchLength, startPos, branchDirection, branchTangent1, branchTangent2, 0);
        //UpdateMesh();
        //leafs.GetComponent<LeafGenerator>().leafPositions = leafPositions;
        //Instantiate(leafs, treePosition, Quaternion.identity);
    }

    private void Update()
    {
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
            //leafs.GetComponent<LeafGenerator>().leafPositions = leafPositions;
            //Instantiate(leafs, treePosition, Quaternion.identity);
            //noLeafs = false;
        }

        foreach (Bud bud in currentBuds)
        {
            Grow(bud);
        }

        UpdateMesh();
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
        return Vector3.Normalize(new Vector3(1, 1, -(v.x + v.y) / v.z));
    }

    Vector3 GetPerpendicularVector(Vector3 v1, Vector3 v2)
    {
        return Vector3.Cross(v1, v2).normalized;
    }

    void Grow(Bud bud)
    {
        // Break if too thin or too many vertices
        if(vertices.Count >= MAX_NUMBER_OF_VERTICES || bud.Size < MIN_BRANCH_THICKNESS)
        {
            leafPositions.Add(bud.Position);
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
        CreateBlock(bud.Size, bud.Size * 0.7f, bud.Position, endPos);

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

    void GrowTree(float branchThickness, float branchLength, Vector3 startPos, Vector3 branchDirection, Vector3 branchTangent1, Vector3 branchTangent2, int depth)
    {
        // Break if max depth reached or if maximum number of vertices is reached
        if (depth == MAX_DEPTH || vertices.Count >= MAX_NUMBER_OF_VERTICES || branchThickness <= MIN_BRANCH_THICKNESS || GetRandom(0f, 1f) < DEATH_CHANCE)
        {
            leafPositions.Add(startPos * TREE_SCALE);
            return;
        }


        // Update branch thickness and length 
        float newBranchThickness = branchThickness * GetRandom(BRANCH_THICKNESS_DECLINE - BRANCH_THICKNESS_VARIATION, BRANCH_THICKNESS_DECLINE + BRANCH_THICKNESS_VARIATION);
        float newBranchLength = branchLength * GetRandom(BRANCH_LENGTH_DECLINE - BRANCH_LENGTH_VARIATION, BRANCH_LENGTH_DECLINE + BRANCH_LENGTH_VARIATION);

        // Gravitropism
        branchDirection = ApplyGravitropism(branchDirection);
        // Phototropism
        branchDirection = ApplyPhototropism(branchDirection);

        // Create current branch
        Vector3 endPos = startPos + branchLength * branchDirection;
        CreateBlock(branchThickness, branchThickness * 0.7f, startPos, endPos);

        // Continue growing from current base, with a probability of 0.5
        if (GetRandom(0f, 1f) <= 0.5f || depth == 0)
        {
            GrowTree(branchThickness * 0.7f, newBranchLength, endPos, branchDirection, branchTangent1, branchTangent2, depth);
        }

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
                newStartPos = (endPos - startPos) * GetRandom(MIN_BRANCHING_DISTANCE, 1) + startPos;
            }

            float r1 = GetRandom(-1.0f, 1.0f);
            float r2;
            if (r1 <= 0) r2 = -1 - r1;
            else r2 = 1 - r1;

            Vector3 offset1 = r1 * branchTangent1 * newBranchLength;
            Vector3 offset2 = r2 * branchTangent2 * newBranchLength;
            Vector3 newBranchDirection = Vector3.Normalize((endPos + offset1 + offset2 + branchDirection * newBranchLength) - endPos);
            Vector3 newBranchTangent1 = GetPerpendicularVector(newBranchDirection);
            Vector3 newBranchTangent2 = GetPerpendicularVector(newBranchDirection, newBranchTangent1);
            GrowTree(newBranchThickness, newBranchLength, newStartPos, newBranchDirection, newBranchTangent1, newBranchTangent2, depth + 1);
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

        mesh.vertices = meshVertices;
        mesh.triangles = meshTriangles;
        mesh.RecalculateNormals();
    }
}
