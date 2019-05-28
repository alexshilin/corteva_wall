using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures;

public class PanelExtrasButtons : MonoBehaviour {

	public enum BtnType
	{
		Close,
		More,
		Back
	};
	private TapGesture tapGesture;
	public BtnType btn;

	void OnEnable(){
		tapGesture = GetComponent<TapGesture> ();

		tapGesture.Tapped += tapHandler;
	}

	void OnDisable(){

		tapGesture.Tapped -= tapHandler;
	}

	private void tapHandler(object sender, EventArgs e){
		if (btn == BtnType.Close) {
			GetComponentInParent<PanelBase> ().BackToGrid ();
		}
		if (btn == BtnType.More || btn == BtnType.Back) {
			GetComponentInParent<PanelBase> ().FlipAround ();
		}
	}
}
