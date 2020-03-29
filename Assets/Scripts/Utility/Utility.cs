using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility : MonoBehaviour
{
    // Start is called before the first frame update
    public static bool Has<T>(GameObject Obj)
    {
        return Obj.GetComponent<T>() != null;
    }
}
