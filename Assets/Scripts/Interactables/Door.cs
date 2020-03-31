using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : InteractionParent
{
    // Start is called before the first frame update
    public enum doorStates { locked, open, closed}
    public doorStates currentState;
    private doorStates startState;
    public Material lockedMat;
    public Material unlockedMat;
    public Renderer rend;
    public BoxCollider doorCollision;
    private Animator anim;
    public float startUnlockTime;
    public float endUnlockTime;
    public float startOpenTime;
    public float endOpenTime;
    void Start()
    {
        doesItFuckingMatter = true;
        shouldPickLocks = true;
        anim = GetComponent<Animator>();
        startState = currentState;
        if(currentState == doorStates.locked)
        {
            rend.sharedMaterial = lockedMat;
        }
        else
        {
            rend.sharedMaterial = unlockedMat;
        }

    }
    // Update is called once per framed
    void Update()
    {
        if(anim != null)
        {
            anim.SetBool("open", currentState == doorStates.open);
        }
        startInteractionTime = startOpenTime;
        endInteractionTime = endOpenTime;
        switch (currentState)
        {
            case doorStates.locked:
                startInteractionTime = startUnlockTime;
                endInteractionTime = endUnlockTime;
                doesItFuckingMatter = true;
                doorCollision.enabled = true;
                rend.sharedMaterial = lockedMat;
                break;
            case doorStates.closed:
                doesItFuckingMatter = false;
                doorCollision.enabled = true;
                rend.sharedMaterial = unlockedMat;
                break;
            case doorStates.open:
                doesItFuckingMatter = false;
                doorCollision.enabled = false;
                rend.sharedMaterial = unlockedMat;
                break;
        }
    }
    public override void Interact(PlayerBase playerBase)
    {
        Debug.Log("Drop the fucking Body");
        base.Interact(playerBase);
        switch (currentState)
        {
            case doorStates.locked:
                currentState = doorStates.closed;
                break;
            case doorStates.closed:
                currentState = doorStates.open;
                break;
            case doorStates.open:
                currentState = doorStates.closed;
                break;
        }
    }
    public override IEnumerator InteractRoutine(PlayerBase p)
    {
        return base.InteractRoutine(p);
    }
    public override void InteractionReset()
    {
        currentState = startState;
    }
}
