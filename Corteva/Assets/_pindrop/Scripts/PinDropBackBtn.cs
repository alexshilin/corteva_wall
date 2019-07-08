using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures;

public class PinDropBackBtn : MonoBehaviour {
    public GameObject PindropObj;//menu
	private TapGesture tapGesture;

	void OnEnable(){
		tapGesture = GetComponent<TapGesture> ();

		tapGesture.Tapped += tapHandler;
	}

	void OnDisable(){
		tapGesture.Tapped -= tapHandler;
	}

	public void tapHandler(object sender, System.EventArgs e){
        //GA--user taps on back button to go back to wall
        PindropObj.GetComponent<PinDropMenu> ().PD.myKiosk.StopPinDrop ();
        PindropObj.SetActive(false);
    }
}
