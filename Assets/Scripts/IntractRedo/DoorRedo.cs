using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorRedo : InteractRedo
{
    // Start is called before the first frame update
    public enum State { locked, closed, open};
    [System.NonSerialized]
    public State currentState;
    [SerializeField]
    private State startState;
    public Material lockedMat;
    public Material unlockedMat;
    public BoxCollider doorCollider;
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private Renderer rend;
    void Start()
    {
        currentState = startState;
        intAnim = currentState == State.locked ? PlayerBase.States.LockPick : PlayerBase.States.Idle;
        rend.sharedMaterial = currentState == State.locked ? lockedMat : unlockedMat;
        if(anim == null)
        {
            anim = GetComponent<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
    public override void Interact(PlayerBase p)
    {
        switch (currentState)
        {
            case State.locked:
                currentState = State.closed;
                intAnim = PlayerBase.States.Idle;
                break;
            case State.closed:
                currentState = State.open;
                doorCollider.enabled = false;
                break;
            case State.open:
                currentState = State.closed;
                doorCollider.enabled = true;
                break;
        }
        rend.sharedMaterial = currentState == State.locked ? lockedMat : unlockedMat;
        if (anim != null)
        {
            anim.SetBool("open", currentState == State.open);
        }
    }
    public override void InteractionReset()
    {
        intAnim = PlayerBase.States.Idle;
        currentState = startState;
        rend.sharedMaterial = currentState == State.locked ? lockedMat : unlockedMat;
    }
}
