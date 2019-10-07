using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PinDropResultsKey : MonoBehaviour {

	public SpriteRenderer icon;
	public TextMeshPro label;
	[HideInInspector]
	public float labelWidth;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	public void SetLabel (string _label, Color _color) {
		icon.color = _color;
		label.text = _label;
		label.ForceMeshUpdate ();
		//for the bounds sizing to be accurate,
		//the textmesh object must have a scale of 1
		labelWidth = label.bounds.size.x;
	}
}
