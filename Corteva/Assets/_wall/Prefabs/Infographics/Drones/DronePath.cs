using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DronePath : MonoBehaviour {

	public DroneViewMask mask;
	public List<Transform> positions = new List<Transform>();
	public List<float> marks = new List<float> ();
	private int mark = 0;
	private float pathDistance;
	private float pathProgress;
	public List<float> steps = new List<float> ();
	private Vector3 direction;
	private int step = 0;
	private Vector3 lastPosition;
	private float distanceToMark;
	private Vector3 velocity = Vector3.zero;
	// Use this for initialization
	void Start () {
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

	// Update is called once per frame
	void Update () {
		//transform.position += direction * Time.deltaTime * 0.3f;
		transform.position = Vector3.MoveTowards(transform.position, positions [mark].position, Time.deltaTime * 0.8f);
		//transform.position = Vector3.SmoothDamp(transform.position, positions [mark].position, ref velocity, Time.deltaTime * 10f);
		if (mark > 0) {
			pathProgress += Vector3.Distance (transform.position, lastPosition);
		}
		distanceToMark = Vector3.Distance (transform.position, positions [mark].position);
		if (step < steps.Count - 1) {
			if (pathProgress > steps [step]) {
				step++;
				mask.UpdateMask (step);
				Debug.Log (step + ": " + pathProgress + "/" + pathDistance);
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
//		if (mark < marks.Count - 1) {
//			if (pathProgress > marks [mark]) {
//				//if(direction == Vector3.zero){
//				mark++;
//				direction = positions [mark].position - transform.position;
//			}
//		}
		lastPosition = transform.position;
	}
}
