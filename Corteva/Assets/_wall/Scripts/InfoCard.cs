using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoCard : MonoBehaviour {

	public SpriteRenderer topBar;
	public SpriteRenderer bg;
	public TextMeshPro title;
	public TextMeshPro body;
	private float titleMarginBottom = -0.1f;

	public void SetText(string _title, string _body, Color _barColor, Color _bgColor, Color _txtColor){
		title.text = _title;
		title.color = _txtColor;
		float titleHeight = 0.1f;
		if (_body != "") {
			//get updated mesh (so textInfo param is accurate)
			title.ForceMeshUpdate ();
			//set the body text
			body.text = _body;
			body.color = _txtColor;
			body.ForceMeshUpdate ();
			//reposition it to be directly under the title
			if (_title != "") {
				titleHeight = title.bounds.size.y;
			}
			body.transform.localPosition = title.transform.localPosition + Vector3.down * (titleHeight + titleMarginBottom);

		}
		bg.size = new Vector2(bg.size.x, 0 + titleHeight + body.bounds.size.y + 0.1f);
		topBar.color = _barColor;
		bg.color = _bgColor;
	}

	// Use this for initialization
	void Start () {
		/*
		SetText ("Our crop protection", 
			"Corteva Agroscrience's Crop Protection Platform operates in over 130 countries.\n\nOur Products feature 65 different ingredients.\n\nWe protext over 100 crops from insect pests, disease and weeds.",
			new Color (1, 1, 1, 1), new Color (0, 0, 0, 0.5f), Color.white);
		*/
	}
}
