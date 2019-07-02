using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TouchScript.Gestures.TransformGestures;
using TMPro;

public class CropPipeline : MonoBehaviour {
	public Transform handle;
	public PinnedTransformGesture pinnedGesture;
	public Image ring;
	public List<Transform> stops = new List<Transform>();
	public TextMeshPro title;
	public TextMeshPro body;

	private string[] titles = new string[6]{ 
		"1. Discovery", 
		"2. Development", 
		"3. Launch", 
		"4. Ramp-Up",
		"5. Extend & Improve", 
		"6. Operation" 
	};
	private string[] bodies = new string[6]{ 
		"Invent new products\nand technology.", 
		"Complete research to enable product development.", 
		"Complete preparations and launch product globally.",
		"Optimize product usage.", 
		"Expand use or launch new varieties.", 
		"Product support and defense." 
	};


	float disToStop;

	float stopAngle = 0;
	Quaternion toRotation = Quaternion.identity;
	float currStop = 0;
	//float disFromCurrStop = 0;


	void Start () {
		
	}

	void OnEnable(){
		stopAngle = 360f / stops.Count;
		updateRing ();
		pinnedGesture.Transformed += transformHandler;
		pinnedGesture.TransformCompleted += transformEndHandler;
	}

	void OnDisable(){
		pinnedGesture.Transformed -= transformHandler;
		pinnedGesture.TransformCompleted -= transformEndHandler;
	}

	void transformStartHandler(object sender, System.EventArgs e){
		
	}
	void transformHandler(object sender, System.EventArgs e){
		toRotation = handle.rotation * Quaternion.AngleAxis (pinnedGesture.DeltaRotation, pinnedGesture.RotationAxis);

		handle.rotation = toRotation;
		updateRing ();


		//Debug.Log (">>> " + currStop + " || " + disFromCurrStop);
	}
	void transformEndHandler(object sender, System.EventArgs e){
		toRotation = Quaternion.identity * Quaternion.AngleAxis (360f - (currStop * stopAngle), pinnedGesture.RotationAxis);

		handle.rotation = toRotation;
		updateRing ();
	}

	public void manualStop(int _stop){
		toRotation = Quaternion.identity * Quaternion.AngleAxis (360f - (_stop * stopAngle), pinnedGesture.RotationAxis);

		handle.rotation = toRotation;
		updateRing ();
	}

	void updateRing(){
		//currStop = Mathf.Floor (((360f - handle.localEulerAngles.z) / stopAngle) + 0.1f); //switch On #
		currStop = Mathf.Round (((360f - handle.localEulerAngles.z) / stopAngle)); //switch Btw #s
		currStop = currStop >= stops.Count ? 0 : currStop ;

		float txtStop = Mathf.Floor (((360f - handle.localEulerAngles.z) / stopAngle) + 0.1f); //switch On #
		//float txtStop = Mathf.Round (((360f - handle.localEulerAngles.z) / stopAngle)); //switch Btw #s
		txtStop = txtStop >= stops.Count ? 0 : txtStop ;

		float fill = 1f - (handle.localEulerAngles.z / 360f); 
		ring.fillAmount = fill > .999999f ? 0f : fill;

		title.text = titles [Mathf.RoundToInt(txtStop)];
		body.text = bodies [Mathf.RoundToInt(txtStop)];
	}

	void Update () {
		foreach (Transform s in stops) {
			float dis = Vector3.Distance(s.position, handle.GetChild (0).position);
			if (Mathf.Abs(dis) < 0.325f) {
				s.localScale = Vector3.one * (1 - Mathf.Abs(dis*2));
			} else {
				s.localScale = Vector3.one * 0.35f;
			}
		}
	}
}
