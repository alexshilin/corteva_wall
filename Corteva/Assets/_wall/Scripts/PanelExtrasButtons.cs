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
	private PanelBase panel;

	void OnEnable(){
		panel = GetComponentInParent<PanelBase> ();
		tapGesture = GetComponent<TapGesture> ();

		tapGesture.Tapped += tapHandler;
	}

	void OnDisable(){

		tapGesture.Tapped -= tapHandler;
	}

	private void tapHandler(object sender, EventArgs e){
		if (btn == BtnType.Close) {
			panel.BackToGrid ();
		}
		if (btn == BtnType.More || btn == BtnType.Back) {
			panel.FlipAround ();
			if (btn == BtnType.More) {
				//Track
				AssetManager.Instance.GA.LogEvent(new EventHitBuilder()
					.SetEventCategory(AssetManager.Instance.displayName)
					.SetEventAction("Panel > More")
					.SetEventLabel("["+panel.panelID+"] "+panel.panelName));
			}
			if (btn == BtnType.Back) {
				//Track
				AssetManager.Instance.GA.LogEvent(new EventHitBuilder()
					.SetEventCategory(AssetManager.Instance.displayName)
					.SetEventAction("Panel > Back")
					.SetEventLabel("["+panel.panelID+"] "+panel.panelName));
			}
		}
	}
}
