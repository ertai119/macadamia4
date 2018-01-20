using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMaterial : MonoBehaviour {

    public Material[] materials;
    Renderer rend;

	// Use this for initialization
	void Start ()
    {
        rend = GetComponent<Renderer>();
        rend.enabled = true;
        rend.sharedMaterial = materials[0];
	}
	
    public void Change(int index)
    {
        if (index < 0 || index >= materials.Length)
            return;
        
        rend.sharedMaterial = materials[index];
    }
}
