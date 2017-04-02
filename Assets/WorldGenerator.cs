using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public static int roomSize;

    public GameObject emptyRoom;

    public GameObject wind;
    public GameObject poop;

    public GameObject monster;
    public float monsterRate;
    //public GameObject poop;

    public GameObject hole;
    public float holeRate;
    //public GameObject wind;

    public GameObject exit;
    public GameObject start;
    public int levelSize;
    //public Data data;
    public Room[,] level;
    public GameObject[,] levelObjects;
    public Vector2 startPosition;

    // Use this for initialization
    void Start()
    {
        Room.poop = poop;
        Room.wind = wind;
        //data = FindObjectOfType<Data>();
        monsterRate %= 1;
        holeRate %= 1;
        levelSize = 3 + Data.Level;
        level = new Room[levelSize, levelSize];
        levelObjects = new GameObject[levelSize, levelSize];
        AddStartAndExit(levelSize);
        List<Vector2> monsters = new List<Vector2>();
        List<Vector2> holes = new List<Vector2>();

        for (int i = 0; i < levelSize; i++)
        {
            for (int j = 0; j < levelSize; j++)
            {
                if (level[i, j] == null)
                {
                    float score = UnityEngine.Random.value;
                    GameObject toInstantiate = emptyRoom;
                    if (score < monsterRate)
                    {
                        toInstantiate = monster;
                        monsters.Add(new Vector2(i, j));
                        level[i, j] = new Monster();
                    }
                    else if (score < monsterRate + holeRate)
                    {
                        toInstantiate = hole;
                        holes.Add(new Vector2(i, j));
                        level[i, j] = new Hole();
                    }
                    else
                        level[i, j] = new EmptyRoom();
                    levelObjects[i, j] = Instantiate(toInstantiate, new Vector3(i * roomSize, j * roomSize, 0), transform.rotation, transform.parent);
                }
            }
        }

        foreach (Vector2 position in monsters)
        {
            for (int i = -1; i < 2; i += 2)
            {

                if ((int)position.x + i >= 0 && (int)position.x + i < levelSize)
                {
                    level[(int)position.x + i, (int)position.y].AddPoop();
                    Instantiate(poop, new Vector3(position.x + i, position.y, 0), transform.rotation, transform.parent);
                }
                if ((int)position.y + i >= 0 && (int)position.y + i < levelSize)
                {
                    level[(int)position.x, (int)position.y + i].AddPoop();
                    Instantiate(poop, new Vector3(position.x, position.y + i, 0), transform.rotation, transform.parent);
                }
            }
        }

        foreach (Vector2 position in holes)
        {
            for (int i = -1; i < 2; i += 2)
            {
                if ((int)position.x + i >= 0 && (int)position.x + i < levelSize)
                {
                    level[(int)position.x + i, (int)position.y].AddWind();
                    Instantiate(wind, new Vector3(position.x + i, position.y, 0), transform.rotation, transform.parent);
                }
                if ((int)position.y + i >= 0 && (int)position.y + i < levelSize)
                {
                    level[(int)position.x, (int)position.y + i].AddWind();
                    Instantiate(wind, new Vector3(position.x, position.y + i, 0), transform.rotation, transform.parent);
                }
            }
        }

    }

    private void AddStartAndExit(int levelSize)
    {
        int instantiatedObject = 0;
        while (instantiatedObject < 2)
        {
            int x = Random.Range(0, levelSize);
            int y = Random.Range(0, levelSize);
            if (level[x, y] == null)
            {
                GameObject toInstantiate = instantiatedObject == 0 ? start : exit /*Exit*/;
                if (instantiatedObject == 0)
                    startPosition = new Vector2(x, y);
                //ToDo replace GameObject by player via Instantiate.
                level[x, y] = Instantiate(toInstantiate, new Vector3(x * roomSize, y * roomSize, 0), transform.rotation, transform.parent).GetComponent<Room>();
                instantiatedObject++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Room GetRoom(int x, int y)
    {
        if ((int)startPosition.x + x < 0 || (int)startPosition.x + x >= levelSize || (int)startPosition.y + y < 0 || (int)startPosition.y + y >= levelSize)
            return null;
        return level[(int)startPosition.x + x, (int)startPosition.y + y];
    }

    public void TryKillMonsterAt(int x, int y)
    {
        if (level[(int)startPosition.x + x, (int)startPosition.y + y] is Monster)
        {
            GameObject tmp = levelObjects[(int)startPosition.x + x, (int)startPosition.y + y];
            levelObjects[(int)startPosition.x + x, (int)startPosition.y + y] = Instantiate(emptyRoom, tmp.transform.position, tmp.transform.rotation, tmp.transform.parent);
            DestroyImmediate(tmp);
            level[(int)startPosition.x + x, (int)startPosition.y + y] = new Room(level[(int)startPosition.x + x, (int)startPosition.y + y]);
        }
    }
}
