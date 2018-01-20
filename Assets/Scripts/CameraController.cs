using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    GameObject player;
	// Use this for initialization
	void Start ()
    {
        player = GameObject.FindGameObjectWithTag("Player");
	}

    void LateUpdate()
    {
        Vector3 newPos = player.transform.position;

        transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);
    }
}
