using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour {

	public float speed = 5;
	public float waitSec = .3f;
	public float turnSpeed = 90;

    Vector3 backPosition;

	public Light spotLight;
	public float viewDistance;
    public float maxFollowDst;
	public LayerMask viewMask;
	float viewAngle;

	Transform player;
	public Transform pathHolder;
	Color originalSpotlightColor;
    Color originalColor;

	void Start(){
		player = GameObject.FindGameObjectWithTag ("Player").transform;
        backPosition = transform.position;

        originalColor = GetComponent<Renderer>().material.color;

        if (spotLight)
        {
            originalSpotlightColor = spotLight.color;
            viewAngle = spotLight.spotAngle;    
        }		

        if (pathHolder != null)
        {
            Vector3[] waypoints = new Vector3[pathHolder.childCount];
            for (int i = 0; i < waypoints.Length; i++)
            {
                waypoints[i] = pathHolder.GetChild(i).position;
                waypoints[i].y = transform.position.y;
            }

            StartCoroutine(FollowPath(waypoints));    
        }		
	}

	void Update()
    {
        if (SearchAroundPlayer())
        {
            GetComponent<Renderer>().material.color = Color.red;
            GetComponent<ChangeMaterial>().Change(1);
		}
        else
        {
            GetComponent<Renderer>().material.color = originalColor;
            GetComponent<ChangeMaterial>().Change(0);
        }

        if (FarFromSpawnPos())
        {
            GetComponent<GuardController>().SetGoalPos(backPosition);
        }
        else
        {
            GetComponent<GuardController>().SetGoalPos(player.transform.position);
        }
	}

    bool FarFromSpawnPos()
    {
        if (Vector3.Distance(backPosition, player.position) > maxFollowDst)
        {
            return true;
        }

        return false;
    }

    bool SearchAroundPlayer()
    {
        if (Vector3.Distance(transform.position, player.position) < viewDistance)
        {
            return true;
        }

        return false;
    }

	bool CanSeePlayer(){
		if (Vector3.Distance (transform.position, player.position) < viewDistance) {
			Vector3 dirToPlayer = (player.position - transform.position).normalized;
			float angleBetweenGuardAndPlayer = Vector3.Angle (transform.forward, dirToPlayer);
			if (angleBetweenGuardAndPlayer < viewAngle / 2f) {
				if (!Physics.Linecast(transform.position, player.position, viewMask)){
					return true;
				}
			}
		}
		return false;
	}

	IEnumerator FollowPath(Vector3[] waypoints){
		transform.position = waypoints [0];

		int targetWaypointIndex = 1;
		Vector3 targetWaypoint = waypoints [targetWaypointIndex];
		transform.LookAt (targetWaypoint);

		while (true) {
			transform.position = Vector3.MoveTowards (transform.position, targetWaypoint, speed * Time.deltaTime);
			if (transform.position == targetWaypoint) {
				targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
				targetWaypoint = waypoints [targetWaypointIndex];
				yield return new WaitForSeconds (waitSec);
				yield return StartCoroutine (TurnToFace (targetWaypoint));
			}

			yield return null;
		}
	}

	IEnumerator TurnToFace(Vector3 lookTarget){
		Vector3 dirToLookTarget = (lookTarget - transform.position).normalized;
		float targetToAngle = 90 - Mathf.Atan2 (dirToLookTarget.z, dirToLookTarget.x) * Mathf.Rad2Deg;

		while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetToAngle)) > 0.05f) {
			float angle = Mathf.MoveTowardsAngle (transform.eulerAngles.y, targetToAngle, turnSpeed * Time.deltaTime);
			transform.eulerAngles = Vector3.up * angle;
			yield return null;
		}
	}

	void OnDrawGizmos()
    {
        if (pathHolder == null)
            return;
        
		Vector3 startPos = pathHolder.GetChild (0).position;
		Vector3 prevPos = startPos;

		foreach (Transform waypoint in pathHolder) {
			Gizmos.DrawSphere (waypoint.position, .3f);
			Gizmos.DrawLine (prevPos, waypoint.position);
			prevPos = waypoint.position;
		}
		Gizmos.DrawLine (startPos, prevPos);

		Gizmos.color = Color.red;
		Gizmos.DrawLine (transform.position, transform.forward * viewDistance);
	}
}
