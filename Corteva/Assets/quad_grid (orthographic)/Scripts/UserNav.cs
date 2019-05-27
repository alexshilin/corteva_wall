using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures;
using UnityEngine.UI;

public class UserNav : MonoBehaviour {

	public UserKiosk myKiosk;
	public int envID;
	public bool hasRing = false;
	private Image ring;
	private float currPos;
	public float goPos = 0f;

	private TapGesture tapGesture;
	private 

	// Use this for initialization
	void Start () {
		if (hasRing) {
			ring = transform.Find ("ring").GetComponent<Image> ();
			ring.fillAmount = 0;
		}
	}

	void OnEnable(){
		tapGesture = GetComponent<TapGesture> ();

		tapGesture.Tapped += tapHandler;

		if(hasRing)
			EventsManager.Instance.OnEnvironmentSwitch += envSwitchHandler;
	}

	void OnDisable(){
		tapGesture.Tapped -= tapHandler;

		if(hasRing)
			EventsManager.Instance.OnEnvironmentSwitch -= envSwitchHandler;
	}
	
	// Update is called once per frame
	void Update () {
		if (hasRing) {
			ring.fillAmount = Mathf.Lerp (ring.fillAmount, goPos, Time.deltaTime * 4f);
		}
	}

	void envSwitchHandler(int _env){
		if (_env != envID) {
			ring.fillClockwise = false;
			goPos = 0f;
		}
	}

	private void tapHandler(object sender, EventArgs e){
		if (!myKiosk.somePanelIsAnimating) {
			if (envID == -1) {
				myKiosk.StartPinDrop ();
			} else {
				ring.fillClockwise = true;
				goPos = 1f;
				EventsManager.Instance.EnvironmentSwitchRequest (envID);
				myKiosk.SwitchEnvironment (envID);
			}
		}
	}
}
