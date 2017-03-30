using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data : MonoBehaviour
{
    private int level = 0;

    public int Level
    {
        get
        {
            return level;
        }
    }

    // Use this for initialization
    void Awake()
    {
        if (FindObjectsOfType<Data>().Length > 1)
            DestroyImmediate(gameObject);
        else
            DontDestroyOnLoad(this);
    }

    void IncreaseLevel()
    {
        level++;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
