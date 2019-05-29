using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// This is added as a component to PanelModules with text fields
/// </summary>
public class PanelText : MonoBehaviour {

	public TextMeshPro title;
	public TextMeshPro body;
	private float titleMarginBottom = 0.1f;

	/// <summary>
	/// Sets the text for a given module.
	/// </summary>
	/// <param name="_category">Category text (the small bit that appears above the title)</param>
	/// <param name="_title">Title text.</param>
	/// <param name="_body">Body text.</param>
	public void SetText(string _category, string _title, string _body, Color _color){
		//set the title text
		title.text = _title;
		title.color = _color;
		if (body != null) {
			//if theres body text
			if (_body != "") {
				//get updated mesh (so textInfo param is accurate)
				title.ForceMeshUpdate ();
				//set the body text
				body.text = _body;
				body.color = _color;
				//reposition it to be directly under the title
				float titleHeight = 0f;
				if (_title != "") {
					titleHeight = title.bounds.size.y;
				}
				body.rectTransform.localPosition = title.transform.localPosition + Vector3.down * (titleHeight + titleMarginBottom);
			} else {
				//otherwise just hide the gameObject
				body.gameObject.SetActive (false);
			}
		}
	}

	void Start () {
		/*
		SetText ( "", 
			"Pearl Millet is an<br>important staple<br>crop in India.", 
			"However, it goes rancid very quickly after being milled into flour, which causes huge amounts of Pearl Millet to be wasted.<br><br>Traditionally, agriculture companies don’t focus on millet because of its relatively low profitability. Corteva Agriscience believes in a different way of operating. Recently, researchers partnered with ICRISAT, a non-profit research institute, to develop improved Pearl Millet varieties that have a longer shelf life. This reduces waste and enriches the lives of both farmers and consumers. ", 
			Color.white);
		*/
	}
}
