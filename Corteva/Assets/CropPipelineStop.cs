using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures;

public class CropPipelineStop : MonoBehaviour {

	public CropPipeline home;
	public int id;
	private TapGesture tapGesture;

	void OnEnable(){
		tapGesture = GetComponent<TapGesture> ();

		tapGesture.Tapped += tapHandler;
	}

	void OnDisable(){
		tapGesture.Tapped -= tapHandler;
	}

	void tapHandler(object sender, System.EventArgs e){
		home.manualStop (id);
	}
}
