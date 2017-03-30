using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Arc
{
    public Vector3 roomI;
    public Vector3 roomJ;
    public Arc() { }
    public Arc(Vector3 i, Vector3 j) { roomI = i; roomJ = j; }
    public Arc GetReverseArc()
    {
        return new Arc(roomJ, roomI);
    }

    public override int GetHashCode()
    {
        return roomI.GetHashCode() + roomJ.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj is Arc)
            if ((obj as Arc).roomI == roomI && (obj as Arc).roomJ == roomJ)
                return true;
        return false;
    }
}

