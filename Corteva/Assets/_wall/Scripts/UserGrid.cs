﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class UserGrid : MonoBehaviour {

	public UserKiosk myKiosk;
	private int maxPanelsPerColumn = 3;
	private float panelSpacing = 0.1f;

	private float currColumn = 0;
	private float currRow = 0;
	private int currPanels = 0;
	private bool flip = false;

	public Vector3 emptySpot;
	public float emptySize;

	private List<int> gridCells = new List<int>();

	void Start () {
		//adjust vertical position of grid container so its centered with menu
		transform.localPosition += Vector3.down * -1.85f;

//		flip = Random.Range (0, 2) == 0 ? true : false;
//		if (Random.Range (0, 2) == 0) {
//			currPanels = 0;
//			currColumn = 0;
//		} else {
//			currPanels = 3;
//			currColumn = -1.5f;
//		}
	}

	/// <summary>
	/// Clears the grid.
	/// </summary>
	public void ClearGrid(){
		Debug.Log("[ClearGrid]");
		//remove all child objects
		foreach (Transform child in transform) {
			Destroy (child.gameObject);
		}
		//reset grid position calc variables
		currColumn = 0;
		currRow = 0;
		currPanels = 0;

//		flip = Random.Range (0, 2) == 0 ? true : false;
//		if (Random.Range (0, 2) == 0) {
//			currPanels = 0;
//			currColumn = 0;
//		} else {
//			currPanels = 3;
//			currColumn = -1.5f;
//		}

		//currPanels = Random.Range (0, 2) == 0 ? 0 : 3;
		gridCells.Clear ();
	}

	/// <summary>
	/// Generates the user grid based on this environments panels
	/// </summary>
	public void MakeGrid(){
		Debug.Log ("[MakeGrid]");
		Debug.Log ("\twith active panel? " + (myKiosk.activePanel!=null));
		Debug.Log ("\ttotal panels" + myKiosk.env.envPanelData.Count);

		//set up positioning vars
		float panelX, panelY, panelScale;
		Vector3 panelPostion;

		currPanels = 3;
		currColumn = 0.5f;//-1.5f;

		//calc grid width using groupings of 3
		//each group is 3.25 units of movement
		myKiosk.gridWidth = Mathf.Ceil(myKiosk.env.envPanelData.Count / 3f) * -3.25f;
//		Debug.Log("**** "+myKiosk.env.envPanelData.Count+
//			" | "+Mathf.Ceil(myKiosk.env.envPanelData.Count / 3f)+
//			" | "+(Mathf.Ceil(myKiosk.env.envPanelData.Count / 3f) * -3.25f));
		 
		//loop through enviroment panels JSON
		for (int i = 0; i < myKiosk.env.envPanelData.Count; i++) {

			panelScale = 1f;
			//update position vars
			currPanels++;
			//currRow++;
			if (currPanels / 3f <= 1) {
				
				currRow += 1;

			} else if (currPanels / 4f <= 1) {
				
				currRow = flip ? 2.5065f : 1.4933f;
				currColumn += 0.5f;//1.5f;
				panelScale = 2.019f;

			} else if (currPanels / 5f <= 1) {
				
				currRow = flip ? 1f : 3f;
				currColumn -= 0.5f;

			} else if (currPanels / 6f <= 1) {
				
				currRow = flip ? 1f : 3f;
				currColumn++;

			}

			//Debug.Log ("----- "+currPanels + " " + flip+" "+currColumn);

			//calculate panel position
			panelX = (currColumn * 5.333333f) + (currColumn * panelSpacing);
			panelY = -((currRow * 3) + (currRow * panelSpacing));
			panelPostion = new Vector3 (panelX, panelY, 0);
			JSONNode panelData = JSON.Parse (myKiosk.env.envPanelData[i]);


			if (currPanels / 6f == 1) {
				currRow = 0f;
				currColumn++;
				currPanels = 3;//0;
				flip = !flip;

			}

			//leave empty spot is theres an active panel (from activtion from idle)
			//if (i == 0) {
			if (myKiosk.activePanel != null) {
				if (panelData ["nid"] == myKiosk.activePanel.GetComponent<PanelBase> ().panelID) {
					Debug.Log ("\ti: " + panelPostion + " (reserved)");
					//save its position in the grid
					emptySpot = panelPostion;
					emptySize = panelScale < 1.1f ? 0.3f : 0.606f;
					//go to next item
					continue;
				}
			}
			//}


			//instantiate panel and make child of user grid
			GameObject panel = Instantiate (AssetManager.Instance.NEWpanelPrefab, transform);
			panel.transform.localPosition = panelPostion;
			panel.transform.localScale = Vector3.one * panelScale;
			panel.name = "p"+i;

			//update panel contentd
			PanelBase po = panel.GetComponent<PanelBase> ();
			po.panelContext = PanelBase.PanelContext.Kiosk;
			po.panelState = PanelBase.PanelState.Ready;
			po.myKiosk = myKiosk; //TMP, change this
			po.environment = myKiosk.env;
			po.panelID = panelData ["nid"];
			po.panelName = panelData ["reference_title"];
			po.Assemble (panelData);
			po.ActivateView (PanelBase.PanelView.Thumbnail, false);
		}
	}
}
