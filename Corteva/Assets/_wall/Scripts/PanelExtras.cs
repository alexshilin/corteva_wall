using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PanelExtras : MonoBehaviour {

	public GameObject closeBtn;
	public GameObject backBtn;
	public GameObject moreBtn;


	public void ToggleBtns(bool _closeBtn, bool _backBtn, bool _moreBtn){
		closeBtn.SetActive (_closeBtn);
		backBtn.SetActive (_backBtn);
		moreBtn.SetActive (_moreBtn);
	}

	public void ColorBtns(Color _bgColor, Color _txtColor){
		closeBtn.GetComponentInChildren<SpriteRenderer> ().color = _bgColor;
		backBtn.GetComponentInChildren<SpriteRenderer> ().color = _bgColor;
		moreBtn.GetComponentInChildren<SpriteRenderer> ().color = _bgColor;

		closeBtn.GetComponentInChildren<TextMeshPro> ().color = _txtColor;
		backBtn.GetComponentInChildren<TextMeshPro> ().color = _txtColor;
		moreBtn.GetComponentInChildren<TextMeshPro> ().color = _txtColor;
	}
}
