using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AC3
{
    //private void QueueChecker(Queue<Arc> queue)
    //{
    //    while (queue.Count>0)
    //    {
    //    }
    //}

    public static void Execute(ref Dictionary<Vector3, List<Room>> csp)
    {
        Queue<Arc> queue = GenerateArcsQueue(csp);
        while (queue.Count > 0)
        {
            Arc arc = queue.Dequeue();
            if (RemoveInconsistentValues(arc, ref csp))
            {
                foreach (Arc neighbor in GetNeighbors(csp, arc.roomI))
                {
                    queue.Enqueue(neighbor.GetReverseArc());
                }
            }
        }
    }

    private static bool RemoveInconsistentValues(Arc arc, ref Dictionary<Vector3, List<Room>> csp)
    {
        bool removed = false;
        List<Room> toRemove = new List<Room>();
        foreach (Room room in csp[arc.roomI])
        {
            if (!isConstraintCompliant(room, csp[arc.roomJ]))
            {
                toRemove.Add(room);
                removed = true;
            }
        }
        if (toRemove.Count >= csp[arc.roomI].Count)
        {
            return false;
        }
        for (int i = 0; i < toRemove.Count; i++)
        {
            //arc.roomI.Remove(toRemove[i]);
            csp[arc.roomI].Remove(toRemove[i]);

        }
        return removed;
    }

    private static bool isConstraintCompliant(Room room, List<Room> roomJ)
    {
        int nbCandidate = roomJ.Count;
        foreach (Room candidate in roomJ)
        {
            foreach (RoomRule rule in room.rules)
            {
                if (!rule.self.Equals(room))
                    Debug.Log("Mahna Mahna !!");
                //bool isAdmissible = true;
                if (rule.isConstrained(candidate))
                {
                    if (!rule.isAdmissible(candidate))
                    {
                        //futureRoomJ.Remove(candidate);
                        nbCandidate--;
                        //isAdmissible = false;
                        break;
                    }
                }
            }
        }
        return nbCandidate > 0;
    }

    private static Queue<Arc> GenerateArcsQueue(Dictionary<Vector3, List<Room>> csp)
    {
        Queue<Arc> arcs = new Queue<Arc>();
        foreach (Vector3 variable in csp.Keys)
        {
            List<List<Room>> neighbors = new List<List<Room>>();

            Queue<Arc> tmp = GetNeighbors(csp, variable);
            foreach (Arc arc in tmp)
            {
                arcs.Enqueue(arc);
            }
        }
        return arcs;
    }

    public static Queue<Arc> GetNeighbors(Dictionary<Vector3, List<Room>> csp, Vector3 variable)
    {
        Queue<Arc> output = new Queue<Arc>();
        foreach (Room room in csp[variable])
        {
            foreach (RoomRule rule in room.rules)
            {
                foreach (Vector3 neigborPosition in rule.GetConstrainedPositions())
                {
                    if (csp.ContainsKey(neigborPosition + room.position))
                    {
                        Arc toAdd = new Arc(room.position, neigborPosition + room.position);
                        if (!output.Contains(toAdd))
                            output.Enqueue(new Arc(room.position, neigborPosition + room.position));
                    }
                }
            }
        }
        return output;
    }
}
