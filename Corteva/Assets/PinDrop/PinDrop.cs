using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinDrop : MonoBehaviour {

	public float initGlobeSize = 1f;
	public float maxGlobeSize = 2f;
	public float initWindowSize = 3f;
	public PinDropMenu menu;
	public PinDropEarth globe;
	public Camera globeCam;

	void Start(){

	}

	public void Init(){
		globe.transform.localScale = Vector3.one * initGlobeSize;
		menu.transform.localScale = Vector3.one * initWindowSize;
		globe.cam = globeCam;
		menu.ToggleWelcome (1);
	}
}
