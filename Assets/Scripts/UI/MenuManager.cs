using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public ButtonBoi[] buttons = new ButtonBoi[3];

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].selected = false;
        }
        Debug.Log(GameManager.index);
        buttons[GameManager.index].selected = true;
    }
}
