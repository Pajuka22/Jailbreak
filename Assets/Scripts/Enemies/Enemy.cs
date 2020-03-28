using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class Enemy : InteractionParent
{
    public Transform startingPoint;
    //movement and stuff
    NavMeshAgent navAgent;
    Rigidbody rb;
    public bool canMove = true;
    public List<Transform> patrolPoints;
    public int currentPatrolPoint;
    public float patrolPointStopTime;
    public float destinationAcceptanceRadius = 2;
    //visible changes
    public Renderer rend;
    public Material baseMaterial;
    public Material suspiciousMaterial;
    public Material caughtMaterial;
    //end visible changes
    //line of sight
    public LineRenderer lineRenderer;
    public int pointsInArc;
    public Transform head;
    [Range(0, 90)]
    public float coneAngle;
    public float sightRange;
    public float autoAlertRange;
    //end line of sight
    //player stuff
    public PlayerBase[] Players = new PlayerBase[2];
    public bool alive = true;
    public LayerMask blocksSight;
    public PlayerBase alertedPlayer;
    //end player stuff
    public float timeToAlert = 1.5f;

    public enum EnemyStates { Idle, Suspicious, Alerted}
    public EnemyStates ghostState;
    public EnemyStates currentState;
    bool alerted;
    float time;
    bool waiting;
    // Start is called before the first frame update
    void Start()
    {
        shouldPickLocks = false;
        startingPoint = transform;
        if(rend == null)
        {
            rend = GetComponent<Renderer>();
        }
        navAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        ResetSightCone();
    }

    // Update is called once per frame
    void Update()
    {
        //navAgent.destination = patrolPoints[currentPatrolPoint].position;
        switch (currentState)
        {
            case EnemyStates.Idle:
                if (Vector3.Distance(transform.position, navAgent.destination) <= destinationAcceptanceRadius && !waiting)
                {
                    //Debug.Log("Done" + currentPatrolPoint);
                    StartCoroutine(StopAtPatrolPoint());
                }
                break;
            case EnemyStates.Suspicious:
                navAgent.destination = alertedPlayer.transform.position;
                break;
            case EnemyStates.Alerted:
                break;
        }
        if (alive)
        {
            foreach (PlayerBase p in Players)
            {
                if (CanSeePlayer(p))
                {
                    Debug.Log("i see you");
                    if ((p.transform.position - transform.position).magnitude > autoAlertRange && !alerted)
                    {
                        StartCoroutine(BecomeAlerted(p));
                    }
                    else
                    {
                        if (PlayerBase.firstRun)
                        {
                            rend.sharedMaterial = caughtMaterial;
                            ghostState = EnemyStates.Alerted;
                        }
                        else
                        {
                            currentState = EnemyStates.Alerted;
                        }
                    }
                }
                else
                {
                    Debug.Log("can't see you");
                }
            }
            
        }
    }
    private void FixedUpdate()
    {
        if (PlayerBase.firstRun)
        {
            time++;
        }
        else
        {
            time--;
        }
    }
    public override void Interact(PlayerBase playerBase)
    {
        if (alive)
        {
            StartCoroutine(Die());
        }
    }
    void ResetSightCone()
    {
        float coneRad = coneAngle * Mathf.PI / 180;
        float baseRadius = sightRange * Mathf.Sin(coneRad);
        //lineRenderer.positionCount = 26 + 2 * pointsInArc;
        List<Vector3> points = new List<Vector3>();
        points.Add(new Vector3(0, 0, sightRange));
        points.Add(Vector3.zero);
        //points.Add(sightRange * new Vector3(Mathf.Sin(coneRad), Mathf.Cos(coneRad), 0));
        for(int i = 5; i >= -5; i--)
        {
            points.Add(sightRange * new Vector3(Mathf.Sin(coneRad * i/5), 0, Mathf.Cos(coneRad * i/5)));
        }
        points.Add(Vector3.zero);
        
        for (int i = 5; i >= -5; i--)
        {
            points.Add(sightRange * new Vector3(0, Mathf.Sin(coneRad * i/5), Mathf.Cos(coneRad * i/5)));
        }
        for(int i = 0; i <= 20; i++)
        {
            points.Add(new Vector3(baseRadius * Mathf.Sin(2 * Mathf.PI * i/20 + Mathf.PI), baseRadius * Mathf.Cos(2 * Mathf.PI * i/20 + Mathf.PI), sightRange * Mathf.Cos(coneRad)));
        }
        points.Add(Vector3.zero);
        //Debug.Log(points.Count);
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }
    public bool CanSeePlayer(PlayerBase p)
    {
        if (p != null)
        {
            return (p.transform.position - head.position).magnitude <= sightRange
                && VectorMath.RadiansToVector(p.transform.position - head.position, head.forward) <= Mathf.Deg2Rad * coneAngle
                && !Physics.Linecast(head.position, p.transform.position, blocksSight);
        }
        return false;
    }
    public override void InteractionReset()
    {
        transform.position = startingPoint.position;
        transform.rotation = startingPoint.rotation;
        alive = true;
        currentPatrolPoint = 0;
        navAgent.destination = patrolPoints[currentPatrolPoint].position;
        currentState = EnemyStates.Idle;
        alertedPlayer = null;
        enabled = true;
        navAgent.enabled = true;
    }
    public IEnumerator Die()
    {
        rb.isKinematic = false;
        navAgent.enabled = false;
        yield return new WaitForSeconds(interactionTime);
        alive = false;
        lineRenderer.enabled = false;
        enabled = false;
    }
    public IEnumerator BecomeAlerted(PlayerBase player)
    {
        if (alive)
        {
            rend.sharedMaterial = suspiciousMaterial;
            alertedPlayer = player;
            yield return new WaitForSeconds(timeToAlert);
            if (CanSeePlayer(player))
            {
                if (PlayerBase.firstRun)
                {
                    rend.sharedMaterial = caughtMaterial;
                    ghostState = EnemyStates.Alerted;
                    alerted = true;
                }
                else
                {
                    currentState = EnemyStates.Alerted;
                }
            }
            else if (!alerted)
            {
                currentState = EnemyStates.Idle;
                rend.sharedMaterial = baseMaterial;
            }
        }
    }
    public IEnumerator StopAtPatrolPoint()
    {
        if (alive)
        {
            waiting = true;
            yield return new WaitForSeconds(patrolPointStopTime);
            currentPatrolPoint++;
            if (currentPatrolPoint >= patrolPoints.Count)
            {
                currentPatrolPoint = 0;
            }
            //Debug.Log(currentPatrolPoint);
            navAgent.destination = patrolPoints[currentPatrolPoint].position;
            waiting = false;
        }
    }

}
