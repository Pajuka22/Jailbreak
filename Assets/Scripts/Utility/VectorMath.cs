using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorMath : MonoBehaviour
{
    // Start is called before the first frame update
    public static float RadiansToVector(Vector3 v1, Vector3 v2)
    {
        return Mathf.Acos(Vector3.Dot(v1, v2) / v1.magnitude / v2.magnitude);
    }
    //calculate length of vector in direction of another
    public static float DistanceInDirection(Vector3 vector, Vector3 direction)
    {
        return Vector3.Dot(vector, direction) / direction.magnitude;
    }
    //convert local position to world position relative to parent.
    public static Vector3 LocalToWorld(Vector3 vector, Transform localSpace)
    {
        return vector.z * localSpace.forward + vector.y * localSpace.up + vector.x * localSpace.right;
    }
    public static Vector3 ZeroY(Vector3 vectIn)
    {
        return new Vector3(vectIn.x, 0, vectIn.z);
    }
}
