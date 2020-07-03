using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    public Animator anim;
    [System.NonSerialized]
    public List<Rigidbody> ragdollColliders = new List<Rigidbody>();
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
            if (rb != GetComponent<Rigidbody>())
            {
                ragdollColliders.Add(rb);
                rb.isKinematic = true;
            }
        }
        /*if (GetComponent<Rigidbody>() != null)
        {
            ragdollColliders.Remove(GetComponent<Rigidbody>());
            GetComponent<Rigidbody>().isKinematic = false;
        }*/
    }
    public void TurnRagdollOn()
    {
        anim.enabled = false;
        foreach (Rigidbody rb in ragdollColliders)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
    public void TurnRagdollOff()
    {
        anim.enabled = true;
        foreach (Rigidbody rb in ragdollColliders)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }
}
