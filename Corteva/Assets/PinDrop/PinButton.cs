using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TouchScript.Gestures;
using UnityEngine.Events;

public class PinButton : MonoBehaviour{

	private TextMeshPro label;
	private SpriteRenderer outline;
	private SpriteRenderer bg;
	private BoxCollider bc;

	private TapGesture tapGesture;

	public string labelText;
	public Color btnColor = Color.white;
	private Color btnColorOff;

	public bool isSelected = false;

	void OnEnable(){
		label = GetComponentInChildren<TextMeshPro> ();
		outline = transform.Find ("outline").GetComponent<SpriteRenderer> ();
		bg = transform.Find ("bg").GetComponent<SpriteRenderer> ();
		bg.color = new Color (1f, 1f, 1f, 0f);
		bc = GetComponent<BoxCollider> ();

		tapGesture = GetComponent<TapGesture> ();

		tapGesture.Tapped += tapHandler;
	}
		
	void Start () {
		if (labelText != null) {
			SetBtnText (labelText);
		}
		if (btnColor != null) {
			SetBtnColor (btnColor);
		}
		SetBtnState (isSelected);
	}

	void OnDisable(){
		tapGesture.Tapped -= tapHandler;
	}

	public void SetBtnText(string _text){
		label.text = _text;
		label.ForceMeshUpdate ();
		float txtWidth = (label.bounds.size.x * 32f);
		Debug.Log (label.bounds.size.x+" "+txtWidth + " "+_text);
		outline.size = new Vector2 (txtWidth + 4f, outline.size.y);
		bg.size = new Vector2 (txtWidth + 4f, bg.size.y);
		float colliderWidth = (label.bounds.size.x * 1.15f);
		bc.size = new Vector3 (colliderWidth, bc.size.y, bc.size.z);
		bc.center = new Vector3 (colliderWidth * 0.5f, bc.center.y, bc.center.z);
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
		bg.color = new Color (1f, 1f, 1f, 0.25f);
		Interact ();
	}

	#region IPointerClickHandler implementation

	[System.Serializable]
	public class InteractionEvent : UnityEvent {}

	public InteractionEvent onInteraction = new InteractionEvent();

	public void Interact()
	{
		onInteraction.Invoke();
	}

	#endregion
}
