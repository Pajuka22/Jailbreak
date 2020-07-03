using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Timer : MonoBehaviour
{
    public static float time = 0;
    [SerializeField]
    static float alarmTime = 5;
    public RectTransform hand;
    static bool hasRung = false;
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
        if(time >= alarmTime / 2 && !hasRung)
        {
            EventManager.current.SoundAlarm();
            hasRung = true;
        }
        if(time >= alarmTime)
        {
            EventManager.current.TimeOut();
        }
        else
        {
            hand.rotation = Quaternion.Euler(0, 0, 360 * (time - alarmTime) / alarmTime + 180);
        }
    }
    public static void ResetAlarm()
    {
        hasRung = false;
        time = 0;
    }
}
