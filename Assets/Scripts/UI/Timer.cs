using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Timer : MonoBehaviour
{
    float time = 0;
    [SerializeField]
    float AlarmTime = 5;
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
        if(time >= AlarmTime)
        {
            EventManager.current.SoundAlarm();
        }
    }
    void ResetAlarm()
    {
        time = 0;
    }
}
