﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    public Animator anim;
    [System.NonSerialized]
    public List<Rigidbody> ragdollColliders = new List<Rigidbody>();
    private List<TransformValues> defaultTransforms = new List<TransformValues>();
    // Start is called before the first frame update
    void Start()
    {
        if(anim == null)
        {
            anim = GetComponent<Animator>();
        }
        SetRagdollRBs();
        EventManager.current.resetInteractables += TurnRagdollOff;
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
                defaultTransforms.Add(new TransformValues(rb.transform.position, rb.transform.rotation, rb.transform.localScale));
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
    public void Unexplode()
    {
        for(int i = 0; i < defaultTransforms.Count; i++)
        {
            ragdollColliders[i].transform.position = defaultTransforms[i].loc;
            ragdollColliders[i].transform.rotation = defaultTransforms[i].rot;
            ragdollColliders[i].transform.localScale = defaultTransforms[i].scale;
        }
    }
    private struct TransformValues
    {
        public Vector3 loc;
        public Quaternion rot;
        public Vector3 scale;
        public TransformValues(Vector3 position, Quaternion rotation, Vector3 localScale)
        {
            loc = position;
            rot = rotation;
            scale = localScale;
        }
    }
}
