using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshHelper
{
    public static void cylinder(List<Vector3> vertices, List<int> triangles, Vector3 start, Vector3 end, float startRadius, float endReadius, int nmbrOfSides)
    {
        Vector3 dir = Vector3.Normalize(end - start);
        Vector3 outDir = MathHelper.GetPerpendicularVector(dir); //GetPerpendicularVector(dir);
        float angleStep = 360f / nmbrOfSides;
        int triIndex = vertices.Count;

        // BOTTOM
        vertices.Add(start);
        for (int i = 0; i < nmbrOfSides; i++)
        {
            Vector3 newPoint = start + (Quaternion.AngleAxis(i * angleStep, dir) * outDir).normalized * (startRadius / 2);
            vertices.Add(newPoint);
        }

        // TOP
        vertices.Add(end);
        for (int i = 0; i < nmbrOfSides; i++)
        {
            Vector3 newPoint = end + (Quaternion.AngleAxis(i * angleStep, dir) * outDir).normalized * (endReadius / 2);
            vertices.Add(newPoint);
        }

        for (int i = 0; i < nmbrOfSides; i++)
        {

            triangles.Add(triIndex + 1 + (i % nmbrOfSides));
            triangles.Add(triIndex);
            triangles.Add(triIndex + 1 + ((i + 1) % nmbrOfSides));

            triangles.Add(triIndex + nmbrOfSides + 1);
            triangles.Add((triIndex + nmbrOfSides + 1) + 1 + (i % nmbrOfSides));
            triangles.Add((triIndex + nmbrOfSides + 1) + 1 + ((i + 1) % nmbrOfSides));


        }

        // SIDES
        for (int i = 0; i < nmbrOfSides; i++)
        {

            triangles.Add(triIndex + 1 + (i % nmbrOfSides));
            triangles.Add(triIndex + 1 + ((i + 1) % nmbrOfSides));
            triangles.Add((triIndex + nmbrOfSides + 1) + 1 + (i % nmbrOfSides));

            triangles.Add((triIndex + nmbrOfSides + 1) + 1 + (i % nmbrOfSides));
            triangles.Add(triIndex + 1 + ((i + 1) % nmbrOfSides));
            triangles.Add((triIndex + nmbrOfSides + 1) + 1 + ((i + 1) % nmbrOfSides));
        }
    }

    public static void square(List<Vector3> vertices, List<int> triangles, Vector3 position, float size)
    {

        int triIndex = vertices.Count;

        vertices.Add(position);
        vertices.Add(position + new Vector3(size, 0, 0));
        vertices.Add(position + new Vector3(0, 0, size));
        vertices.Add(position + new Vector3(size, 0, size));

        triangles.Add(triIndex);
        triangles.Add(triIndex + 1);
        triangles.Add(triIndex + 2);

        triangles.Add(triIndex + 2);
        triangles.Add(triIndex + 1);
        triangles.Add(triIndex + 3);
    }
}
