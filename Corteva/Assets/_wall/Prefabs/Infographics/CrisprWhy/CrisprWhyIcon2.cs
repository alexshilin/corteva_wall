using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CrisprWhyIcon2 : MonoBehaviour {

	public Transform gfx;
	public Transform labels;
	public SpriteRenderer icon;
	public Image ring;

	private Vector3 gfxPos;
	private float ring_dFill; 
	private Vector3 txtPos;
	private Vector3 txtRot;
	private Vector3 afterScl;
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
			txtPos = new Vector3 (0f, -0.2f, -0.066f);
			txtRot = Vector3.zero;
			afterScl = Vector3.one * 0.01f;
			afterCol = new Color32 (0, 191, 111, 0);
			t = 0;
			step = 0;
		}
	}
	public void Ready(){
		if (step != 1) {
			gfxPos = new Vector3 (0, 0.075f, 0);
			ring_dFill = 1f;
			txtPos = new Vector3 (0f, -0.2f, -0.066f);
			txtRot = Vector3.zero;
			afterScl = Vector3.one * 0.01f;
			afterCol = new Color32 (0, 191, 111, 0);
			t = 0;
			step = 1;
		}
	}
	public void End(){
		if (step != 2) {
			gfxPos = new Vector3 (0, 0.05f, 0);
			ring_dFill = 1f;
			txtPos = new Vector3 (0f, -0.242f, -0.066f);
			txtRot = new Vector3 (-90f, 0, 0);
			afterScl = Vector3.one * 0.03f;
			afterCol = new Color32 (0, 191, 111, 255);
			t = 0;
			step = 2;
		}
	}

	void Update () {
		float rate = 1 / lerpDuration;
		if (t < 1.0f) {
			t += rate * Time.deltaTime;
			//gfx.localPosition = Vector3.Lerp (gfx.localPosition, gfxPos, t);
			//before.transform.localScale = Vector3.Lerp(before.transform.localScale, beforeScl, t);
			ring.fillAmount = Mathf.Lerp (ring.fillAmount, ring_dFill, t);
			//ring_d.rectTransform.localScale = Vector3.Lerp(ring_d.rectTransform.localScale, Vector3.one * ring_dScl, t);
			//ring_d.color = Color.Lerp (ring_d.color, ring_dCol, t);
			//ring.fillAmount = Mathf.Lerp (ring.fillAmount, ringFill, t);

			labels.localPosition = Vector3.Lerp (labels.localPosition, txtPos, t);
			labels.localRotation = Quaternion.Lerp (labels.localRotation, Quaternion.Euler (txtRot), t);

			//before.transform.localScale = Vector3.Lerp (before.transform.localScale, beforeScl, t);
			icon.transform.localScale = Vector3.Lerp (icon.transform.localScale, afterScl, t);
			//before.color = Color.Lerp (before.color, beforeCol, t);
			icon.color = Color.Lerp (icon.color, afterCol, t);
		}
	}
}
