using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserKioskController : MonoBehaviour {

	[System.Serializable]
	public class UserKioskObject{
		public int col;
		public GameObject kioskGO;
	}

	List<UserKioskObject> kiosks = new List<UserKioskObject>();



	void Start(){
		EventsManager.Instance.OnUserKioskOpenRequest += tryOpenKiosk;
		EventsManager.Instance.OnUserKioskCloseRequest += tryCloseKiosk;
	}
	void OnDisable(){
		EventsManager.Instance.OnUserKioskOpenRequest -= tryOpenKiosk;
		EventsManager.Instance.OnUserKioskCloseRequest -= tryCloseKiosk;
	}

	/*

		kiosk auto close
		every user touch in the kiosk resets kiosk idle timer
		[fire event] 5 seconds before main idle loop expects to switch 
		if any kiosks have an idle timer longer than X seconds it starts an "are you there?" countdown
		if countdown isnt stopped, kiosk closes with next main idle loop changeover


	*/

	private void tryOpenKiosk(Vector2 _gridPos, Vector2 _screenPos, Environment _env, Transform _panel){
		Debug.Log ("!![tryOpenKiosk] at col "+_gridPos.x);
		Vector2 gridPos = _gridPos;
		GameObject uK = Instantiate (AssetManager.Instance.userKioskPrefab);
		uK.name = "UserKiosk_" + _gridPos.x;
		uK.transform.parent = AssetManager.Instance.kiosks;
		uK.transform.localPosition = Vector3.zero + Vector3.right * _gridPos.x * 20f;
		if (_env == null) {
			_env = AssetManager.Instance.environments [IdleStateController.Instance.currEnv];
		}
		UserKiosk uKkiosk = uK.GetComponent<UserKiosk> ();
		uKkiosk.env = _env;
		uKkiosk.column = (int)_gridPos.x;
		uKkiosk.tapScreenPos = _screenPos;
		if (_panel != null) {
			uKkiosk.activePanel = _panel;
		}
		uKkiosk.userGrid.GetComponent<UserGrid> ().MakeGrid();
		uKkiosk.SetCam (GridManagerOrtho.Instance.desiredGrid.x, _gridPos.x);
//		if (_gridPos.x > 2)
//			_gridPos.x -= 3;
		UserKioskObject uKo = new UserKioskObject ();
		uKo.col = (int)_gridPos.x;
		uKo.kioskGO = uK;
		kiosks.Add (uKo);
	}


	//TEMP for manual testing
	private void KioskWantsToClose(int _col){
		Debug.Log ("[KioskWantsToClose] " + _col);
		//EventsManager.Instance.UserKioskCloseRequest (new Vector2(_col, 0), false);
		UserKioskObject uKo = kiosks.Find (x => x.col == _col);
		if (uKo != null) {
			//Destroy (uKo.kioskGO);
			//kiosks.Remove (uKo);
			kiosks.Remove (uKo);
			uKo.kioskGO.GetComponent<UserKiosk> ().CloseKiosk ();
		}
	}

	private void tryCloseKiosk(Vector2 _gridPos, bool _now){
		if (_now) {
			Debug.Log ("!![tryCloseKiosk] " + _gridPos.x);
			UserKioskObject uKo = kiosks.Find (x => x.col == (int)_gridPos.x);
			if (uKo != null) {
				//Destroy (uKo.kioskGO);
				kiosks.Remove (uKo);
				//uKo.kioskGO.GetComponent<UserKiosk> ().CloseKiosk ();
			}
		} else {
			//wait
		}
	}

	void Update(){
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			KioskWantsToClose (0);
		}
		if (Input.GetKeyDown (KeyCode.Alpha2)) {
			KioskWantsToClose (1);
		}
		if (Input.GetKeyDown (KeyCode.Alpha3)) {
			KioskWantsToClose (2);
		}
		if (Input.GetKeyDown (KeyCode.Alpha4)) {
			KioskWantsToClose (3);
		}
		if (Input.GetKeyDown (KeyCode.Alpha5)) {
			KioskWantsToClose (4);
		}
		if (Input.GetKeyDown (KeyCode.Alpha6)) {
			KioskWantsToClose (5);
		}
	}
}
