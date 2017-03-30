using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public static GameObject wind;
    public static GameObject poop;
    public bool hasPoop;
    public bool hasWind;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddWind()
    {
        if (!hasWind)
            Instantiate(wind, Vector3.zero, transform.rotation, transform).transform.localPosition = Vector3.zero;
        hasWind = true;
    }

    public void AddPoop()
    {
        if (!hasPoop)
            Instantiate(poop, Vector3.zero, transform.rotation, transform).transform.localPosition = Vector3.zero;
        hasWind = true;
    }
}
