using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GuardController : MonoBehaviour {
    
    NavMeshAgent agent;
    Vector3 goalPosition;
    bool patrollMode = false;
	
	void Start ()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = false;
	}
	
    public void PatrolMode()
    {
        if (patrollMode)
            return;
        
        patrollMode = true;
        SetGoalPos(GetComponent<PatrolManager>().GetNextPos());
    }

    public void TrackingMode(Vector3 targetPos)
    {
        patrollMode = false;
        SetGoalPos(targetPos);
    }

    public void SetGoalPos(Vector3 position)
    {
        goalPosition = position;

        if (agent)
        {
            agent.enabled = true;
            agent.destination = goalPosition;
        }
    }

	void Update()
    {
        if (agent.enabled)
        {
            if (!agent.pathPending && agent.remainingDistance < 1.0f)
            {
                if (patrollMode)
                {
                    if (GetComponent<PatrolManager>().HasPath())
                    {
                        SetGoalPos(GetComponent<PatrolManager>().GetNextPos());
                    }
                    else
                    {
                        agent.enabled = false;
                    }
                }
                else
                {
                    agent.enabled = false;
                }
            }    
        }
        		
	}
}
