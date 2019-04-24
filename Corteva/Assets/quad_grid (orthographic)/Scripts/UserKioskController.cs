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
		EventsManager.Instance.OnUserKioskRequest += tryOpenKiosk;
	}
	void OnDisable(){
		EventsManager.Instance.OnUserKioskRequest -= tryOpenKiosk;
	}

	/*

		kiosk auto close
		every user touch in the kiosk resets kiosk idle timer
		[fire event] 5 seconds before main idle loop expects to switch 
		if any kiosks have an idle timer longer than X seconds it starts an "are you there?" countdown
		if countdown isnt stopped, kiosk closes with next main idle loop changeover


	*/

	private void tryOpenKiosk(Vector2 _gridPos, bool _doOpen){
		if (_doOpen) {
			Vector2 gridPos = _gridPos;
			GameObject uK = Instantiate (AssetManager.Instance.userKioskPrefab);
			uK.name = "UserKiosk_" + _gridPos.x;
			uK.transform.parent = AssetManager.Instance.kiosks;
			uK.transform.localPosition = Vector3.zero + Vector3.right * _gridPos.x * 20f;
			uK.GetComponent<UserManager> ().SetCam (GridManagerOrtho.Instance.desiredGrid.x, _gridPos.x);
//		if (_gridPos.x > 2)
//			_gridPos.x -= 3;
			UserKioskObject uKo = new UserKioskObject ();
			uKo.col = (int)_gridPos.x;
			uKo.kioskGO = uK;
			kiosks.Add (uKo);
		}
	}

	private void tryCloseKiosk(int _col){
		UserKioskObject uKo = kiosks.Find (x => x.col == _col);
		if(uKo!=null){
			EventsManager.Instance.UserKioskRequest (new Vector2(_col, 0), false);
			Destroy (uKo.kioskGO);
			kiosks.Remove (uKo);
		}
	}

	void Update(){
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			tryCloseKiosk (0);
		}
		if (Input.GetKeyDown (KeyCode.Alpha2)) {
			tryCloseKiosk (1);
		}
		if (Input.GetKeyDown (KeyCode.Alpha3)) {
			tryCloseKiosk (2);
		}
		if (Input.GetKeyDown (KeyCode.Alpha4)) {
			tryCloseKiosk (3);
		}
		if (Input.GetKeyDown (KeyCode.Alpha5)) {
			tryCloseKiosk (4);
		}
		if (Input.GetKeyDown (KeyCode.Alpha6)) {
			tryCloseKiosk (5);
		}
	}
}
