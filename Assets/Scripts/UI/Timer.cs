using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Timer : MonoBehaviour
{
    public static float time = 0;
    [SerializeField]
    float alarmTime = 5;
    public RectTransform hand;
    // Start is called before the first frame update
    void Start()
    {
        EventManager.current.resetInteractables += ResetAlarm;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        time += Time.fixedDeltaTime;
        if(time >= alarmTime)
        {
            EventManager.current.SoundAlarm();
        }
        else
        {
            hand.rotation = Quaternion.Euler(0, 0, 360 * (time - alarmTime) / alarmTime + 180);
        }
    }
    public static void ResetAlarm()
    {
        time = 0;
    }
}
