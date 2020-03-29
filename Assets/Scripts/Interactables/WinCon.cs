using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinCon : InteractionParent
{
    public string WinScene;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(picked && smacked)
        {
            SceneManager.LoadScene(WinScene);
        }
    }
    public override void Interact(PlayerBase playerBase)
    {
        if (playerBase.canPickLocks && PlayerBase.firstRun)
        {
            playerBase.GhostSwap();
        }
    }
}
