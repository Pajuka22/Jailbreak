﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinCon : InteractionParent
{
    public float pickStartTime;
    public float pickEndTime;
    public float smackStartTime;
    public float smackEndTime;
    public string WinScene;
    // Start is called before the first frame update
    protected override  void Start()
    {
        picked = false;
        smacked = false;
        doesItFuckingMatter = true;
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if(picked && smacked)
        {
            SceneManager.LoadScene(WinScene);
        }
    }
    public override void Interact(PlayerBase playerBase)
    {
        if (!PlayerBase.firstRun)
        {
            if (playerBase.canPickLocks)
            {
                picked = true;
            }
            else if(picked)
            {
                smacked = true;
            }
        }
        else
        {
            playerBase.GhostSwap();
        }
    }
    public override IEnumerator InteractRoutine(PlayerBase p)
    {
        p.canMove = false;
        p.input.velocity = Vector3.zero;
        p.rb.velocity = Vector3.zero;
        p.transform.rotation = Quaternion.LookRotation((transform.position - p.transform.position) - Vector3.up * (transform.position.y - p.transform.position.y));
        p.input.rotation = p.transform.rotation;
        p.state = PlayerBase.States.Idle;
        p.input.state = PlayerBase.States.Idle;
        if(p.holding != null && emptyHands)
        {
            p.holding.Drop(p);
        }
        if (doesItFuckingMatter)
        {
            if (p.canPickLocks)
            {
                startInteractionTime = pickStartTime;
                endInteractionTime = pickEndTime;
                p.state = PlayerBase.States.LockPick;
                p.input.state = PlayerBase.States.LockPick;
                //Debug.Log(("Set your fucking state to lock pick"));
            }
            else
            {
                startInteractionTime = smackStartTime;
                endInteractionTime = smackEndTime;
                p.state = PlayerBase.States.Smack;
                p.input.state = PlayerBase.States.Smack;   
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
