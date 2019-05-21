using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pin : MonoBehaviour {

	private bool active = true;
	public LayerMask globeLayer;
	public Vector3 worldPos;
	public Vector3 localPos;

	private float baseSize = 0.2f;

	void Start(){
		
	}

	void Update(){
		//worldPos = transform.position;
		if (transform.position.z>100f) {
			if (active) {
				foreach (Transform child in transform) {
					child.gameObject.SetActive (false);
				}
				active = false;
			}
		} else {
			if (!active) {
				foreach (Transform child in transform) {
					child.gameObject.SetActive (true);
				}
				active = true;
			}
		}
		transform.rotation = Quaternion.identity;
		transform.localScale = Vector3.one * (0.02f/(transform.parent.parent.localScale.x*0.02f)) * baseSize;
	}
}
