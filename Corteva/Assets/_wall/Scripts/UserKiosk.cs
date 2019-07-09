using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;
using System;
using TMPro;

public class UserKiosk : MonoBehaviour {

	public Camera userCam;
	public GameObject nav;
	public Transform navBG;
	public Renderer navBGrend;
	public Transform userGrid;
	public Transform menu;
	public Transform closer;
	public TextMeshPro countdown;
	public Renderer tint;
	public GameObject pinDropPrefab;
	private GameObject pinDrop;

	private GameObject bgPanel;
	private GameObject headerPanel;
	private GameObject bgPanelOld;
	private GameObject headerPanelOld;
	private Material navMat;
	private Color prevNavBGColor;

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

	public Transform activePanel;
	public bool somePanelIsAnimating = false;

	public bool menuFollowPanel = false;

	public bool dragNav = false;
	public bool dragGrid = false;
	public Vector3 dragDelta;
	public float gridWidth = 0;

	private PressGesture pressGesture;
	private TransformGesture transformGesture;

	public float timeSinceLastTouch = 0f;
	private float maxWaitUntouched = 20f;
	private bool waitingForSave = false;
	private float timeTillClose = 0f;
	private float maxTimeTillClose = 5f;
	private bool closing = false;

	void Start () {
		
	}

	void OnEnable(){
		pressGesture = GetComponent<PressGesture> ();
		transformGesture = GetComponent<TransformGesture> ();
		pressGesture.Pressed += pressedHandler;
		transformGesture.Transformed += transformHandler;
		transformGesture.TransformCompleted += transformCompleteHandler;
	}

	void OnDisable(){
		pressGesture.Pressed -= pressedHandler;
		transformGesture.Transformed -= transformHandler;
		transformGesture.TransformCompleted += transformCompleteHandler;
	}

	/// <summary>
	/// Resets the Kiosks idle timer.
	/// </summary>
	void pressedHandler(object sender, EventArgs e){
		Debug.Log ("[pressedHandler] "+name);
		//turn off the "are you still there?" overlay
		waitingForSave = false;
		closer.gameObject.SetActive (false);
		//reset idle clock
		timeSinceLastTouch = 0f;
	}

	void transformHandler(object sender, EventArgs e){
		if (activePanel == null) {
			dragDelta = transformGesture.DeltaPosition;
			dragNav = true;
		}
		//reset idle clock
		timeSinceLastTouch = 0f;
	}

	void transformCompleteHandler(object sender, EventArgs e){
		dragNav = false;
	}

	/// <summary>
	/// Activates the "are you still there?" overlay
	/// </summary>
	private void AreYouStillThere(){
		if (pinDrop != null) {
			closer.localPosition = Vector3.zero + Vector3.forward * closer.localPosition.z;
		} else {
			if (activePanel != null) {
				closer.localPosition = new Vector3 (closer.localPosition.x, activePanel.localPosition.y, closer.localPosition.z);
			} else {
				closer.localPosition = new Vector3 (closer.localPosition.x, menu.localPosition.y, closer.localPosition.z);
			}
		}
		closer.gameObject.SetActive (true);
		timeTillClose = maxTimeTillClose;
		waitingForSave = true;
	}

