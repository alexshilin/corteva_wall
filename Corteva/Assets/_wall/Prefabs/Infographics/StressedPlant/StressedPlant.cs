using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TouchScript.Gestures.TransformGestures;

public class StressedPlant : MonoBehaviour {

	public List<GameObject> plants = new List<GameObject>();
	public List<Renderer> plantRenderers = new List<Renderer>();
	public List<GameObject> fields = new List<GameObject>();
	public List<GameObject> leaves = new List<GameObject>();
	public List<Transform> marks = new List<Transform> ();
	public List<SpriteRenderer> colors = new List<SpriteRenderer>();
	private List<float[]> colorSets = new List<float[]> ()
	{
		new float[] {5.12f, 6.8f, 5f, 8.75f},
		new float[] {6f, 6.9f, 5.3f, 7f},
		new float[] {5.9f, 5.6f, 5.3f, 6.8f}
	};
	public List<string> leafLabels = new List<string>();

	public TextMeshPro leavesTitle;

	private Transform plant;
	public TransformGesture plantGesture;
	private Transform slider;
	public TransformGesture sliderGesture;

	//private List<Renderer> plantRenderers = new List<Renderer> ();
	private float[] plantTransparency = new float[]{1,0,0};

	private int step;
	private float dist;
	float distToA;
	float distToB;
	float distToC;

	void OnEnable(){
		step = 0;
		distToA = 0;
		distToB = Vector3.Distance (marks [0].localPosition, marks [1].localPosition);
		distToC = Vector3.Distance (marks [0].localPosition, marks [2].localPosition); 
		plant = plantGesture.GetComponent<Transform> ();
//		for (int i = 0; i < plants.Count; i++) {
//			plantRenderers.Add (plants [i].GetComponentInChildren<Renderer> ());
//		}
		slider = sliderGesture.GetComponent<Transform> ();
		plantGesture.Transformed += plantHandler;
		sliderGesture.Transformed += sliderHandler;
		sliderGesture.TransformCompleted += sliderEndHandler;
	}

	void OnDisable(){
		plantGesture.Transformed -= plantHandler;
		sliderGesture.Transformed -= sliderHandler;
		sliderGesture.TransformCompleted -= sliderEndHandler;
	}

	void Start(){
		SetStep (0);
	}

	void plantHandler(object sender, System.EventArgs e){
		plant.Rotate(Vector3.up, plantGesture.DeltaPosition.x*-50, Space.Self);
	}

	void sliderHandler(object sender, System.EventArgs e){
		Vector3 sliderGoTo = new Vector3(sliderGesture.LocalDeltaPosition.x, 0, 0);
		sliderGoTo += slider.localPosition;

		if (sliderGoTo.x < marks [0].localPosition.x)
			sliderGoTo.x = marks [0].localPosition.x;

		if (sliderGoTo.x > marks [2].localPosition.x)
			sliderGoTo.x = marks [2].localPosition.x;

		slider.localPosition = sliderGoTo;

		setStepBySlider (slider.localPosition);
	}

	void sliderEndHandler(object sender, System.EventArgs e){
		float dist = 999;
		int mark = 0;
		for (int i = 0; i < 3; i++) {
			float d = Vector3.Distance (slider.localPosition, marks [i].localPosition);
			if (d < dist) {
				dist = d;
				mark = i;
			}
		}
		Vector3 sliderGoTo = new Vector3(marks[mark].localPosition.x, 0, slider.localPosition.z);
		slider.localPosition = sliderGoTo;
		setStepBySlider (slider.localPosition);
	}

	void setStepBySlider(Vector3 _slider){
		dist = Vector3.Distance (slider.localPosition, marks [0].localPosition);
		//Debug.Log (distToA + ", " + distToB + ", " + distToC + " || " + dist);
		if (dist >= distToA)
			SetStep (0);
		if (dist >= distToB)
			SetStep (1);
		if (dist >= distToC)
			SetStep (2);
	}

	void SetStep(int _step){
		for (int i = 0; i < 3; i++) {
			if (i == _step) {
				//plants [i].SetActive (true);
				plantTransparency [i] = 1;
				fields [i].SetActive (true);
				leaves [i].SetActive (true);
				leavesTitle.text = leafLabels [i];
				for (int ii = 0; ii < colors.Count; ii++) {
					colors [ii].size = new Vector2 (colors [ii].size.x, colorSets [i] [ii]);
				}

			} else {
				//plants [i].SetActive (false);
				plantTransparency [i] = 0;
				fields [i].SetActive (false);
				leaves [i].SetActive (false);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
		for (int i = 0; i < plantRenderers.Count; i++) {
			if (plantRenderers [i].material.GetColor("_Color").a != plantTransparency [i]) {
				float tStep = 0.2f;
				if (plantRenderers [i].material.GetColor("_Color").a > plantTransparency [i])
					tStep = -0.2f;

				//plantRenderers [i].material.SetFloat ("_Transparency", Mathf.Clamp01 (plantRenderers [i].material.GetFloat ("_Transparency") + tStep));
				plantRenderers[i].material.SetColor("_Color", new Color(1,1,1,Mathf.Clamp01(plantRenderers [i].material.GetColor("_Color").a + tStep)));
			}
		}
	}
}
