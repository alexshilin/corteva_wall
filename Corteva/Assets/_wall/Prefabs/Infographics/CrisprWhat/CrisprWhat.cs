using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures.TransformGestures;
using UnityEngine.UI;

public class CrisprWhat : MonoBehaviour {

	public Transform helix;
	public TransformGesture helixGesture;

	public Transform snap;
	public Transform oldPiece;
	public TransformGesture oldPieceGesture;
	public Transform newPiece;
	public TransformGesture newPieceGesture;
	public Renderer helixR;

	public Image oldPieceRing;
	public Image newPieceRing;

	public LineRenderer line;

	public GameObject oldPieceTitle;
	public GameObject oldPieceInstruct;

	public GameObject newPieceTitle;
	public GameObject newPieceInstruct;

	public GameObject newTitle;

	private float lerpDuration = 0.5f;
	private float t = 0.0f;
	private float pieceScl = 0.6f;

	private bool autoRotate = false;
	private float rotateSpeed = -10f;

	private Vector3 newPieceStart = new Vector3 (-0.175f, -0.449f, -1.027319f);

	private float oldPieceRingFill = 1;
	private float newPieceRingFill = 0;

	void OnEnable(){
		autoRotate = true;
		newPiece.parent = transform;
		newPiece.localScale = Vector3.one * 0.6f;
		newPiece.localPosition = newPieceStart;
		newPiece.gameObject.SetActive (false);
		newPieceRing.fillAmount = 0;
		newPieceRing.gameObject.SetActive (false);
		newPieceTitle.SetActive (false);
		newPieceInstruct.SetActive (false);

		oldPiece.parent = helix;
		oldPiece.localRotation = snap.localRotation;
		oldPiece.localScale = Vector3.one;
		oldPiece.localPosition = snap.localPosition;
		oldPiece.gameObject.SetActive (true);
		oldPieceRing.fillAmount = 1;
		oldPieceRing.gameObject.SetActive (false);
		oldPieceTitle.SetActive (true);
		oldPieceInstruct.SetActive (true);

		line.positionCount = 2;
		line.gameObject.SetActive (true);

		newTitle.SetActive (false);


		helixR.material.color = new Color32 (234, 73, 35, 255);
		helix.localRotation = Quaternion.Euler (0, -40, 0);

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
		helix.Rotate(Vector3.up, helixGesture.DeltaPosition.x*-50f, Space.World);
		helix.Rotate(Vector3.right, helixGesture.DeltaPosition.y*50f, Space.World);
	}


	void oldPieceTransformHandler(object sender, System.EventArgs e){
		autoRotate = false;
		oldPiece.parent = transform;
		oldPiece.localPosition += oldPieceGesture.DeltaPosition;
		if(!oldPieceRing.gameObject.activeSelf)
			oldPieceRing.gameObject.SetActive (true);
		if (line.gameObject.activeSelf)
			line.gameObject.SetActive (false);
//		if (Vector2.Distance (oldPiece.position, snap.position) < 0.3f) {
//			oldPiece.position = snap.position;
//			oldPiece.parent = helix;
//			if(oldPieceRing.gameObject.activeSelf)
//				oldPieceRing.gameObject.SetActive (false);
//			if (!line.gameObject.activeSelf)
//				line.gameObject.SetActive (true);
//		}
	}
	void oldPieceTransformEndHandler(object sender, System.EventArgs e){
		if (Vector2.Distance (oldPiece.position, snap.position) < 0.1f) {
			oldPiece.position = snap.position;
			oldPiece.parent = helix;
			autoRotate = true;
		} else {
			oldPiece.parent = transform;
			oldPiece.localPosition = new Vector3 (2.17f, -0.449f, -1.027319f);
			oldPiece.localScale = Vector3.one * 0.6f;
			StartCoroutine (showNewPiece ());
			autoRotate = true;
		}
	}

	IEnumerator showNewPiece(){
		oldPieceRing.gameObject.SetActive (true);
		oldPieceRingFill = 0;
		t = 0;
		float rate = 1 / lerpDuration;
		while (t < 1.0f) {
			if (transform.gameObject.activeSelf) {
				t += rate * Time.deltaTime;
				oldPieceRing.fillAmount = Mathf.Lerp (oldPieceRing.fillAmount, oldPieceRingFill, t);
				yield return null;
			} else {
				yield break;
			}
		}
		oldPiece.gameObject.SetActive (false);
		oldPieceRing.gameObject.SetActive (false);
		oldPieceTitle.SetActive (false);
		oldPieceInstruct.SetActive (false);
		newPieceRing.gameObject.SetActive (true);
		newPieceRingFill = 1;
		t = 0;
		rate = 1 / lerpDuration;
		while (t < 1.0f) {
			if (transform.gameObject.activeSelf) {
				t += rate * Time.deltaTime;
				newPieceRing.fillAmount = Mathf.Lerp (newPieceRing.fillAmount, newPieceRingFill, t);
				yield return null;
			} else {
				yield break;
			}
		}
		newPiece.gameObject.SetActive (true);
		newPieceTitle.SetActive (true);
		newPieceInstruct.SetActive (true);
	}

	void newPieceTransformHandler(object sender, System.EventArgs e){
		newPiece.localScale = Vector3.one * 0.8f;
		autoRotate = false;
		newPiece.parent = transform;
		newPiece.localPosition += newPieceGesture.DeltaPosition;
//		if (Vector2.Distance (newPiece.position, snap.position) < 0.3f) {
//			StartCoroutine(setNewPiece ());
//		}
	}
	void newPieceTransformEndHandler(object sender, System.EventArgs e){
		if (Vector2.Distance (newPiece.position, snap.position) < 0.3f) {
			StartCoroutine(setNewPiece ());
		} else {
			newPiece.parent = transform;
			newPiece.localPosition = newPieceStart;
			newPiece.localScale = Vector3.one * 0.6f;
		}
	}

	IEnumerator setNewPiece(){
		newPieceGesture.Transformed -= newPieceTransformHandler;
		newPieceGesture.TransformCompleted -= newPieceTransformEndHandler;
		newPiece.position = snap.position;
		newPiece.parent = helix;
		newPiece.localScale = Vector3.one;
		//autoRotate = true;
		newPieceRing.gameObject.SetActive (false);
		newPieceTitle.SetActive (false);
		newPieceInstruct.SetActive (false);
		EaseCurve.Instance.MatColor (helixR.material, helixR.material.color, new Color32 (0, 114, 206, 255), 3f, 0f, EaseCurve.Instance.linear);
		t = 0;
		float rate = 1 / 4f;
		float spd = -12f;
		while (t < 1.0f) {
			if (transform.gameObject.activeSelf) {
				t += rate * Time.deltaTime;
				helix.Rotate (Vector3.up, spd, Space.Self);
				spd = Mathf.Lerp (-12f, -1f, t);
				yield return null;
			} else {
				yield break;
			}
		}
		autoRotate = true;
		newTitle.SetActive (true);
	}
	
	// Update is called once per frame
	void Update () {
		if (autoRotate) {
			helix.Rotate (Vector3.up, Time.deltaTime * rotateSpeed, Space.Self);
		}
		if(newPiece.parent == transform)
			newPiece.rotation = snap.rotation;
		if(oldPiece.parent == transform)
			oldPiece.rotation = snap.rotation;

		if (line.gameObject.activeSelf) {
			Vector3 lineEnd = line.transform.InverseTransformPoint(snap.position);
			lineEnd.z = 0;
			line.SetPosition (1, lineEnd);
		}
	}
}
