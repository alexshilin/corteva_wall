using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TouchScript.Gestures;

public class Pin : MonoBehaviour {

	private bool ready = false;
	private bool active = true;
	private bool user = false;
	public Transform icon;
	public Transform info;
	public BoxCollider bc;
	public LayerMask globeLayer;
	public TextMeshPro label;
	public SpriteRenderer bg;
	private TapGesture tapGesture;

	public Vector2 latLon;

	[HideInInspector]
	public float baseSize;

	void Awake(){
	}

	void OnEnable(){
		baseSize = 0.2f;
	}

	void Update(){
		if (!ready)
			return;
		
		if (transform.position.z>100f) {
			if (active) {
				TogglePin(false);
				active = false;
			}
		} else {
			if (!active) {
				TogglePin(true);
				active = true;
			}
		}
		UpdatePinView ();
	}

	public void SetConfirm(){
		user = true;
		bc.enabled = true;
		tapGesture = GetComponent<TapGesture> ();
		tapGesture.Tapped += tapHandler;
	}

	public void UnsetConfirm(){
		if (tapGesture != null) {
			//user = false;
			bc.enabled = false;
			tapGesture.Tapped -= tapHandler;
		}
	}

	void OnDisable(){
		UnsetConfirm ();
	}

	void tapHandler(object sender, System.EventArgs e){
		Debug.Log ("QUESTION TIME");
		transform.parent.parent.GetComponent<PinDropEarth> ().PD.menu.ShowQuestionOne ();
	}
		
	public void SetPinText(string _text){
		label.text = _text;
		label.ForceMeshUpdate ();
		bg.size = new Vector2 (bg.size.y + (label.textBounds.size.x * 5.5f), bg.size.y);
		ready = true;
	}

	public void SetPinColor(Color _color){
		bg.GetComponent<Renderer> ().material.color = _color;
	}

	void UpdatePinView(){
		//WARN if you got here from a console error, its likely all those parent.parent... traversals
		if (transform.parent.parent.localScale.x < transform.parent.parent.GetComponent<PinDropEarth>().PD.initGlobeSize) {
			ToggleInfo (false);
		} else {
			ToggleInfo (true);
		}
		transform.rotation = Quaternion.identity;
		transform.localScale = Vector3.Lerp (transform.localScale, Vector3.one * (0.02f / (transform.parent.parent.localScale.x * 0.02f)) * baseSize, 0.75f);
	}

	void TogglePin(bool _on){
		icon.gameObject.SetActive (_on);
		ToggleInfo (false);
	}

	void ToggleInfo(bool _on){
		if (active && !user) {
			info.gameObject.SetActive (_on);
		}
	}
}
