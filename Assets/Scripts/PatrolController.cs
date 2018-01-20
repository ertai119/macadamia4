using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolController : MonoBehaviour {

    Vector3[] paths;
    Vector3 curPos;
    Vector3 nextPos;
    int nextPosIndex = 0;

    bool start;

	// Use this for initialization
	void Start ()
    {
        curPos = transform.position;

        if (paths != null && paths.Length > 0)
        {
            SetNextPos();
        }
	}

    void SetNextPos()
    {
        nextPos = paths[(nextPosIndex + 1) % paths.Length];
        nextPosIndex++;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (start == false)
            return;
	}
}
