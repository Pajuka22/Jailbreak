using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRestart : MonoBehaviour
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
    public enum States { Idle, Sneak, Run, LockPick, Smack};
    public States state;
    public PlayerRestart otherPlayer;
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
    public bool firstRun = true;
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
        if (anim == null)
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
        //end movement keys up
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
            input.rotation = Quaternion.LookRotation(-input.velocity);
            if(input.velocity.magnitude == 0)
            {
                input.state = States.Idle;
            }
            else if(input.velocity.magnitude == sneakSpeed)
            {
                input.state = States.Sneak;
            }
            else
            {
                input.state = States.Run;
            }
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
                if (input.interact)
                {
                    Debug.Log("fixed update got interaction");
                }
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
