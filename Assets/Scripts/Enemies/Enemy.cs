using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(RagdollController))]
public class Enemy : InteractionParent
{
    public float startSmackTime;
    public float endSmackTime;
    public float startPickUpTime;
    public float endPickUpTime;
    public static bool PlayerLost;
    public Vector3 startingPoint;
    public Quaternion startingRot;
    //movement and stuff
    NavMeshAgent navAgent;
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
    public float sightAngle;
    public float sightRange;
    public float autoAlertRange;
    //end line of sight
    //player stuff
    public PlayerBase[] Players = new PlayerBase[2];
    [System.NonSerialized]
    public bool alive = true;
    public LayerMask blocksSight;
    public PlayerBase alertedPlayer;
    //end player stuff
    public float alertWinTime = 1.5f;
    

    public enum EnemyStates { Idle, Suspicious, Alerted, Look}
    public EnemyStates ghostState;
    public EnemyStates currentState;
    bool alerted;
    float time;
    public int framesToCatch = 60;
    private Coroutine currentCoroutine;
    int suspicion;
    private RagdollController ragdoll;
    bool held;
    CharacterJoint headGrab;
    public static List<Enemy> allEnemies = new List<Enemy>();
    public static List<Enemy> deadEnemies = new List<Enemy>();
    public List<Enemy> myDeadEnemies = new List<Enemy>();
    private Enemy deadEnemySpotted;
    public static bool winning;
    // Start is called before the first frame update

