using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{

    public Dictionary<Room, float>[,] knownLevel;
    private int initialPosX;
    private int initialPosY;
    private Vector3 initialPosition;
    public int posX;
    public int posY;
    private WorldGenerator world;
    private int memorySize = int.MaxValue;


    private void Awake()
    {
        world = FindObjectOfType<WorldGenerator>();
        posX = int.MaxValue / 2;
        posY = int.MaxValue / 2;
        initialPosX = posX;
        initialPosY = posY;

        knownLevel = new Dictionary<Room, float>[memorySize, memorySize];
        for (int i = 0; i < int.MaxValue; i++)
        {
            for (int j = 0; j < int.MaxValue; j++)
            {
                knownLevel[i, j] = new Dictionary<Room, float>();
                knownLevel[i, j].Add(new Monster(), .25f);
                knownLevel[i, j].Add(new Exit(), .25f);
                knownLevel[i, j].Add(new Hole(), .25f);
                knownLevel[i, j].Add(new Room(), .25f);
            }
        }
        initialPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private List<Vector2> FindHighScoredRooms()
    {
        List<Vector2> rooms = new List<Vector2>();
        int actualMaxScore = int.MinValue;
        for (int i = 0; i < memorySize; i++)
        {
            for (int j = 0; j < memorySize; j++)
            {
                int score = GetScoreOfCase(knownLevel[i, j]);
                if (score >= actualMaxScore)
                {
                    if (score > actualMaxScore)
                        rooms.Clear();
                    actualMaxScore = score;
                    rooms.Add(new Vector2(i, j));
                }
            }
        }
        return rooms;
    }

    private int GetScoreOfCase(Dictionary<Room, float> dictionary)
    {
        throw new NotImplementedException();
    }

    private void MoveUp()
    {
        posY--;
        transform.Translate(Vector3.up * WorldGenerator.roomSize);
        Move();
    }

    private void MoveDown()
    {
        posY++;
        transform.Translate(Vector3.down * WorldGenerator.roomSize);
        Move();
    }

    private void MoveRight()
    {
        posX++;
        transform.Translate(Vector3.right * WorldGenerator.roomSize);
        Move();
    }

    private void MoveLeft()
    {
        posX--;
        transform.Translate(Vector3.left * WorldGenerator.roomSize);
        Move();
    }

    private void Move()
    {
        Data.score += Data.moveScore;
        UpdateCurrentState();
        CheckStatut();
    }

    private void ThrowRock(Vector2 direction)
    {
        world.TryKillMonsterAt(posX - initialPosX + (int)direction.x, posY - initialPosY + (int)direction.y);
        Data.score += Data.rockScore;
        Room toRemove = null;
        foreach (Room item in knownLevel[posX + (int)direction.x, posY + (int)direction.y].Keys)
        {
            if (item is Monster)
            {
                toRemove = item;
            }
        }
        knownLevel[posX + (int)direction.x, posY + (int)direction.y].Remove(toRemove);
    }

    private void UpdateCurrentState()
    {
        knownLevel[posX, posY].Clear();
        knownLevel[posX, posY].Add(world.GetRoom(posX - initialPosX, posY - initialPosY), 1);
    }

    private void CheckStatut()
    {
        if (knownLevel[posX, posY].Count == 1)
        {
            if ((knownLevel[posX, posY].Keys.GetEnumerator().Current is Hole || knownLevel[posX, posY].Keys.GetEnumerator().Current is Monster))
                Die();
            if (knownLevel[posX, posY].Keys.GetEnumerator().Current is Exit)
                CompleteLevel();
        }
    }

    private void Die()
    {
        Data.score += Data.dieScore;
        posX = initialPosX;
        posY = initialPosY;
        transform.position = initialPosition;
    }

    private void CompleteLevel()
    {
        Data.score += Data.exitScore;
        Data.IncreaseLevel();
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

}
