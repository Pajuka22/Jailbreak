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
    public Material alertedMaterial;
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
    public float alertWinTime = 1.5f;

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
    private void FixedUpdate()
    {
        float nearestPlayer = sightRange + 1;
        if (alive)
        {
            if (canMove)
            {
                if (PlayerBase.firstRun)
                {
                    time++;
                }
                else
                {
                    time--;
                }
                foreach (PlayerBase p in Players)
                {
                    if (p.enabled && CanSeePlayer(p))
                    {
                        if (nearestPlayer > Vector3.Distance(head.position, p.transform.position))
                        {
                            nearestPlayer = Vector3.Distance(head.position, p.transform.position);
                            alertedPlayer = p;
                            suspicion++;
                        }
                        if ((p.transform.position - head.position).magnitude <= autoAlertRange)
                        {
                            currentState = EnemyStates.Alerted;
                        }
                        else if(currentState != EnemyStates.Alerted)
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
                        if (Vector3.Distance(navAgent.destination, transform.position) <= destinationAcceptanceRadius)
                        {
                            currentCoroutine = StartCoroutine(StopAtPatrolPoint());
                        }
                        break;
                    case EnemyStates.Suspicious:
                        navAgent.speed = 2.5f;
                        navAgent.angularSpeed = 30;
                        if (currentCoroutine != null)
                        {
                            StopCoroutine(currentCoroutine);
                            currentCoroutine = null;
                        }
                        rend.sharedMaterial = suspiciousMaterial;

                        if (CanSeePlayer(alertedPlayer))
                        {
                            navAgent.destination = alertedPlayer.transform.position;
                            if (suspicion >= framesToCatch)
                            {
                                currentState = EnemyStates.Alerted;
                            }
                        }
                        else
                        {
                            if ((navAgent.destination - transform.position).magnitude <= destinationAcceptanceRadius)
                            {
                                suspicion = 0;
                                currentState = EnemyStates.Idle;
                            }
                        }
                        break;
                    case EnemyStates.Alerted:
                        rend.sharedMaterial = alertedMaterial;
                        if (!PlayerBase.firstRun)
                        {
                            navAgent.speed = 0;
                            Invoke("Win", alertWinTime);
                        }
                        else
                        {
                            navAgent.enabled = false;
                        }
                        break;
                }
            }
        }
        else
        {
            rend.sharedMaterial = alertedMaterial;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (Utility.Has<PlayerBase>(collision.collider.gameObject))
        {
            alertedPlayer = collision.collider.gameObject.GetComponent<PlayerBase>();
            currentState = EnemyStates.Alerted;
            transform.rotation = Quaternion.LookRotation(alertedPlayer.transform.position, Vector3.up);
        }
    }
    public override void Interact(PlayerBase playerBase)
    {
        if (alive)
        {
            rb.isKinematic = false;
            navAgent.enabled = false;
            alive = false;
            lineRenderer.enabled = false;
            enabled = false;
            rend.sharedMaterial = alertedMaterial;
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
        if (alive)
        {
            canMove = false;
            navAgent.destination = transform.position;
            navAgent.speed = 0;
            navAgent.acceleration = 10000000000;
            alertedPlayer.canMove = false;
            GameObject.FindObjectOfType<GameManager>().GameOver(alertedPlayer);
        }
    }
    public bool CanSeePlayer(PlayerBase p)
    {
        if (p != null)
        {
            return (p.transform.position - head.position).magnitude <= sightRange
                && VectorMath.RadiansToVector(p.transform.position - head.position, head.forward) <= Mathf.Deg2Rad * coneAngle
                && !Physics.Linecast(head.position, p.transform.position + Vector3.up, blocksSight);
        }
        return false;
    }
    public override void InteractionReset()
    {
        rb.isKinematic = true;
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
        yield return new WaitForSeconds(startInteractionTime);
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
    public override IEnumerator InteractRoutine(PlayerBase p)
    {
        canMove = false;
        navAgent.destination = transform.position;
        navAgent.speed = 0;
        navAgent.acceleration = 10000000000;
        //Debug.Log("StopMoving");
        return base.InteractRoutine(p);
    }
}
