using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures;

public class FlowsOfTradeBtn : MonoBehaviour {

	public FlowsOfTrade home;
	public int id;
	[Header("Parts")]
	public SpriteRenderer icon;
	public SpriteRenderer circle;
	private TapGesture tapGesture;

	void OnEnable(){
		tapGesture = GetComponent<TapGesture> ();

		tapGesture.Tapped += tapHandler;
	}

	void OnDiasable(){
		tapGesture.Tapped -= tapHandler;
	}

	void tapHandler(object sender, System.EventArgs e){
		home.PlayClip (id);
	}

	public void ToggleButton(bool _onOff){
		if (_onOff) {
			icon.color = Color.white;
			circle.color = new Color32 (0, 114, 206, 255);
		} else {
			icon.color = new Color32 (0, 114, 206, 255);
			circle.color = new Color32 (0, 114, 206, 0);
		}
	}
}
