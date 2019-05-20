using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures;

public class UserNav : MonoBehaviour {

	public UserKiosk myKiosk;
	public int envID;

	private TapGesture tapGesture;

	// Use this for initialization
	void Start () {
		
	}

	void OnEnable(){
		tapGesture = GetComponent<TapGesture> ();

		tapGesture.Tapped += tapHandler;
	}

	void OnDisable(){

		tapGesture.Tapped -= tapHandler;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void tapHandler(object sender, EventArgs e){
		if (!myKiosk.somePanelIsAnimating) {
			if (envID == -1) {
				myKiosk.StartPinDrop ();
			} else {
				myKiosk.SwitchEnvironment (envID);
			}
		}
	}
}
