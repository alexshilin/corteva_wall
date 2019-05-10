using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class UserGrid : MonoBehaviour {

	public GameObject panelPrefab;
	private int panels = 6;
	private int maxPanelsPerColumn = 3;
	private float totalHeight = 3;
	private float panelSpacing = 0.1f;

	private int currColumn = 1;
	private int currRow = 1;
	private int currPanelsInColumn = 0;

	public Vector3 emptySpot;

	void Start () {
		MakeGrid ();
	}

	public void MakeGrid(){
		transform.localPosition += Vector3.down * 1.85f;
		for (int i = 1; i <= panels; i++) {
			Vector3 panelPostion = new Vector3 ((currColumn * 5.333333f) + (currColumn * panelSpacing), (currRow * 3) + (currRow * panelSpacing), 0);

			currPanelsInColumn++;
			currRow++;
			if (currPanelsInColumn == maxPanelsPerColumn) {
				currPanelsInColumn = 0;
				currRow = 1;
				currColumn++;
			}

			if (i == 3) {
				if (transform.GetComponentInParent<UserKiosk> ().activePanel != null) {
					emptySpot = panelPostion;
					continue;
				}
			}

			GameObject panel = Instantiate (AssetManager.Instance.NEWpanelPrefab, transform);
			panel.transform.localPosition = panelPostion;
			panel.name = "p"+i;

			PanelBase po = panel.GetComponent<PanelBase> ();

			po.panelContext = PanelBase.PanelContext.Kiosk;
			po.panelState = PanelBase.PanelState.Ready;
			po.myKiosk = transform.GetComponentInParent<UserKiosk>(); //TMP, change this
			po.environment = po.myKiosk.env;
			JSONNode panelData = po.myKiosk.env.envPanelData[Random.Range(0, po.myKiosk.env.envPanelData.Count)];
			po.Assemble (panelData);
			po.ActivateView (PanelBase.PanelView.Thumbnail, false);

		}
	}
}
