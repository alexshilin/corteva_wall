using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures;

public class PinDropBackBtn : MonoBehaviour {

	private TapGesture tapGesture;

	void OnEnable(){
		tapGesture = GetComponent<TapGesture> ();

		tapGesture.Tapped += tapHandler;
	}

	void OnDisable(){
		tapGesture.Tapped -= tapHandler;
	}

	void tapHandler(object sender, System.EventArgs e){
		GetComponentInParent<PinDropMenu> ().PD.myKiosk.StopPinDrop ();
	}
}
