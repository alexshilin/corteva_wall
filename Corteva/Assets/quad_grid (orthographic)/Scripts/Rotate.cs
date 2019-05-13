using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;

public class Rotate : MonoBehaviour {

	public float rotationSpeed = 10f;
	public bool randomSpeed = false;

	bool spinning = true;
	float spinVelocity = 0f;
	Vector3 spinAxis = Vector3.up;
	float spinDamp = 0.8f;

	private TransformGesture transformGesture;
	private FlickGesture flickGesture;

	void Start(){
		if (randomSpeed) {
			rotationSpeed = UnityEngine.Random.Range (-20, 20);
		}
	}

	void OnEnable(){
		transformGesture = GetComponent<TransformGesture> ();
		flickGesture = GetComponent<FlickGesture> ();

		//transformGesture.TransformStarted += transformStartedHandler;
		transformGesture.Transformed += transformedHandler;
		//transformGesture.TransformCompleted += transformCompletedHandler;

		flickGesture.Flicked += flickedHandler;
	}

	void OnDisable(){

		//transformGesture.TransformStarted -= transformStartedHandler;
		transformGesture.Transformed -= transformedHandler;
		//transformGesture.TransformCompleted -= transformCompletedHandler;

		flickGesture.Flicked -= flickedHandler;
	}

	void Update () {
		if(spinning){
			if (spinVelocity > 0) {
				transform.RotateAround (transform.position, spinAxis, spinVelocity);
				spinVelocity *= spinDamp;
			}
		}
	}

	private void transformStartedHandler(object sender, EventArgs e){
		spinning = false;
		spinVelocity = 0;
	}
	private void transformedHandler(object sender, EventArgs e){
		transform.RotateAround (Vector3.down, transformGesture.DeltaPosition.x);
		transform.RotateAround (Vector3.right, transformGesture.DeltaPosition.y);

	}
	private void transformCompletedHandler(object sender, EventArgs e){
	}

	private void flickedHandler(object sender, EventArgs e){
		spinVelocity = flickGesture.ScreenFlickTime * 500;
		spinAxis = new Vector3(flickGesture.ScreenFlickVector.y, -flickGesture.ScreenFlickVector.x, 0);
		spinning = true;
		Debug.Log ("FLICK " + spinAxis + " " + spinVelocity);
	}
}
