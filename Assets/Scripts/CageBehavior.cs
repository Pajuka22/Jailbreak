using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CageBehavior : MonoBehaviour
{
    public float distanceToFall;
    public Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        Physics.IgnoreLayerCollision(gameObject.layer, 8);
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Catch(PlayerBase p)
    { 
        transform.position = p.transform.position + Vector3.up * distanceToFall;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == 0)
        {
            FindObjectOfType<GameManager>().LossMenu();
        }
    }
}
