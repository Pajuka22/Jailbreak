using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class PlayerBase : MonoBehaviour
{
    public struct Inputs
    {
        public Vector3 velocity;
        public bool interact;
        public States state;
        public Quaternion rotation;
    }
    public Inputs input;
    public List<Inputs> ghostInputs = new List<Inputs>();
    public enum States { Idle, Sneak, Run, LockPick, Smack };
    public States state;
    public PlayerBase otherPlayer;
    public bool canPickLocks;
    [System.NonSerialized]
    public int time = 0;
    [System.NonSerialized]
    public int ghostTime = 0;
    [System.NonSerialized]
    protected bool isGhost = false;
    public List<Vector3> startPositions = new List<Vector3>();
    public Animator anim;
    [System.NonSerialized]
    public static bool firstRun = true;
    [System.NonSerialized]
    public bool canMove = true;
    private bool canMoveCache = true;
    [System.NonSerialized]
    public Rigidbody rb;
    [System.NonSerialized]
    public Vector3 direction;
    [System.NonSerialized]
    public Quaternion facing;
    [System.NonSerialized]
    public bool encumbered;
    public float speed;
    public float sneakSpeed;
    public float encumberedSpeed;
    private InteractionParent interactable;
    private List<InteractionParent> allInteractables = new List<InteractionParent>();


    // Start is called before the first frame update
    void Start()
    {
        Physics.IgnoreLayerCollision(gameObject.layer, gameObject.layer);
        rb = GetComponent<Rigidbody>();
        startPositions.Add(transform.position);
        if (canPickLocks)
        {
            if (otherPlayer != null)
            {
                otherPlayer.enabled = false;
            }
        }
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        input.interact = false;
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
        if (Input.GetButtonDown("Interact"))
        {
            Debug.Log("Interact");
            input.interact = true;
        }
        //end movement keys up
        direction.Normalize();
        input.velocity = direction;
        if (Input.GetButton("Sneak"))
        {

            if (!encumbered)
            {
                input.velocity *= sneakSpeed;
            }
        }
        else if (encumbered)
        {
            input.velocity *= encumberedSpeed;
        }
        else
        {
            input.velocity *= speed;
        }
        if (canMove)
        {
            if (input.velocity.magnitude == 0)
            {
                input.state = States.Idle;
            }
            else 
            {
                if (input.velocity.magnitude == sneakSpeed)
                {
                    input.state = States.Sneak;
                }
                else
                {
                    input.state = States.Run;
                }
                input.rotation = Quaternion.LookRotation(-input.velocity, Vector3.up);
            }
        }
        else
        {
            input.state = States.Idle;
        }
        if (state != States.Idle)
        {
            input.state = state;
        }
    }
    private void FixedUpdate()
    {
        if (canMove)
        {
            if (!isGhost)
            {
                ghostInputs.Add(input);
                time++;
                Move(input);
                input.state = state;
            }
            else
            {
                ghostTime++;
                if (ghostTime >= ghostInputs.Count)
                {
                    //GhostSwap();
                }
                else
                {
                    Move(ghostInputs[ghostTime]);
                }
            }
        }
    }

    void Move(Inputs Input)
    {
        if(state != States.Idle)
        {
            Input.state = state;
        }
        Debug.Log(Input.state);
        if (anim != null)
        {
            anim.SetInteger("state", (int)Input.state);
        }
        transform.rotation = Input.rotation;
        if (canMove)
        {
            rb.velocity = Input.velocity;
            if (Input.interact)
            {
                //Debug.Log("interact registered");
                if (CanInteract())
                {
                    //Debug.Log("You could interact");
                    StartCoroutine(interactable.InteractRoutine(this));
                }
                else
                {
                    //Debug.Log("you could not interact");
                }
            }
        }
    }
    private bool CanInteract()
    {
        return interactable != null ? (interactable.doesItFuckingMatter ? (canPickLocks ? interactable.shouldPickLocks : interactable.shouldSmack ): true) && canMove : false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!encumbered && Utility.Has<InteractionParent>(other.gameObject))
        {
            interactable = other.gameObject.GetComponent<InteractionParent>();
            allInteractables.Add(interactable);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (Utility.Has<InteractionParent>(other.gameObject))
        {
            allInteractables.Remove(other.gameObject.GetComponent<InteractionParent>());
            if (allInteractables.Count > 0)
            {
                interactable = allInteractables[allInteractables.Count - 1];
            }
            else
            {
                interactable = null;
            }
        }
    }
    public void GhostSwap()
    {
        Debug.Log("swap");
        if (firstRun)
        {
            InteractionParent[] interactables = FindObjectsOfType<InteractionParent>();
            for (int i = 0; i < interactables.Length; i++)
            {
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
            ghostTime = 0;
        }
        else
        {
            firstRun = true;
            time = 0;
            ghostInputs.Clear();
            if (canPickLocks)
            {
                otherPlayer.enabled = false;
                otherPlayer.direction = Vector3.zero;
                otherPlayer.input.velocity = Vector3.zero;
            }
        }
    }
    /*
    public PlayerBase otherPlayer;
    public static bool firstRun = true;
    public bool canMove = true;
    private bool canMoveCache = true;
    public struct InputData
    {
        public Vector3 direction;
        public bool interact;
        public PlayerStates state;
        public Vector3 facing;
    }
    private Rigidbody rb;

    //movement speed
    public float sneakSpeed;
    public float speed;
    public float encumberedSpeed;

    //not even sure this is necessary
    public float strength;

    //interaction
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
    public InputData inputs;
    public Vector3 direction;
    public Vector3 facing;

    //animation stuff
    public enum PlayerStates { Idle, Sneak, Run, LockPick, Smack}
    public PlayerStates state;
    public Animator anim;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Time.fixedDeltaTime);
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
            Debug.Log("update interaction pressed");
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
        }
        canMoveCache = canMove;
        inputs.facing = facing;

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
                if (inputs.interact)
                {
                    Debug.Log("fixed update got interaction");
                }
                inputs.state = state;
            }
            else
            {
                simulatedTime++;
                if (simulatedTime >= TimeInput.Count)
                {
                    //GhostSwap();
                }
                else
                {
                    Move(TimeInput[simulatedTime]);
                }
            }
        }
        else
        {
            if (inputs.interact)
            {
                Debug.Log("couldn't move");
            }
        }
    }
    private void Move(InputData inputData)
    {
        if (anim != null)
        {
            anim.SetInteger("state", (int)inputData.state);
        }
        transform.rotation = Quaternion.LookRotation(-inputData.facing);
        if (canMove)
        {
            rb.velocity = inputData.direction;
            if (inputData.interact)
            {
                Debug.Log("interact registered");
                if (CanInteract())
                {
                    Debug.Log("You could interact");
                    StartCoroutine(interactable.InteractRoutine(this));
                }
                else
                {
                    Debug.Log("you could not interact");
                }
            }
        }
    }
    public void GhostSwap()
    {
        Debug.Log("swap");
        if (firstRun)
        {
            InteractionParent[] interactables = FindObjectsOfType<InteractionParent>();
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
        return interactable != null ? (interactable.doesItFuckingMatter ? interactable.shouldPickLocks == canPickLocks : true) && canMove : false;
    }
    /*private IEnumerator DoInteraction()
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
    }*/
}
