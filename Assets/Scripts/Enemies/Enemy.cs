using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(RagdollController))]
public class Enemy : InteractionParent
{
    [SerializeField]
    private float startSmackTime;
    [SerializeField]
    private float endSmackTime;
    [SerializeField]
    private float startPickUpTime;
    [SerializeField]
    private float endPickUpTime;
    [SerializeField]
    private static bool PlayerLost;
    [SerializeField]
    private Vector3 startingPoint;
    [SerializeField]
    private Quaternion startingRot;
    //movement and stuff
    NavMeshAgent navAgent;
    private bool canMove = true;
    [SerializeField]
    private List<Transform> patrolPoints;
    [SerializeField]
    private int currentPatrolPoint;
    [SerializeField]
    private float patrolPointStopTime;
    [SerializeField]
    private float destinationAcceptanceRadius = 2;
    //visible changes
    [SerializeField]
    private Renderer rend;
    [SerializeField]
    private Material baseMaterial;
    [SerializeField]
    private Material suspiciousMaterial;
    [SerializeField]
    private Material alertedMaterial;
    //end visible changes
    //line of sight
    [SerializeField]
    private LineRenderer lineRenderer;
    [SerializeField]
    private int pointsInArc;
    public Transform head;
    [Range(0, 90)] 
    [SerializeField]
    private float sightAngle;
    [Range(0, 90)]
    [SerializeField]
    private float alarmedSightAngle;
    [SerializeField]
    private float sightRange;
    [SerializeField]
    private float alarmedSightRange;
    [SerializeField]
    private float autoAlertRange;
    [SerializeField]
    private float alarmedAutoAlertRange;
    private float cSightAngle;
    private float cSightRange;
    private float cAutoAlertRange;
    //end line of sight
    //player stuff
    public PlayerBase[] Players = new PlayerBase[2];
    [System.NonSerialized]
    public bool alive = true;
    [SerializeField]
    private LayerMask blocksSight;
    [SerializeField]
    private PlayerBase alertedPlayer;
    //end player stuff
    [SerializeField]
    private float alertWinTime = 1.5f;
    

    public enum States { Idle, Suspicious, Alerted, Look}
    public enum AnimStates { IdleWalkRun, Scan, Whistle}
    AnimStates animState;
    public enum AlertType {None, Sound, Corpse, Player }
    [SerializeField]
    private States state;
    [SerializeField]
    private AlertType alert;
    public int framesToCatch = 60;
    private Coroutine interruptableCoroutine;
    float playerSus;
    private RagdollController ragdoll;
    bool held;
    public CharacterJoint headGrab;
    public static List<Enemy> allEnemies = new List<Enemy>();
    public static List<Enemy> deadEnemies = new List<Enemy>();
    private  List<Enemy> myDeadEnemies = new List<Enemy>();
    private Enemy alertedCorpse;
    public static bool winning;
    float soundSus;
    private float corpseSus;
    [SerializeField]
    private Animator anim;
    private float suspicion;

    // Start is called before the first frame update