    protected override void Start()
    {
        headGrab = head.GetComponent<CharacterJoint>();
        ragdoll = GetComponent<RagdollController>();
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
        if (navAgent == null)
        {
            navAgent = GetComponent<NavMeshAgent>();
        }
        ResetSightCone();
        navAgent.destination = patrolPoints[0].transform.position;
        currentPatrolPoint = 0;
        base.Start();
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
                    if (p != null && p.enabled && CanSeePlayer(p))
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
                            canMove = true;
                            currentState = EnemyStates.Suspicious;
                            navAgent.destination = p.transform.position;
                        }
                    }
                }
                if(alertedPlayer == null)
                {
                    float nearestDeadEnemy = sightRange;
                    foreach (Enemy e in deadEnemies)
                    {
                        if (!myDeadEnemies.Contains(e) && CanSeePosition(e.head.position))
                        {
                            if(Vector3.Distance(head.position, e.head.position) < nearestDeadEnemy)
                            {
                                myDeadEnemies.Add(e);
                                deadEnemySpotted = e;
                                currentState = EnemyStates.Suspicious;
                                navAgent.destination = e.head.position;
                            }
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
                        if (alertedPlayer != null)
                        {
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
                                    //currentState = EnemyStates.Look;
                                    StartCoroutine(LookAround());
                                }
                            }
                        }
                        else
                        {
                            if(deadEnemySpotted != null)
                            {
                                if ((navAgent.destination - transform.position).magnitude <= destinationAcceptanceRadius)
                                {
                                    suspicion = 0;
                                    StartCoroutine(LookAround());
                                }
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
                    default:
                        Debug.Log(currentState);
                        break;
                }
            }
        }
        else
        {
        }
    }
    public IEnumerator StopAtPatrolPoint()
    {
        if (alive)
        {
            canMove = false;
            yield return new WaitForSeconds(patrolPointStopTime);
            currentPatrolPoint++;
            if (currentPatrolPoint >= patrolPoints.Count)
            {
                currentPatrolPoint = 0;
            }
            if (alive)
            {
                navAgent.destination = patrolPoints[currentPatrolPoint].position;
                canMove = true;
            }
        }
    }
    public IEnumerator LookAround()
    {
        canMove = false;
        currentState = EnemyStates.Look;
        yield return new WaitForSeconds(1.625f);
        suspicion = 0;
        canMove = true;
        deadEnemySpotted = null;
        alertedPlayer = null;
        currentState = EnemyStates.Idle;
    }
    public bool CanSeePlayer(PlayerBase p)
    {
        if (p != null)
        {
            return (p.head.position - head.position).magnitude <= sightRange
                && VectorMath.RadiansToVector(p.head.position - head.position, head.forward) <= Mathf.Deg2Rad * sightAngle
                && !Physics.Linecast(head.position, p.head.position, blocksSight);
        }
        return false;
    }
    public bool CanSeePosition(Vector3 pos)
    {
        return (pos - head.position).magnitude <= sightRange &&
            VectorMath.RadiansToVector(pos - head.position, head.forward) <= Mathf.Deg2Rad * sightAngle &&
            !Physics.Linecast(head.position, pos, blocksSight);
    }
    public override void Interact(PlayerBase playerBase)
    {
        if (alive)
        {
            Die();
        }
        else
        {
            if (!held)
            {
                PickUp(playerBase);
            }
            /*
            else
            {
                Drop(playerBase);
            }
            held = !held;
            */
        }
    }
    public void Die()
    {
        deadEnemies.Add(this);
        head.gameObject.SetActive(false);
        navAgent.enabled = false;
        alive = false;
        lineRenderer.enabled = false;
        enabled = false;
        rend.sharedMaterial = alertedMaterial;
        GetComponent<CapsuleCollider>().enabled = false;
        enabled = false;
        ragdoll.TurnRagdollOn();
    }
    public override void PickUp(PlayerBase p)
    {
        headGrab.gameObject.SetActive(true);
        headGrab.connectedBody = p.leftHand.GetComponent<Rigidbody>();
        headGrab.connectedAnchor = Vector3.zero;
        p.holding = this;
        p.encumbered = true;
        Physics.IgnoreLayerCollision(this.gameObject.layer, 8);
    }
    public override void Drop(PlayerBase p)
    {
        p.encumbered = false;
        headGrab.gameObject.SetActive(false);
        headGrab.connectedBody = null;
        p.holding = null;
        Physics.IgnoreLayerCollision(gameObject.layer, 8, false);
    }
    public override IEnumerator InteractRoutine(PlayerBase p)
    {
        canMove = false;
        if (navAgent.enabled)
        {
            navAgent.destination = transform.position;
            navAgent.speed = 0;
            navAgent.acceleration = 10000000000;
        }
        if (alive)
        {
            startInteractionTime = startSmackTime;
            endInteractionTime = endSmackTime;
            deadEnemies.Add(this);
        }
        else
        {
            startInteractionTime = startPickUpTime;
            endInteractionTime = endPickUpTime;
        }
        //start base stuff but slightly edited
        p.canMove = false;
        p.input.velocity = Vector3.zero;
        p.rb.velocity = Vector3.zero;
        p.transform.rotation = Quaternion.LookRotation((head.position - p.transform.position) - Vector3.up * (head.position.y - p.transform.position.y));
        p.input.rotation = p.transform.rotation;
        p.state = PlayerBase.States.Idle;
        p.input.state = PlayerBase.States.Idle;
        if (p.holding != null && emptyHands)
        {
            Debug.Log("Drop the fucking body");
            p.holding.Drop(p);
        }
        if (doesItFuckingMatter)
        {
            if (p.canPickLocks)
            {
                picked = true;
                p.state = PlayerBase.States.LockPick;
                p.input.state = PlayerBase.States.LockPick;
            }
            else
            {
                if (alive)
                {
                    p.state = PlayerBase.States.Smack;
                    p.input.state = PlayerBase.States.Smack;
                    smacked = true;
                }
                else
                {
                    p.state = PlayerBase.States.PickUp;
                    p.input.state = PlayerBase.States.PickUp;
                }
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
    public override void InteractionReset()
    {
        deadEnemies.Clear();
        head.gameObject.SetActive(true);
        held = false;
        headGrab.connectedBody = null;
        currentPatrolPoint = 0;
        suspicion = 0;
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
        canMove = true;
        ragdoll.TurnRagdollOff();
        ResetSightCone();
        GetComponent<CapsuleCollider>().enabled = true;
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
    void Win()
    {
        if (alive && !winning)
        {
            canMove = false;
            navAgent.destination = transform.position;
            navAgent.speed = 0;
            navAgent.acceleration = 10000000000;
            alertedPlayer.canMove = false;
            alertedPlayer.rb.velocity = Vector3.zero;
            alertedPlayer.otherPlayer.rb.velocity = Vector3.zero;
            alertedPlayer.otherPlayer.canMove = false;
            alertedPlayer.anim.SetInteger("state", 0);
            alertedPlayer.otherPlayer.anim.SetInteger("state", 0);
            FindObjectOfType<GameManager>().GameOver(alertedPlayer);
        }
    }
    void ResetSightCone()
    {
        float coneRad = sightAngle * Mathf.PI / 180;
        float baseRadius = sightRange * Mathf.Sin(coneRad);
        //lineRenderer.positionCount = 26 + 2 * pointsInArc;
        List<Vector3> points = new List<Vector3>();
        points.Add(new Vector3(0, 0, sightRange));
        points.Add(Vector3.zero);
        //points.Add(sightRange * new Vector3(Mathf.Sin(coneRad), Mathf.Cos(coneRad), 0));
        for (int i = 5; i >= -5; i--)
        {
            points.Add(sightRange * new Vector3(Mathf.Sin(coneRad * i / 5), 0, Mathf.Cos(coneRad * i / 5)));
        }
        points.Add(Vector3.zero);

        for (int i = 5; i >= -5; i--)
        {
            points.Add(sightRange * new Vector3(0, Mathf.Sin(coneRad * i / 5), Mathf.Cos(coneRad * i / 5)));
        }
        for (int i = 0; i <= 20; i++)
        {
            points.Add(new Vector3(baseRadius * Mathf.Sin(2 * Mathf.PI * i / 20 + Mathf.PI), baseRadius * Mathf.Cos(2 * Mathf.PI * i / 20 + Mathf.PI), sightRange * Mathf.Cos(coneRad)));
        }
        points.Add(Vector3.zero);
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
        lineRenderer.enabled = true;
    }
}