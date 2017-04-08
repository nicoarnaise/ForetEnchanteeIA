using System;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{

	/// <summary>
	/// Beliefs Matrix and its size
	/// </summary>
    public Dictionary<Room, float>[,] knownLevel;
	private int memorySize;

	/// <summary>
	/// Initial Agent Position
	/// </summary>
    private int initialPosX;
    private int initialPosY;
    private Vector3 initialPosition;

	/// <summary>
	/// Agent position
	/// </summary>
    public int posX;
    public int posY;

	/// <summary>
	/// The real world.
	/// </summary>
    private WorldGenerator world;

	/// <summary>
	/// Value determining whether or not the Agent should throw a rock
	/// </summary>
    private float monsterThreshold = 0.6f;

	/// <summary>
	/// Score given to a room where nothing should happen
	/// </summary>
    private float emptyRoomScore = 0.0f;

	/// <summary>
	/// The number of room left to visit
	/// </summary>
    private int numberRoomLeft;

	/// <summary>
	/// The action list : The path containing the next rooms to go 
	/// The last element countains its goal room to explore
	/// </summary>
    private List<Vector2> actionList;

	/// <summary>
	/// Initializing the position of the agent
	/// Initializing the beliefs Matrix of the agent
	/// </summary>
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

	/// <summary>
	/// Main IA function according to BDI
	/// </summary>
    public void Play()
    {
        if (actionList.Count == 0) // Testing if he still have actions to do 
        {
            SetBeliefs(); // Set pourcentage of each possibilities for each room of knownLevel
            actionList = FindClosestRoomPath(FindHighScoredRooms()); // List of Rooms to visit
        }
        MakeAction(actionList); // Do the correct action taken over the rooms to visit.
    }

	/// <summary>
	/// Makes the action corresponding to the first element of the room list
	/// </summary>
	/// <param name="rooms">List of rooms to visit</param>
    private void MakeAction(List<Vector2> rooms)
    {
            int coordX = (int)rooms[0].x; // X coordinate of the room position
            int coordY = (int)rooms[0].y; // Y coordinate of the room position
            Room[] keys = new Room[knownLevel[coordX, coordY].Count]; // Number of possibilities left for the room
			
            knownLevel[coordX, coordY].Keys.CopyTo(keys,0); // Copy the different possibilities

            foreach (Room room in keys) // According to each possibility
            {
                if(knownLevel[coordX,coordY].Count == 1) // Only one possibility 
                {
                    if(room is Monster) // And it's a monster
                    {
                        ThrowRock(new Vector2(coordX - posX, coordY - posY)); // Throw a rock
                        break;
                    }
                    else
                    {
                        MakeMove(rooms[0]); // Move to the room
                        if (rooms.Count > 0) // In case the agent dies, the list of rooms is automatically cleared so we need to check this case
                            rooms.RemoveAt(0); // Remove the room
                    }
                }
                else // All possibilities are left 
                {
                    float roomChance = 0f;
                    if (knownLevel[coordX, coordY].TryGetValue(room, out roomChance))
                    {
                        if (room is Monster) // only this possibility matters because it will determine the next action
                        {
                            if (roomChance > monsterThreshold) // The probability of a monster is too high
                            {
                                ThrowRock(new Vector2(coordX-posX, coordY-posY)); // Throw a rock
                                break;
                            }
                            else // The probability os a monster is low enough
                            {
                                MakeMove(rooms[0]); // Move to the room
							if(rooms.Count > 0) // In case the agent dies, the list of rooms is automatically cleared so we need to check this case
                                    rooms.RemoveAt(0); // Remove the room
                                break;
                            }
                        }
                    }
                }
                
            }
    }

	/// <summary>
	/// Makes the good move according to the position of the room
	/// </summary>
	/// <param name="room">Room.</param>
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

	/// <summary>
	/// Gets the list of unvisited rooms at the frontier of the known map.
	/// </summary>
	/// <returns>The eligible rooms.</returns>
    private Dictionary<Room, float>[,] GetEligibleRooms()
    {
        Dictionary<Room, float>[,] eligibleRooms = knownLevel.Clone() as Dictionary<Room, float>[,]; // Copy of the Beliefs Matrix
        for (int i = 0; i < memorySize; i++)
        {
            for (int j = 0; j < memorySize; j++)
            {
                if (!IsEligible(i, j))
					eligibleRooms[i, j] = null; // Only the unvisited rooms at the frontier of the known map aren't set to null
            }
        }
        return eligibleRooms;
    }

	/// <summary>
	/// Determines whether the room in the belief Matrix at i,j is an unvisited room at the frontier of the known map
	/// </summary>
	/// <returns><c>true</c> if the room in the belief Matrix at i,j is an unvisited room at the frontier of the known map; otherwise, <c>false</c>.</returns>
	/// <param name="i">column</param>
	/// <param name="j">line</param>
    private bool IsEligible(int i, int j)
    {
        if (i < 0 || i >= memorySize || j < 0 || j >= memorySize) // if outside of the matrix
            return false;
        if (knownLevel[i, j].Count == 1) // if already visited
            return false;
        if (knownLevel[i, j].Count == 0) // if outside of the world
            return false;
        for (int k = -1; k < 2; k += 2)
        {

            if (i + k >= 0 && i + k < memorySize)
            {
                if (knownLevel[i + k, j].Count == 1) // if a left or right neighbour has been visited
                    return true;
            }
            if (j + k >= 0 && j + k < memorySize)
            {
                if (knownLevel[i, j + k].Count == 1) // if a top or bottom neighbour has been visited
                    return true;
            }
        }
        return false; // In any other case
    }

	/// <summary>
	/// Sets the beliefs of the cases the rooms that are unvisited room at the frontier of the known map
	/// </summary>
    private void SetBeliefs()
    {
        UpdateCurrentState(); // Updates the Belief Matrix at the current agent position 
        CheckForBorder(); // Checks if the agent is next to the real map boarder, in which case it changes its Belief Matrix for each case that aren't in the real map
        for (int i = 0; i < memorySize; i++)
        {
            for (int j = 0; j < memorySize; j++)
            {
                if (IsEligible(i, j)) 
                {
                    SetProbabilities(i, j); // Set the probabilities of the room at the i,j position in the belief matrix
                }
            }
        }
    }

	/// <summary>
	/// // Set the probabilities of the room at the i,j position in the belief Matrix
	/// </summary>
	/// <param name="i">column</param>
	/// <param name="j">line</param>
    private void SetProbabilities(int i, int j)
    {
        float total = 0f;
        Room[] keys = new Room[knownLevel[i,j].Count];
        knownLevel[i, j].Keys.CopyTo(keys, 0);
        foreach (Room room in keys)
        {
			if (!(room is EmptyRoom))
			{
				if (room is Exit) // Sets the probability of the room being an Exit
	            {
	                knownLevel[i, j][room] = ((float)numberRoomLeft / (float)(world.levelSize * world.levelSize)); 
	            }
				if (room is Monster) // Sets the probability of the room being a Monster
	            {
	                knownLevel[i, j][room] = getRoomProb(i, j, room, world.monsterRate);
	            }
				if (room is Hole) // Sets the probability of the room being a Hole
	            {
	                knownLevel[i, j][room] = getRoomProb(i, j, room, world.holeRate);
	            }
                total += knownLevel[i, j][room]; 
            }
        }
		foreach (Room room in keys) {
			if (room is EmptyRoom) { // Sets the probability of the room being an Empty Room
				knownLevel [i, j] [room] = 1 - total;
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

    /// <summary>
    /// Returns the list of rooms with the highest scores
    /// </summary>
    /// <returns>The high scored rooms.</returns>
    private List<Vector2> FindHighScoredRooms()
    {
        Dictionary<Room, float>[,] goodRooms = (Dictionary < Room, float>[,]) GetEligibleRooms().Clone(); // copy the matrix of the eligible rooms list
        List<Vector2> rooms = new List<Vector2>();
        float actualMaxScore = float.MinValue; // Reference to max score to update

        for (int i = 0; i < memorySize; i++)
        {
            for (int j = 0; j < memorySize; j++)
            {
                if (goodRooms[i, j]!=null) // the room is eligible
                {
                    float score = GetScoreOfCase(goodRooms[i, j]); // Get the score of the room
                    if (score >= actualMaxScore) // if the room score is at least as high as the current highest score
                    {
                        if (score > actualMaxScore) // if the room score is even higher than the current highest score
                            rooms.Clear(); // then the list of rooms with the highest scores is cleared
                        actualMaxScore = score; // in any of these case, the MaxScore is Reset
                        rooms.Add(new Vector2(i, j)); // the room is added to the highest scores room list
                    }
                }
            }
        }
        return rooms; // return the list
    }

	/// <summary>
	/// Returns the list with the room that should be visited
	/// </summary>
	/// <returns>The path.</returns>
	/// <param name="eligibleRooms">Rooms with the highest score</param>
    private List<Vector2> FindClosestRoomPath(List<Vector2> eligibleRooms)
    {

		/***************************************/
		/*********GRAPH ARRAY CREATION**********/
		/***************************************/

        int potentialRoomSize = memorySize * memorySize; // Size of the graph array
        int nbLine = memorySize; 


        int[] potentialRooms = new int[potentialRoomSize]; // Graph Array creation, countains the score associated to each room to initialize the arcs weights going to those rooms
        for (int i = 0; i < potentialRoomSize; i++)
        {
            int coordX = (i % nbLine); // Converts the position of the array in Column
            int coordY = (i / nbLine); // Converts the position of the array in Line
            bool checkSafe = false;
            if(knownLevel[coordX,coordY].Count == 1)  // the room is known
            {
                foreach(Room r in knownLevel[coordX, coordY].Keys)
                {
                    checkSafe = r is EmptyRoom;
                }
            }
            if (checkSafe || eligibleRooms.Contains(new Vector2(coordX, coordY))) // The room is empty or is one of the room with the highest score
            {
                potentialRooms[i] = 1;
            }
            else if(knownLevel[coordX,coordY].Count > 1) // The room is not visited
            {
                potentialRooms[i] = 100000;
            }
            else
            {
                if(knownLevel[coordX,coordY].Count == 0) // The room is not in the map
                {
                    potentialRooms[i] = 1000000000;
                }
                else
                {
                    foreach (Room r in knownLevel[coordX, coordY].Keys)
                    {
                        if (r is Monster)
                            potentialRooms[i] = 10000; // The room is a monster
                        if (r is Hole)
                            potentialRooms[i] = 10000000; // The room is a Hole
                    }
                }
            }
        }

		/***********************************************************/
		/*********DIJKSTRA algorithm applied to each rooms**********/
		/***********************************************************/

        int minLength = int.MaxValue; // Reference to the minimum distance to the goal room
        List<Vector2> minPath = new List<Vector2>(); // Reference to the Optimal Path to the goal room
        foreach (Vector2 eligibleRoom in eligibleRooms) // Determining the closest room
        {
            int length = int.MaxValue; // Distance to the goal room
            List<Vector2> path = new List<Vector2>(); // Shortest path to the goal room

			// Root node info
            int rootCol = posX; 
            int rootLine = posY;
            int rootId = rootLine * nbLine + rootCol;

			// end node info
            int endCol = (int)eligibleRoom.x;
            int endLine = (int)eligibleRoom.y;
            int endId = endLine * nbLine + endCol;

			// Dikstra applied to the graph with the node root being the current position and end root being the goal position
            path = Dijkstra(new Graph(potentialRooms, potentialRoomSize), rootId, endId, out length);

            if (length < minLength) // updating the global minimum distance and optimal path
            {
                minLength = length;
                minPath = path;
            }
        }
        return minPath; // Return the optimal Path
    }


	/// <summary>
	/// Gets the score of the room.
	/// </summary>
	/// <returns>The score of room.</returns>
	/// <param name="dictionary">Dictionary.</param>
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
                if (room is Exit) // Adds the score of the exit probability
                {
                    totalScore += roomChance * Data.exitScore;
                }
                if (room is Hole) // Adds the score of the hole probability
                {
                    totalScore += roomChance * Data.deathScore;
                }
                if (room is Monster) // Adds the score of the monster probability 
                {
                    totalScore += roomChance > monsterThreshold ? roomChance * Data.rockScore : roomChance * Data.deathScore;
                }
                if (room is EmptyRoom) // Adds the score of the room being empty probability
                {
                    totalScore += roomChance * emptyRoomScore;
                }
            }
        }
        return totalScore; // return the score of the room
    }

	/// <summary>
	/// Moves up.
	/// </summary>
    private void MoveUp() 
    {
		posY++; 
        transform.Translate(Vector3.up);
        Move();
    }

	/// <summary>
	/// Moves down.
	/// </summary>
    private void MoveDown()
    {
        posY--;
        transform.Translate(Vector3.down);
        Move();
    }

	/// <summary>
	/// Moves right.
	/// </summary>
    private void MoveRight()
    {
        posX++;
        transform.Translate(Vector3.right);
        Move();
    }

	/// <summary>
	/// Moves left.
	/// </summary>
    private void MoveLeft()
    {
        posX--;
        transform.Translate(Vector3.left);
        Move();
    }

	/// <summary>
	/// Move action consequences.
	/// </summary>
    private void Move()
    {
        Data.addScore(Data.moveScore); // Update global Score
		CheckForBorder(); // Checks if the agent is next to the real map boarder, in which case it changes its Belief Matrix for each case that aren't in the real map
		UpdateCurrentState(); // Updates the Belief Matrix at the current agent position 
        CheckStatut(); // Checks the effect of the room on the agent
    }

	/// <summary>
	/// Clears the Belief Matrix of the rooms outside the border
	/// </summary>
    private void CheckForBorder()
    {
        if (world.GetRoom(posX - initialPosX + 1, posY - initialPosY) == null) // Right border
        {
            for (int i = posX + 1; i < memorySize; i++)
            {
                for (int j = 0; j < memorySize; j++)
                {
                    knownLevel[i, j].Clear();
                }
            }
        }
        if (world.GetRoom(posX - initialPosX - 1, posY - initialPosY) == null) // Left border
        {
            for (int i = posX - 1; i >= 0; i--)
            {
                for (int j = 0; j < memorySize; j++)
                {
                    knownLevel[i, j].Clear();
                }
            }
        }
        if (world.GetRoom(posX - initialPosX, posY - initialPosY + 1) == null) // Top border
        {
            for (int i = 0; i < memorySize; i++)
            {
                for (int j = posY + 1; j < memorySize; j++)
                {
                    knownLevel[i, j].Clear();
                }
            }
        }
        if (world.GetRoom(posX - initialPosX, posY - initialPosY - 1) == null) // Bottom border
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

	/// <summary>
	/// Throws the rock towards a direction vector
	/// </summary>
	/// <param name="direction">Direction.</param>
    private void ThrowRock(Vector2 direction)
    {
		// calls the world to change the real map
        world.TryKillMonsterAt(posX - initialPosX + (int)direction.x, posY - initialPosY + (int)direction.y);
		Data.addScore(Data.rockScore); // Update the global score
        Room toRemove = null; 
        foreach (Room item in knownLevel[posX + (int)direction.x, posY + (int)direction.y].Keys)
        {
            if (item is Monster)
            {
                toRemove = item; 
            }
        }

        if (knownLevel[posX + (int)direction.x, posY + (int)direction.y].Count == 0) // if no possiblities left for the room
        {
            knownLevel[posX + (int)direction.x, posY + (int)direction.y].Add(world.GetRoom(posX + (int)direction.x - initialPosX, posY + (int)direction.y - initialPosY), 1); // Adds the empty room possibility
        }
		knownLevel[posX + (int)direction.x, posY + (int)direction.y].Remove(toRemove); // remove the monster possibility
    }

	/// <summary>
	/// Updates the current room possibilities
	/// </summary>
    private void UpdateCurrentState()
    {
        if (knownLevel[posX, posY].Count > 1)
        {
            knownLevel[posX, posY].Clear();
            knownLevel[posX, posY].Add(world.GetRoom(posX -initialPosX, posY-initialPosY), 1);
            numberRoomLeft--;
        }
    }

	/// <summary>
	/// Checks the room and affects the agent according to its type.
	/// </summary>
    private void CheckStatut()
    {
        if (knownLevel[posX, posY].Count == 1)
        {
            foreach (Room val in knownLevel[posX, posY].Keys)
            {
                if ((val is Hole || val is Monster)) // if it's a hole or a monster
                    Die(); // the agent dies
                if (val is Exit) // if it's an exit
                    CompleteLevel(); // the level is complete 
            }
        }
    }

	/// <summary>
	/// Die effect.
	/// </summary>
    private void Die()
    {
		actionList.Clear (); // Clears the list of rooms to visit
        Data.addScore(Data.deathScore); // Updates the global score

		// reset the position
        posX = initialPosX; 
        posY = initialPosY; 
        transform.position = initialPosition;
    }

	/// <summary>
	/// Completes the level effect.
	/// </summary>
    private void CompleteLevel()
    {
        Data.addScore(Data.exitScore); // Updates the global score
        Data.IncreaseLevel(); // Increase the level 
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex, UnityEngine.SceneManagement.LoadSceneMode.Single); // Reload the scene with the new Data updates
    }

	/// <summary>
	/// Dijkstra applied to the graph G, with the root node id and end node id,
	///  returns the length of the path and the path
	/// </summary>
	/// <param name="G">the graph </param>
	/// <param name="idRoot">Identifier root.</param>
	/// <param name="idEnd">Identifier end.</param>
	/// <param name="length">Length.</param>
    public List<Vector2> Dijkstra(Graph G, int idRoot, int idEnd, out int length)
    {
        List<Vector2> optimalPath = new List<Vector2>(); // Path reference

        G.graphNodes[idRoot].score = 0; // Updates the score of root node
        List<Node> remainingNodes = G.graphNodes; // Reference to the graphNodes List
        Node sdeb = remainingNodes[idRoot]; // Reference to root node
        Node sfin = remainingNodes[idEnd]; // Reference to end node
        int pathLength = 0;

        while (remainingNodes.Count > 0) // While there are still nodes to explore
        {
            Node s1 = findminScore(remainingNodes); // Gets the node with the min score
            remainingNodes.Remove(s1); // Removes the node from the remaining list
            updateScores(s1); // Updates the scores of the nodes linked to s1
        }

        Node s = sfin; // Reference to end node
        while (s != sdeb) // While the node is not root
        {
            optimalPath.Insert(0,new Vector2(s.column, s.line)); // adds the node to the path
            s = s.father; // Sets the node to the father of the current node
            pathLength++; // Adds 1 to the current length
        }
        length = pathLength; // Returns the Shortest Path length
        return optimalPath; // Returns the Shortest Path
    }


	/// <summary>
	/// Returns the node with the lowest score
	/// </summary>
	/// <returns>The node</returns>
	/// <param name="nodes">List of nodes</param>
    public Node findminScore(List<Node> nodes)
    {
        int minScore = int.MaxValue; // reference to the minimum score
        Node minNode = null; // reference to the node with the minimum score
        for (int i = 0; i < nodes.Count; i++)
        {
            Node currentNode = nodes[i];
            int currentScore = currentNode.score;
            if (currentScore < minScore) // if the score of the node is smaller than the current minimum score
            {
                minScore = currentScore; // updates the minimum score
                minNode = currentNode; // updates the minium node
            }
        }
        return minNode; // returns the node with the lowest score
    }

	/// <summary>
	/// Updates the scores of the nodes linked to s1
	/// </summary>
	/// <param name="s1">Node S1.</param>
    public void updateScores(Node s1)
    {
        List<Arc> arcs = s1.arcs; // Reference to the arcs of s1

        foreach(Arc arc in arcs) 
        {
            int newScore = s1.score + arc.weight; // the possible new score equals s1 score and the weight of its arc to the next node
            if(arc.finish.score > newScore) // if this new score is lower than the actual score of the linked node to s1
            {
                arc.finish.score = newScore; // updates the score of the linked node
                arc.finish.father = s1; // updates the father of the linked node
            }
        }
    }
}
