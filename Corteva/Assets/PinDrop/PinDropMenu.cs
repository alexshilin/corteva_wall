using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PinDropMenu : MonoBehaviour {

	public PinDrop PD;
	public TextMeshPro welcomeTxt;

	void OnEnable(){
		welcomeTxt.alpha = 0;
	}

	public void ToggleWelcome(float _to){
		EaseCurve.Instance.TextAlpha (welcomeTxt, welcomeTxt.alpha, _to, 1f, 0, EaseCurve.Instance.linear, null);
	}

}
