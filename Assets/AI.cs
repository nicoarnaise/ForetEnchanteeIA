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
    private int memorySize;
    private float monsterThreshold = 0.7f;
    private float emptyRoomScore = 0.0f;
    private int numberRoomLeft;
    private bool isActionFinished;
    private List<Vector2> actionList;

    private void Awake()
    {
        world = FindObjectOfType<WorldGenerator>();
        memorySize = world.levelSize * 2 + 1;
        posX = memorySize / 2 + 1;
        posY = memorySize / 2 + 1;
        initialPosX = posX;
        initialPosY = posY;
        numberRoomLeft = world.levelSize * world.levelSize;
        isActionFinished = true;
        actionList = new List<Vector2>();

        knownLevel = new Dictionary<Room, float>[memorySize, memorySize];
        for (int i = 0; i < memorySize; i++)
        {
            for (int j = 0; j < memorySize; j++)
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
        if (isActionFinished)
        {
            isActionFinished = false;
            SetBeliefs(); // Pourcentage of possibilities
            actionList = FindClosestRoomPath(FindHighScoredRooms()); // List of Rooms to visit
        }
        MakeAction(actionList); // Action taken over the rooms to visit
    }

    private void MakeAction(List<Vector2> rooms)
    {
        List<Vector2> auxRooms = rooms;
        if (auxRooms.Count > 1)
        {
            MakeMove(auxRooms[0]);
            auxRooms.RemoveAt(0);
        }
        if (auxRooms.Count == 1)
        {
            int line = (int)auxRooms[0].x;
            int column = (int)auxRooms[0].y;

            int potentialRoomSize = (world.levelSize * 2 - 1) * (world.levelSize * 2 - 1);
            int nbLine = (int)Mathf.Sqrt(potentialRoomSize);
            int lineRoot = nbLine / 2;

            int coordX = line + (memorySize / 2) - lineRoot;
            int coordY = column + (memorySize / 2) - lineRoot;

            foreach (Room room in knownLevel[coordX, coordY].Keys)
            {
                float roomChance = 0f;
                if (knownLevel[coordX, coordY].TryGetValue(room, out roomChance))
                {
                    if (room is Monster)
                    {
                        if (roomChance > monsterThreshold)
                        {
                            ThrowRock(new Vector2(coordX, coordY));
                        }
                    }
                    else
                    {
                        MakeMove(auxRooms[0]);
                        auxRooms.RemoveAt(0);
                        isActionFinished = true;
                    }
                }
            }
        }
    }

    private void MakeMove(Vector2 room)
    {
        if (room.x > posX)
        {
            MoveRight();
        }
        if (room.x < posX)
        {
            MoveLeft();
        }
        if (room.y < posY)
        {
            MoveUp();
        }
        if (room.y > posY)
        {
            MoveDown();
        }
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
        float total = 0f;
        foreach (Room room in knownLevel[i, j].Keys)
        {
            if (room is Exit)
            {
                knownLevel[i, j][room] = ((float)numberRoomLeft / (float)(world.levelSize * world.levelSize));
            }
            if (room is Monster)
            {
                knownLevel[i, j][room] = getRoomProb(i, j, room, world.monsterRate);
            }
            if (room is Hole)
            {
                knownLevel[i, j][room] = getRoomProb(i, j, room, world.holeRate);
            }
            if (room is EmptyRoom)
            {
                knownLevel[i, j][room] = 1 - total;
            }
            else
            {
                total += knownLevel[i, j][room];
            }
        }
    }

    private float getRoomProb(int i, int j, Room room, float rate)
    {
        float dangerProb = 0;
        float nonDangerProb = 1;
        switch (getPredictorsCount(i, j, room))
        {
            case 0:
                dangerProb = rate;
                nonDangerProb = 1 - rate;
                break;
            case 1:
                dangerProb = rate;
                nonDangerProb = (1 - rate) * ((float)Math.Pow(rate, 2) + 2 * rate * (1 - rate));
                break;
            case 2:
                dangerProb = rate;
                nonDangerProb = (1 - rate) * ((float)Math.Pow(rate, 4) + 4 * (float)Math.Pow(rate, 3) * (1 - rate) + 4 * (float)Math.Pow(rate, 2) * (float)Math.Pow(1 - rate, 2));
                break;
            case 3:
                // none of the 3 predictors are fully known
                dangerProb = rate;
                nonDangerProb = (1 - rate) * ((float)Math.Pow(rate, 4) + 4 * (float)Math.Pow(rate, 3) * (1 - rate) + 3 * (float)Math.Pow(rate, 2) * (float)Math.Pow(1 - rate, 2));
                break;
            case 4:
            case 5:
                dangerProb = 1;
                nonDangerProb = 0;
                break;
        }
        return dangerProb / (dangerProb + nonDangerProb);
    }

    private int getPredictorsCount(int i, int j, Room room)
    {
        int count = 4;
        // count == -1 => no danger possible
        // count == 5 => fully filled predictor => danger in this room
        count = majPredictorsCount(i - 1, j, room, count);
        if (count == -1 || count == 5) return count;
        count = majPredictorsCount(i + 1, j, room, count);
        if (count == -1 || count == 5) return count;
        count = majPredictorsCount(i, j - 1, room, count);
        if (count == -1 || count == 5) return count;
        count = majPredictorsCount(i, j + 1, room, count);

        return count;
    }

    private int majPredictorsCount(int i, int j, Room room, int count)
    {
        if (knownLevel[i, j].Count == 1)
        {
            foreach (Room neighboor in knownLevel[i, j].Keys)
            {
                if ((room is Monster && !neighboor.hasPoop) || (room is Hole && !neighboor.hasWind))
                {
                    // known and not predictor => no danger
                    count = -1;
                }
                else
                {
                    // knonw and predictor => check predictor's neighborhood
                    int noDanger = 0;
                    for (int k = -1; k < 2; k += 2)
                    {
                        for (int l = -1; l < 2; l += 2)
                        {
                            if (knownLevel[k, l].Count == 1)
                            {
                                foreach (Room neiV2 in knownLevel[k, l].Keys)
                                {
                                    // the predictor has a known danger in his neighborhood
                                    if (neiV2 is Monster && room is Monster || neiV2 is Hole && room is Hole)
                                        return Math.Max(0, count - 1);
                                    if (neiV2 is EmptyRoom)
                                    {
                                        noDanger++;
                                    }
                                }
                            }
                        }
                    }
                    if (noDanger == 3)
                    {
                        // predictor has three known and empty neighboors
                        count = 5;
                    }
                }
            }
        }
        else
        {
            // unknown => no predictor
            count = Math.Max(0, count - 1);
        }
        return count;
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

    private List<Vector2> FindClosestRoomPath(List<Vector2> eligibleRooms)
    {
        List<Vector2> identicalRooms = new List<Vector2>();
        int potentialRoomSize = (world.levelSize * 2 - 1) * (world.levelSize * 2 - 1);
        int nbLine = (int)Mathf.Sqrt(potentialRoomSize);
        int lineRoot = nbLine / 2;

        Node root = new Node(potentialRoomSize / 2, lineRoot, lineRoot, 0);

        int[] potentialRooms = new int[potentialRoomSize];
        for (int i = 0; i < potentialRoomSize; i++)
        {
            int coordX = (i / nbLine) + (memorySize / 2) - lineRoot;
            int coordY = (i % nbLine) + (memorySize / 2) - lineRoot;
            if (knownLevel[coordX, coordY].Count == 1 || eligibleRooms.Contains(new Vector2(coordX, coordY)))
            {
                potentialRooms[i] = 1;
            }
            else
            {
                potentialRooms[i] = 10000;
            }
        }
        identicalRooms = Disjtra(new Graph(potentialRooms, (world.levelSize * 2 - 1) * (world.levelSize * 2 - 1), root));
        return identicalRooms;
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
            Debug.Log((initialPosX) + "," + (initialPosY));
            knownLevel[posX, posY].Add(world.GetRoom(posX -initialPosX, posY-initialPosY), 1);
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


    public List<Vector2> Disjtra(Graph G)
    {
        List<Vector2> optimalPath = new List<Vector2>();
        List<Node> remainingNodes = G.graphNodes;
        Node sdeb = remainingNodes[0];
        Node sfin = remainingNodes[G.graphNodes.Count - 1];

        while (remainingNodes.Count > 0)
        {
            Node s1 = findminScore(remainingNodes);
            remainingNodes.Remove(s1);
            for (int i = 0; i < s1.arcs.Count; i++)
            {
                updateScores(s1, s1.arcs[i].finish);
            }
            if (IsEligible(s1.line, s1.column))
            {
                sfin = s1;
                break;
            }
        }

        Node s = sfin;
        while (s != sdeb)
        {
            optimalPath.Add(new Vector3(s.line, s.column));
            s = s.father;
        }

        return optimalPath;
    }



    public Node findminScore(List<Node> nodes)
    {
        int minScore = int.MaxValue;
        Node minNode = null;
        for (int i = 0; i < nodes.Count; i++)
        {
            Node currentNode = nodes[i];
            int currentScore = currentNode.score;
            if (currentScore < minScore)
            {
                minScore = currentScore;
                minNode = currentNode;
            }
        }
        return minNode;
    }

    public void updateScores(Node s1, Node s2)
    {
        int newScore = int.MaxValue;
        List<Arc> arcs = s1.arcs;

        for (int i = 0; i < arcs.Count; i++)
        {
            if (arcs[i].finish == s2)
            {
                newScore = s1.score + arcs[i].weight;
                break;
            }
        }

        if (s2.score > newScore)
        {
            s2.score = newScore;
            s2.father = s1;
        }
    }
}
