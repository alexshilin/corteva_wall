using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PinDropResultsBar : MonoBehaviour {

	public SpriteRenderer fgBar;
	public TextMeshPro label;
	public TextMeshPro labelB;
	public TextMeshPro pctLabel;
	public TextMeshPro pctLabelB;

	private bool startPlaying = false;
	private float t = 0;
	private float barFill;

	void Start(){
		//SetBar (false, "Farmer", 50, Color.green);
	}

	public void SetBar (bool _highlight, string _labelValue, int _pctValue, Color _color) {
		barFill = 70 * (_pctValue * .01f);
		fgBar.size = new Vector2 (5, fgBar.size.y);
		fgBar.color = _color;
		if (_highlight) {
			labelB.text = _labelValue;
			pctLabelB.text = _pctValue + "%";
		} else {
			label.text = _labelValue;
			pctLabel.text = _pctValue + "%";
		}
		startPlaying = true;
	}
		
	void Update(){
		if (startPlaying) {
			if (t < 1) {
				t += Time.deltaTime;
			} else if (t >= 1) {
				startPlaying = false;
			}
				
			fgBar.size = new Vector2 (Mathf.Lerp (fgBar.size.x, barFill, t), fgBar.size.y);

		}
	}

}
