using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PinDrop : MonoBehaviour {

	public float initGlobeSize = 1f;
	public float maxGlobeSize = 2f;
	public float initWindowSize = 3f;
	public PinDropMenu menu;
	public PinDropEarth globe;
	public Camera globeCam;
	public UserKiosk myKiosk;
	private Vector3 baseRotation;

	void Start(){
		if(SceneManager.GetActiveScene().name == "pindrop"){
			Init ();	
		}
	}

	public void Init(){
		if (myKiosk != null) {
			menu.backBtn.SetActive (true);
			Vector3 backGoTo = menu.backBtn.transform.localPosition;
			backGoTo.x = -2.5f;
			EaseCurve.Instance.Vec3 (menu.backBtn.transform, menu.backBtn.transform.localPosition, backGoTo, 1f, 3f, EaseCurve.Instance.easeOut, null, "local");
		}

		menu.transform.localScale = Vector3.one * initWindowSize;
		menu.ToggleWelcome (1, 2);

		globe.transform.localScale = Vector3.one * initGlobeSize * 0.5f;
		globe.cam = globeCam;
		EaseCurve.Instance.Scl(globe.transform, globe.transform.localScale, Vector3.one * initGlobeSize, 3f, 0f, EaseCurve.Instance.easeOut, ShowInstructions);
	}

	void ShowInstructions(){
		menu.instruct.gameObject.SetActive (true);
		menu.icons.SetActive (true);
	}

	public void Off(){
		globe.pinContainer.transform.localPosition += Vector3.up * 100f;
	}
}
