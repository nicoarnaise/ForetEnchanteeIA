using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data : MonoBehaviour
{
    private static int level = 0;
    public static int score = 0;
    public static int moveScore = -1;
    public static int rockScore = -10;
    public static int deathScore;
    public static int exitScore;

    public static int Level
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
        {
            DontDestroyOnLoad(this);
            deathScore = -10 * (int)Mathf.Pow(level + 3, 2);
            exitScore = 10 * (int)Mathf.Pow(level + 3, 2);
        }
    }

    public static void IncreaseLevel()
    {
        level++;
        exitScore = 10 * (int)Mathf.Pow(level + 3, 2);
        deathScore = -10 * (int)Mathf.Pow(level + 3, 2);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
