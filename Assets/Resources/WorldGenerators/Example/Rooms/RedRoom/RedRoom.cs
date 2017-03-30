using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedRoom : Room
{
    public RedRoom()
    {
    }

    public RedRoom(Room room, Vector3 position) : base(room, position) { }

    public override Room GetCopy(Vector3 position)
    {
        return new RedRoom(this, position);
    }

    protected override void Initialize()
    {
        base.Initialize();
        rules.Add(new NoSameNeighborRule(this));
        canBeFirst = true;
        prefab = Resources.Load<GameObject>("WorldGenerators/Example/Rooms/RedRoom/Prefab");

    }
}