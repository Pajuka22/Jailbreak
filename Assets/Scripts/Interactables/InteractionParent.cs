using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionParent : MonoBehaviour
{
    public float startInteractionTime;
    public float endInteractionTime;
    public bool shouldPickLocks;
    public bool shouldSmack;
    public bool doesItFuckingMatter = true;
    protected bool picked;
    protected bool smacked;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public virtual void Interact(PlayerBase playerBase)
    {
        //Debug.Log("Interact");
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
        p.canMove = false;
        p.transform.rotation = Quaternion.LookRotation((transform.position - p.transform.position) - Vector3.up * (transform.position.y - p.transform.position.y));
        p.input.rotation = p.transform.rotation;
        p.state = PlayerBase.States.Idle;
        p.input.state = PlayerBase.States.Idle;
        if (doesItFuckingMatter)
        {
            if (p.canPickLocks)
            {
                picked = true;
                p.state = PlayerBase.States.LockPick;
                p.input.state = PlayerBase.States.LockPick;
                Debug.Log(("Set your fucking state to lock pick"));
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
}
