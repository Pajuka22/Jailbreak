using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class Enemy : InteractionParent
{
    public static bool PlayerLost;
    public Vector3 startingPoint;
    public Quaternion startingRot;
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
    public int framesToCatch = 60;
    private Coroutine currentCoroutine;
    int suspicion;
    // Start is called before the first frame update
    
    void Start()
    {
        if(head == null)
        {
            head = transform;
        }
        shouldPickLocks = false;
        startingPoint = transform.position;
        startingRot = transform.rotation;
        if(rend == null)
        {
            rend = GetComponent<Renderer>();
        }
        navAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        ResetSightCone();
        navAgent.destination = patrolPoints[0].transform.position;
        currentPatrolPoint = 0;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //InteractionReset();
        }
    }
    // Update is called once per frame
    /*void Update()
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
                if (!(PlayerLost || PlayerBase.firstRun))
                {
                    Win();
                    PlayerLost = true;
                }
                break;
        }
        if (alive)
        {
            foreach (PlayerBase p in Players)
            {
                if (CanSeePlayer(p))
                {
                    if((p.transform.position - transform.position).magnitude < autoAlertRange){
                        Debug.Log("alerted");
                        currentState = EnemyStates.Alerted;
                    }
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
                            currentState = EnemyStates.Alerted;
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            InteractionReset();
        }
    }*/
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
        float nearestPlayer = sightRange + 1;
        if (alive)
        {
            foreach (PlayerBase p in Players)
            {
                if (p.enabled && CanSeePlayer(p))
                {
                    if (nearestPlayer > Vector3.Distance(head.position, p.transform.position))
                    {
                        nearestPlayer = Vector3.Distance(head.position, p.transform.position);
                        alertedPlayer = p;
                    }
                    if ((p.transform.position - head.position).magnitude <= autoAlertRange)
                    {
                        currentState = EnemyStates.Alerted;
                    }
                    else
                    {
                        currentState = EnemyStates.Suspicious;
                        navAgent.destination = p.transform.position;
                    }
                }
            }
            switch (currentState)
            {
                case EnemyStates.Idle:
                    navAgent.speed = 3.5f;
                    navAgent.angularSpeed = 120;
                    rend.sharedMaterial = baseMaterial;
                    if(Vector3.Distance(navAgent.destination, transform.position) <= destinationAcceptanceRadius)
                    {
                        currentCoroutine = StartCoroutine(StopAtPatrolPoint());
                    }
                    break;
                case EnemyStates.Suspicious:
                    navAgent.speed = 2.5f;
                    navAgent.angularSpeed = 30;
                    if(currentCoroutine != null)
                    {
                        StopCoroutine(currentCoroutine);
                        currentCoroutine = null;
                    }
                    rend.sharedMaterial = suspiciousMaterial;
                    
                    if (CanSeePlayer(alertedPlayer))
                    {
                        navAgent.destination = alertedPlayer.transform.position;
                        suspicion++;
                        if(suspicion >= framesToCatch)
                        {
                            currentState = EnemyStates.Alerted;
                        }
                    }
                    else
                    {
                        if((navAgent.destination - transform.position).magnitude <= destinationAcceptanceRadius)
                        {
                            suspicion = 0;
                            currentState = EnemyStates.Idle;
                        }
                    }
                    break;
                case EnemyStates.Alerted:
                    rend.sharedMaterial = caughtMaterial;
                    if (!PlayerBase.firstRun)
                    {
                        Win();
                    }
                    else
                    {
                        navAgent.enabled = false;
                    }
                    break;
            }
        }
        else
        {
            rend.sharedMaterial = caughtMaterial;
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
    void Win()
    {
        alertedPlayer.canMove = false;
        GameObject.FindObjectOfType<GameManager>().GameOver(alertedPlayer);
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
        transform.position = startingPoint;
        transform.rotation = startingRot;
        alive = true;
        currentPatrolPoint = 0;
        navAgent.enabled = true;
        navAgent.destination = patrolPoints[currentPatrolPoint].position;
        currentState = EnemyStates.Idle;
        rend.sharedMaterial = baseMaterial;
        alertedPlayer = null;
        enabled = true;
    }
    public IEnumerator Die()
    {
        rb.isKinematic = false;
        navAgent.enabled = false;
        alive = false;
        yield return new WaitForSeconds(interactionTime);
        lineRenderer.enabled = false;
        enabled = false;
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
