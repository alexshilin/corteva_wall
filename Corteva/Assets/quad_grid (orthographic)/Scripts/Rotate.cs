using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;

public class Rotate : MonoBehaviour {
	private bool autoSpin = true;
	private float rotationSpeed = -1f;

	bool flicking = true;
	float spinVelocity = 0f;
	Vector3 spinAxis = Vector3.up;
	float spinDamp = 0.8f;

	private TransformGesture transformGesture;
	private FlickGesture flickGesture;

	void Start(){
		
	}

	void OnEnable(){
		transformGesture = GetComponent<TransformGesture> ();
		flickGesture = GetComponent<FlickGesture> ();

		transformGesture.TransformStarted += transformStartedHandler;
		transformGesture.Transformed += transformedHandler;
		//transformGesture.TransformCompleted += transformCompletedHandler;

		flickGesture.Flicked += flickedHandler;
	}

	void OnDisable(){

		transformGesture.TransformStarted -= transformStartedHandler;
		transformGesture.Transformed -= transformedHandler;
		//transformGesture.TransformCompleted -= transformCompletedHandler;

		flickGesture.Flicked -= flickedHandler;
	}

	void Update () {
		if (autoSpin) {
			transform.RotateAround (transform.position, transform.up, rotationSpeed);
		}
		if(flicking){
			if (spinVelocity > 0) {
				transform.RotateAround (transform.position, spinAxis, spinVelocity);
				spinVelocity *= spinDamp;
			}
		}
	}

	private void transformStartedHandler(object sender, EventArgs e){
		flicking = false;
		autoSpin = false;
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
		flicking = true;
		Debug.Log ("FLICK " + spinAxis + " " + spinVelocity);
	}
}
