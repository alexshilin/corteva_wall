using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneViewMask : MonoBehaviour {

	// tiles	  x 8, y 4
	// tile sizes x .125, y .25

	// offset x+.125 to more right
	// 		  y-.25 to move down

	public Renderer mask;
	public int cellsX = 8;
	public int cellsY = 4;
	public int cellsTotal = 32;
	private float cellOffsetX;
	private float cellOffsetY;
	private float startOffsetX = 0;
	private float startOffsetY = 0;

	void OnEnable(){
		
	}

	// Use this for initialization
	void Start () {
		cellOffsetX = 1f / cellsX;
		cellOffsetY = 1f / cellsY;
		startOffsetY = 1f - cellOffsetY;
		mask.material.mainTextureScale = new Vector2 (cellOffsetX, cellOffsetY);
		mask.material.SetTextureOffset ("_MainTex", new Vector2 (startOffsetX, startOffsetY));

		//mask.material.SetTextureOffset("_MainTex",
		//StartCoroutine(AnimateMask());
	}

	public void UpdateMask(int _i){
		int col = Mathf.RoundToInt(_i/cellsX);
		int row = Mathf.RoundToInt(_i%cellsX);
		//Debug.Log (_i + " = " + row + ", " + col);
		float xPos = startOffsetX + (cellOffsetX * row);
		float yPos = startOffsetY - (cellOffsetY * col);
		mask.material.SetTextureOffset ("_MainTex", new Vector2 (xPos, yPos));


//		float xPos = startOffsetX;
//		float yPos = startOffsetY;
//		for (int i = 1; i < cellsTotal; i++) {
//			if (_i == i) {
//				xPos += cellOffsetX;
//				if (i % cellsX == 0) {
//					yPos += -cellOffsetY;
//				}
//				Debug.Log (xPos + ", " + yPos);
//				mask.material.SetTextureOffset ("_MainTex", new Vector2 (xPos, yPos));
//				if (i % cellsX == 0) {
//					xPos = 0;
//				}
//			}
//		}

	}

	IEnumerator AnimateMask(){
		yield return new WaitForSeconds (0.5f);
		float xPos = startOffsetX;
		float yPos = startOffsetY;
		for (int i = 1; i < cellsTotal; i++) {
			xPos += cellOffsetX;
			if (i % cellsX == 0) {
				yPos += -cellOffsetY;
			}
			Debug.Log (xPos+", "+yPos);
			mask.material.SetTextureOffset ("_MainTex", new Vector2 (xPos, yPos));
			if (i % cellsX == 0) {
				xPos = 0;
			}
			yield return new WaitForSeconds (Random.Range(0.05f, 0.2f));
		}

	}
}