	/// <summary>
	/// Initiates the transition of the Kiosk from one environment to another.
	/// Mainly used by UserNav
	/// </summary>
	/// <param name="_envID">index of the environment to switch to</param>
	public void SwitchEnvironment(int _envID){
		Debug.Log ("[SwitchEnvironment] to: " + _envID);
		//this sequence takes some time, this is a toggle that prevents interruptions from user input during
		somePanelIsAnimating = true;
		//set the new environment grabbing from the main environments object in the AssetManager
		env = AssetManager.Instance.environments [_envID];

		//prepare to remove prev bg
		bgPanelOld = bgPanel;
		bgPanel = null;
		bgPanelOld.transform.localPosition += Vector3.forward * -10f;

		//prepare to remove prev title
//		headerPanelOld = headerPanel;
//		headerPanel = null;
//		headerPanelOld.transform.localPosition += Vector3.forward * 10f;

		//animate out the active panel (if applicable)
		if (activePanel != null) {
			EaseCurve.Instance.Vec3 (activePanel, activePanel.localPosition, activePanel.localPosition + Vector3.right * 10, 0.3f, 0f, EaseCurve.Instance.easeIn, null, "local");
			//activePanel.GetComponent<PanelBase>().BackToGrid();
			ToggleTint (false);
		}

		//animate the grid out
		Vector3 gridLeave = userGrid.transform.localPosition;
		gridLeave.x += -10f;
		EaseCurve.Instance.Vec3 (userGrid.transform, userGrid.transform.localPosition, gridLeave, 0.3f, 0f, EaseCurve.Instance.easeIn, ContinueSwitch, "local"); 
		//EaseCurve.Instance.Vec3 (headerPanelOld.transform, headerPanelOld.transform.localPosition, headerPanelOld.transform.localPosition + Vector3.up * 6f, 0.5f, 0f, EaseCurve.Instance.linear, null, "local"); 
	}

	/// <summary>
	/// Step 2 in the Kiosk environment trasition sequence
	/// </summary>
	void ContinueSwitch(){
		//clean/unreference the previous active panel if any
		if (activePanel != null) {
			Destroy (activePanel.gameObject);
			activePanel = null;
		}
		//remove old items from user grid
		userGrid.GetComponent<UserGrid> ().ClearGrid ();
		//reset user grid position
		userGrid.transform.localPosition = new Vector3 (2, userGrid.transform.localPosition.y, userGrid.transform.localPosition.z);

		//create new title and backgrounds
		Populate ();

		userGrid.GetComponent<UserGrid> ().MakeGrid ();

		//calculate where new grid should animate to
		gridFinalPos = userGrid.transform.localPosition + Vector3.left * 3.5f;

		//animate
		EaseCurve.Instance.Vec3 (userGrid.transform, userGrid.transform.localPosition, gridFinalPos, 0.5f, 0f, EaseCurve.Instance.easeOut, null, "local");
		EaseCurve.Instance.Vec3 (bgPanel.transform, bgPanel.transform.localPosition, bgFinalPos, 0.5f, 0f, EaseCurve.Instance.easeOut, null, "local");
		//EaseCurve.Instance.Vec3 (headerPanel.transform, headerPanel.transform.localPosition, headerInitPos, 0.5f, 0f, EaseCurve.Instance.easeInOut, null, "local");

		//animate bg
		Material bgMat = bgPanelOld.transform.Find ("Front/1x1_texture/TextureQuad").GetComponent<Renderer> ().material;
		Color32 toColor = bgMat.color;
		toColor.a = 0;
		EaseCurve.Instance.Scl (bgPanelOld.transform, bgPanelOld.transform.localScale, bgPanelOld.transform.localScale * 1.5f, 0.51f, 0, EaseCurve.Instance.linear, FinishSwitch);
		EaseCurve.Instance.MatColor (bgMat, Color.white, toColor, 0.5f, 0, EaseCurve.Instance.linear);

		//animate nav bg color
		string prop;
		float start, end;
		if (navMat.GetFloat ("_position") < 0.5f) {
			prop = "_color2";
			start = 0;
			end = 1;
		} else {
			prop = "_color1";
			start = 1;
			end = 0;
		}
		navMat.SetColor (prop, env.envColor);
		navMat.SetFloat ("_position", start);
		EaseCurve.Instance.GradientPos (navMat, start, end, 0.5f, 0f, EaseCurve.Instance.linear, null);

		Material headerMat = headerPanel.transform.Find ("Front/1x1_kiosk_title/Bg").GetComponent<Renderer> ().material;
		if (headerMat.GetFloat ("_position") < 0.5f) {
			prop = "_color2";
			start = 0;
			end = 1;
		} else {
			prop = "_color1";
			start = 1;
			end = 0;
		}
		headerMat.SetColor (prop, env.envColor);
		headerMat.SetFloat ("_position", start);
		EaseCurve.Instance.GradientPos (headerMat, start, end, 0.5f, 0f, EaseCurve.Instance.linear, null);
		headerPanel.transform.Find("Front/1x1_kiosk_title/Icon").GetComponent<Renderer> ().material.mainTexture = AssetManager.Instance.GetTexture (env.envIconPath);
		headerPanel.transform.Find ("Front/1x1_kiosk_title/Title").GetComponent<TextMeshPro> ().text = env.envTitle;
		headerPanel.transform.Find ("Front/1x1_kiosk_title/Body").GetComponent<TextMeshPro> ().text = env.envSummary;
	}

