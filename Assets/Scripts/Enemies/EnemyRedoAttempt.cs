using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(RagdollController))]
public class EnemyRedoAttempt : InteractRedo
{
    bool alive = true;
    public RagdollController ragdoll;
    public enum States { Idle, Suspicious, Alert, Look}
    public States state;
    //line of sight
    public Transform head;
    [Range(0, 180)]
    public float sightAngle;
    [Min(0)]
    public float sightRange;
    public LineRenderer lineRenderer;
    public int linesInCurve;
    //patrol stuff
    public NavMeshAgent navAgent;
    public List<Transform> patrolPoints = new List<Transform>();
    public int pPoint;

    //on start
    Vector3 startingPos;
    Quaternion startingRot;
    PlayerBase[] players = new PlayerBase[0];

    public Animator anim;

    private void Start()
    {
        startingPos = transform.position;
        startingRot = transform.rotation;
        players = FindObjectsOfType<PlayerBase>();
    }
    private void FixedUpdate()
    {
        if (alive)
        {
            float nearestPlayer = sightRange + 1;
            foreach(PlayerBase p in players)
            {
                if((p.transform.position - transform.position).magnitude <= sightRange)
                {

                }
            }

        }
        else
        {
            transform.position = head.position;
        }
    }

}