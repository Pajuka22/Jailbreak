using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locker : InteractionParent
{
    [Tooltip("Time to unlock")]
    public float startUnlockTime;
    [Tooltip("Time to finish unlocking")]
    public float endUnlockTime;
    [Tooltip("How long it takes to start the opening/closing interaction")]
    public float startOpenCloseTime;
    [Tooltip("How long it takes to end the opening/closing interaction")]
    public float endOpenCloseTime;
    [Tooltip("How long it takes to start stuffing someone in a locker")]
    public float startFillTime;
    [Tooltip("How long it takes to finish stuffing someone in a locker")]
    public float endFillTime;
    public Animator anim;
    public Rigidbody rb;
    public bool locked;
    public bool open;
    public Material lockedMat;
    public Material unlockedMat;
    public Renderer doorRenderer;
    [System.NonSerialized]
    public bool empty;
    Enemy occupant;
    public Collider doorCollider;
    bool wasLocked;
    bool wasOpen;
    // Start is called before the first frame update
    void Start()
    {
        wasLocked = locked;
        wasOpen = open;
        open = false;
        doorRenderer.sharedMaterial = locked ? lockedMat : unlockedMat;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        anim.SetBool("Open", open);
        doorRenderer.sharedMaterial = locked ? lockedMat : unlockedMat;
    }
    public override void InteractionReset()
    {
        locked = wasLocked;
        open = wasOpen;
        occupant = null;
    }
    public override IEnumerator InteractRoutine(PlayerBase p)
    {
        p.input.velocity = Vector3.zero;
        p.rb.velocity = Vector3.zero;
        p.canMove = false;
        p.transform.rotation = Quaternion.LookRotation((transform.position - p.transform.position) - Vector3.up * (transform.position.y - p.transform.position.y));
        p.input.rotation = p.transform.rotation;
        p.state = PlayerBase.States.Idle;
        p.input.state = PlayerBase.States.Idle;
        if (p.canPickLocks)
        {
            if (locked)
            {
                p.input.state = PlayerBase.States.LockPick;
                p.state = PlayerBase.States.LockPick;
                p.anim.SetInteger("state", (int)p.input.state);
                yield return new WaitForSeconds(startUnlockTime);
                locked = false;
                yield return new WaitForSeconds(endUnlockTime);
            }
            else
            {
                if (open)
                {
                    yield return new WaitForSeconds(startOpenCloseTime);
                    doorCollider.enabled = false;
                    open = false;
                    yield return new WaitForSeconds(endOpenCloseTime);
                    doorCollider.enabled = true;
                }
                else
                {
                    yield return new WaitForSeconds(startOpenCloseTime);
                    doorCollider.enabled = false;
                    open = true;
                    yield return new WaitForSeconds(endOpenCloseTime);
                }
            }
        }
        else
        {
            if (!locked)
            {
                if (p.holding is Enemy)
                {
                    if (!open)
                    {
                        yield return new WaitForSeconds(startOpenCloseTime);
                        doorCollider.enabled = false;
                        open = true;
                        yield return new WaitForSeconds(endOpenCloseTime);
                    }
                    if (occupant == null)
                    {
                        yield return new WaitForSeconds(startFillTime);
                        occupant = (Enemy)p.holding;
                        occupant.head.position = transform.position;
                        occupant.Drop(p);
                        occupant.headGrab.gameObject.SetActive(true);
                        occupant.headGrab.connectedBody = rb;
                        occupant.interactionCollider.enabled = false;
                        Physics.IgnoreLayerCollision(occupant.gameObject.layer, p.gameObject.layer);
                        Enemy.deadEnemies.Remove(occupant);
                        yield return new WaitForSeconds(endFillTime);
                        Physics.IgnoreLayerCollision(occupant.gameObject.layer, p.gameObject.layer, false);
                    }
                    else
                    { 
                        //barf?
                    }
                }
                else
                {
                    if (open)
                    {
                        yield return new WaitForSeconds(startOpenCloseTime);
                        doorCollider.enabled = false;
                        open = false;
                        yield return new WaitForSeconds(endOpenCloseTime);
                        doorCollider.enabled = true;
                    }
                    else
                    {
                        yield return new WaitForSeconds(startOpenCloseTime);
                        doorCollider.enabled = false;
                        open = true;
                        yield return new WaitForSeconds(endOpenCloseTime);
                    }
                }
            }
        }
        p.canMove = true;
        p.state = PlayerBase.States.Idle;
        p.input.state = PlayerBase.States.Idle;
        p.anim.SetInteger("state", (int)p.input.state);
    }
}
