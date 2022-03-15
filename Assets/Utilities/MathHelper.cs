using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathHelper
{
    public static Vector3 GetPerpendicularVector(Vector3 v)
    {
        return Vector3.Normalize(new Vector3(v.y, -v.x, 0));
    }

    public static float GetRandom(float min, float max)
    {
        return Random.Range(min, max);
    }

    public static int GetRandom(int min, int max)
    {
        return Random.Range(min, max + 1);
    }

    public static Vector3 SamplePerceptionSphere(Vector3 direction, float angle)
    {
        var angleInRad = Random.Range(0.0f, angle) * Mathf.Deg2Rad;
        var PointOnCircle = (Random.insideUnitCircle.normalized) * Mathf.Sin(angleInRad);
        var V = new Vector3(PointOnCircle.x, PointOnCircle.y, Mathf.Cos(angleInRad));
        return Quaternion.LookRotation(direction) * V;
    }

}
