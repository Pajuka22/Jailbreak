using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractRedo : MonoBehaviour
{
    public enum InteractionType { pick, smack, both}
    [System.NonSerialized]
    public InteractionType intType;
    [System.NonSerialized]
    public bool picked;
    [System.NonSerialized]
    public bool smacked;
    public Collider interactionCollider;
    public Transform indicatorLocation;
    public PlayerBase.States intAnim;
    public bool emptyHands;
    public int animIterations;
    // Start is called before the first frame update
    void Start()
    {
        interactionCollider.isTrigger = true;
        EventManager.current.resetInteractables += InteractionReset;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public virtual void Interact(PlayerBase p)
    {
        if (p.canPickLocks)
        {
            picked = true;
        }
        else
        {
            smacked = true;
        }
    }
    public virtual void InteractionReset()
    {

    }
    public virtual void PickUp(PlayerBase p)
    {

    }
    public virtual void Drop(PlayerBase p)
    {

    }
}
