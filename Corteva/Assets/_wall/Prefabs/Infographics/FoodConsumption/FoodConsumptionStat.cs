using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FoodConsumptionStat : MonoBehaviour {

	public float pctValue;
	public TextMeshPro pct;
	public TextMeshPro label;
	public SpriteRenderer line;

	private Vector2 finalLineSize;
	private float _duration = 0.5f;
	private float currPctValue;

	void OnEnable(){
		if (currPctValue > 0) {
			currPctValue = pctValue;
			pct.text = pctValue + "%";
		}
	}

	public void Show(){
		label.gameObject.SetActive (false);
		pct.gameObject.SetActive (false);
		currPctValue = 0;
		finalLineSize = line.size;
		line.size = new Vector2 (0, line.size.y);
		StartCoroutine (ShowStats());
	}

	IEnumerator ShowStats(){
		float t = 0.0f;
		float rate = 1 / _duration;
		yield return new WaitForSeconds (0.3f);
		while (t < 1) {
			if (gameObject.activeSelf) {
				t += rate * Time.deltaTime;
				line.size = Vector2.Lerp (line.size, finalLineSize, t);
				yield return null;
			} else {
				yield break;
			}
		}

		if (!label.gameObject.activeSelf && !pct.gameObject.activeSelf) {
			pct.text = currPctValue + "%";
			line.size = finalLineSize;
			label.gameObject.SetActive (true);
			pct.gameObject.SetActive (true);
		}

		while (currPctValue < pctValue) {
			if (gameObject.activeSelf) {
				currPctValue += 2f;
				pct.text = currPctValue + "%";
				yield return null;
			} else {
				yield break;
			}
		}

		pct.text = pctValue + "%";

		yield return null;
	}

}
