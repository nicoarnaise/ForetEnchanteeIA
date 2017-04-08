using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldGenerator : MonoBehaviour
{

    public int roomSize = 300;

    public GameObject emptyRoomObj;

    public GameObject windObj;
    public GameObject poopObj;

    public GameObject monsterObj;
    public float monsterRate;
    //public GameObject poop;

    public GameObject holeObj;
    public float holeRate;
    //public GameObject wind;

    public GameObject exitObj;
    public GameObject startObj;
    public GameObject AIObj;
    public Button PlayButton;
	public Button AutoButton;

    public int levelSize;
    //public Data data;
    public Room[,] level;
    public GameObject[,] levelObjects;
    public Vector2 startPosition;

    // Use this for initialization
    void Start()
    {
        Room.poop = poopObj;
        Room.wind = windObj;
        //-move the camera--
        int camXY = 300 + Data.Level * 150;
        int camZ = -1000 - Data.Level * 300;
        Camera.main.transform.position = new Vector3(camXY, camXY, camZ);
        //------------------
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
                    GameObject toInstantiate = emptyRoomObj;
                    if (score < monsterRate)
                    {
                        toInstantiate = monsterObj;
                        monsters.Add(new Vector2(i, j));
                        level[i, j] = new Monster();
                    }
                    else if (score < monsterRate + holeRate)
                    {
                        toInstantiate = holeObj;
                        holes.Add(new Vector2(i, j));
                        level[i, j] = new Hole();
                    }
                    else
                        level[i, j] = new EmptyRoom();

                    levelObjects[i, j] = Instantiate(toInstantiate, new Vector3(i*roomSize, j*roomSize, 0), transform.rotation, transform.parent);
                    levelObjects[i, j].transform.localScale = new Vector3(roomSize, roomSize, 1);
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
                    Instantiate(poopObj, new Vector3((position.x + i) * roomSize, position.y * roomSize, 0), transform.rotation, levelObjects[(int)position.x,(int)position.y].transform);
                }
                if ((int)position.y + i >= 0 && (int)position.y + i < levelSize)
                {
                    level[(int)position.x, (int)position.y + i].AddPoop();
                    Instantiate(poopObj, new Vector3(position.x * roomSize, (position.y + i) * roomSize, 0), transform.rotation, levelObjects[(int)position.x, (int)position.y].transform);
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
                    Instantiate(windObj, new Vector3((position.x + i) * roomSize, position.y * roomSize, 0), transform.rotation, transform.parent).transform.localScale = new Vector3(roomSize, roomSize, 1);
                }
                if ((int)position.y + i >= 0 && (int)position.y + i < levelSize)
                {
                    level[(int)position.x, (int)position.y + i].AddWind();
                    Instantiate(windObj, new Vector3(position.x * roomSize, (position.y + i)*roomSize, 0), transform.rotation, transform.parent).transform.localScale = new Vector3(roomSize, roomSize, 1);
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
                Room toAdd;
                if (instantiatedObject == 0)
                    toAdd = new EmptyRoom();
                else
                    toAdd = new Exit();
                GameObject toInstantiate = instantiatedObject == 0 ? startObj : exitObj /*Exit*/;
                if (instantiatedObject == 0)
                {
                    startPosition = new Vector2(x, y);
                    GameObject pTemp = Instantiate(AIObj, new Vector3(x*roomSize, y*roomSize, -1), transform.rotation, transform.parent);
                    pTemp.transform.localScale = new Vector3(roomSize, roomSize, 1);
                    PlayButton.onClick.AddListener(() => pTemp.GetComponent<AI>().Play());
					AutoButton.onClick.AddListener(() => pTemp.GetComponent<AI>().AutoPlay());
                }    
                level[x, y] = toAdd;
                Instantiate(toInstantiate, new Vector3(x*roomSize, y*roomSize, 0), transform.rotation, transform.parent).transform.localScale = new Vector3(roomSize, roomSize, 1);
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
        int toKillX = (int)startPosition.x + x;
        int toKillY = (int)startPosition.y + y;
        if (level[toKillX, toKillY] is Monster)
        {
			DestroyImmediate(levelObjects [toKillX, toKillY]);
			levelObjects[toKillX, toKillY] = Instantiate(emptyRoomObj, new Vector3(toKillX*roomSize,toKillY*roomSize,0), Quaternion.identity, transform.parent);
			levelObjects[toKillX, toKillY].transform.localScale = new Vector3(roomSize, roomSize, 1);
            for (int i = -1; i<2; i += 2)
            {
                for(int j = -1; j<2; j += 2)
                {
					if (toKillX + i > 0 && toKillX + i < levelSize && toKillY + j > 0 && toKillY + j < levelSize) {
						level [toKillX + i, toKillY + j].hasPoop = false;
					}
                }
            }
			EmptyRoom temp = new EmptyRoom();
			temp.hasPoop = level [toKillX, toKillY].hasPoop;
			temp.hasWind = level [toKillX, toKillY].hasWind;
			level [toKillX, toKillY] = temp;
        }
    }
}
