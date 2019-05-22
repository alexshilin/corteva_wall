using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TouchScript.Gestures;

public class Pin : MonoBehaviour {

	private bool active = true;
	public Transform icon;
	public Transform info;
	public BoxCollider bc;
	public LayerMask globeLayer;
	private TextMeshPro label;
	private SpriteRenderer bg;
	private TapGesture tapGesture;

	[HideInInspector]
	public float baseSize;

	void Start(){
		
	}

	void OnEnable(){
		baseSize = 0.2f;
		UpdatePinView ();
		label = info.GetComponentInChildren<TextMeshPro> ();
		bg = info.GetComponentInChildren<SpriteRenderer> ();
	}

	void Update(){
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
		bc.enabled = true;
		tapGesture = GetComponent<TapGesture> ();
		tapGesture.Tapped += tapHandler;
	}

	void UnsetConfirm(){
		if (tapGesture != null) {
			bc.enabled = false;
			tapGesture.Tapped -= tapHandler;
		}
	}

	void OnDisable(){
		UnsetConfirm ();
	}

	void tapHandler(object sender, System.EventArgs e){
		Debug.Log ("QUESTION TIME");
		UnsetConfirm ();
		SetPinText ("<b>Farming</b><br>Farmer");
		SetPinColor (new Color32 (0, 191, 111, 255));
		baseSize *= 0.5f;
		transform.parent.parent.GetComponent<PinDropEarth> ().newUserPin = null;
	}
		
	public void SetPinText(string _text){
		label.text = _text;
		label.ForceMeshUpdate ();
		bg.size = new Vector2 (bg.size.y + (label.textBounds.size.x * 5.5f), bg.size.y);
	}

	public void SetPinColor(Color _color){
		bg.GetComponent<Renderer> ().material.color = _color;
	}

	void UpdatePinView(){
		transform.rotation = Quaternion.identity;
		transform.localScale = Vector3.Lerp (transform.localScale, Vector3.one * (0.02f / (transform.parent.parent.localScale.x * 0.02f)) * baseSize, 0.75f);
		if (transform.parent.parent.localScale.x < 1) {
			ToggleInfo (false);
		} else {
			ToggleInfo (true);
		}
	}

	void TogglePin(bool _on){
		icon.gameObject.SetActive (_on);
		ToggleInfo (false);
	}

	void ToggleInfo(bool _on){
		if(active)
			info.gameObject.SetActive (_on);
	}
}
