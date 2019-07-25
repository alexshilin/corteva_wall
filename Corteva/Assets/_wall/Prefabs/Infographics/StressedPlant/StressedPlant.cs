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


	public GameObject sliderGO;
	//public GameObject infoGO;
	//public GameObject plantGO;
	public SpriteRenderer satellite;
	public SpriteRenderer scanDown;
	public SpriteRenderer scanBox;
	public SpriteRenderer scanBox2;
	public SpriteRenderer farm;
	public SpriteRenderer dropShadow;
	public SpriteRenderer scanRight1;
	public SpriteRenderer scanRight2;
	public Renderer plantBase;
	public SpriteRenderer infoBG;
	public SpriteRenderer infoLeaf;
	public GameObject infoColors;

	private bool introFinished = false;


	private float[] plantTransparency = new float[]{1,0,0};

	private int step;
	private float dist;
	float distToA;
	float distToB;
	float distToC;

	void OnEnable(){
		introFinished = false;
		//hide all;
		sliderGO.SetActive(false);
		satellite.color = new Color(1,1,1,0);
		scanDown.color = new Color(1,1,1,0);
		scanBox.gameObject.SetActive(false);
		scanBox2.gameObject.SetActive(false);
		farm.color = new Color(1,1,1,0);
		dropShadow.color = new Color(1,1,1,0);
		scanRight1.color = new Color(1,1,1,0);
		scanRight2.color = new Color(1,1,1,0);
		plantRenderers [0].material.SetColor ("_Color", new Color (1, 1, 1, 0));
		plantBase.materials[0].SetColor ("_Color", new Color (1, 1, 1, 0));
		plantBase.materials[1].SetColor ("_Color", new Color (1, 1, 1, 0));
		infoBG.size = new Vector2(0.32f, 0);
		infoLeaf.color = new Color (1, 1, 1, 0);
		infoColors.SetActive (false);
		leavesTitle.alpha = 0;

		//set up nav
		step = 0;
		distToA = 0;
		distToB = Vector3.Distance (marks [0].localPosition, marks [1].localPosition);
		distToC = Vector3.Distance (marks [0].localPosition, marks [2].localPosition); 
		plant = plantGesture.GetComponent<Transform> ();
		slider = sliderGesture.GetComponent<Transform> ();
		plantGesture.Transformed += plantHandler;
		sliderGesture.Transformed += sliderHandler;
		sliderGesture.TransformCompleted += sliderEndHandler;

		//
		StartCoroutine(BeginIntro ());
	}

	void OnDisable(){
		plantGesture.Transformed -= plantHandler;
		sliderGesture.Transformed -= sliderHandler;
		sliderGesture.TransformCompleted -= sliderEndHandler;
		introFinished = false;
	}

	void Start(){
		
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
				plantTransparency [i] = 1;
				fields [i].SetActive (true);
				leaves [i].SetActive (true);
				leavesTitle.text = leafLabels [i];
				for (int ii = 0; ii < colors.Count; ii++) {
					colors [ii].size = new Vector2 (colors [ii].size.x, colorSets [i] [ii]);
				}

			} else {
				plantTransparency [i] = 0;
				fields [i].SetActive (false);
				leaves [i].SetActive (false);
			}
		}
	}

	IEnumerator BeginIntro(){
		float speedMod = 0.5f;

		yield return new WaitForSeconds (0.3f);

		satellite.transform.localPosition += Vector3.left * 0.16f;
		farm.transform.localPosition += Vector3.down * 0.08f;

		//0
		EaseCurve.Instance.Vec3 (farm.transform, farm.transform.localPosition, farm.transform.localPosition + Vector3.up * 0.08f, 1f * speedMod, 0f * speedMod, EaseCurve.Instance.easeIn, null, "local");
		EaseCurve.Instance.SpriteColor (farm, farm.color, new Color (1, 1, 1, 1), 1f * speedMod, 0f * speedMod, EaseCurve.Instance.linear);
		EaseCurve.Instance.SpriteColor (dropShadow, dropShadow.color, new Color (1, 1, 1, 1), 1f * speedMod, 0f * speedMod, EaseCurve.Instance.linear);

		//1
		EaseCurve.Instance.Vec3 (satellite.transform, satellite.transform.localPosition, satellite.transform.localPosition + Vector3.right * 0.16f, 1.5f * speedMod, 1f * speedMod, EaseCurve.Instance.easeOut, null, "local");
		EaseCurve.Instance.SpriteColor (satellite, satellite.color, new Color (1, 1, 1, 1), 1f * speedMod, 1f * speedMod, EaseCurve.Instance.easeOut);

		//2.5
		EaseCurve.Instance.SpriteColor (scanDown, scanDown.color, new Color (1, 1, 1, 1), 0.5f * speedMod, 2.5f * speedMod, EaseCurve.Instance.easeIn);

		yield return new WaitForSeconds (2.5f * speedMod);
		scanBox.gameObject.SetActive (true);
		yield return new WaitForSeconds (0.1f);
		scanBox.gameObject.SetActive (false);
		yield return new WaitForSeconds (0.1f);
		scanBox.gameObject.SetActive (true);
		yield return new WaitForSeconds (0.1f);
		scanBox.gameObject.SetActive (false);
		yield return new WaitForSeconds (0.1f);
		scanBox.gameObject.SetActive (true);

		//0
		EaseCurve.Instance.SpriteColor (scanRight1, scanRight1.color, new Color (1, 1, 1, 1), 0.5f * speedMod, 0 * speedMod, EaseCurve.Instance.easeIn);
		 
		EaseCurve.Instance.MatColor(plantRenderers[0].material, new Color(1,1,1,0), new Color(1,1,1,1), 1f * speedMod, 0.25f * speedMod, EaseCurve.Instance.linear);
		EaseCurve.Instance.MatColor(plantBase.materials[0], new Color(1,1,1,0), new Color(1,1,1,1), 1f * speedMod, 0.25f * speedMod, EaseCurve.Instance.linear);
		EaseCurve.Instance.MatColor(plantBase.materials[1], new Color(1,1,1,0), new Color(1,1,1,1), 1f * speedMod, 0.25f * speedMod, EaseCurve.Instance.linear);


		yield return new WaitForSeconds (2f * speedMod);
		scanBox2.gameObject.SetActive (true);
		yield return new WaitForSeconds (0.1f);
		scanBox2.gameObject.SetActive (false);
		yield return new WaitForSeconds (0.1f);
		scanBox2.gameObject.SetActive (true);
		yield return new WaitForSeconds (0.1f);
		scanBox2.gameObject.SetActive (false);
		yield return new WaitForSeconds (0.1f);
		scanBox2.gameObject.SetActive (true);

		//0
		EaseCurve.Instance.SpriteColor (scanRight2, scanRight2.color, new Color (1, 1, 1, 1), 0.5f * speedMod, 0 * speedMod, EaseCurve.Instance.easeIn);

		EaseCurve.Instance.SpriteSize (infoBG, infoBG.size, new Vector2(0.32f, 0.32f), 0.25f * speedMod, 0.25f * speedMod, EaseCurve.Instance.easeIn);

		EaseCurve.Instance.SpriteColor (infoLeaf, infoLeaf.color, new Color (1, 1, 1, 1), 0.5f * speedMod, 0.5f * speedMod, EaseCurve.Instance.easeIn);

		yield return new WaitForSeconds (1.25f * speedMod);
		infoColors.SetActive (true);
		for (int ii = 0; ii < colors.Count; ii++) {
			EaseCurve.Instance.SpriteSize (colors [ii], colors [ii].size, new Vector2 (colors [ii].size.x, colorSets [0] [ii]), 0.25f * speedMod, ii*0.25f * speedMod, EaseCurve.Instance.easeIn);
		}

		yield return new WaitForSeconds (1.25f * speedMod);
		//leavesTitle.gameObject.SetActive (true);
		EaseCurve.Instance.TextAlpha(leavesTitle, leavesTitle.alpha, 1, 0.25f * speedMod, 0 * speedMod, EaseCurve.Instance.linear, null);

		introFinished = true;
		SetStep (0);

		yield return new WaitForSeconds (1.5f * speedMod);
		sliderGO.SetActive (true);
	}

	void Update () {
		if (introFinished) {
			for (int i = 0; i < plantRenderers.Count; i++) {
				if (plantRenderers [i].material.GetColor ("_Color").a != plantTransparency [i]) {
					float tStep = 0.2f;
					if (plantRenderers [i].material.GetColor ("_Color").a > plantTransparency [i])
						tStep = -0.2f;

					//plantRenderers [i].material.SetFloat ("_Transparency", Mathf.Clamp01 (plantRenderers [i].material.GetFloat ("_Transparency") + tStep));
					plantRenderers [i].material.SetColor ("_Color", new Color (1, 1, 1, Mathf.Clamp01 (plantRenderers [i].material.GetColor ("_Color").a + tStep)));
				}
			}
		}
	}
}