	/// <summary>
	/// Last step of the Kiosk environemnt transition sequence
	/// </summary>
	void FinishSwitch(){
		//remove old previous background and title panels
		if (bgPanelOld != null)
			Destroy (bgPanelOld);
		if (headerPanelOld != null)
			Destroy (headerPanelOld);

		//sequence is done, toggle user input on
		somePanelIsAnimating = false;
	}


	/// <summary>
	/// Begins sequence to close the Kiosk
	/// </summary>
	public void CloseKiosk(){
		Debug.Log ("[CloseKiosk] " + column);
		//toggle to ignore all user input, once kiosk is closing it cannot be stopped
		closing = true;
		//ping that this kiosk is closing for anyone who cares
		EventsManager.Instance.UserKioskCloseRequest (new Vector2(column, 0), true);
		//animate the camera closing
		EaseCurve.Instance.CamRect (userCam, camOpenedRect, camClosedRect, 0.5f, EaseCurve.Instance.easeIn, HariKari);
	}

	/// <summary>
	/// Step 2 of the Kiosk close sequence
	/// </summary>
	private void HariKari(){
		//die with honor
		Destroy (gameObject);
	}

	/// <summary>
	/// Begins the Kiosk activation sequence
	/// </summary>
	/// <param name="_totalColumns">The total columns in this grid</param>
	/// <param name="_userColumn">The column in which to open this Kiosk</param>
	public void SetCam(float _totalColumns, float _userColumn){
		float userColumn = _userColumn;
		Debug.Log ("[SetCam] at col " + userColumn + " of " + _totalColumns+" | "+tapScreenPos);

		//calculate kiosk (camera frustrum) width from 0-1 based on the total columns
		camRectW = 1f / _totalColumns;
		//calculate horizontal position on screen from 0-1 based on desired column and cam frus width
		camRectX = camRectW * userColumn;
		//the viewport will animate from this to wide, so we need to get the center pos of our column
		float camRectXm = camRectX + (camRectW * 0.5f);
		//set the starting x,y,w,h (closed aperture, at center of col, anchor at bottom left)
		camClosedRect = new Rect (camRectXm, 0, 0, 1);
		//set the ending x,y,w,h (open aperture, at left of col, anchor at bottom left)
		camOpenedRect = new Rect (camRectX, 0, camRectW, 1);
		//set the camera to the starting setting
		userCam.rect = camClosedRect;

		//apply environemnt color to the navigation
		navMat = navBGrend.material;
		string prop;
		float start, end;
		if (navMat.GetFloat ("_position") < 0.5f) {
			prop = "_color1";
			start = 1;
			end = 0;
		} else {
			prop = "_color2";
			start = 0;
			end = 1;
		}
		navMat.SetColor (prop, env.envColor);
		navMat.SetFloat ("_position", end);

		//create the content panels
		Populate ();

		somePanelIsAnimating = true;

		//start the camera open animation
		EaseCurve.Instance.CamRect (userCam, camClosedRect, camOpenedRect, 0.5f, EaseCurve.Instance.easeIn, Next);
	}

