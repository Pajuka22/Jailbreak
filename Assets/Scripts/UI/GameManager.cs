using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CameraFollowPlayer cam;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GameOver(PlayerBase gotCaught)
    {
        Debug.Log("YOU LOSE");
        if(cam.toFollow != gotCaught)
        {
            cam.SwapPlayers();
        }
    }
}
