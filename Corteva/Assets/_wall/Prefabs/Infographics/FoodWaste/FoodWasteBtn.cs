using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures;
using TMPro;
using System;

public class FoodWasteBtn : MonoBehaviour {

	public FoodWaste home;
	public int id;
	private bool active = false;
	[Header("Parts")]
	public SpriteRenderer icon;
	public SpriteRenderer circle;
	public TextMeshPro label;
	private TapGesture tapGesture;

	void OnEnable(){
		tapGesture = GetComponent<TapGesture> ();

		tapGesture.Tapped += tapHandler;
	}

	void OnDisable(){
		tapGesture.Tapped -= tapHandler;
		active = false;
		ToggleButton (active);
	}

	void tapHandler(object sender, EventArgs e){
		if (!active) {
			active = true;
			home.UpdateLevel (1);
		} else {
			active = false;
			home.UpdateLevel (-1);
		}
		ToggleButton (active);
	}

	void ToggleButton(bool _onOff){
		if (_onOff) {
			icon.color = Color.white;
			circle.color = new Color32 (0, 114, 206, 255);
			label.color = new Color32 (0, 114, 206, 255);
			home.UpdateCard (id);
		} else {
			icon.color = new Color32(65, 64, 66, 255);
			circle.color = new Color32(0, 114, 206, 0);
			label.color = new Color32(65, 64, 66, 255);
			home.UpdateCard (0);
		}
	}
}