	/// <summary>
	/// This instantiates all the necessary content panels for this environment
	/// </summary>
	void Populate(){
		bgPanel = Instantiate (AssetManager.Instance.NEWpanelPrefab, transform);
		bgPanel.name = "bgPanel";
		bgPanel.transform.localPosition = new Vector3 (0, 0, 50);
		bgPanel.transform.localScale = new Vector3 (3, 3, 1);
		bgPanel.GetComponent<PanelBase> ().AssembleBasic ("kiosk_bg", env.envKioskBg);
		bgPanel.GetComponent<PanelBase> ().panelContext = PanelBase.PanelContext.Kiosk;
		bgPanel.GetComponent<PanelBase> ().panelView = PanelBase.PanelView.Background;
		bgPanel.GetComponent<PanelBase> ().myKiosk = transform.GetComponent<UserKiosk> ();
		bgFinalPos = new Vector3(0, 0, 50);

		if (headerPanel == null) {
			headerPanel = Instantiate (AssetManager.Instance.NEWpanelPrefab, transform);
			headerPanel.name = "headerPanel";
			headerPanel.transform.localPosition = new Vector3 (0, 6, 20);
			//headerPanel.GetComponent<PanelObject> ().SetPanelColors (env.envColor);
			//headerPanel.GetComponent<PanelObject> ().SetAsTitle (env.envTitle);
			headerPanel.GetComponent<PanelBase>().environment = env;
			headerPanel.GetComponent<PanelBase> ().AssembleBasic ("title_kiosk");
			headerPanel.GetComponent<PanelBase> ().panelContext = PanelBase.PanelContext.Kiosk;
			headerPanel.GetComponent<PanelBase> ().panelView = PanelBase.PanelView.Background;
			headerInitPos = headerPanel.transform.localPosition + (Vector3.down * 3);
		}
	}

	/// <summary>
	/// Step 2 of the Kiosk open sequence.
	/// This is responsible for positioning the active panel (if applicable) and menu at the location where the user tapped the screen.
	/// </summary>
	void Next(){
		EventsManager.Instance.UserKioskEnvironmentSwitchRequest (GetComponent<UserKiosk>(), env.envID);

		Vector3 tapOffset = userCam.ScreenToViewportPoint (tapScreenPos);
		Vector3 menuFinalPos = Vector3.forward * 30f;
		if (tapOffset.y < 0.5f) {
			menuFinalPos.y += (0.5f - tapOffset.y) * -10f;
		}
		if (menuFinalPos.y < -3) {
			menuFinalPos.y = -3f;
		}

		if (activePanel) {
			Vector3 panelInitPos = activePanel.transform.localPosition;
			if (tapOffset.y < 0.5f) {
				panelInitPos.y = (0.5f - tapOffset.y) * -10f;
			}
			if (panelInitPos.y < -3f){
				panelInitPos.y = -3f;
			}
			//set active panel slighty to the right of center in kiosk to make nav more visible
			panelInitPos.x = 0.16f;
			EaseCurve.Instance.Vec3 (activePanel.transform, activePanel.transform.localPosition, panelInitPos, 1f, 0f, EaseCurve.Instance.easeInOut, null, "local");
		}

		EaseCurve.Instance.Vec3 (headerPanel.transform, headerPanel.transform.localPosition, headerInitPos, 1f, 0f, EaseCurve.Instance.easeInOut, null, "local");
		EaseCurve.Instance.Vec3 (menu.transform, menu.transform.localPosition, menuFinalPos, 1f, 0f, EaseCurve.Instance.easeOut, Next2, "local");
	}

	/// <summary>
	/// Step 3 of the Kiosk open sequence
	/// Animates the grid and background in
	/// </summary>
	void Next2(){
		gridFinalPos = userGrid.transform.localPosition + Vector3.left * 3.5f;
		EaseCurve.Instance.Vec3 (userGrid.transform, userGrid.transform.localPosition, gridFinalPos, 1f, 0f, EaseCurve.Instance.easeOut, null, "local");
		EaseCurve.Instance.Vec3 (bgPanel.transform, bgPanel.transform.localPosition, bgFinalPos, 1f, 0f, EaseCurve.Instance.easeOut, FinishOpen, "local");
	}

	/// <summary>
	/// Last step in Kiosk open sequence
	/// </summary>
	void FinishOpen(){
		//toggle interaction active
		somePanelIsAnimating = false;
	}

	public void ToggleTint(bool _show){
		if (_show) {
			EaseCurve.Instance.MatColor (tint.material, tint.material.color, new Color (0, 0, 0, 0.5f), 0.2f, 0.3f, EaseCurve.Instance.linear);
		} else {
			EaseCurve.Instance.MatColor (tint.material, tint.material.color, new Color (0, 0, 0, 0f), 0.2f, 0, EaseCurve.Instance.linear);
		}
	}

