using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    public Animator anim;
    private List<Rigidbody> ragdollColliders = new List<Rigidbody>();
    // Start is called before the first frame update
    void Start()
    {
        if(anim == null)
        {
            anim = GetComponent<Animator>();
        }
        SetRagdollRBs();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void SetRagdollRBs()
    {
        foreach(Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            ragdollColliders.Add(rb);
            rb.isKinematic = true;
        }
    }
    public void TurnRagdollOn()
    {
        Debug.Log("Ragdoll On");
        anim.enabled = false;
        foreach (Rigidbody rb in ragdollColliders)
        {
            rb.isKinematic = false;
        }
    }
    public void TurnRagdollOff()
    {
        Debug.Log("Ragdoll Off");
        anim.enabled = true;
        foreach (Rigidbody rb in ragdollColliders)
        {
            rb.isKinematic = true;
        }
    }
}
