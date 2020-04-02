using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionParent : MonoBehaviour
{
    public float startInteractionTime;
    public float endInteractionTime;
    public bool shouldPickLocks;
    public bool shouldSmack;
    public bool emptyHands;
    public bool doesItFuckingMatter = true;
    protected bool picked;
    protected bool smacked;
    public Collider interactionCollider;
    public Transform indicatorLocation;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        interactionCollider.isTrigger = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public virtual void Interact(PlayerBase playerBase)
    {
        if (playerBase.canPickLocks)
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
    public virtual IEnumerator InteractRoutine(PlayerBase p)
    {
        p.input.velocity = Vector3.zero;
        p.rb.velocity = Vector3.zero;
        p.canMove = startInteractionTime + endInteractionTime == 0;
        p.transform.rotation = Quaternion.LookRotation((transform.position - p.transform.position) - Vector3.up * (transform.position.y - p.transform.position.y));
        p.input.rotation = p.transform.rotation;
        p.state = PlayerBase.States.Idle;
        p.input.state = PlayerBase.States.Idle;
        if (p.holding != null && emptyHands)
        {
            p.holding.Drop(p);
        }
        if (doesItFuckingMatter)
        {
            if (p.canPickLocks)
            {
                picked = true;
                p.state = PlayerBase.States.LockPick;
                p.input.state = PlayerBase.States.LockPick;
            }
            else
            {
                p.state = PlayerBase.States.Smack;
                p.input.state = PlayerBase.States.Smack;
                smacked = true;
            }
        }
        p.anim.SetInteger("state", (int)p.input.state);
        yield return new WaitForSeconds(startInteractionTime);
        Interact(p);
        yield return new WaitForSeconds(endInteractionTime);
        p.canMove = true;
        p.state = PlayerBase.States.Idle;
        p.input.state = PlayerBase.States.Idle;
        p.anim.SetInteger("state", (int)p.input.state);
    }
    public virtual void Drop(PlayerBase p)
    {

    }
    public virtual void PickUp(PlayerBase p)
    {

    }
}
