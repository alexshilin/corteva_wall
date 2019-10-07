using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PinDropResultsRing : MonoBehaviour {

	public Image ringThin;
	public Image ringThick;
	public Transform pct;
	public TextMeshPro pctValue;
	private Image activeRing;
	private bool startPlaying = false;
	private float ringFill;
	private float t = 0;

	void Start () {
		
	}

	public void SetRing (bool _highlight, int _pctValue, float _ringFillAmt, float _ringOffset, float _pctOffset, Color _color) {
		if (_highlight) {
			ringThin.enabled = false;
			ringThick.enabled = true;
			activeRing = ringThick;
		} else {
			ringThin.enabled = true;
			ringThick.enabled = false;
			activeRing = ringThin;
			pctValue.text = _pctValue + "%";
			pct.localRotation *= Quaternion.Euler (0, 0, -360 * (_pctOffset * 0.01f));
			pctValue.rectTransform.localRotation = Quaternion.Euler(180, 180, -pct.localEulerAngles.z);
		}
		activeRing.rectTransform.localRotation = Quaternion.Euler (0, 0, -360 * (_ringOffset * 0.01f));
		activeRing.fillAmount = 0;
		ringFill = _ringFillAmt * 0.01f;
		activeRing.color = _color;
		startPlaying = true;
	}

	void Update(){
		if (startPlaying) {
			if (t < 1) {
				t += Time.deltaTime;
			} else if (t >= 1) {
				startPlaying = false;
			}
				
			activeRing.fillAmount = Mathf.Lerp (0, ringFill, t);

		}
	}
}
