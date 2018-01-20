using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GuardController : MonoBehaviour {
    
    NavMeshAgent agent;
    Vector3 goalPosition;

	// Use this for initialization
	void Start ()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = false;
	}
	
    public void SetGoalPos(Vector3 position)
    {
        goalPosition = position;

        agent.enabled = true;
        agent.destination = goalPosition;
    }

	// Update is called once per frame
	void Update ()
    {
        if (agent == null)
            return;
        
        if (!agent.pathPending && agent.remainingDistance < 1.0f)
        {
            agent.enabled = false;
        }		
	}
}
