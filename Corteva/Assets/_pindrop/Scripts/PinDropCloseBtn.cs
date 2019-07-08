using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures;

public class PinDropCloseBtn : MonoBehaviour {

	private TapGesture tapGesture;

	void OnEnable(){
		tapGesture = GetComponent<TapGesture> ();

		tapGesture.Tapped += tapHandler;
	}

	void OnDisable(){
		tapGesture.Tapped -= tapHandler;
	}

	void tapHandler(object sender, System.EventArgs e){
		transform.parent.GetComponent<PinDropMenu>().instruct.text = "Tap CONFIRM to continue";
		//GetComponentInParent<PinDropMenu> ().CloseQuestions ();
	}
}
