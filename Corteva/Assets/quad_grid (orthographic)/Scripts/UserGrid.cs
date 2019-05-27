using System.Collections;
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
		flip = Random.Range (0, 2) == 0 ? true : false;
		if (Random.Range (0, 2) == 0) {
			currPanels = 0;
			currColumn = 0;
		} else {
			currPanels = 3;
			currColumn = -1.5f;
		}
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
		flip = Random.Range (0, 2) == 0 ? true : false;
		if (Random.Range (0, 2) == 0) {
			currPanels = 0;
			currColumn = 0;
		} else {
			currPanels = 3;
			currColumn = -1.5f;
		}

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

		 

		//loop through enviroment panels JSON
		for (int i = 0; i < myKiosk.env.envPanelData.Count; i++) {
			panelScale = 1f;
			//update position vars
			currPanels++;
			//currRow++;
			if (currPanels / 3f <= 1) {
				
				currRow += 1;
				Debug.Log (i + ": " + currPanels + " " + (currPanels / 3f) + " " +currRow+", "+currColumn);

			} else if (currPanels / 4f <= 1) {
				currRow = flip ? 2.5f : 1.5f;
				currColumn += 1.5f;
				panelScale = 2.03f;
				Debug.Log (i + ": " + currPanels + " " + (currPanels / 4f) + " " +currRow+", "+currColumn);

			} else if (currPanels / 5f <= 1) {
				currRow = flip ? 1f : 3f;
				currColumn -= 0.5f;
				Debug.Log (i + ": " + currPanels + " " + (currPanels / 5f) + " " +currRow+", "+currColumn);

			} else if (currPanels / 6f <= 1) {
				currRow = flip ? 1f : 3f;
				currColumn++;
				Debug.Log (i + ": " + currPanels + " " + (currPanels / 6f) + " " +currRow+", "+currColumn);

			}



			//calculate panel position
			panelX = (currColumn * 5.333333f) + (currColumn * panelSpacing);
			panelY = -((currRow * 3) + (currRow * panelSpacing));
			panelPostion = new Vector3 (panelX, panelY, 0);
			JSONNode panelData = JSON.Parse (myKiosk.env.envPanelData[i]);


			if (currPanels / 6f == 1) {
				currRow = 0f;
				currColumn++;
				currPanels = 0;
				flip = !flip;
				Debug.Log (i + ": " + currPanels + " " + (currPanels / 6f) + " " +currRow+", "+currColumn);

			}

			//leave empty spot is theres an active panel (from activtion from idle)
			//if (i == 0) {
			if (myKiosk.activePanel != null) {
				if (panelData ["reference_title"] == myKiosk.activePanel.GetComponent<PanelBase> ().panelID) {
					Debug.Log ("\ti: " + panelPostion + " (reserved)");
					//save its position in the grid
					emptySpot = panelPostion;
					emptySize = panelScale < 1.1f ? 0.3f : 0.609f;
					//go to next item
					continue;
				}
			}
			//}

			Debug.Log ("\ti: " + panelPostion);

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
			po.Assemble (panelData);
			po.ActivateView (PanelBase.PanelView.Thumbnail, false);

		}
	}
}
