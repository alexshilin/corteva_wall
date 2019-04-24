using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using TouchScript;

public class UserManager : MonoBehaviour {

	public Camera userCam;
	public GameObject panelPrefab;
	public GameObject navPrefab;
	public Transform userGrid;
	public Transform menu;

	public Texture2D bgTexture;
	private GameObject bgPanel;
	private GameObject headerPanel;

	 float totalColumns = 6;
	 float userColumn = 1;

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


//	void OnEnable(){
//		EventsManager.Instance.OnUserKioskRequest += openKiosk;
//	}
//	void OnDisable(){
//		EventsManager.Instance.OnUserKioskRequest -= openKiosk;
//	}

	// Use this for initialization
	void Start () {

		/*
		SetCam (totalColumns, userColumn);

		headerInitPos = panelPrefab.transform.localPosition + (Vector3.down * 3);
		navInitPos = navPrefab.transform.localPosition + (Vector3.up * 6);
		//gridInitPos = (userGrid.transform.localPosition + (Vector3.up * 6)) + (userGrid.transform.localPosition + (Vector3.left * 1.66f));
		gridInitPos = new Vector3(1.7f, -1.9f, 50);
		gridFinalPos = new Vector3(-0.3f, -1.9f, 50);
		bgFinalPos = new Vector3(1f, 0, 60);
		navPrefab.SetActive (false);
		userGrid.gameObject.SetActive (false);


		bgPanel = Instantiate (panelPrefab, transform);
		bgPanel.name = "bgPanel";
		//bgPanel.transform.localPosition = new Vector3 (-12, 0, 60);
		bgPanel.transform.localPosition = new Vector3 (0, 0, 60);
		bgPanel.transform.localScale = new Vector3 (3, 3, 1);
		bgPanel.GetComponent<PanelObject> ().SetAsImage ();
		//Renderer bgPanelRenderer = bgPanel.transform.Find("Panel").Find("Front").GetComponent<Renderer> ();
		//bgPanelRenderer.material.mainTexture = bgTexture;
		//bgPanelRenderer.material.color = new Color32 (70, 70, 70, 0);
		bgPanel.GetComponent<PanelObject> ().SetAsNonInteractive ();
		if(ScreenManager.Instance!=null)
			ScreenManager.Instance.MoveToLayer (bgPanel.transform, LayerMask.NameToLayer ("User1"));
		bgPanel.SetActive (false);

		headerPanel = Instantiate (panelPrefab, transform);
		headerPanel.name = "headerPanel";
		headerPanel.transform.localPosition = new Vector3 (0, 6, 50);
		//Renderer headerPanelRenderer = headerPanel.transform.Find ("Panel").Find ("Front").GetComponent<Renderer> ();
		//headerPanelRenderer.material.color = Color.blue;
		headerPanel.GetComponent<PanelObject> ().envHeadlineText.text = "Globe";
		headerPanel.GetComponent<PanelObject> ().envSummaryText.text = "Today, food is gown and traded globally.<br>How does food flow globally from farms to your plate?";
		headerPanel.GetComponent<PanelObject>().SetAsNonInteractive ();
		if(ScreenManager.Instance!=null)
			ScreenManager.Instance.MoveToLayer (headerPanel.transform, LayerMask.NameToLayer ("User1"));
		headerPanel.SetActive (false);
		*/

//		Transform cube = GameObject.Find ("TestCube").transform;
//		EaseCurve.Instance.Vec3(cube, cube.transform.position, cube.transform.position + Vector3.right * 3, 2f);
	}

	void OnEnable(){
		
	}

	void Done(){
		Debug.Log ("done");
	}

	public void SetCam(float _totalColumns, float _userColumn){
		float userColumn = _userColumn;
		Debug.Log ("Placing user camera at col " + userColumn + " of " + _totalColumns);
//		if (userColumn > 2) {
//			userColumn -= 3;
//			userCam.targetDisplay = 1;
//			Debug.Log ("\ton display 2");
//		}
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
		bgPanel.transform.localPosition = new Vector3 (0, 0, 60);
		bgPanel.transform.localScale = new Vector3 (3, 3, 1);
		bgPanel.GetComponent<PanelObject> ().SetAsImage ();
		bgPanel.GetComponent<PanelObject> ().SetAsNonInteractive ();
		bgFinalPos = new Vector3(1f, 0, 60);
		bgPanel.SetActive (false);


		headerPanel = Instantiate (AssetManager.Instance.panelPrefab, transform);
		headerPanel.name = "headerPanel";
		headerPanel.transform.localPosition = new Vector3 (0, 6, 50);
		headerPanel.GetComponent<PanelObject> ().SetPanelColors (new Color32 (0, 114, 206, 255));
		headerPanel.GetComponent<PanelObject> ().SetAsTitle ("Title");
		headerInitPos = headerPanel.transform.localPosition + (Vector3.down * 3);
		headerPanel.SetActive (false);


		navInitPos = navPrefab.transform.localPosition + (Vector3.up * 6);

		gridInitPos = new Vector3(1.7f, -1.9f, 50);
		gridFinalPos = new Vector3(-0.3f, -1.9f, 50);
		userGrid.gameObject.SetActive (false);
	}

	public void Open(){
		bgPanel.SetActive (true);
		EaseCurve.Instance.CamRect (userCam, camClosedRect, camOpenedRect, 0.5f, EaseCurve.Instance.easeIn, Next);
	}

	void Next(){
		navPrefab.SetActive (true);
		headerPanel.SetActive (true);
		userGrid.gameObject.SetActive (true);
		//bgPanel.SetActive (true);
		EaseCurve.Instance.Vec3 (headerPanel.transform, headerPanel.transform.localPosition, headerInitPos, 1f, 0f, EaseCurve.Instance.easeInOut, null, "local");
		EaseCurve.Instance.Vec3 (navPrefab.transform, navPrefab.transform.localPosition, navInitPos, 1f, 0f, EaseCurve.Instance.easeOut, null, "local");
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
	[HideInInspector]
	public PanelObject activePanel;
	private Vector3 activePanelStart;

	private Vector3 panelTouchStart;
	private bool panelDoFollow = false;
	// Update is called once per frame
	void Update () {
//		if (Input.GetKeyDown (KeyCode.Space)) {
//			Open ();
//		}

		if (Input.GetMouseButtonDown (0)) {
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
