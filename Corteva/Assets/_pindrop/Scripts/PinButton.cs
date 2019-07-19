using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TouchScript.Gestures;
using UnityEngine.Events;

public class PinButton : MonoBehaviour{

	public TextMeshPro label;
	public SpriteRenderer outline;
	public SpriteRenderer bg;
	public BoxCollider bc;

	private TapGesture tapGesture;

	public string labelText;
	public Color btnColor = Color.white;
	private Color btnColorOff;

	public bool isSelected = false;

	void Awake(){
		
	}

	void OnEnable(){
		tapGesture = GetComponent<TapGesture> ();

		tapGesture.Tapped += tapHandler;
	}
		
	void Start () {
		SetBtnColor (btnColor);
		bg.color = btnColorOff;
		if (labelText != null) {
			SetBtnText (labelText);
		}
		SetBtnColor (btnColor);
		SetBtnState (isSelected);
	}

	void OnDisable(){
		tapGesture.Tapped -= tapHandler;
		SetBtnState (false);
	}

	public void SetBtnText(string _text){
		labelText = _text;
		label.text = _text;
		label.ForceMeshUpdate ();
		float txtWidth = (label.bounds.size.x * 32f);
		//Debug.Log (label.bounds.size.x+" "+txtWidth + " "+_text);
		outline.size = new Vector2 (txtWidth + 4f, outline.size.y);
		bg.size = new Vector2 (txtWidth + 4f, bg.size.y);
		bc.size = bg.size;
		bc.center = new Vector3 (bc.size.x * 0.5f, 0, 0);
	}

	public void SetBtnColor(Color _color){
		btnColor = _color;
		btnColorOff = btnColor;
		btnColorOff.a = 0;
		outline.color = btnColor;
		bg.color = btnColor;
	}

	public void SetBtnState(bool _active){
		if (_active) {
			bg.color = btnColor;
		} else {
			bg.color = btnColorOff;
		}
	}

	private void tapHandler(object sender, System.EventArgs e){
		Color highlightColor = btnColorOff;
		highlightColor.a = 0.25f;
		bg.color = highlightColor;
		Debug.Log (onInteraction.GetPersistentEventCount ());
		Interact ();
	}

	#region Click implementation

	[System.Serializable]
	public class InteractionEvent : UnityEvent {}

	public InteractionEvent onInteraction = new InteractionEvent();

	public void Interact()
	{
		onInteraction.Invoke();
	}

	#endregion
}
