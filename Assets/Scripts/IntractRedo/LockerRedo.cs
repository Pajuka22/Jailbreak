using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class LockerRedo : InteractRedo
{
    public Animator anim;
    public Rigidbody rb;
    public DoorRedo.State state;
    public Material lockedMat;
    public Material unlockedMat;
    bool startLocked;
    bool locked;
    Enemy occupant;
    public MeshRenderer doorRend;
    // Use this for initialization
    void Start()
    {
        locked = startLocked;
        doorRend.sharedMaterial = locked ? lockedMat : unlockedMat;
        intAnim = locked ? PlayerBase.States.LockPick : PlayerBase.States.Idle;
        intType = locked ? InteractionType.pick : InteractionType.both;
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("Open", state == DoorRedo.State.open);
    }
    public override void Interact(PlayerBase p)
    {
        switch (state)
        {
            case DoorRedo.State.locked:
                state = DoorRedo.State.closed;
                break;
            case DoorRedo.State.closed:
                state = DoorRedo.State.open;
                break;
            case DoorRedo.State.open:
                state = DoorRedo.State.closed;
                break;
        }
    }
}
