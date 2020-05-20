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
    public enum States { Idle, Sneak, Run, LockPick, Smack, PickUp, Drag};
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
    bool sneaking;
    public Transform head;
    public Transform leftHand;
    [System.NonSerialized]
    public InteractionParent holding;
    public GameObject interactionIndicator;

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
        state = States.Idle;
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
            input.interact = true;
            Debug.Log("InteractPressed");
        }
        //end movement keys up
        direction.Normalize();
        input.velocity = direction;
        if (Input.GetButtonDown("Sneak"))
        {
            sneaking = true;
        }
        if (Input.GetButtonUp("Sneak") || encumbered)
        {
            sneaking = false;
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
            if (input.velocity == Vector3.zero)
            {
                input.state = States.Idle;
            }
            else 
            {
                if (sneaking)
                {
                    state = States.Sneak;
                }
                else if (encumbered)
                {
                    state = States.Drag;
                }
                else
                {
                    state = States.Run;
                }
                input.rotation = Quaternion.LookRotation(input.velocity, Vector3.up);
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
        if (interactable != null && !isGhost)
        {
            interactionIndicator.SetActive(true);
            if (interactable.indicatorLocation != null)
            {
                interactionIndicator.transform.position = interactable.indicatorLocation.position + Vector3.up * 0.5f;
            }
            else
            {
                interactionIndicator.transform.position = interactable.transform.position + Vector3.up;
            }
            interactionIndicator.transform.rotation = Quaternion.LookRotation(Camera.main.transform.position - interactionIndicator.transform.position -
                Vector3.up * VectorMath.DistanceInDirection(Camera.main.transform.position - interactionIndicator.transform.position, Vector3.up), Vector3.up);
        }
        else
        {
            interactionIndicator.SetActive(false);
        }
    }
    private void FixedUpdate()
    {
        if (canMove)
        {
            if (!isGhost)
            {
                if (!canMove)
                {
                    input.velocity = Vector3.zero;
                }
                if (firstRun)
                {   
                    ghostInputs.Add(input);
                }
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
        if (anim != null)
        {
            anim.SetInteger("state", (int)Input.state);
        }
        transform.rotation = Input.rotation;
        if (canMove)
        {
            if (!Input.Equals(null))
            {
                rb.velocity = new Vector3(Input.velocity.x, rb.velocity.y, Input.velocity.z);
            }
            else
            {
                rb.velocity = Vector3.zero;
            }
            Debug.ClearDeveloperConsole();
            if (Input.interact)
            {
                input.interact = false;
                if (CanInteract())
                {
                    if(interactable == null && holding != null)
                    {
                        holding.Drop(this);
                    }
                    else
                    {
                        StartCoroutine(interactable.InteractRoutine(this));
                    }
                }
            }
        }
    }
    private bool CanInteract()
    {
        return holding != null || (interactable != null ? (interactable.doesItFuckingMatter ? (canPickLocks ? interactable.shouldPickLocks : interactable.shouldSmack ): true) && canMove : false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (Utility.Has<InteractionParent>(other.transform.root.gameObject) && 
            other == other.transform.root.gameObject.GetComponent<InteractionParent>().interactionCollider)
        {
            interactable = other.transform.root.gameObject.GetComponent<InteractionParent>();
            allInteractables.Add(interactable);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (Utility.Has<InteractionParent>(other.transform.root.gameObject)&&
            other == other.transform.root.gameObject.GetComponent<InteractionParent>().interactionCollider)
        {
            allInteractables.Remove(other.transform.root.gameObject.GetComponent<InteractionParent>());
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
}
