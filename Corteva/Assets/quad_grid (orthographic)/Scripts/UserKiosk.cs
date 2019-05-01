using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	private bool userOpened = false;
	private bool openCamera = false;
	private bool cameraOpened = false;
	private bool menuOpened = false;
	private Rect camRect;

	private Vector3 headerInitPos;
	private Vector3 navInitPos;
	private Vector3 gridInitPos;
	private Vector3 gridFinalPos;
	private Vector3 bgFinalPos;

	public Environment env;
	public int column;

	private int timesIveBeenTapped;

	public Transform activePanel;
	public bool menuFollowPanel = false;
	public bool panelFollowMenu = false;

	// Use this for initialization
	void Start () {
		
	}

	void OnEnable(){
		timesIveBeenTapped = 0;
		EventsManager.Instance.OnEnvironmentSwitch += environmentSwitchHandler;
	}
	void OnDisable(){
		EventsManager.Instance.OnEnvironmentSwitch -= environmentSwitchHandler;
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
			//EventsManager.Instance.UserKioskCloseRequest (new Vector2(column, 0), false);
		} else {
			timesIveBeenTapped = 0;
		}
	}

	private void kioskTapped(){
		timesIveBeenTapped++;
		Debug.Log ("[kioskTappedHandler] " + transform.name + "tapped "+timesIveBeenTapped+" times.");
		if (closer.gameObject.activeSelf) {
			closer.gameObject.SetActive (false);
		}
	}

	public void CloseKiosk(){
		Debug.Log ("[CloseKiosk] " + column);
		EventsManager.Instance.UserKioskCloseRequest (new Vector2(column, 0), true);
		Destroy (gameObject);
	}

	public void SetCam(float _totalColumns, float _userColumn){
		float userColumn = _userColumn;
		Debug.Log ("[SetCam] at col " + userColumn + " of " + _totalColumns);

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
		bgPanel = Instantiate (AssetManager.Instance.panelPrefab, transform);
		bgPanel.name = "bgPanel";
		bgPanel.transform.localPosition = new Vector3 (0, 0, 50);
		bgPanel.transform.localScale = new Vector3 (3, 3, 1);
		bgPanel.GetComponent<PanelObject> ().SetAsImage ();
		bgPanel.GetComponent<PanelObject> ().panelContext = PanelObject.PanelContext.Kiosk;
		bgPanel.GetComponent<PanelObject> ().panelMode = PanelObject.PanelView.Background;
		bgFinalPos = new Vector3(1f, 0, 60);
		bgPanel.SetActive (false);


		headerPanel = Instantiate (AssetManager.Instance.panelPrefab, transform);
		headerPanel.name = "headerPanel";
		headerPanel.transform.localPosition = new Vector3 (0, 6, 20);
		if (env == null)
			env = AssetManager.Instance.environments [Random.Range (0, AssetManager.Instance.environments.Count)];
		headerPanel.GetComponent<PanelObject> ().SetPanelColors (env.envColor);
		headerPanel.GetComponent<PanelObject> ().SetAsTitle (env.envTitle);
		headerPanel.GetComponent<PanelObject> ().panelContext = PanelObject.PanelContext.Kiosk;
		headerPanel.GetComponent<PanelObject> ().panelMode = PanelObject.PanelView.Background;
		headerInitPos = headerPanel.transform.localPosition + (Vector3.down * 3);
		headerPanel.SetActive (false);


		navInitPos = nav.transform.localPosition + (Vector3.up * 6);

		gridInitPos = new Vector3(1.7f, -1.9f, 0);
		gridFinalPos = new Vector3(-0.3f, -1.9f, 0);
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
		//bgPanel.SetActive (true);
		EaseCurve.Instance.Vec3 (headerPanel.transform, headerPanel.transform.localPosition, headerInitPos, 1f, 0f, EaseCurve.Instance.easeInOut, null, "local");
		EaseCurve.Instance.Vec3 (nav.transform, nav.transform.localPosition, navInitPos, 1f, 0f, EaseCurve.Instance.easeOut, null, "local");
		EaseCurve.Instance.Vec3 (userGrid.transform, userGrid.transform.localPosition, gridInitPos, 1f, 0f, EaseCurve.Instance.easeOut, Next2, "local");
	}

	void Next2(){
		EaseCurve.Instance.Vec3 (userGrid.transform, userGrid.transform.localPosition, gridFinalPos, 1f, 0f, EaseCurve.Instance.easeIn, null, "local");
		EaseCurve.Instance.Vec3 (bgPanel.transform, bgPanel.transform.localPosition, bgFinalPos, 1f, 0f, EaseCurve.Instance.easeIn, null, "local");
		//PixelsToUnits ();
	}

	RaycastHit hit;
	private bool navDoFollow = false;
	private Vector3 navTouchStart;
	private Vector3 menuObjectStart;
	private float distanceMovedY = 0f;
	private float distanceMovedX = 0f;
	//[HideInInspector]
	//public PanelObject activePanel;
	private Vector3 activePanelStart;

	private Vector3 panelTouchStart;
	private bool panelDoFollow = false;
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			if (userCam.pixelRect.Contains (Input.mousePosition)) {
				kioskTapped ();
			}
			//make sure camera has a valied viewport (its collapsed on start)
			if (userCam.rect.width > 0 && userCam.rect.height > 0) {
				if (Physics.Raycast (userCam.ScreenPointToRay (Input.mousePosition), out hit)) {
					if (hit.transform.name == "Nav") {
						//nav follow mouse
						//setNavFollow (Input.mousePosition);
					}
				}
			}
		}
		if (menuFollowPanel) {
			Vector3 menuGoTo = activePanel.localPosition;
			menuGoTo.z = menu.localPosition.z;
			menuGoTo.x = menu.localPosition.x;
			menu.localPosition = Vector3.Lerp (menu.localPosition, menuGoTo, 2f * Time.deltaTime);
		}



		if (Input.GetMouseButtonUp (0)) {
			navDoFollow = false;
			panelDoFollow = false;
		}

		if (navDoFollow) {
			float distanceMovedPx = navTouchStart.y - Input.mousePosition.y;
			distanceMovedY = distanceMovedPx / WorldToPixelAmount.y;
			Vector3 moveMenuTo = menuObjectStart + Vector3.up * -distanceMovedY;
			menu.localPosition = Vector3.Lerp (menu.localPosition, moveMenuTo, 4f * Time.deltaTime);
			if (activePanel != null) {
				Vector3 activePanelMoveTo = activePanelStart + Vector3.up * -distanceMovedY;
				activePanel.transform.localPosition = Vector3.Lerp (activePanel.transform.localPosition, activePanelMoveTo, 2f * Time.deltaTime);
			}
		}

		if (panelDoFollow) {
			float distanceMovedPxY = panelTouchStart.y - Input.mousePosition.y;
			float distanceMovedPxX = panelTouchStart.x - Input.mousePosition.x;
			distanceMovedY = distanceMovedPxY / WorldToPixelAmount.y;
			distanceMovedX = distanceMovedPxX / WorldToPixelAmount.x;
			Vector3 moveMenuTo = menuObjectStart + Vector3.up * -distanceMovedY;
			menu.localPosition = Vector3.Lerp (menu.localPosition, moveMenuTo, 2f * Time.deltaTime);
			Vector3 activePanelMoveTo = activePanelStart + (Vector3.up * -distanceMovedY) + (Vector3.right * -distanceMovedX);
			activePanel.transform.localPosition = Vector3.Lerp (activePanel.transform.localPosition, activePanelMoveTo, 4f * Time.deltaTime);
		}
	}

	public void setNavFollow(Vector3 _mousePos){
		distanceMovedY = 0f;
		distanceMovedX = 0f;
		navTouchStart = _mousePos;
		menuObjectStart = menu.localPosition;
		if (activePanel != null) {
			activePanelStart = activePanel.transform.localPosition;
		}
		navDoFollow = true;
		Debug.Log("following nav");
	}
	public void setPanelFollow(Vector3 _mousePos){
		distanceMovedY = 0f;
		distanceMovedX = 0f;
		panelTouchStart = _mousePos;
		menuObjectStart = menu.localPosition;
		if (activePanel != null) {
			activePanelStart = activePanel.transform.localPosition;
		}
		panelDoFollow = true;
		Debug.Log("following panel");
	}



	private void openKiosk(int _col){
		SetCam (6, _col);
		Open ();
	}



	Vector2 WorldUnitsInCamera;
	Vector2 WorldToPixelAmount;
	void PixelsToUnits(){
		WorldUnitsInCamera.y = userCam.orthographicSize * 2;
		WorldUnitsInCamera.x = WorldUnitsInCamera.y * Screen.width / Screen.height;

		WorldToPixelAmount.x = Screen.width / WorldUnitsInCamera.x;
		WorldToPixelAmount.y = Screen.height / WorldUnitsInCamera.y;
		Debug.Log (WorldUnitsInCamera + ", " + WorldToPixelAmount);
	}

//	public Vector2 ConvertToWorldUnits(Vector2 TouchLocation)
//	{
//		Vector2 returnVec2;
//
//		returnVec2.x = ((TouchLocation.x / WorldToPixelAmount.x) - (WorldUnitsInCamera.x / 2)) +
//			Camera.transform.position.x;
//		returnVec2.y = ((TouchLocation.y / WorldToPixelAmount.y) - (WorldUnitsInCamera.y / 2)) +
//			Camera.transform.position.y;
//
//		return returnVec2;
//	}
}
