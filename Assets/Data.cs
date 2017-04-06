using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Data : MonoBehaviour
{
    private static int level = 0;
    private static int score = 0;
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

    public static void addScore(int value)
    {
        Text scoreText = GameObject.Find("UI/BottomPanel/ScoreText").GetComponent<Text>();
        score += value;
        scoreText.text = "Score : " + score;
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
        Text scoreText = GameObject.Find("UI/BottomPanel/ScoreText").GetComponent<Text>();
        scoreText.text = "Score : " + score;
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
