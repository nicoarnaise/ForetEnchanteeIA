using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public static GameObject wind;
    public static GameObject poop;
    public bool hasPoop;
    public bool hasWind;

    public Room()
    {
        hasPoop = false;
        hasWind = false;
    }

    public Room(Room room)
    {
        hasPoop = room.hasPoop;
        hasWind = room.hasWind;
    }

    public void AddPoop()
    {
        hasWind = true;
    }

    public void AddWind()
    {
        hasWind = true;
    }
}
