using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBoi : MonoBehaviour
{
    public bool selected;
    public bool canSelect;
    public Material sDeselected;
    public Material sSelected;
    public Material cantSelect;
    MeshRenderer rend;
    //public  renderer;
    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (selected)
        {
            rend.material = sSelected;
        }
        else
        {
            rend.material = sDeselected;
        }
    }
}
