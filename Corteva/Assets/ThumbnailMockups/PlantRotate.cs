using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantRotate : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.RotateAround (transform.position, transform.up, Time.deltaTime * 10f);
	}
}
