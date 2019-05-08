using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
		for (int i = 1; i <= panels; i++) {
			Vector3 panelPostion = new Vector3 ((currColumn * 5.333333f) + (currColumn * panelSpacing), (currRow * 3) + (currRow * panelSpacing), 0);

			currPanelsInColumn++;
			currRow++;
			if (currPanelsInColumn == maxPanelsPerColumn) {
				currPanelsInColumn = 0;
				currRow = 1;
				currColumn++;
			}

			//if (transform.GetComponentInParent<UserKiosk>().activePanel != null && i == 2) {
			if (i == 2) {
				emptySpot = panelPostion;
				continue;
			}

			GameObject panel = Instantiate (AssetManager.Instance.NEWpanelPrefab, transform);
			panel.transform.localPosition = panelPostion;
			panel.name = "p"+i;

			PanelBase po = panel.GetComponent<PanelBase> ();

			string temp = UnityEngine.Random.Range (0, 2) == 0 ? "template_01" : "template_02";
			po.AssemblePanel (temp);
			/*
			int r = Random.Range (0, 3);
			if (r == 1) {
				//po.SetAs3dViz ();
				po.SetAsImage ();
			} else if (r == 2) {
				po.SetAsVideo (false, false);
				//panel.GetComponent<PanelObject> ().SetAsImage ();
			} else {
				po.SetAsImage ();
			}
			po.SetAsThumbnail ();
			*/
			po.panelView = PanelBase.PanelView.Thumbnail;
			po.panelContext = PanelBase.PanelContext.Kiosk;
			po.panelState = PanelBase.PanelState.Ready;
			po.myKiosk = transform.GetComponentInParent<UserKiosk>(); //TMP, change this
			po.ActivateView (PanelBase.PanelView.Thumbnail, false);
			//po.ActivateView (PanelBase.PanelView.Front, true);
			//panel.transform.rotation *= Quaternion.AngleAxis (360, transform.up);
			//panel.SetActive (true);

		}
	}
}
