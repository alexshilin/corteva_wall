using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchContentItems : MonoBehaviour {

	public List<GameObject> items = new List<GameObject>();
	private int showing = 0;
	private Vector3 showPos = Vector3.zero;
	private Vector3 hidePos = new Vector3 (0, -10, 0);

	// Use this for initialization
	void Start () {
		SetItem (showing);
	}

	void SetItem(int _item){
		for (int i = 0; i < items.Count; i++) {
			if (i == _item) {
				items [i].transform.position = showPos;
			} else {
				items [i].transform.position = hidePos;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			showing++;
			if (showing == items.Count)
				showing = 0;
			SetItem (showing);
		}
	}
}
