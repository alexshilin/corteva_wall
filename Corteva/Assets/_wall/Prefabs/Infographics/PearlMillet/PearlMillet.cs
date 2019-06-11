using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures.TransformGestures;
using TMPro;

public class PearlMillet : MonoBehaviour {

	public SpriteRenderer bar1;
	public Transform bar1info;
	public TextMeshPro bar1txt;
	public SpriteRenderer bar2;
	public Transform bar2info;
	public TextMeshPro bar2txt;

	public Transform handle;
	public List<Transform> dots = new List<Transform> ();
	public TransformGesture transformGesture;

	private bool sliding = false;
	private Vector3 handlePos;
	private float distance = 0;
	private float handleDis = 0;
	private float handlePct = 0;
	private Vector2 bar1size;
	private Vector2 bar2size;
	private Vector3 bar1infoPos;
	private Vector3 bar2infoPos;

	void Start () {
		
	}

	void OnEnable(){
		handlePos = dots [0].localPosition;
		handlePos.z = handle.localPosition.z;
		handle.localPosition = handlePos;
		distance = Vector3.Distance (dots [0].localPosition, dots [2].localPosition);
		transformGesture.TransformStarted += transformStartHandler;
		transformGesture.Transformed += transformHandler;
		transformGesture.TransformCompleted += transformCompleteHandler;
	}
	void OnDisable(){
		transformGesture.TransformStarted -= transformStartHandler;
		transformGesture.Transformed -= transformHandler;
		transformGesture.TransformCompleted -= transformCompleteHandler;
	}

	void transformStartHandler(object sender, System.EventArgs e){

	}
	void transformHandler(object sender, System.EventArgs e){
		MoveHandle ();
	}
	void transformCompleteHandler(object sender, System.EventArgs e){
		MoveHandle ();
	}

	void MoveHandle(){
		handlePos = handle.localPosition + transformGesture.LocalDeltaPosition;
		handlePos.y = handle.localPosition.y;
		if (handlePos.x < dots [0].localPosition.x)
			handlePos.x = dots [0].localPosition.x;
		if (handlePos.x > dots [2].localPosition.x)
			handlePos.x = dots [2].localPosition.x;
		handle.localPosition = handlePos;
	}

	void Update () {
		handleDis = handle.localPosition.x - dots [0].localPosition.x;
		handlePct = handleDis / distance;
		//Debug.Log ("handleDis: " + handleDis+" | "+handlePct);
		bar1size = bar1.size;
		bar1size.x = handlePct * 0.8f;
		bar1.size = bar1size;
		bar1infoPos = bar1.transform.localPosition;
		bar1infoPos.y = bar1info.localPosition.y;
		bar1infoPos.x += bar1.size.x - 0.02f;
		bar1info.localPosition = bar1infoPos;
		bar1txt.text = Mathf.Round ((handlePct * 0.8f) * 100) + "%";

		bar2size = bar2.size;
		bar2size.x = handlePct * 0.2f;
		bar2.size = bar2size;
		bar2infoPos = bar2.transform.localPosition;
		bar2infoPos.y = bar2info.localPosition.y;
		bar2infoPos.x += bar2.size.x - 0.02f;
		bar2info.localPosition = bar2infoPos;
		bar2txt.text = Mathf.Round ((handlePct * 0.2f) * 100) + "%";
	}
}
