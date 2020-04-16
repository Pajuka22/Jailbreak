using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SoundUtility : MonoBehaviour
{
    // Start is called before the first frame update
    public struct Sound
    {
        public Vector3 location;
        public float strength;
        public float maxRadius;
        public Sound (Vector3 loc, float str, float rad)
        {
            location = loc;
            strength = str;
            maxRadius = rad;
        }
    }
    public static float GetPathLength(NavMeshPath path)
    {
        float lng = 0.0f;

        if ((path.status != NavMeshPathStatus.PathInvalid) && (path.corners.Length > 1))
        {
            for (int i = 1; i < path.corners.Length; ++i)
            {
                lng += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
        }

        return lng;
    }
}