	/// <summary>
	/// Running all the time
	/// </summary>
	void Update () {
		//if kiosk closing, stop
		if (closing)
			return;

		//is the "are you still there?" overlay on?
		if (waitingForSave) {
			//countdown
			timeTillClose -= Time.deltaTime;
			//display countdown
			countdown.text = Mathf.RoundToInt(timeTillClose).ToString ();
			//if countdown reaches 0
			if (timeTillClose < 0) {
				//close the kiosk
				countdown.text = "0";
				CloseKiosk ();
			}
		} else {
			//not at overlay stage yet, just keep updating the idle timer
			timeSinceLastTouch += Time.deltaTime;
			//have we reach the limit?
			if (timeSinceLastTouch > maxWaitUntouched) {
				//activate the "are you still there?" overlay
				AreYouStillThere ();
			}
		}

		//makes the menu follow an active panel thats being moved around
		//this is activated by PanelBase
		if (menuFollowPanel) {
			Vector3 menuGoTo = activePanel.localPosition;
			menuGoTo.z = menu.localPosition.z;
			menuGoTo.x = menu.localPosition.x;
			menu.localPosition = Vector3.Lerp (menu.localPosition, menuGoTo, 4f * Time.deltaTime);
		}

		//allows user to drag the grid left and right
		//and the whole menu up and down
		if (dragGrid) {
			Vector3 gridGoTo = userGrid.localPosition;
			gridGoTo.x += dragDelta.x;
			userGrid.localPosition = gridGoTo;

			Vector3 bgGoTo = bgPanel.transform.localPosition; 
			bgGoTo.x -= dragDelta.x * 0.1f;
			bgPanel.transform.localPosition = bgGoTo;

			if(userGrid.localPosition.x > -1.5f)
				userGrid.localPosition = new Vector3 (-1.5f, userGrid.localPosition.y, userGrid.localPosition.z);
			if(userGrid.localPosition.x < gridWidth+2f)
				userGrid.localPosition = new Vector3 (gridWidth+2f, userGrid.localPosition.y, userGrid.localPosition.z);

			if(bgPanel.transform.localPosition.x < 0)
				bgPanel.transform.localPosition = new Vector3 (0f, bgPanel.transform.localPosition.y, bgPanel.transform.localPosition.z);
			if(bgPanel.transform.localPosition.x > Mathf.Abs(gridWidth) * 0.05f)
				bgPanel.transform.localPosition = new Vector3 (Mathf.Abs(gridWidth) * 0.05f, bgPanel.transform.localPosition.y, bgPanel.transform.localPosition.z);
		}

		if (dragNav) {
			Vector3 menuGoTo = menu.localPosition;
			menuGoTo.y += dragDelta.y;
			menu.localPosition = menuGoTo;
			if (menu.localPosition.y < -3f)
				menu.localPosition = new Vector3 (menu.localPosition.x, -3f, menu.localPosition.z);
			if (menu.localPosition.y > 0f)
				menu.localPosition = new Vector3 (menu.localPosition.x, -0, menu.localPosition.z);
			//
		}
	}

