using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneFlight: MonoBehaviour {

	public Transform body;
	public List<Transform> rotors = new List<Transform> ();
	public GameObject cam;
	private Vector3 tempPos;
	private Vector3 posOffset;
	private float frequency = 0.2f;
	private float amplitude = 0.05f;
	private int rotateDir = 1;

	public DroneViewMask mask;
	public List<Transform> positions = new List<Transform>();
	private List<float> marks = new List<float> ();
	private int mark = 0;
	private float pathDistance;
	private float pathProgress;
	private List<float> steps = new List<float> ();
	private Vector3 direction;
	private int step = 0;
	private Vector3 lastPosition;
	private float distanceToMark;
	private Vector3 velocity = Vector3.zero;

	void Start(){
		CalcPathDistance ();
		step = 0;
		mark = 0;
		lastPosition = transform.position;
		direction = positions [mark].position - transform.position;
	}

	void CalcPathDistance(){
		pathDistance = 0;
		marks.Add (pathDistance);
		for (int i = 0; i < positions.Count; i++) {
			if (i < positions.Count - 1) {
				pathDistance += Vector3.Distance (positions [i].position, positions [i + 1].position);
				marks.Add (pathDistance);
			}
		}
		float step = pathDistance / mask.cellsTotal;
		for (int i = 0; i < mask.cellsTotal; i++) {
			steps.Add (step * i);
		}
	}

	IEnumerator TakePic(){
		cam.SetActive (true);
		yield return 0;
		cam.SetActive (false);
	}

	void Update(){
		transform.position = Vector3.MoveTowards(transform.position, positions [mark].position, Time.deltaTime * 0.8f);
		transform.LookAt (positions [mark].position, transform.up);
		if (mark > 0) {
			pathProgress += Vector3.Distance (transform.position, lastPosition);
		}
		distanceToMark = Vector3.Distance (transform.position, positions [mark].position);
		if (step < steps.Count - 1) {
			if (pathProgress > steps [step]) {
				Debug.Log (step);
				step++;
				mask.UpdateMask (step);
				//StartCoroutine (TakePic ());
				cam.SetActive (true);
				Debug.Log (step + ": " + pathProgress + "/" + pathDistance);
			} else {
				cam.SetActive (false);
			}
		}
		if (distanceToMark < 0.01f) {
			if (mark < marks.Count - 1) {
				mark++;
			} else {
				mark = 0;
				step = 0;
				pathProgress = 0;
			}
		}
		tempPos = posOffset;
		tempPos.y += Mathf.Sin (Time.fixedTime * Mathf.PI * frequency) * amplitude;
		body.localPosition = tempPos;

		lastPosition = transform.position;

		foreach (Transform r in rotors) {
			r.localRotation *= Quaternion.Euler (0, 100f * rotateDir, 0);
			rotateDir *= -1;
		}
	}
}
