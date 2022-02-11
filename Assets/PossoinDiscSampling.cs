using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PossoinDiscSampling
{
    public static List<Vector2> GeneratePoints(float radius, Vector2 regionSize, int numberOfPoints)
    {
        List<Vector2> points = new List<Vector2>();

        while(points.Count < numberOfPoints)
        {
            Vector2 newPoint = new Vector2(Random.Range(-regionSize.x/2, regionSize.x/2), Random.Range(-regionSize.y/2, regionSize.y/2));

            bool accepted = true;
            foreach (Vector2 point in points)
            {
                if (Vector2.Distance(point, newPoint) <= radius)
                {
                    accepted = false;
                    break;
                }
            }

            if (accepted) points.Add(newPoint);
        }

        return points;
    }
}

