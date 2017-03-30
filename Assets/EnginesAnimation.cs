using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnginesAnimation : MonoBehaviour
{


    public float ThrottlePercent = 0.0f;
    ParticleSystem[] engines;
    // Use this for initialization
    void Start()
    {
        engines = GetComponentsInChildren<ParticleSystem>();
        //for (int i = 0; i < engines.Length; i++)
        //{
        //    maxSpeed[i]=
        //}
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateThrottle(float input)
    {
        ThrottlePercent = input;
        for (int i = 0; i < engines.Length; i++)
        {
            ParticleSystem.MainModule tmp = engines[i].main;
            tmp.startSpeedMultiplier = ThrottlePercent;
        }
    }
}
