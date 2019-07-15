using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TouchScript.Gestures;

public class DroneBtn : MonoBehaviour {

	public Drones home;
	public int id;
	public bool selected;
	public SpriteRenderer icon;
	public SpriteRenderer ring;
	public SpriteRenderer circle;
	public TextMeshPro label;
	private Color32 colorOn = new Color32(0, 190, 110, 255);
	private Color32 colorOff = new Color32 (57, 60, 57, 255);
	private TapGesture tapGesture;

	void OnEnable(){
		tapGesture = GetComponent<TapGesture> ();

		tapGesture.Tapped += tapHandler;

		if(selected)
			home.ToggleButtons (id);
	}

	void OnDisable(){
		tapGesture.Tapped -= tapHandler;
	}

	void tapHandler(object sender, System.EventArgs e){
		home.ToggleButtons (id);
	}

	public void Toggle(bool _onOff){
		selected = _onOff;
		if (selected) {
			icon.color = Color.white;
			ring.color = colorOn;
			circle.color = colorOn;
			label.color = colorOn;
		} else {
			icon.color = colorOff;
			ring.color = colorOff;
			circle.color = new Color32(0, 190, 110, 0);
			label.color = colorOff;
		}
	}
}
