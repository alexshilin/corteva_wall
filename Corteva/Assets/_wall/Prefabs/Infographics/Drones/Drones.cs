using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drones : MonoBehaviour {

	public List<DroneBtn> btns = new List<DroneBtn>();
	public List<DroneView> views = new List<DroneView>();

	public void ToggleButtons(int _id){
		foreach(DroneBtn b in btns){
			if (b.id == _id) {
				b.Toggle (true);
			} else {
				b.Toggle (false);
			}
		}
		for(int i=0; i<views.Count; i++){
			if (i == _id) {
				views[i].transform.gameObject.SetActive (true);
			} else {
				views[i].transform.gameObject.SetActive (false);
			}
		}
	}
}
