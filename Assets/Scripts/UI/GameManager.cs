using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public CameraFollowPlayer cam;
    public CageBehavior cage;
    public MenuManager pauseMenu;
    public MenuManager restartMenu;
    public MenuManager gameOverMenu;
    [System.NonSerialized]
    public bool paused = false;
    [System.NonSerialized]
    public bool restarting = false;
    [System.NonSerialized]
    public bool isGameOver = false;
    public static int index = 0;
    private int buttons;
    // Start is called before the first frame update
    void Start()
    {
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
                    if(index == 0)
                    {
                        Restart();
                    }
                    else
                    {
                        Quit();
                    }
                }
                if (paused)
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
                if (restarting)
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
        buttons = 1;
        Debug.Log("YOU LOSE");
        if(cam.toFollow != gotCaught)
        {
            cam.SwapPlayers();
        }
        cage.gameObject.SetActive(true);
        cage.Catch(gotCaught);
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
        pauseMenu.gameObject.SetActive(false);
        restartMenu.gameObject.SetActive(true);
        Debug.Log("Restart Menu");
        restarting = true;
        paused = false;
        buttons = 2;
        Time.timeScale = 0;
        index = 0;
    }
    void RestartWhole()
    {
        PlayerBase.firstRun = true;
        restarting = false;
        paused = false;
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    void RestartJustSmacky()
    {
        restarting = false;
        if (!PlayerBase.firstRun)
        {
            if (cam.toFollow.canPickLocks)
            {
                cam.otherPlayer.transform.position = cam.otherPlayer.startPositions[0];
                cam.toFollow.ghostTime = 0;
                cam.toFollow.transform.position = cam.toFollow.startPositions[0];
            }
            else
            {
                cam.toFollow.transform.position = cam.toFollow.startPositions[0];
                cam.otherPlayer.transform.position = cam.otherPlayer.startPositions[0];
                cam.otherPlayer.ghostTime = 0;
            }
        }
        else
        {
            RestartWhole();
        }
    }
    void Quit()
    {
        Application.Quit();
    }
}
