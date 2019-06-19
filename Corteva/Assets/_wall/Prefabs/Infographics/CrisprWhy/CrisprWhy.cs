using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures.TransformGestures;

public class CrisprWhy : MonoBehaviour {

	public Transform crispr;
	public TransformGesture transformGesture;
	public SpriteRenderer plant;
	public List<CrisprWhyIcon> icons = new List<CrisprWhyIcon>();

	private float dist;

	void OnEnable(){
		transformGesture.Transformed += transformHandler;
		transformGesture.TransformCompleted += transformEndHandler;
	}
	void OnDisable(){
		transformGesture.Transformed -= transformHandler;
		transformGesture.TransformCompleted -= transformEndHandler;

		plant.color = Color.white;
		crispr.localPosition = new Vector3 (-1.69f, -0.874f, -0.02f);
		crispr.localScale = Vector3.one * 0.07033154f;
		foreach (CrisprWhyIcon i in icons) {
			i.Clean ();
		}
	}

	void transformHandler(object sender, System.EventArgs e){
		crispr.localScale = Vector3.one * 0.07033154f;
		crispr.localPosition += transformGesture.LocalDeltaPosition;
		dist = Vector3.Distance (crispr.localPosition, plant.transform.localPosition);
		if (dist < 1f) {
			foreach (CrisprWhyIcon i in icons) {
				i.Ready ();
			}
		} else {
			plant.color = Color.white;
			foreach (CrisprWhyIcon i in icons) {
				i.Clean ();
			}
		}
	}

	void transformEndHandler(object sender, System.EventArgs e){
		dist = Vector3.Distance (crispr.localPosition, plant.transform.localPosition);
		if (dist < 1f) {
			plant.color = new Color32 (0, 191, 111, 255);
			crispr.localPosition = new Vector3 (0.808f, -0.616f, -0.02f);
			crispr.localScale = Vector3.one * 0.05f;
			foreach (CrisprWhyIcon i in icons) {
				i.End ();
			}
		} else {
			plant.color = Color.white;
			crispr.localPosition = new Vector3 (-1.69f, -0.874f, -0.02f);
			foreach (CrisprWhyIcon i in icons) {
				i.Clean ();
			}
		}
	} 
}
