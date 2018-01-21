using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour {

	public float speed = 5;
	public float waitSec = .3f;
	public float turnSpeed = 90;

    Vector3 spawnPos;

	public Light spotLight;
	public float viewDistance;
    public float maxFollowDst;
	public LayerMask viewMask;
	float viewAngle;

	Transform player;
	Color originalSpotlightColor;
    Color originalColor;

	void Start(){
		player = GameObject.FindGameObjectWithTag ("Player").transform;
        spawnPos = transform.position;

        originalColor = GetComponent<Renderer>().material.color;

        if (spotLight)
        {
            originalSpotlightColor = spotLight.color;
            viewAngle = spotLight.spotAngle;    
        }		

        StartCoroutine(AIUpdate(0.5f));    
	}

    IEnumerator AIUpdate(float tickSec)
    {
        while (true)
        {
            GuardController controller = GetComponent<GuardController>();
            ChangeMaterial changeMtl = GetComponent<ChangeMaterial>();

            if (SearchAroundPlayer() && FarFromSpawnPos() == false)
            {
                controller.TrackingMode(player.transform.position);    
                changeMtl.Change(1);
            }
            else
            {
                controller.PatrolMode();
                changeMtl.Change(0);
            }

            yield return new WaitForSeconds(tickSec);
        }
    }

	void Update()
    {
	}

    bool FarFromSpawnPos()
    {
        if (Vector3.Distance(spawnPos, player.position) > maxFollowDst)
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
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(spawnPos, maxFollowDst);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewDistance);
    }

}
