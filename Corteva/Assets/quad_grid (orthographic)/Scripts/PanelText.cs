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
	public Transform titleSizer;
	public float titleHeightCalcMod = 1; //set this value in the inspector!

	/// <summary>
	/// Sets the text for a given module.
	/// </summary>
	/// <param name="_category">Category text (the small bit that appears above the title)</param>
	/// <param name="_title">Title text.</param>
	/// <param name="_body">Body text.</param>
	public void SetText(string _category, string _title, string _body, Color _color){
		//string titleTxt = "Global<br>problems<br>need local<br>solutions."; //cant have line breaks at the end <br> \n 
		//string setBody = "\n<size=.55>\n<width=55%><margin-left=0.03><size=0.88><cspace=-.0008>";
		//string bodyTxt = "That's why Corteva Agriscience™ is helping scientists around the world research new ways of growing.";
		//title.SetText (_title+setBody+_body);


		//annoyingly, the dll only version of textmesh pro seems to have no way to get the bounding height of a text field
		//based on the text thats in it.
		//so we have to roughly calculate it based on the line count and font size
		//but since font sizes and scales are weird in unity, we also need to add a modifier to this calculation (titleHeightCalcMod)
		//titleHeightCalcMod is found manually by plugging in values with actual data and checking to see if the body text container
		//gets placed at the right height.
		float calcHeight;
		//set the title text
		title.text = _title;
		title.color = _color;
		if (body != null) {
			//if theres body text
			if (_body != "") {
				//get updated mesh (so textInfo param is accurate)
				title.ForceMeshUpdate ();
				//do the height calculation
				calcHeight = (float)title.textInfo.lineCount * (title.fontSize * titleHeightCalcMod);
				//set the titleSizer gameobject to match that height for reference
				titleSizer.localScale = new Vector3 (1, calcHeight, 1);
				//set the body text
				body.text = _body;
				body.color = _color;
				//reposition it to be directly under the title
				body.rectTransform.localPosition = title.transform.localPosition + Vector3.down * titleSizer.localScale.y;
			} else {
				//otherwise just hide the gameObject
				body.gameObject.SetActive (false);
			}
		}
	}

	void Start () {
		//SetText ("1x2_txt_layout_l", "", "Pearl Millet is an<br>important staple<br>crop in India.", "However, it goes rancid very quickly after being milled into flour, which causes huge amounts of Pearl Millet to be wasted.<br><br>Traditionally, agriculture companies don’t focus on millet because of its relatively low profitability. Corteva Agriscience believes in a different way of operating. Recently, researchers partnered with ICRISAT, a non-profit research institute, to develop improved Pearl Millet varieties that have a longer shelf life. This reduces waste and enriches the lives of both farmers and consumers. ");
	}
}