    protected override void Start()
    {
        EventManager.current.hearSound += HearSound;
        EventManager.current.soundAlarm += GetAlarmed;
        if(anim == null)
        {
            anim = GetComponent<Animator>();
        }
        if (headGrab == null)
        {
            headGrab = head.GetComponent<CharacterJoint>();
        }
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
        cSightRange = sightRange;
        cSightAngle = sightAngle;
        cAutoAlertRange = autoAlertRange;
        ResetSightCone();
        if (patrolPoints[1] != null)
        {
            navAgent.destination = patrolPoints[0].transform.position;
            currentPatrolPoint = 0;
        }
        base.Start();
    }
    void GetAlarmed()
    {
        if (alive)
        {
            cSightAngle = alarmedSightAngle;
            cSightRange = alarmedSightRange;
            cAutoAlertRange = alarmedAutoAlertRange;
            ResetSightCone();
        }
    }
    private void FixedUpdate()
    {
        if (alive)
        {
            float nearestPlayer = cSightRange + 1;
            foreach (PlayerBase p in Players)
            {
                if (p != null && p.enabled && CanSeePlayer(p))
                {
                    if (nearestPlayer > Vector3.Distance(head.position, p.transform.position))
                    {
                        nearestPlayer = Vector3.Distance(head.position, p.transform.position);
                        alertedPlayer = p;
                        playerSus++;
                        InterruptCoroutine();
                    }
                    if ((p.transform.position - head.position).magnitude <= cAutoAlertRange || playerSus >= framesToCatch)
                    {
                        state = States.Alerted;
                        navAgent.destination = transform.position;
                    }
                    else if (state != States.Alerted)
                    {
                        canMove = true;
                        state = States.Suspicious;
                        navAgent.destination = p.transform.position;
                    }
                }
            }
            if (alertedPlayer == null)
            {
                float nearestDeadEnemy = cSightRange;
                foreach (Enemy e in deadEnemies)
                {
                    if (!myDeadEnemies.Contains(e) && CanSeePosition(e.head.position))
                    {
                        if (Vector3.Distance(head.position, e.head.position) < nearestDeadEnemy)
                        {
                            myDeadEnemies.Add(e);
                            alertedCorpse = e;
                            state = States.Suspicious;
                            navAgent.destination = e.head.position;
                        }
                    }
                }
            }
            switch (state)
            {
                case States.Idle:
                    rend.sharedMaterial = baseMaterial;
                    animState = AnimStates.IdleWalkRun;
                    // idle stuff
                    alert = AlertType.None;
                    if (soundSus > 0)
                    {
                        Debug.Log("sensed sound");
                        alert = AlertType.Sound;
                        state = States.Suspicious;
                        InterruptCoroutine();
                    }
                    if (corpseSus > 0)
                    {
                        Debug.Log("sensed corpse");
                        alert = AlertType.Corpse;
                        state = States.Suspicious;
                        InterruptCoroutine();
                    }
                    if (playerSus > 0)
                    {
                        Debug.Log("sensed player");
                        alert = AlertType.Player;
                        state = States.Suspicious;
                        InterruptCoroutine();
                    }
                    navAgent.speed = 3.5f;
                    navAgent.angularSpeed = 120;
                    if (patrolPoints[currentPatrolPoint] != null)
                    {
                        navAgent.destination = patrolPoints[currentPatrolPoint].position;
                    }
                    rend.sharedMaterial = baseMaterial;
                    if (Vector3.Distance(transform.position, navAgent.destination) < destinationAcceptanceRadius)
                    {
                        animState = AnimStates.Scan;
                        if (interruptableCoroutine == null)
                        {
                            interruptableCoroutine = StartCoroutine(StopAtPatrolPoint());
                        }
                    }
                    break;
                case States.Suspicious:
                    animState = AnimStates.IdleWalkRun;
                    rend.sharedMaterial = suspiciousMaterial;
                    if (alertedPlayer != null)//CanSeePlayer(alertedPlayer)
                    {
                        //playerSus++;
                        //navAgent.destination = alertedPlayer.transform.position;
                        ForgetSound();
                        ForgetCorpse();
                    }
                    else if (alertedCorpse != null)
                    {
                        corpseSus++;
                        //navAgent.destination = alertedCorpse.transform.position;
                    }
                    alert = playerSus > corpseSus ? (playerSus > soundSus ? AlertType.Player : AlertType.Sound) : (corpseSus > soundSus ? AlertType.Corpse : AlertType.Sound);
                    Debug.Log(alert);
                    switch (alert)
                    {
                        case AlertType.None:
                            playerSus = 0;
                            corpseSus = 0;
                            soundSus = 0;
                            state = States.Idle;
                            break;
                        case AlertType.Sound:
                            playerSus = soundSus;
                            corpseSus = 0;
                            if (soundSus == 0)
                            {
                                alert = AlertType.None;
                            }
                            break;
                        case AlertType.Corpse:
                            playerSus = 0;
                            soundSus = 0;
                            if (corpseSus == 0)
                            {
                                alert = AlertType.None;
                            }
                            break;
                        case AlertType.Player:
                            corpseSus = 0;
                            soundSus = 0;
                            if (playerSus == 0)
                            {
                                alert = AlertType.None;
                            }
                            break;
                    }
                    if ((transform.position - navAgent.destination).magnitude <= destinationAcceptanceRadius)
                    {
                        state = States.Look;
                    }
                    break;
                case States.Look:
                    navAgent.destination = transform.position;
                    animState = AnimStates.Scan;
                    if (interruptableCoroutine == null)
                    {
                        interruptableCoroutine = StartCoroutine(LookAround());
                    }
                    break;
                case States.Alerted:
                    animState = AnimStates.Whistle;
                    rend.sharedMaterial = alertedMaterial;
                    if (!PlayerBase.firstRun && !winning)
                    {/*
                        navAgent.speed = 0;
                        Invoke("Win", alertWinTime);*/
                    }
                    else
                    {
                        //navAgent.enabled = false;
                    }
                    break;
            }
            Debug.Log(state);
            anim.SetInteger("State", (int)animState);
            anim.SetFloat("Speed", navAgent.velocity.magnitude / 3.5f);
            /*if (canMove)
            {
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
                            state = States.Alerted;
                        }
                        else if(state != States.Alerted)
                        {
                            canMove = true;
                            state = States.Suspicious;
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
                                alertedCorpse = e;
                                state = States.Suspicious;
                                navAgent.destination = e.head.position;
                            }
                        }
                    }
                }
                switch (state)
                {
                    case States.Idle:
                        navAgent.speed = 3.5f;
                        navAgent.angularSpeed = 120;
                        rend.sharedMaterial = baseMaterial;
                        if (Vector3.Distance(navAgent.destination, transform.position) <= destinationAcceptanceRadius)
                        {
                            currentCoroutine = StartCoroutine(StopAtPatrolPoint());
                        }
                        break;
                    case States.Suspicious:
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
                                    state = States.Alerted;
                                }
                            }
                            else
                            {
                                if ((navAgent.destination - transform.position).magnitude <= destinationAcceptanceRadius)
                                {
                                    suspicion = 0;
                                    //state = States.Look;
                                    StartCoroutine(LookAround());
                                }
                            }
                        }
                        else
                        {
                            if(alertedCorpse != null)
                            {
                                if ((navAgent.destination - transform.position).magnitude <= destinationAcceptanceRadius)
                                {
                                    suspicion = 0;
                                    StartCoroutine(LookAround());
                                }
                            }
                        }
                        break;
                    case States.Alerted:
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
                        Debug.Log(state);
                        break;
                }
            }*/
        }
        else
        {
            transform.position = head.transform.position;
        }
    }
    public IEnumerator StopAtPatrolPoint()
    {
        if (alive)
        {
            yield return new WaitForSeconds(patrolPointStopTime);
            currentPatrolPoint++;
            if (currentPatrolPoint >= patrolPoints.Count)
            {
                currentPatrolPoint = 0;
            }
            if (alive)
            {
                if (patrolPoints[currentPatrolPoint] != null)
                {
                    navAgent.destination = patrolPoints[currentPatrolPoint].position;
                }
                canMove = true;
            }
            interruptableCoroutine = null;
        }
    }
    public IEnumerator LookAround()
    {
        canMove = false;
        state = States.Look;
        yield return new WaitForSeconds(1.625f);
        suspicion = 0;
        ForgetPlayer();
        ForgetCorpse();
        ForgetSound();
        canMove = true;
        alertedCorpse = null;
        alertedPlayer = null;
        state = States.Idle;
        interruptableCoroutine = null;
    }
    public bool CanSeePlayer(PlayerBase p)
    {
        if (p != null)
        {
            return (p.head.position - head.position).magnitude <= cSightRange
                && VectorMath.RadiansToVector(p.head.position - head.position, head.forward) <= Mathf.Deg2Rad * cSightAngle
                && !Physics.Linecast(head.position, p.head.position, blocksSight);
        }
        return false;
    }
    public bool CanSeePosition(Vector3 pos)
    {
        return (pos - head.position).magnitude <= cSightRange &&
            VectorMath.RadiansToVector(pos - head.position, head.forward) <= Mathf.Deg2Rad * cSightAngle &&
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
        //head.gameObject.SetActive(false);
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
        alive = true;
        winning = false;

        deadEnemies.Clear();
        myDeadEnemies.Clear();
        alertedPlayer = null;

        head.gameObject.SetActive(true);
        held = false;
        headGrab.connectedBody = null;
        headGrab.gameObject.SetActive(false);

        state = States.Idle;
        currentPatrolPoint = 0;
        suspicion = 0;
        playerSus = 0;
        soundSus = 0;
        corpseSus = 0;
        transform.position = startingPoint;
        transform.rotation = startingRot;
        
        currentPatrolPoint = 0;
        navAgent.enabled = true;
        navAgent.destination = patrolPoints[currentPatrolPoint].position;
        
        rend.sharedMaterial = baseMaterial;
        
        enabled = true;
        canMove = true;
        ragdoll.TurnRagdollOff();
        cSightAngle = sightAngle;
        cSightRange = sightRange;
        cAutoAlertRange = autoAlertRange;
        ResetSightCone();
        GetComponent<CapsuleCollider>().enabled = true;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (Utility.Has<PlayerBase>(collision.collider.gameObject))
        {
            alertedPlayer = collision.collider.gameObject.GetComponent<PlayerBase>();
            state = States.Alerted;
            transform.rotation = Quaternion.LookRotation(alertedPlayer.transform.position, Vector3.up);
        }
    }
    void Win()
    {
        if (alive && !winning && !PlayerBase.firstRun)
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
            GameManager.current.GameOver(alertedPlayer);
            winning = true;
        }
    }
    void ResetSightCone()
    {
        float coneRad = cSightAngle * Mathf.PI / 180;
        float baseRadius = cSightRange * Mathf.Sin(coneRad);
        //lineRenderer.positionCount = 26 + 2 * pointsInArc;
        List<Vector3> points = new List<Vector3>();
        points.Add(new Vector3(0, 0, cSightRange));
        points.Add(Vector3.zero);
        //points.Add(sightRange * new Vector3(Mathf.Sin(coneRad), Mathf.Cos(coneRad), 0));
        for (int i = 5; i >= -5; i--)
        {
            points.Add(cSightRange * new Vector3(Mathf.Sin(coneRad * i / 5), 0, Mathf.Cos(coneRad * i / 5)));
        }
        points.Add(Vector3.zero);

        for (int i = 5; i >= -5; i--)
        {
            points.Add(cSightRange * new Vector3(0, Mathf.Sin(coneRad * i / 5), Mathf.Cos(coneRad * i / 5)));
        }
        for (int i = 0; i <= 20; i++)
        {
            points.Add(new Vector3(baseRadius * Mathf.Sin(2 * Mathf.PI * i / 20 + Mathf.PI), baseRadius * Mathf.Cos(2 * Mathf.PI * i / 20 + Mathf.PI), cSightRange * Mathf.Cos(coneRad)));
        }
        points.Add(Vector3.zero);
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
        lineRenderer.enabled = true;
    }
    public void HearSound(SoundUtility.Sound sound)
    {
        if (alive)
        {
            if (Vector3.Distance(transform.position, sound.location) <= sound.maxRadius)
            {
                NavMeshPath path = new NavMeshPath();
                Utility.GetPath(path, transform.position, sound.location, NavMesh.AllAreas);
                float pathLength = SoundUtility.GetPathLength(path);
                Debug.Log("nav distance to sound: " + pathLength);
                if (pathLength <= sound.maxRadius)
                {
                    soundSus = sound.strength / pathLength;
                    Debug.Log(soundSus);
                    if (soundSus > playerSus && soundSus > corpseSus)
                    {
                        ForgetCorpse();
                        ForgetPlayer();
                        navAgent.SetDestination(sound.location);
                        state = States.Suspicious;
                        alert = AlertType.Sound;
                    }
                }
            }
        }
    }
    public void ForgetCorpse()
    {
        if (alertedCorpse != null && !myDeadEnemies.Contains(alertedCorpse))
        {
            myDeadEnemies.Add(alertedCorpse);
        }
        alertedCorpse = null;
        corpseSus = 0;
    }
    public void ForgetPlayer()
    {
        alertedPlayer = null;
        playerSus = 0;
    }
    public void ForgetSound()
    {
        soundSus = 0;
    }
    void InterruptCoroutine()
    {
        if(interruptableCoroutine != null)
        {
            StopCoroutine(interruptableCoroutine);
            interruptableCoroutine = null;
        }
    }
}