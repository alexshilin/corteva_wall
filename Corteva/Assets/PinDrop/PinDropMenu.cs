using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PinDropMenu : MonoBehaviour {

	public PinDrop PD;
	public TextMeshPro welcomeTxt;
	public TextMeshPro instruct;
	public GameObject icons;

	public GameObject q1;
	public GameObject q2;
	public GameObject bg;
	public GameObject backBtn;
	public GameObject closeBtn;


	private string q1a;
	private string q2a;

	void OnEnable(){
		welcomeTxt.alpha = 0;
	}

	public void ToggleWelcome(float _to, float _delay = 0){
		EaseCurve.Instance.TextAlpha (welcomeTxt, welcomeTxt.alpha, _to, 1f, _delay, EaseCurve.Instance.linear, null);
	}

	public void ShowQuestionOne(){
		PD.menu.instruct.text = "";
		closeBtn.SetActive (true);
		bg.SetActive (true);
		q1.SetActive (true);
		q2.SetActive (false);
	}

	public void ShowQuestionTwo(){
		q1.SetActive (false);
		q2.SetActive (true);
	}

	public void AnswerQuestionOne(string _answer){
		Debug.Log ("1: " + _answer);
		q1a = _answer;
		Invoke ("ShowQuestionTwo", 0.2f);
	}
	public void AnswerQuestionTwo(string _answer){
		Debug.Log ("2: " + _answer);
		q2a = _answer;
		Invoke ("FinishQuestions", 0.2f);
	}

	private void FinishQuestions(){
		CloseQuestions ();
		Pin p = PD.globe.newUserPin.GetComponent<Pin> ();
		p.UnsetConfirm ();
		p.SetPinText ("<b>"+q2a+"</b><br>"+q1a);
		p.SetPinColor (new Color32 (0, 191, 111, 255));
		p.baseSize *= 0.5f;
		PD.globe.GetComponent<PinDropEarth> ().newUserPin = null;
		PD.menu.instruct.text = "THANK YOU";
		//save to json
	}

	public void CloseQuestions(){
		closeBtn.SetActive (false);
		bg.SetActive (false);
		q1.SetActive (false);
		q2.SetActive (false);
	}

}