	/// <summary>
	/// Starts the transition to open Pin Drop.
	/// </summary>
	public void StartPinDrop(){
		somePanelIsAnimating = true;

		//initialize the pin drop
		pinDrop = Instantiate (pinDropPrefab, transform);
		pinDrop.GetComponent<PinDrop> ().initGlobeSize = 0.8f;
		pinDrop.GetComponent<PinDrop> ().initWindowSize = 1f;
		pinDrop.GetComponent<PinDrop> ().globeCam = userCam;
		pinDrop.GetComponent<PinDrop> ().myKiosk = GetComponent<UserKiosk> ();
		pinDrop.GetComponent<PinDrop>().Init ();

		//these should get animated out
		//animate nav
		EaseCurve.Instance.Vec3(nav.transform, nav.transform.localPosition, nav.transform.localPosition+Vector3.left, 0.5f, 0, EaseCurve.Instance.easeIn, null, "local");
		//animate grid
		Vector3 gridGoTo = userGrid.localPosition;
		gridGoTo.x = 3f;
		EaseCurve.Instance.Vec3(userGrid, userGrid.localPosition, gridGoTo, 0.5f, 0, EaseCurve.Instance.easeIn, null, "local");
		//animate header
		EaseCurve.Instance.Vec3 (headerPanel.transform, headerPanel.transform.localPosition, headerPanel.transform.localPosition + Vector3.up * 3f, 0.5f, 0, EaseCurve.Instance.easeIn, null, "local");
		//animate active panel (if applicable
		if (activePanel != null) {
			EaseCurve.Instance.Vec3 (activePanel, activePanel.localPosition, activePanel.localPosition + Vector3.right * 10, 0.3f, 0f, EaseCurve.Instance.easeIn, null, "local");
		}
		//animate bg
		Material bgMat = bgPanel.transform.Find ("Front/1x1_texture/TextureQuad").GetComponent<Renderer> ().material;
		Color32 toColor = bgMat.color;
		toColor.a = 0;
		EaseCurve.Instance.Scl (bgPanel.transform, bgPanel.transform.localScale, bgPanel.transform.localScale * 1.5f, 0.51f, 0.5f, EaseCurve.Instance.linear, StartPinDrop2);
		EaseCurve.Instance.MatColor (bgMat, Color.white, toColor, 0.5f, 0.5f, EaseCurve.Instance.linear);
	}

	private void StartPinDrop2(){
        Debug.Log("START PIN DROPPPP ");
		menu.gameObject.SetActive (false);
		tint.gameObject.SetActive (false);
		headerPanel.gameObject.SetActive (false);
		bgPanel.gameObject.SetActive (false);
		if(activePanel!=null)
			activePanel.gameObject.SetActive (false);

		somePanelIsAnimating = false;
	}

	public void StopPinDrop(){
		somePanelIsAnimating = true;

		//these should get animated in
		menu.gameObject.SetActive (true);
		tint.gameObject.SetActive (true);
		headerPanel.gameObject.SetActive (true);
		bgPanel.gameObject.SetActive (true);
		if(activePanel!=null)
			activePanel.gameObject.SetActive (true);

		pinDrop.GetComponent<PinDrop> ().Off ();

		//animate bg
		Material bgMat = bgPanel.transform.Find ("Front/1x1_texture/TextureQuad").GetComponent<Renderer> ().material;
		Color32 toColor = bgMat.color;
		toColor.a = 255;
		EaseCurve.Instance.Scl (bgPanel.transform, bgPanel.transform.localScale, bgPanel.transform.localScale * 0.666666f, 0.5f, 0f, EaseCurve.Instance.linear, null);
		EaseCurve.Instance.MatColor (bgMat, bgMat.color, toColor, 0.5f, 0f, EaseCurve.Instance.linear);

		//animate nav
		EaseCurve.Instance.Vec3(nav.transform, nav.transform.localPosition, nav.transform.localPosition-Vector3.left, 0.5f, 0.5f, EaseCurve.Instance.easeOut, null, "local");
		//animate grid
		Vector3 gridGoTo = userGrid.localPosition;
		gridGoTo.x = 0f;
		EaseCurve.Instance.Vec3(userGrid, userGrid.localPosition, gridGoTo, 0.5f, 0.5f, EaseCurve.Instance.easeOut, null, "local");
		//animate header
		EaseCurve.Instance.Vec3 (headerPanel.transform, headerPanel.transform.localPosition, headerPanel.transform.localPosition - Vector3.up * 3f, 0.51f, 0.5f, EaseCurve.Instance.easeOut, StopPinDrop2, "local");
		//animate active panel (if applicable
		if (activePanel != null) {
			EaseCurve.Instance.Vec3 (activePanel, activePanel.localPosition, activePanel.localPosition - Vector3.right * 10, 0.5f, 0.5f, EaseCurve.Instance.easeOut, null, "local");
		}

	}

	private void StopPinDrop2(){
		Destroy (pinDrop);
		somePanelIsAnimating =	 false;
	}
}
