using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TouchScript.Gestures;

public class SeedPipelineBtn : MonoBehaviour {

	public SeedPipeline home;
	public int id;
	public SpriteRenderer icon;
	public SpriteRenderer ring;
	public SpriteRenderer circle;
	public TextMeshPro label;
	private TapGesture tapGesture;

	void OnEnable(){
		tapGesture = GetComponent<TapGesture> ();
		tapGesture.Tapped += tapHandler;
	}

	void OnDisable(){
		tapGesture.Tapped -= tapHandler;
	}

	void tapHandler(object sender, System.EventArgs e){
		home.Tap (id);
	}

	public void Toggle(bool _onOff){
		if (_onOff) {
			icon.color = Color.white;
			ring.color = new Color32 (0, 190, 107, 255);
			circle.color = new Color32 (0, 190, 107, 255);
			label.color = new Color32 (0, 190, 107, 255);
		} else {
			icon.color = new Color32 (26, 26, 26, 255);
			ring.color = new Color32 (26, 26, 26, 255);
			circle.color = new Color32 (0, 190, 107, 0);
			label.color = new Color32 (26, 26, 26, 255);
		}
	}
}
