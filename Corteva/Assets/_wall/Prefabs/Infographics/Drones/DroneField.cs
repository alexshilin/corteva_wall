using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures.TransformGestures;

public class DroneField : MonoBehaviour {

	private bool autoSpin = true;
	private TransformGesture transformGesture;

	void OnEnable(){
		transformGesture = GetComponent<TransformGesture> ();

		transformGesture.Transformed += transformHandler;
	}

	void OnDisable(){
		transformGesture.Transformed -= transformHandler;
	}

	void transformHandler(object sender, System.EventArgs e){
		autoSpin = false;
		transform.Rotate(Vector3.up, transformGesture.DeltaPosition.x*-50, Space.Self);
	}
	
	// Update is called once per frame
	void Update () {
		if(autoSpin)
			transform.RotateAround (transform.position, transform.up, Time.deltaTime * -5f);
	}
}
