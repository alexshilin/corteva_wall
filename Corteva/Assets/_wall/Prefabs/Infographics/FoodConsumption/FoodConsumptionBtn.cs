using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TouchScript.Gestures;

public class FoodConsumptionBtn : MonoBehaviour {

	public int id;
	public bool active;
	public FoodConsumption home;
	public Image fg;
	public Image bg;
	public TextMeshPro txt;

	private TapGesture tapGesture;

	void Start(){
		if (active)
			home.Toggle (id);
	}

	void OnEnable(){
		tapGesture = GetComponent<TapGesture> ();

		tapGesture.Tapped += tapHandler;
	}

	void OnDisable(){
		tapGesture.Tapped -= tapHandler;
	}

	void tapHandler(object sender, System.EventArgs e){
		home.Toggle (id);
	}

	public void ToggleBtn(bool _onOff){
		if (_onOff) {
			fg.color = new Color32 (0, 114, 206, 255);
			bg.color = new Color32 (189, 207, 226, 255);
			txt.color = fg.color;
		} else {
			fg.color = new Color32 (0, 114, 206, 0);
			bg.color = new Color32 (191, 191, 191, 255);
			txt.color = bg.color;
		}
	}
}
