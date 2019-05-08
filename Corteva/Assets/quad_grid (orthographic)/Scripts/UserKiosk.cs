using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures;
using System;

public class UserKiosk : MonoBehaviour {

	public Camera userCam;
	public GameObject nav;
	public Transform userGrid;
	public Transform menu;
	public Transform closer;

	private GameObject bgPanel;
	private GameObject headerPanel;

	private float camRectW;
	private float camRectX;
	private Rect camClosedRect;
	private Rect camOpenedRect;

	private Rect camRect;

	private Vector3 headerInitPos;
	private Vector3 gridFinalPos;
	private Vector3 bgFinalPos;

	public Environment env;
	public int column;
	public Vector2 tapScreenPos;

	private int timesIveBeenTapped;

	public Transform activePanel;
	public bool somePanelIsAnimating = false;

	public bool menuFollowPanel = false;

	public bool dragGrid = false;
	public Vector3 dragDelta;

	private PressGesture pressGesture;

	void Start () {
		
	}

	void OnEnable(){
		pressGesture = GetComponent<PressGesture> ();

		pressGesture.Pressed += pressedHandler;

		timesIveBeenTapped = 0;
		EventsManager.Instance.OnEnvironmentSwitch += environmentSwitchHandler;
	}

	void OnDisable(){
		pressGesture.Pressed -= pressedHandler;

		EventsManager.Instance.OnEnvironmentSwitch -= environmentSwitchHandler;
	}

	void pressedHandler(object sender, EventArgs e){
		Debug.Log ("[pressedHandler] "+name);
		kioskTapped ();
	}
	public void kioskTapped(){
		timesIveBeenTapped++;
		//Debug.Log ("[kioskTappedHandler] " + transform.name + "tapped "+timesIveBeenTapped+" times.");
		if (closer.gameObject.activeSelf) {
			closer.gameObject.SetActive (false);
		}
	}

	private void environmentSwitchHandler(){
		Debug.Log ("!![environmentSwitchHandler] col " + column + " ("+(timesIveBeenTapped>0?"active":"inactive")+")");
		//environemnts are switching, has there been any user activity?
		if (timesIveBeenTapped == 0) {
			//no, is the keep alive timer showing?
			Debug.Log("\t"+(closer.gameObject.activeSelf?"close":"show keep alive"));
			if (!closer.gameObject.activeSelf) {
				//no, show it
				closer.gameObject.SetActive (true);
			} else {
				//yes, close kiosk
				CloseKiosk();
			}
		} else {
			timesIveBeenTapped = 0;
		}
	}



	public void CloseKiosk(){
		Debug.Log ("[CloseKiosk] " + column);
		EventsManager.Instance.UserKioskCloseRequest (new Vector2(column, 0), true);
		Close();
	}

	public void SetCam(float _totalColumns, float _userColumn){
		float userColumn = _userColumn;
		Debug.Log ("[SetCam] at col " + userColumn + " of " + _totalColumns+" | "+tapScreenPos);

		camRectW = 1f / _totalColumns;
		camRectX = camRectW * userColumn;
		float camRectXm = camRectX + (camRectW * 0.5f);
		camClosedRect = new Rect (camRectXm, 0, 0, 1);
		camOpenedRect = new Rect (camRectX, 0, camRectW, 1);
		userCam.rect = camClosedRect;

		Init ();
		Open ();
	}

	void Init(){
		bgPanel = Instantiate (AssetManager.Instance.NEWpanelPrefab, transform);
		bgPanel.name = "bgPanel";
		bgPanel.transform.localPosition = new Vector3 (0, 0, 50);
		bgPanel.transform.localScale = new Vector3 (3, 3, 1);
		bgPanel.GetComponent<PanelBase> ().AssembleBasic ("kiosk_bg", env.kioskBg);
		bgPanel.GetComponent<PanelBase> ().panelContext = PanelBase.PanelContext.Kiosk;
		bgPanel.GetComponent<PanelBase> ().panelView = PanelBase.PanelView.Background;
		bgPanel.GetComponent<PanelBase> ().myKiosk = transform.GetComponent<UserKiosk> ();
//		bgPanel.GetComponent<PanelObject> ().SetAsImage ();
//		bgPanel.GetComponent<PanelObject> ().panelContext = PanelObject.PanelContext.Kiosk;
//		bgPanel.GetComponent<PanelObject> ().panelMode = PanelObject.PanelView.Background;
		bgFinalPos = new Vector3(1f, 0, 60);
		bgPanel.SetActive (false);


		headerPanel = Instantiate (AssetManager.Instance.panelPrefab, transform);
		headerPanel.name = "headerPanel";
		headerPanel.transform.localPosition = new Vector3 (0, 6, 20);
		if (env == null)
			env = AssetManager.Instance.environments [UnityEngine.Random.Range (0, AssetManager.Instance.environments.Count)];
		headerPanel.GetComponent<PanelObject> ().SetPanelColors (env.envColor);
		headerPanel.GetComponent<PanelObject> ().SetAsTitle (env.envTitle);
		headerPanel.GetComponent<PanelObject> ().panelContext = PanelObject.PanelContext.Kiosk;
		headerPanel.GetComponent<PanelObject> ().panelMode = PanelObject.PanelView.Background;
		headerInitPos = headerPanel.transform.localPosition + (Vector3.down * 3);
		headerPanel.SetActive (false);

		userGrid.gameObject.SetActive (false);
	}

