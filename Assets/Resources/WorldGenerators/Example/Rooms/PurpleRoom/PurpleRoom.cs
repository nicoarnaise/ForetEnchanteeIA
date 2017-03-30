using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurpleRoom : Room
{
    public PurpleRoom()
    {
    }

    public PurpleRoom(Room room, Vector3 position) : base(room, position) { }

    public override Room GetCopy(Vector3 position)
    {
        return new RedRoom(this, position);
    }

    protected override void Initialize()
    {
        base.Initialize();
        rules.Add(new NoSameNeighborRule(this));
        prefab = Resources.Load<GameObject>("WorldGenerators/Example/Rooms/PurpleRoom/Prefab");

    }
}