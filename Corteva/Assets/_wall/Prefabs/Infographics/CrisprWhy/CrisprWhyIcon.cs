using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CrisprWhyIcon : MonoBehaviour {

	public Transform gfx;
	public Transform labels;
	public SpriteRenderer icon;
	public Image ring;

	private float ring_dFill; 
	private float ring_dRot;
	private float ring_dScl;
	private Color ring_dCol;
	private Vector3 txtPos;
	private Vector3 txtRot;
	private Vector3 beforeScl;
	private Color beforeCol;

	private float lerpDuration = 0.5f;
	private float t = 0.0f;

	private int step = -1;

	void Start () {
		Clean ();
	}

	public void Clean(){
		if (step != 0) {
			ring_dFill = 0;
			ring_dRot = 0;
			ring_dScl = 0.00276f;
			ring_dCol = new Color32 (0, 191, 111, 255);
			txtPos = new Vector3 (0f, -0.2f, -0.066f);
			txtRot = Vector3.zero;
			beforeScl = Vector3.one * 0.0345f;
			beforeCol = new Color (1, 1, 1, 1);
			t = 0;
			step = 0;
		}
	}
	public void Ready(){
		if (step != 1) {
			ring_dFill = 1f;
			ring_dRot = 1f;
			ring_dScl = 0.00276f;
			ring_dCol = new Color32 (0, 191, 111, 255);
			txtPos = new Vector3 (0f, -0.242f, -0.066f);
			txtRot = Vector3.zero;
			beforeScl = Vector3.one * 0.025f;
			beforeCol = new Color (1, 1, 1, 1);
			t = 0;
			step = 1;
		}
	}
	public void End(){
		if (step != 2) {
			ring_dFill = 1f;
			ring_dRot = 0f;
			ring_dScl = 0.00347f;
			ring_dCol = new Color32 (0, 191, 111, 0);
			txtPos = new Vector3 (0f, -0.2f, -0.066f);
			txtRot = new Vector3 (-90f, 0, 0);
			beforeScl = Vector3.one * 0.01f;
			beforeCol = new Color (1, 1, 1, 0);
			t = 0;
			step = 2;
		}
	}

	void Update () {
		float rate = 1 / lerpDuration;
		if (t < 1.0f) {
			t += rate * Time.deltaTime;
			//gfx.localPosition = Vector3.Lerp (gfx.localPosition, gfxPos, t);
			icon.transform.localScale = Vector3.Lerp(icon.transform.localScale, beforeScl, t);
			ring.fillAmount = Mathf.Lerp (ring.fillAmount, ring_dFill, t);
			ring.rectTransform.localScale = Vector3.Lerp(ring.rectTransform.localScale, Vector3.one * ring_dScl, t);
			ring.color = Color.Lerp (ring.color, ring_dCol, t);
			//ring.fillAmount = Mathf.Lerp (ring.fillAmount, ringFill, t);

			labels.localPosition = Vector3.Lerp (labels.localPosition, txtPos, t);
			labels.localRotation = Quaternion.Lerp (labels.localRotation, Quaternion.Euler (txtRot), t);

			icon.transform.localScale = Vector3.Lerp (icon.transform.localScale, beforeScl, t);
			//after.transform.localScale = Vector3.Lerp (after.transform.localScale, afterScl, t);
			icon.color = Color.Lerp (icon.color, beforeCol, t);
			//after.color = Color.Lerp (after.color, afterCol, t);
		}

		if (ring_dRot > 0) {
			ring.rectTransform.RotateAround (ring.rectTransform.position, Vector3.forward, ring_dRot);
		} else {
			ring.rectTransform.localRotation = Quaternion.Euler (0, 0, 180f);
		}
	}
}
