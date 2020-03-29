using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionParent : MonoBehaviour
{
    public float interactionTime;
    public bool shouldPickLocks;
    public bool doesItFuckingMatter = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public virtual void Interact(PlayerBase playerBase)
    {
        Debug.Log("Interact");
    }
    public virtual void InteractionReset()
    {

    }
    protected virtual IEnumerator InteractRoutine()
    {
        yield return new WaitForSeconds(interactionTime);
    }
}
