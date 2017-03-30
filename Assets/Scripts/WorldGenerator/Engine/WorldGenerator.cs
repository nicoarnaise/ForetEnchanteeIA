using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public void test(int a, int b = 1) { }
    public int seed;
    public int maxPOI;
    public float xSize;
    public float ySize;
    public static float maxX;
    public static float maxY;
    public bool randomizeSeed;
    private int nbOfPOI;
    [SerializeField]
    private List<POI> POIs;
    // Use this for initialization
    void Start()
    {
        maxX = xSize;
        maxY = ySize;

        POIs = new List<POI>();
        if (randomizeSeed)
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
        }
        Random.InitState(seed);
        nbOfPOI = seed % maxPOI;
        for (int i = 0; i < nbOfPOI; i++)
        {
            POI newPOI = POI.Create(Random.Range(int.MinValue, int.MaxValue));

        }
    }

    // Update is called once per frame
    void Update()
    {


    }
}
