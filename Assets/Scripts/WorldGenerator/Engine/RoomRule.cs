using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomRule
{
    public Room self;
    /// <summary>
    /// Add positions, relative to this room which are constrained by the enforcements of this rule
    /// </summary>
    protected List<Vector3> constainedRooms;

    public IEnumerable<Vector3> GetConstrainedPositions() { return constainedRooms; }

    public RoomRule(Room self)
    {
        this.self = self;
        this.constainedRooms = new List<Vector3>();
    }

    public RoomRule(RoomRule rule, Room newSelf)
    {
        this.constainedRooms = rule.constainedRooms;
        this.self = newSelf;
    }

    public virtual RoomRule GetCopy(Room room)
    {
        return new RoomRule(this, room);
    }

    /// <summary>
    /// Check if the current rule is respected.
    /// </summary>
    /// <returns>True if respected, false if not</returns>
    public virtual bool isAdmissible(Room other)
    {
        return true;
    }

    public bool isConstrained(Room other)
    {
        Vector3 positionToCheck = other.position - self.position;
        return constainedRooms.Contains(positionToCheck);
    }


}