	public void Open(){
		bgPanel.SetActive (true);
		EaseCurve.Instance.CamRect (userCam, camClosedRect, camOpenedRect, 0.5f, EaseCurve.Instance.easeIn, Next);
	}

	void Next(){
		nav.SetActive (true);
		headerPanel.SetActive (true);
		userGrid.gameObject.SetActive (true);

		Vector3 tapOffset = userCam.ScreenToViewportPoint (tapScreenPos);
		Vector3 menuFinalPos = Vector3.zero + Vector3.forward * 30f;
		if (tapOffset.y < 0.5f) {
			menuFinalPos.y += (0.5f - tapOffset.y) * -10f;
		}
		if (menuFinalPos.y < -3) {
			menuFinalPos.y = -3f;
		}

		if (activePanel) {
			Vector3 panelInitPos = activePanel.transform.localPosition;
			//Debug.Log ("++++++++ " + panelInitPos + " " + tapOffset.y);
			if (tapOffset.y < 0.5f) {
				panelInitPos.y = (0.5f - tapOffset.y) * -10f;
				//Debug.Log ("\t++++++++ " + panelInitPos);
			}
			if (panelInitPos.y < -3f){
				panelInitPos.y = -3f;
			}
			EaseCurve.Instance.Vec3 (activePanel.transform, activePanel.transform.localPosition, panelInitPos, 1f, 0f, EaseCurve.Instance.easeInOut, null, "local");
		}

		EaseCurve.Instance.Vec3 (headerPanel.transform, headerPanel.transform.localPosition, headerInitPos, 1f, 0f, EaseCurve.Instance.easeInOut, null, "local");
		EaseCurve.Instance.Vec3 (menu.transform, menu.transform.localPosition, menuFinalPos, 1f, 0f, EaseCurve.Instance.easeOut, Next2, "local");
	}

	void Next2(){
		gridFinalPos = userGrid.transform.localPosition + Vector3.left * 1.25f;
		EaseCurve.Instance.Vec3 (userGrid.transform, userGrid.transform.localPosition, gridFinalPos, 1f, 0f, EaseCurve.Instance.easeIn, null, "local");
		EaseCurve.Instance.Vec3 (bgPanel.transform, bgPanel.transform.localPosition, bgFinalPos, 1f, 0f, EaseCurve.Instance.easeIn, null, "local");
	}

	public void Close(){
		EaseCurve.Instance.CamRect (userCam, camOpenedRect, camClosedRect, 0.5f, EaseCurve.Instance.easeIn, CleanUp);
	}

	private void CleanUp(){
		Destroy (gameObject);
	}
		
	void Update () {

		if (menuFollowPanel) {
			Vector3 menuGoTo = activePanel.localPosition;
			menuGoTo.z = menu.localPosition.z;
			menuGoTo.x = menu.localPosition.x;
			menu.localPosition = Vector3.Lerp (menu.localPosition, menuGoTo, 4f * Time.deltaTime);
		}

		if (dragGrid) {
			Vector3 gridGoTo = userGrid.localPosition;
			gridGoTo.x += dragDelta.x;
			userGrid.localPosition = gridGoTo;

			Vector3 menuGoTo = menu.localPosition;
			menuGoTo.y += dragDelta.y;
			menu.localPosition = menuGoTo;

			Vector3 bgGoTo = bgPanel.transform.localPosition;
			bgGoTo.x -= dragDelta.x * 0.25f;
			bgPanel.transform.localPosition = bgGoTo;
		}
	}
}
