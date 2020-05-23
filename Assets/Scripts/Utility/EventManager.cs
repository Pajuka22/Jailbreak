using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using JetBrains.Annotations;

public class EventManager : MonoBehaviour
{
    public static EventManager current;
    // Start is called before the first frame update
    void Start()
    {
        current = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public event Action resetInteractables;
    public void ResetInteractables()
    {
        resetInteractables?.Invoke();
    }
    public event Action soundAlarm;
    public void SoundAlarm()
    {
        soundAlarm?.Invoke();
    }
    public event Action <SoundUtility.Sound> hearSound;
    public void MakeSound(SoundUtility.Sound sound)
    {
        hearSound?.Invoke(sound);
    }
}
