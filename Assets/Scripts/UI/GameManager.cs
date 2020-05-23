using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public CameraFollowPlayer cam;
    [SerializeField]
    private CageBehavior cage;
    [SerializeField]
    private MenuManager pauseMenu;
    [SerializeField]
    private MenuManager restartMenu;
    [SerializeField]
    private MenuManager gameOverMenu;
    [System.NonSerialized]
    public bool paused = false;
    [System.NonSerialized]
    public bool restarting = false;
    [System.NonSerialized]
    public bool isGameOver = false;
    public static int index = 0;
    private int buttons;
    public static GameManager current;

    // Start is called before the first frame update
    void Start()
    {
        current = this;
        Physics.IgnoreLayerCollision(cage.gameObject.layer, 8);
        cage.gameObject.SetActive(false);
        restartMenu.gameObject.SetActive(false);
        pauseMenu.gameObject.SetActive(false);
        gameOverMenu.gameObject.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {
        if (!isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (paused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                Restart();
            }
        }
        if(paused || restarting || isGameOver)
        {
            if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                index--;
                if (index < 0)
                {
                    index = buttons;
                }
            }
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                index++;
                if (index > buttons)
                {
                    index = 0;
                }
            }
            if (Input.GetButtonDown("Submit"))
            {
                if (isGameOver)
                {
                    if (index == 0)
                    {
                        RestartJustSmacky();
                    }
                    else if (index == 1)
                    {
                        RestartWhole();
                    }
                    else
                    {
                        Quit();
                    }
                }
                else if (paused)
                {
                    if(index == 0)
                    {
                        Resume();
                        Debug.Log("Resume");
                    }
                    else
                    {
                        Quit();
                    }
                }
                else if (restarting)
                {
                    if (index == 0)
                    {
                        Resume();
                    }
                    else if (index == 1)
                    {
                        if (!PlayerBase.firstRun)
                        {
                            RestartJustSmacky();
                        }
                        else
                        {
                            RestartWhole();
                        }
                    }
                    else
                    {
                        RestartWhole();
                    }
                }
            }
        }
    }
    public void GameOver(PlayerBase gotCaught)
    {
        buttons = 2;
        if(cam.toFollow != gotCaught)
        {
            cam.SwapPlayers();
        }
        cage.gameObject.SetActive(true);
        cage.Catch(gotCaught);
        cage.rb.isKinematic = false;
    }
    public void LossMenu()
    {
        cam.toFollow.canMove = false;
        cam.otherPlayer.canMove = false;
        gameOverMenu.gameObject.SetActive(true);
        isGameOver = true;
    }
    public void Pause()
    {
        buttons = 1;
        restarting = false;
        paused = true;
        Time.timeScale = 0;
        restartMenu.gameObject.SetActive(false);
        pauseMenu.gameObject.SetActive(true);
        index = 0;
    }
    void Resume()
    {
        if (!isGameOver)
        {
            paused = false;
            restartMenu.gameObject.SetActive(false);
            pauseMenu.gameObject.SetActive(false);
            Time.timeScale = 1;
        }
        else
        {
            RestartWhole();
        }
    }
    void Restart()
    {
        isGameOver = false;
        pauseMenu.gameObject.SetActive(false);
        gameOverMenu.gameObject.SetActive(false);
        restartMenu.gameObject.SetActive(true);
        restarting = true;
        paused = false;
        buttons = 2;
        Time.timeScale = 0;
        index = 0;
    }
    void RestartWhole()
    {
        Debug.Log("Full Restart");
        PlayerBase.firstRun = true;
        restarting = false;
        paused = false;
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        SetAllMenus(false);
    }
    void RestartJustSmacky()
    {
        Timer.ResetAlarm();
        cage.gameObject.SetActive(false);
        restarting = false;
        if (!PlayerBase.firstRun)
        {

            InteractionParent[] interactables = FindObjectsOfType<InteractionParent>();
            foreach(InteractionParent i in interactables)
            {
                i.InteractionReset();
            }
            cam.toFollow.canMove = true;
            cam.otherPlayer.canMove = true;
            if (cam.toFollow.canPickLocks)
            {
                cam.otherPlayer.transform.position = cam.otherPlayer.startPositions[0];
                cam.toFollow.ghostTime = 0;
                cam.toFollow.transform.position = cam.toFollow.startPositions[0];
                Time.timeScale = 1;
            }
            else
            {
                cam.toFollow.transform.position = cam.toFollow.startPositions[0];
                cam.otherPlayer.transform.position = cam.otherPlayer.startPositions[0];
                cam.otherPlayer.ghostTime = 0;
                Time.timeScale = 1;
            }
        }
        else
        {
            RestartWhole();
        }
        isGameOver = false;
        SetAllMenus(false);
    }
    void SetAllMenus(bool on)
    {
        gameOverMenu.gameObject.SetActive(on);
        pauseMenu.gameObject.SetActive(on);
        restartMenu.gameObject.SetActive(on);
    }
    void Quit()
    {
        Application.Quit();
    }
}
