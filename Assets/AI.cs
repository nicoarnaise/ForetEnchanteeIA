using System;
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
    private float monsterThreshold = 0.6f;
    private float emptyRoomScore = 0.0f;
    private int numberRoomLeft;
    private List<Vector2> actionList;

    private void Awake()
    {
        world = FindObjectOfType<WorldGenerator>();
        memorySize = world.levelSize * 2 - 1;
        initialPosX = memorySize / 2 ;
        initialPosY = memorySize / 2 ;
        posX = initialPosX;
        posY = initialPosY;
        numberRoomLeft = world.levelSize * world.levelSize;
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
		Debug.Log ("ActionList Count : " + actionList.Count);
        if (actionList.Count == 0)
        {
            SetBeliefs(); // Pourcentage of possibilities
            actionList = FindClosestRoomPath(FindHighScoredRooms()); // List of Rooms to visit
        }
        MakeAction(actionList); // Action taken over the rooms to visit
    }

    private void MakeAction(List<Vector2> rooms)
    {
        if (rooms.Count > 1)
        {
            MakeMove(rooms[0]);
			Debug.Log ("rooms.0 ? " + rooms[0]);
            rooms.RemoveAt(0);
        }
        else if (rooms.Count == 1)
        {
            int coordX = (int)rooms[0].x;
            int coordY = (int)rooms[0].y;
            Room[] keys = new Room[knownLevel[coordX, coordY].Count]; 
			Debug.Log ("Next room Coordinates : " + "X : " + coordX + " Y : " + coordY);
			Debug.Log ("how many keys in the next room ? : " + keys.Length);
            knownLevel[coordX, coordY].Keys.CopyTo(keys,0);
            foreach (Room room in keys)
            {
                float roomChance = 0f;
                if (knownLevel[coordX, coordY].TryGetValue(room, out roomChance))
                {
                    if (room is Monster)
                    {
                        if (roomChance > monsterThreshold)
                        {
                            ThrowRock(new Vector2(coordX-posX, coordY-posY));
                        }
                        else
                        {
                            MakeMove(rooms[0]);
							Debug.Log ("rooms.0 ? " + rooms[0]);
                            rooms.RemoveAt(0);
                            break;
                        }
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
            MoveDown();
        }
        if (room.y > posY)
        {
            MoveUp();
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
        if (i < 0 || i >= memorySize || j < 0 || j >= memorySize)
            return false;
        if (knownLevel[i, j].Count == 1)
            return false;
        if (knownLevel[i, j].Count == 0)
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
        UpdateCurrentState();
        CheckForBorder();
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
        Room[] keys = new Room[knownLevel[i,j].Count];
        knownLevel[i, j].Keys.CopyTo(keys, 0);
        foreach (Room room in keys)
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
        if(i-1 > 0)
        {
            count = majPredictorsCount(i - 1, j, room, count);
            if (count == -1 || count == 5) return count;
        }
        if(i+1 < memorySize)
        {
            count = majPredictorsCount(i + 1, j, room, count);
            if (count == -1 || count == 5) return count;
        }
        if(j-1 > 0)
        {
            count = majPredictorsCount(i, j - 1, room, count);
            if (count == -1 || count == 5) return count;
        }
        if(j+1 < memorySize)
        {
            count = majPredictorsCount(i, j + 1, room, count);
        }

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
                            if(k>0 && k<memorySize && l>0 && l < memorySize)
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

    /* Fonctionnement Validé */
    private List<Vector2> FindHighScoredRooms()
    {
        Dictionary<Room, float>[,] goodRooms = (Dictionary < Room, float>[,]) GetEligibleRooms().Clone();
        List<Vector2> rooms = new List<Vector2>();
        float actualMaxScore = float.MinValue;
        for (int i = 0; i < memorySize; i++)
        {
            for (int j = 0; j < memorySize; j++)
            {
                if (goodRooms[i, j]!=null)
                {
                    float score = GetScoreOfCase(goodRooms[i, j]);
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
		Debug.Log ("FCRP : eligibleRooms Count : " + eligibleRooms.Count);
        int potentialRoomSize = memorySize * memorySize;
        int nbLine = memorySize;

        int[] potentialRooms = new int[potentialRoomSize];
        for (int i = 0; i < potentialRoomSize; i++)
        {
            int coordX = (i % nbLine);
            int coordY = (i / nbLine);
            if (knownLevel[coordX, coordY].Count == 1 || eligibleRooms.Contains(new Vector2(coordX, coordY)))
            {
                potentialRooms[i] = 1;
            }
            else
            {
                potentialRooms[i] = 10000;
            }
        }
        int minLength = int.MaxValue;
        List<Vector2> minPath = new List<Vector2>();
        foreach (Vector2 eligibleRoom in eligibleRooms)
        {
            int length = int.MaxValue;
            List<Vector2> path = new List<Vector2>();
            int rootCol = posX;
            int rootLine = posY;
            int rootId = rootLine * nbLine + rootCol;
            int endCol = (int)eligibleRoom.x;
            int endLine = (int)eligibleRoom.y;
            int endId = endLine * nbLine + endCol;
            path = Disjtra(new Graph(potentialRooms, potentialRoomSize), rootId, endId, out length);
            if (length < minLength)
            {
                minLength = length;
                minPath = path;
            }
        }
        return minPath;
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
        posY++;
        transform.Translate(Vector3.up);
        Move();
    }

    private void MoveDown()
    {
        posY--;
        transform.Translate(Vector3.down);
        Move();
    }

    private void MoveRight()
    {
        posX++;
        transform.Translate(Vector3.right);
        Move();
    }

    private void MoveLeft()
    {
        posX--;
        transform.Translate(Vector3.left);
        Move();
    }

    private void Move()
    {
        Data.addScore(Data.moveScore);
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
        Data.addScore(Data.rockScore);
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
            knownLevel[posX, posY].Add(world.GetRoom(posX -initialPosX, posY-initialPosY), 1);
            numberRoomLeft--;
        }
    }

    private void CheckStatut()
    {
        if (knownLevel[posX, posY].Count == 1)
        {
            foreach (Room val in knownLevel[posX, posY].Keys)
            {
                if ((val is Hole || val is Monster))
                    Die();
                if (val is Exit)
                    CompleteLevel();
            }
        }
    }

    private void Die()
    {
		actionList.Clear ();
        Debug.Log("Die");
        Data.addScore(Data.deathScore);
        posX = initialPosX;
        posY = initialPosY;
        transform.position = initialPosition;
    }

    private void CompleteLevel()
    {
        Debug.Log("CompleteLevel");
        Data.addScore(Data.exitScore);
        Data.IncreaseLevel();
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }


    public List<Vector2> Disjtra(Graph G, int idRoot, int idEnd, out int length)
    {
        List<Vector2> optimalPath = new List<Vector2>();
        G.graphNodes[idRoot].score = 0;
        List<Node> remainingNodes = G.graphNodes;
        Node sdeb = remainingNodes[idRoot];
        Node sfin = remainingNodes[idEnd];
        int pathLength = 0;
        while (remainingNodes.Count > 0)
        {
            Node s1 = findminScore(remainingNodes);
            remainingNodes.Remove(s1);
            updateScores(s1);
        }

        Node s = sfin;
        while (s != sdeb)
        {
            optimalPath.Add(new Vector2(s.column, s.line));
            s = s.father;
            pathLength++;
        }
        length = pathLength;
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

    public void updateScores(Node s1)
    {
        List<Arc> arcs = s1.arcs;

        foreach(Arc arc in arcs)
        {
            int newScore = s1.score + arc.weight;
            if(arc.finish.score > newScore)
            {
                arc.finish.score = newScore;
                arc.finish.father = s1;
            }
        }
    }
}
