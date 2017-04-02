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
    private float monsterThreshold = 0.7f;
    private float emptyRoomScore = 0.0f;
    private int numberRoomLeft;

    private void Awake()
    {
        world = FindObjectOfType<WorldGenerator>();
        posX = int.MaxValue / 2;
        posY = int.MaxValue / 2;
        initialPosX = posX;
        initialPosY = posY;
        numberRoomLeft = world.levelSize * world.levelSize;

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

    public void Play()
    {
        SetBeliefs(); // Pourcentage of possibilities
        Action(FindClosestRoom(FindHighScoredRooms())); // Action desired
    }

    private Dictionary<Room, float>[,] GetEligibleRooms()
    {
        Dictionary<Room, float>[,] eligibleRooms = knownLevel.Clone() as Dictionary<Room, float>[,];
        for (int i = 0; i < memorySize; i++)
        {
            for (int j = 0; j < memorySize; j++)
            {
                if (!IsEligible(i, j))
                    eligibleRooms[i, j] = null;
            }
        }
        return eligibleRooms;
    }

    private bool IsEligible(int i, int j)
    {
        if (knownLevel[i, j].Count == 1)
            return false;
        for (int k = -1; k < 2; k += 2)
        {

            if (i + k >= 0 && i + k < memorySize)
            {
                if (knownLevel[i + k, j].Count == 1)
                    return true;
            }
            if (j + k >= 0 && j + k < memorySize)
            {
                if (knownLevel[i, j + k].Count == 1)
                    return true;
            }
        }
        return false;
    }


    private void SetBeliefs()
    {
        for (int i = 0; i < memorySize; i++)
        {
            for (int j = 0; j < memorySize; j++)
            {
                if (IsEligible(i, j))
                {
                    SetProbabilities(i, j);
                }
            }
        }
    }

    private void SetProbabilities(int i, int j)
    {
        foreach (Room room in knownLevel[i, j].Keys)
        {
            if (room is Exit)
            {
                knownLevel[i, j][room] = ((float)numberRoomLeft / (float)(world.levelSize * world.levelSize));
            }
            //TODO smthg
        }
    }

    private List<Vector2> FindHighScoredRooms()
    {
        List<Vector2> rooms = new List<Vector2>();
        float actualMaxScore = float.MinValue;
        for (int i = 0; i < memorySize; i++)
        {
            for (int j = 0; j < memorySize; j++)
            {
                if (IsEligible(i, j))
                {
                    float score = GetScoreOfCase(knownLevel[i, j]);
                    if (score >= actualMaxScore)
                    {
                        if (score > actualMaxScore)
                            rooms.Clear();
                        actualMaxScore = score;
                        rooms.Add(new Vector2(i, j));
                    }
                }
            }
        }
        return rooms;
    }

    private Vector2 FindClosestRoom(List<Vector2> eligibleRooms)
    {
        List<Vector2> identicalRooms = new List<Vector2>();
        int minDistance = int.MaxValue;
        for (int i = 0; i < eligibleRooms.Count; i++)
        {
            int currentDistance = (int)(Mathf.Abs(eligibleRooms[i].x - posX) + Mathf.Abs(eligibleRooms[i].y - posY));
            if (currentDistance <= minDistance)
            {
                if (currentDistance < minDistance)
                    identicalRooms.Clear();
                minDistance = currentDistance;
                identicalRooms.Add(eligibleRooms[i]);
            }
        }

        return identicalRooms[UnityEngine.Random.Range(0, identicalRooms.Count)];
    }



    private float GetScoreOfCase(Dictionary<Room, float> dictionary)
    {
        float totalScore = 0f;
        if (dictionary.Count == 0)
            return float.MinValue;
        if (dictionary.Count == 1)
            if (dictionary.Keys.GetEnumerator().Current is Exit)
                return float.MaxValue;
            else
                return int.MinValue;
        foreach (Room room in dictionary.Keys)
        {
            float roomChance = 0f;
            if (dictionary.TryGetValue(room, out roomChance))
            {
                if (room is Exit)
                {
                    totalScore += roomChance * Data.exitScore;
                }
                if (room is Hole)
                {
                    totalScore += roomChance * Data.deathScore;
                }
                if (room is Monster)
                {
                    totalScore += roomChance > monsterThreshold ? roomChance * Data.rockScore : roomChance * Data.deathScore;
                }
                if (room is EmptyRoom)
                {
                    totalScore += roomChance * emptyRoomScore;
                }
            }
        }
        return totalScore;
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
        CheckForBorder();
        UpdateCurrentState();
        CheckStatut();
    }

    private void CheckForBorder()
    {
        if (world.GetRoom(posX - initialPosX + 1, posY - initialPosY) == null)
        {
            for (int i = posX + 1; i < memorySize; i++)
            {
                for (int j = 0; j < memorySize; j++)
                {
                    knownLevel[i, j].Clear();
                }
            }
        }

        if (world.GetRoom(posX - initialPosX - 1, posY - initialPosY) == null)
        {
            for (int i = posX - 1; i >= 0; i--)
            {
                for (int j = 0; j < memorySize; j++)
                {
                    knownLevel[i, j].Clear();
                }
            }
        }

        if (world.GetRoom(posX - initialPosX, posY - initialPosY + 1) == null)
        {
            for (int i = 0; i < memorySize; i++)
            {
                for (int j = posY + 1; j < memorySize; j++)
                {
                    knownLevel[i, j].Clear();
                }
            }
        }

        if (world.GetRoom(posX - initialPosX, posY - initialPosY - 1) == null)
        {
            for (int i = 0; i < memorySize; i++)
            {
                for (int j = posY - 1; j >= 0; j--)
                {
                    knownLevel[i, j].Clear();
                }
            }
        }
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
        if (knownLevel[posX, posY].Count > 1)
        {
            knownLevel[posX, posY].Clear();
            knownLevel[posX, posY].Add(world.GetRoom(posX - initialPosX, posY - initialPosY), 1);
            numberRoomLeft--;
        }
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
        Data.score += Data.deathScore;
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
