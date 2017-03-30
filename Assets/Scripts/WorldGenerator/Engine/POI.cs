using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POI : MonoBehaviour
{
    //int Seed { get; set; }
    public float x;
    public float y;
    public static POI prefab;
    public LocalWorldGenerator world;
    public static POI Create(int seed)
    {
        POI newPOI = Instantiate<POI>(prefab);
        Random.InitState(seed);
        newPOI.x = Random.Range(-WorldGenerator.maxX, WorldGenerator.maxX);
        newPOI.y = Random.Range(-WorldGenerator.maxY, WorldGenerator.maxY);
        return newPOI;
    }

    void Start()
    {

    }

    void Update()
    {

    }
}