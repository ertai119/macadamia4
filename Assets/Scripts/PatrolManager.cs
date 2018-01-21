using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolManager : MonoBehaviour {

    [HideInInspector]
    public List<Vector3> points = new List<Vector3>();
    public float handleRadius = 5.0f;
    public bool showPointList = false;

    Vector3 curPos;
    public Vector3 nextPos;
    public int nextPosIndex = 0;

	void Start ()
    {
        curPos = transform.position;
	}

    public Vector3 GetNextPos()
    {
        if (points.Count == 0)
            return curPos;
        
        nextPos = points[(nextPosIndex)% points.Count];

        nextPosIndex++;

        if (nextPosIndex >= points.Count)
            nextPosIndex = 0;

        return nextPos;
    }

    public bool HasPath()
    {
        return points.Count > 1;
    }

    void OnDrawGizmos()
    {
        if (points.Count > 0)
        {
            Vector3 startPos = points[0];;
            Vector3 prevPos = startPos;

            foreach (Vector3 waypoint in points)
            {
                Gizmos.DrawSphere(waypoint, .3f);
                Gizmos.DrawLine(prevPos, waypoint);
                prevPos = waypoint;
            }
            Gizmos.DrawLine(startPos, prevPos);
        }
    }
}
