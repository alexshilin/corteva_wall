using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour {

	public float rotationSpeed = 10f;
	public bool randomSpeed = false;

	void Start(){
		if (randomSpeed) {
			rotationSpeed = Random.Range (-20, 20);
		}
	}

	void Update () {
		transform.RotateAround (transform.position, transform.up, rotationSpeed * Time.deltaTime);
	}
}
