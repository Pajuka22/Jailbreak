using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(RagdollController))]
public class EnemyRedoAttempt : InteractionParent
{
    private Enemy.EnemyStates state = Enemy.EnemyStates.Idle;
    public enum AlertType { None, Sound, Corpse, Player}
    AlertType alert;
    public PlayerBase alertedPlayer;
    float soundSus;
    float playerSus;
    float bodySus;
    Vector3 destination;
    // Start is called before the first frame update

    protected override void Start()
    {

    }
    private void FixedUpdate()
    {
        switch (state)
        {
            case Enemy.EnemyStates.Idle:
                // idle stuff
                alert = AlertType.None;
                if(soundSus > 0)
                {
                    alert = AlertType.Sound;

                }
                if(bodySus > 0)
                {
                    alert = AlertType.Corpse;
                }
                if(playerSus > 0)
                {
                    alert = AlertType.Player;
                }
                break;
            case Enemy.EnemyStates.Suspicious:
                break;
            case Enemy.EnemyStates.Look:
                break;
            case Enemy.EnemyStates.Alerted:
                break;
        }
    }
    public void HearSound(SoundUtility.Sound sound)
    {
        if (Vector3.Distance(transform.position, sound.location) <= sound.maxRadius)
        {
            if (alertedPlayer == null)
            {
                NavMeshPath path = new NavMeshPath();
                Utility.GetPath(path, transform.position, sound.location, NavMesh.AllAreas);
                float pathLength = SoundUtility.GetPathLength(path);
                Debug.Log("nav distance to sound: " + pathLength);
                if (pathLength <= sound.maxRadius)
                {

                }
            }
        }
    }
}