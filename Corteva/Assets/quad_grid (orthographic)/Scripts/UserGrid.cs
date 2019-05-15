using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class UserGrid : MonoBehaviour {

	public UserKiosk myKiosk;
	private int maxPanelsPerColumn = 3;
	private float panelSpacing = 0.1f;

	private int currColumn = 1;
	private int currRow = 1;
	private int currPanelsInColumn = 0;

	public Vector3 emptySpot;

	private List<int> gridCells = new List<int>();

	void Start () {
		//adjust vertical position of grid container so its centered with menu
		transform.localPosition += Vector3.down * -1.85f;
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
		currColumn = 1;
		currRow = 1;
		currPanelsInColumn = 0;
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
		float panelX, panelY;
		Vector3 panelPostion;

		//loop through enviroment panels JSON
		for (int i = 0; i < myKiosk.env.envPanelData.Count; i++) {
			//calculate panel position
			panelX = (currColumn * 5.333333f) + (currColumn * panelSpacing);
			panelY = -((currRow * 3) + (currRow * panelSpacing));
			panelPostion = new Vector3 (panelX, panelY, 0);

			//update position vars
			currPanelsInColumn++;
			currRow++;
			if (currPanelsInColumn == maxPanelsPerColumn) {
				currPanelsInColumn = 0;
				currRow = 1;
				currColumn++;
			}

			//leave empty spot is theres an active panel (from activtion from idle)
			//if (i == 0) {
				if (myKiosk.activePanel != null) {
				if (myKiosk.env.envPanelData [i] ["panelID"] == myKiosk.activePanel.GetComponent<PanelBase> ().panelID) {
					Debug.Log ("\ti: " + panelPostion + " (reserved)");
					//save its position in the grid
					emptySpot = panelPostion;
					//go to next item
					continue;
				}
				}
			//}

			Debug.Log ("\ti: " + panelPostion);

			//instantiate panel and make child of user grid
			GameObject panel = Instantiate (AssetManager.Instance.NEWpanelPrefab, transform);
			panel.transform.localPosition = panelPostion;
			panel.name = "p"+i;

			//update panel contentd
			PanelBase po = panel.GetComponent<PanelBase> ();
			po.panelContext = PanelBase.PanelContext.Kiosk;
			po.panelState = PanelBase.PanelState.Ready;
			po.myKiosk = myKiosk; //TMP, change this
			po.environment = myKiosk.env;
			po.Assemble (myKiosk.env.envPanelData[i]);
			po.ActivateView (PanelBase.PanelView.Thumbnail, false);

		}
	}
}
