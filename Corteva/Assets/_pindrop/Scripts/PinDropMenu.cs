using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SimpleJSON;

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
	public GameObject btn;

	private List<GameObject> Q1B = new List<GameObject>();
	private List<GameObject> Q2B = new List<GameObject>();

	private string q1a;
	private string q2a;

	void OnEnable(){
		welcomeTxt.alpha = 0;
	}

	void Start(){
		MakeButtons ();
	}

	public void ToggleWelcome(float _to, float _delay = 0){
		EaseCurve.Instance.TextAlpha (welcomeTxt, welcomeTxt.alpha, _to, 1f, _delay, EaseCurve.Instance.linear, null);
	}


	private void MakeButtons(){
		JSONNode data = PinData.Instance.pinData;
		float row = 0;

		q1.SetActive (true);
		Transform q1btns = q1.transform.Find("Btns");
		for (int i = 0; i < data ["roles"].Count; i++) {
			GameObject b = Instantiate (btn, q1btns);
			PinButton pb = b.GetComponent<PinButton> ();
			pb.SetBtnText(data["roles"][i]["title"]);
			pb.btnColor = Color.white;
			pb.onInteraction.AddListener (() => AnswerQuestionOne(pb.labelText));
			Vector3 pos = Vector3.zero;
			if (Q1B.Count > 0) {
				if (PD.myKiosk != null) {
					pos.x = Q1B [i - 1].transform.localPosition.x + (Q1B [i - 1].GetComponent<PinButton> ().bc.bounds.size.x + 0.03f);
				} else {
					pos.x = Q1B [i - 1].transform.localPosition.x + (Q1B [i - 1].GetComponent<PinButton>().bc.bounds.extents.x * 0.7f);
				}
				if (pos.x > 2f) {
					pos.x = 0f;
					row++;
				}
			}
			pos.y = -0.25f * row;
			Debug.Log ("Q1 "+i+": " + row+" ("+Q1B.Count+")");
			b.transform.localPosition = pos;
			Q1B.Add (b);
		}
		q1.SetActive (false);

		q2.SetActive (true);
		Transform q2btns = q2.transform.Find ("Btns");
		row = 0;
		for (int i = 0; i < data ["challenges"].Count; i++) {
			GameObject b = Instantiate (btn, q2btns);
			PinButton pb = b.GetComponent<PinButton> ();
			pb.SetBtnText(data["challenges"][i]["title"]);
			pb.btnColor = Color.white;
			pb.onInteraction.AddListener (() => AnswerQuestionTwo(pb.labelText));
			Vector3 pos = Vector3.zero;
			if (Q2B.Count > 0) {
				if (PD.myKiosk != null) {
					pos.x = Q2B [i - 1].transform.localPosition.x + (Q2B [i - 1].GetComponent<PinButton> ().bc.bounds.size.x + 0.03f);
				} else {
					pos.x = Q2B [i - 1].transform.localPosition.x + (Q2B [i - 1].GetComponent<PinButton> ().bc.bounds.extents.x * 0.7f);
				}
				if (pos.x > 2f) {
					pos.x = 0f;
					row++;
				}
			}
			pos.y = -0.25f * row;
			b.transform.localPosition = pos;
			Q2B.Add (b);
		}
		q2.SetActive (false);
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
		PinData.Instance.SavePin (p.latLon, q1a, q2a);
		PD.globe.newUserPin = null;
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
