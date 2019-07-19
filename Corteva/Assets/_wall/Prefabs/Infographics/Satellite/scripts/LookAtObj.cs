using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtObj : MonoBehaviour {

	public SatelliteController SC;

	// Use this for initialization
	void Start () {
		transform.LookAt(SC.globe.transform);
        //transform.localEulerAngles = new Vector3(transform.localEulerAngles.x + 60, transform.localEulerAngles.y -120, transform.localEulerAngles.z - 140);
    }
	
	// Update is called once per frame
	void Update () {

       
	}
}
