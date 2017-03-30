using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public bool canBeFirst;
    public GameObject prefab;
    public Vector3 position;
    public float distanceFromCenter;
    public List<RoomRule> rules;


    public Room() : this(Vector3.zero)
    {
        rules = new List<RoomRule>();
        Initialize();
    }

    public Room(Room room) : this(room, room.position) { }

    public Room(Room room, Vector3 position) : this(position)
    {
        this.canBeFirst = room.canBeFirst;
        this.prefab = room.prefab;
        this.distanceFromCenter = Vector3.Distance(Vector3.zero, position);
        this.rules = new List<RoomRule>();
        foreach (RoomRule rule in room.rules)
        {
            this.rules.Add(rule.GetCopy(this));
        }
    }

    public virtual Room GetCopy(Vector3 position)
    {
        return new Room(this, position);
    }

    public Room(Vector3 position)
    {
        this.position = position;
    }


    protected virtual void Initialize()
    {
        //ToImplement if needed in children
    }
}