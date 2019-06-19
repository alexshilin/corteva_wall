using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CrisprWhyIcon : MonoBehaviour {

	public Transform gfx;
	public Transform labels;
	public SpriteRenderer before;
	public SpriteRenderer after;
	public Image ring;
	public Image ring_d;

	private Vector3 gfxPos;
	private float ring_dFill; 
	private float ring_dRot;
	private float ringFill;
	private Vector3 txtRot;
	private Vector3 beforeScl;
	private Vector3 afterScl;
	private Color beforeCol;
	private Color afterCol;

	private float lerpDuration = 0.5f;
	private float t = 0.0f;

	private int step = -1;

	void Start () {
		Clean ();
	}

	public void Clean(){
		if (step != 0) {
			gfxPos = Vector3.zero;
			ring_dFill = 0;
			ring_dRot = 0;
			ringFill = 0;
			txtRot = Vector3.zero;
			beforeScl = Vector3.one * 0.0345f;
			afterScl = Vector3.one * 0.01f;
			beforeCol = new Color (1, 1, 1, 1);
			afterCol = new Color32 (0, 191, 111, 0);
			t = 0;
			step = 0;
		}
	}
	public void Ready(){
		if (step != 1) {
			gfxPos = new Vector3 (0, 0.075f, 0);
			ring_dFill = 1f;
			ring_dRot = 1f;
			ringFill = 0;
			txtRot = Vector3.zero;
			beforeScl = Vector3.one * 0.0345f;
			afterScl = Vector3.one * 0.01f;
			beforeCol = new Color (1, 1, 1, 1);
			afterCol = new Color32 (0, 191, 111, 0);
			t = 0;
			step = 1;
		}
	}
	public void End(){
		if (step != 2) {
			gfxPos = new Vector3 (0, 0.05f, 0);
			ring_dFill = 0f;
			ring_dRot = 0f;
			ringFill = 1f;
			txtRot = new Vector3 (-90f, 0, 0);
			beforeScl = Vector3.one * 0.01f;
			afterScl = Vector3.one * 0.0345f;
			beforeCol = new Color (1, 1, 1, 0);
			afterCol = new Color32 (0, 191, 111, 255);
			t = 0;
			step = 2;
		}
	}

	void Update () {
		float rate = 1 / lerpDuration;
		if (t < 1.0f) {
			t += rate * Time.deltaTime;
			gfx.localPosition = Vector3.Lerp (gfx.localPosition, gfxPos, t);
			ring_d.fillAmount = Mathf.Lerp (ring_d.fillAmount, ring_dFill, t);
			ring.fillAmount = Mathf.Lerp (ring.fillAmount, ringFill, t);
			labels.localRotation = Quaternion.Lerp (labels.localRotation, Quaternion.Euler (txtRot), t);
			before.transform.localScale = Vector3.Lerp (before.transform.localScale, beforeScl, t);
			after.transform.localScale = Vector3.Lerp (after.transform.localScale, afterScl, t);
			before.color = Color.Lerp (before.color, beforeCol, t);
			after.color = Color.Lerp (after.color, afterCol, t);
		}

		if (ring_dRot > 0) {
			ring_d.rectTransform.RotateAround (ring_d.rectTransform.position, Vector3.forward, ring_dRot);
		} else {
			ring_d.rectTransform.localRotation = Quaternion.Euler (0, 0, 180f);
		}
	}
}
