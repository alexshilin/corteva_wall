using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMove : MonoBehaviour {

	float time = 0;
	float tick = 1f;
	Vector3 startPos;
	Vector3 gotoPos;
	// Use this for initialization
	void Start () {
		startPos = transform.localPosition;
		gotoPos = startPos;
	}
	
	// Update is called once per frame
	void Update () {
		if (time > tick) {
			gotoPos = startPos + Random.insideUnitSphere * 5f;
			time = 0;
		}
		transform.localPosition = Vector3.Lerp (transform.localPosition, gotoPos, 0.1f * Time.deltaTime);
		time += Time.deltaTime;
	}
}
