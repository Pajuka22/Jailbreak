using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerBase : MonoBehaviour
{
    public PlayerBase otherPlayer;
    public static bool firstRun = true;
    public bool canMove = true;
    private bool canMoveCache = true;
    protected struct InputData
    {
        public Vector3 direction;
        public bool interact;
        //asdfasdf
    }
    private Rigidbody rb;

    //movement speed
    public float sneakSpeed;
    public float speed;
    public float encumberedSpeed;

    //not even sure this is necessary
    public float strength;

    //interaction
    public float secondsForInteractionStart;
    public float secondsForInteractionEnd;
    public bool canPickLocks;
    [System.NonSerialized]
    public InteractionParent interactable;
    private List<Collider> interactableTriggers = new List<Collider>();
    public bool encumbered;

    //sound if i do that stuff
    public float sneakLoudness;
    public float normalLoudness;
    public float encumberedLoudness;

    //replay stuff
    [System.NonSerialized]
    public int time = 0;
    [System.NonSerialized]
    public int simulatedTime = 0;
    private List<InputData> TimeInput = new List<InputData>();
    protected bool isGhost = false;
    public List<Vector3> startPositions = new List<Vector3>();

    //input stuff
    protected InputData inputs;
    protected Vector3 direction;
    protected Vector3 facing;

    //animation stuff
    private enum PlayerStates { Idle, Sneak, Run, LockPick, Smack}
    private PlayerStates state;
    public Animator anim;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPositions.Add(transform.position);
        if (canPickLocks)
        {
            if (otherPlayer != null)
            {
                otherPlayer.enabled = false;
            }
        }
        if(anim == null)
        {
            anim = GetComponent<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(state);
        if (anim != null)
        {
            anim.SetInteger("state", (int)state);
        }
        //start movement keys down
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            direction.z = 1;
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            direction.x = -1;
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            direction.z = -1;
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            direction.x = 1;
        }
        //end movement keys down

        //start movement keys up
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
        {
            direction.z = 0;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                direction.z = -1;
            }
        }
        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            direction.x = 0;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                direction.x = 1;
            }
        }
        if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
        {
            direction.z = 0;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                direction.z = 1;
            }
        }
        if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
        {
            direction.x = 0;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                direction.x = -1;
            }
        }
        //direction.Normalize();
        inputs.direction = direction;
        transform.rotation = Quaternion.LookRotation(-facing);
        inputs.direction.Normalize();
        if (Input.GetButton("Sneak"))
        {

            if (!encumbered)
            {
                inputs.direction *= sneakSpeed;
            }
        }else if (encumbered)
        {
            inputs.direction *= encumberedSpeed;
        }
        else
        {
            inputs.direction *= speed;
        }
        if (canMove)
        {
            if (inputs.direction == Vector3.zero)
            {
                state = PlayerStates.Idle;
            }
            else
            {
                facing = direction;
                if (inputs.direction.magnitude == sneakSpeed)
                {
                    state = PlayerStates.Sneak;
                }
                else
                {
                    state = PlayerStates.Run;
                }
            }
        }
        
        //end movement keys up

        //start interaction button
        inputs.interact = false;
        if (Input.GetButtonDown("Interact"))
        {
            inputs.interact = true;
        }
        //end interaction button

        //testing
        if (canPickLocks)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                GhostSwap();
            }
        }
        if (!canMove)
        {
            inputs.direction = Vector3.zero;
        }
        else if (!canMoveCache)
        {
            inputs.direction = direction;
            Debug.Log("Input = direction");
        }
        canMoveCache = canMove;
    }
    private void FixedUpdate()
    {
        if (canMove)
        {
            if (!isGhost)
            {
                TimeInput.Add(inputs);
                time++;
                Move(inputs);
            }
            else
            {
                simulatedTime++;
                if (simulatedTime >= TimeInput.Count)
                {
                    GhostSwap();
                }
                else
                {
                    Move(TimeInput[simulatedTime]);
                }
            }
        }
    }
    private void Move(InputData inputData)
    {
        if (canMove)
        {
            rb.velocity = inputData.direction;
            if (inputData.interact && CanInteract())
            {
                StartCoroutine(DoInteraction());
            }
        }
    }
    public void GhostSwap()
    {
        Debug.Log("swap");
        if (firstRun)
        {
            Enemy[] interactables = FindObjectsOfType<Enemy>();
            for (int i = 0; i < interactables.Length; i++)
            {
                Debug.Log("number of enemies" + interactables.Length);
                if (interactables[i] != null)
                {
                    interactables[i].InteractionReset();
                }
            }
        }
        CameraFollowPlayer[] cameras = FindObjectsOfType<CameraFollowPlayer>();
        foreach (CameraFollowPlayer cam in cameras)
        {
            cam.SwapPlayers();
        }
        otherPlayer.enabled = true;
        isGhost = !isGhost;
        if (isGhost)
        {
            firstRun = false;
            transform.position = startPositions[0];
            simulatedTime = 0;
        }
        else
        {
            firstRun = true;
            time = 0;
            TimeInput.Clear();
            if (canPickLocks)
            {
                otherPlayer.enabled = false;
                otherPlayer.direction = Vector3.zero;
                otherPlayer.inputs.direction = Vector3.zero;
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!encumbered && Utility.Has<InteractionParent>(other.gameObject))
        {
            interactable = other.gameObject.GetComponent<InteractionParent>();
            interactableTriggers.Add(other);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (Utility.Has<InteractionParent>(other.gameObject))
        {
            interactableTriggers.Remove(other);
            if(interactableTriggers.Count > 0)
            {
                interactable = interactableTriggers[interactableTriggers.Count - 1].GetComponent<InteractionParent>();
            }
            else
            {
                interactable = null;
            }
        }
    }
    private bool CanInteract()
    {
        return interactable != null ? (interactable.shouldPickLocks == canPickLocks) && canMove : false;
    }
    private IEnumerator DoInteraction()
    {
        state = PlayerStates.Idle;
        if (interactable.doesItFuckingMatter)
        {
            if (canPickLocks)
            {
                state = PlayerStates.LockPick;
            }
            else
            {
                state = PlayerStates.Smack;
            }
        }
        canMove = false;
        yield return new WaitForSeconds(secondsForInteractionStart);
        interactable.Interact(this);
        yield return new WaitForSeconds(secondsForInteractionEnd);
        canMove = true;
    }
}
