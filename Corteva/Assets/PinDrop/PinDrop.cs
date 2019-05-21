using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinDrop : MonoBehaviour {

	public float initGlobeSize = 1f;
	public float maxGlobeSize = 2f;
	public float initWindowSize = 3f;
	public Transform menu;
	public Transform globe;
	public Camera globeCam;

	void Start(){
		Init ();
	}

	public void Init(){
		globe.localScale = Vector3.one * initGlobeSize;
		menu.localScale = Vector3.one * initWindowSize;
		globe.GetComponent<PinDropEarth> ().cam = globeCam;
	}
}
