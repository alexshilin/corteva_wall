using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures.TransformGestures;

public class CrisprWhat : MonoBehaviour {

	public Transform helix;
	public TransformGesture helixGesture;
	public Transform snap;
	public Transform oldPiece;
	public TransformGesture oldPieceGesture;
	public Transform newPiece;
	public TransformGesture newPieceGesture;


	private float lerpDuration = 0.5f;
	private float t = 0.0f;
	private float pieceScl = 0.9f;

	private bool autoRotate = false;

	// Use this for initialization
	void Start () {
		
	}

	void OnEnable(){
		autoRotate = true;

		helixGesture.Transformed += helixTransformHandler;

		oldPieceGesture.Transformed += oldPieceTransformHandler;
		oldPieceGesture.TransformCompleted += oldPieceTransformEndHandler;

		newPieceGesture.Transformed += newPieceTransformHandler;
		newPieceGesture.TransformCompleted += newPieceTransformEndHandler;
	}
	void OnDisable(){
		helixGesture.Transformed -= helixTransformHandler;

		oldPieceGesture.Transformed -= oldPieceTransformHandler;
		oldPieceGesture.TransformCompleted -= oldPieceTransformEndHandler;

		newPieceGesture.Transformed -= newPieceTransformHandler;
		newPieceGesture.TransformCompleted -= newPieceTransformEndHandler;
	}
	void helixTransformHandler(object sender, System.EventArgs e){
		autoRotate = false;
		//helix.rotation *= Quaternion.AngleAxis (helixGesture.DeltaRotation, Vector3.up);
		//helix.localRotation *= Quaternion.Euler(helixGesture.DeltaPosition.y*2, helixGesture.DeltaPosition.x*2, 0);
		helix.Rotate(Vector3.up, helixGesture.DeltaPosition.x*-50f, Space.World);
		helix.Rotate(Vector3.right, helixGesture.DeltaPosition.y*50f, Space.World);
	}


	void oldPieceTransformHandler(object sender, System.EventArgs e){
		//		pieceScl = 0.9f;
		autoRotate = false;
		oldPiece.parent = transform;
		oldPiece.localPosition += oldPieceGesture.DeltaPosition;
		if (Vector2.Distance (oldPiece.position, snap.position) < 0.3f) {
			oldPiece.position = snap.position;
			oldPiece.parent = helix;
		}
	}
	void oldPieceTransformEndHandler(object sender, System.EventArgs e){
		if (Vector2.Distance (oldPiece.position, snap.position) < 0.3f) {
			oldPiece.position = snap.position;
			oldPiece.parent = helix;
			autoRotate = true;
		} else {
			oldPiece.parent = transform;
		}
	}


	void newPieceTransformHandler(object sender, System.EventArgs e){
//		pieceScl = 0.9f;
		autoRotate = false;
		newPiece.parent = transform;
		newPiece.localPosition += newPieceGesture.DeltaPosition;
		if (Vector2.Distance (newPiece.position, snap.position) < 0.3f) {
			newPiece.position = snap.position;
			newPiece.parent = helix;
		}
	}
	void newPieceTransformEndHandler(object sender, System.EventArgs e){
		if (Vector2.Distance (newPiece.position, snap.position) < 0.3f) {
			newPiece.position = snap.position;
			newPiece.parent = helix;
			autoRotate = true;
		} else {
			newPiece.parent = transform;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (autoRotate) {
			helix.Rotate (Vector3.up, Time.deltaTime * -10f, Space.Self);
		}
		if(newPiece.parent == transform)
			newPiece.rotation = snap.rotation;
		if(oldPiece.parent == transform)
			oldPiece.rotation = snap.rotation;

//		float rate = 1 / lerpDuration;
//		if (t < 1.0f) {
//			t += rate * Time.deltaTime;
//			newPiece.localScale = Vector3.Lerp (newPiece.localScale, pieceScl, t);
//		}
	}
}
