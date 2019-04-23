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

	// Use this for initialization
	void Start () {
		for (int i = 1; i <= panels; i++) {
			GameObject panel = Instantiate (panelPrefab, transform);
			panel.transform.localPosition = new Vector3 ((currColumn * 5.333333f) + (currColumn * panelSpacing), (currRow * 3) + (currRow * panelSpacing), 0);
//			if(ScreenManager.Instance!=null)
//				ScreenManager.Instance.MoveToLayer (panel.transform, LayerMask.NameToLayer ("User1"));
			currPanelsInColumn++;
			currRow++;
			if (currPanelsInColumn == maxPanelsPerColumn) {
				currPanelsInColumn = 0;
				currRow = 1;
				currColumn++;
			}
			panel.SetActive (true);
			PanelObject po = panel.GetComponent<PanelObject> ();
			int r = Random.Range (0, 3);
			if (r == 1) {
				po.SetAs3dViz ();
			} else if (r == 2) {
				po.SetAsVideo (false, false);
				//panel.GetComponent<PanelObject> ().SetAsImage ();
			} else {
				po.SetAsImage ();
			}
			po.panelState = "Thumbnail";

		}
		//this.gameObject.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
