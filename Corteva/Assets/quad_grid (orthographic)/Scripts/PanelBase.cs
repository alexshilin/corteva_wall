using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;



public class PanelBase : MonoBehaviour {

	[Header("Components")]
	public Transform front;
	public Transform back;
	public Transform thumbnail;

	public enum PanelContext
	{
		None,
		Idle,
		Kiosk
	}
	public enum PanelState
	{
		Ready,
		Animating,
		Active,
		Hidden
	}
	public enum PanelView
	{
		Blank,
		Background,
		Thumbnail,
		Front,
		Back
	}

	public Vector3 forwardPos = new Vector3 (0, 0, -0.01f);
	public Vector3 forwardRot = new Vector3 (0, 0, 0);
	public Vector3 awayPos = new Vector3 (0, 0, 0.01f);
	public Vector3 awayRot = new Vector3 (0, 180, 0);

	public PanelView currViewFacingForward;
	public PanelView currViewFacingAway;

	[Header("Dont Edit in Inspector")]
	public PanelState panelState;
	public PanelContext panelContext = PanelContext.None;
	public PanelView panelView = PanelView.Blank;

	public Environment environment{ get; set; }
	public int panelID;
	public UserKiosk myKiosk;
	public Vector2 panelGridPos;

	private TapGesture tapGesture;
	private TransformGesture transformGesture;

	private PanelModulePool PMP;

	void Awake(){
		tapGesture = GetComponent<TapGesture> ();
		transformGesture = GetComponent<TransformGesture> ();
		panelState = PanelState.Ready;
	}

	private void OnEnable()
	{
		tapGesture.Tapped += tappedHandler;

		transformGesture.TransformStarted += transformStartedHandler;
		transformGesture.Transformed += transformedHandler;
		transformGesture.TransformCompleted += transformCompletedHandler;
	}

	private void OnDisable()
	{
		tapGesture.Tapped -= tappedHandler;
		transformGesture.TransformStarted -= transformStartedHandler;
		transformGesture.Transformed -= transformedHandler;
		transformGesture.TransformCompleted -= transformCompletedHandler;
	}

	void Start(){
		PMP = PanelModulePool.Instance;
		//AssemblePanel ();
	}

	void Update(){
		if (Input.GetKeyDown (KeyCode.A)) {
			panelState = PanelState.Active;
			panelContext = PanelContext.Kiosk;
			AssemblePanel ();
		}



		if (Input.GetKeyDown (KeyCode.S)) {
			ActivateView (PanelView.Front, false);
		}
		if (Input.GetKeyDown (KeyCode.X)) {
			ActivateView (PanelView.Front, true);
		}

		if (Input.GetKeyDown (KeyCode.D)) {
			ActivateView (PanelView.Back, false);
		}
		if (Input.GetKeyDown (KeyCode.C)) {
			ActivateView (PanelView.Back, true);
		}

		if (Input.GetKeyDown (KeyCode.F)) {
			ActivateView (PanelView.Thumbnail, false);
		}
		if (Input.GetKeyDown (KeyCode.V)) {
			ActivateView (PanelView.Thumbnail, true);
		}
	}

	#region module assembly
	public void AssemblePanel()//(Environment _environment, int _id)
	{
//		environment = _environment;
//		panelID = _id;
		Debug.Log("[AssemblePanel]");
		LoadModule ("1x1_texture_color", PanelView.Thumbnail, "test_thumbnail");
		LoadModule ("1x1_texture_color", PanelView.Front, "test_front");
		LoadModule ("1x1_texture_color", PanelView.Back, "test_back");

		ActivateView (PanelView.Thumbnail, false);
		ActivateView (PanelView.Front, true);
	}
	public void LoadModule(string _type, PanelView _view, string _img)
	{
		Debug.Log("\t[LoadModule] '"+_type+"' into "+_view);

		//determine parent container
		Transform viewParent = front;
		if (_view == PanelView.Front) viewParent = front;
		if (_view == PanelView.Back) viewParent = back;
		if (_view == PanelView.Thumbnail) viewParent = thumbnail;

		//instantiate module
		GameObject module = Instantiate (PMP.modules.Find (x => x.name == _type).prefab, viewParent);

		//populate the module
		Renderer panelRenderer = module.transform.Find("TextureQuad").GetComponent<Renderer> ();
		panelRenderer.material.mainTexture = AssetManager.Instance.GetTexture (_img);
	}
	public void ActivateView(PanelView _viewToShow, bool _faceAway)
	{
		Debug.Log ("[ActivateView] "+_viewToShow+" to "+(_faceAway ? "away" : "forward"));

		Transform viewToShow = front;
		if (_viewToShow == PanelView.Front) viewToShow = front;
		if (_viewToShow == PanelView.Back) viewToShow = back;
		if (_viewToShow == PanelView.Thumbnail) viewToShow = thumbnail;

		if (_faceAway) {
			viewToShow.transform.localPosition = awayPos;
			viewToShow.transform.localEulerAngles = awayRot;
			viewToShow.gameObject.SetActive (true);
			currViewFacingAway = _viewToShow;
		} else {
			viewToShow.transform.localPosition = forwardPos;
			viewToShow.transform.localEulerAngles = forwardRot;
			viewToShow.gameObject.SetActive (true);
			currViewFacingForward = _viewToShow;
		}

		if (currViewFacingAway != PanelView.Front && currViewFacingForward != PanelView.Front)
			front.gameObject.SetActive (false);
		if (currViewFacingAway != PanelView.Back && currViewFacingForward != PanelView.Back)
			back.gameObject.SetActive (false);
		if (currViewFacingAway != PanelView.Thumbnail && currViewFacingForward != PanelView.Thumbnail)
			thumbnail.gameObject.SetActive (false);
	}

	void PanelFlipped(){
		PanelView prevViewFacingFront = currViewFacingForward;
		PanelView prevViewFacingAway = currViewFacingAway;
		currViewFacingForward = prevViewFacingAway;
		currViewFacingAway = prevViewFacingFront;

		if (Mathf.RoundToInt(transform.localEulerAngles.y) == 180) {
			awayPos = new Vector3 (0, 0, -0.01f);
			awayRot = new Vector3 (0, 0, 0);
			forwardPos = new Vector3 (0, 0, 0.01f);
			forwardRot = new Vector3 (0, 180, 0);
		}else{
			forwardPos = new Vector3 (0, 0, -0.01f);
			forwardRot = new Vector3 (0, 0, 0);
			awayPos = new Vector3 (0, 0, 0.01f);
			awayRot = new Vector3 (0, 180, 0);;
		}

		panelState = PanelState.Active;
		Debug.Log ("[PanelFlipped] " + prevViewFacingFront + " | " + prevViewFacingAway + " >> " + currViewFacingForward + " | " + currViewFacingAway);
	}
	#endregion

	#region touch handlers
	private void tappedHandler(object sender, EventArgs e)
	{
		Debug.Log ("[tappedHandler] "+ tapGesture.ScreenPosition+ " "+ transform.name);
		Debug.Log ("\t" + panelState + " | " + panelContext);


		//panels in the Idle context can only be tapped, and should only activate user kiosks
		if (panelContext == PanelContext.Idle 
			&& panelView == PanelView.Front 
			&& panelState == PanelState.Active) 
		{
			//content panels are interactable, and should remain when tapped
			transform.parent = AssetManager.Instance.panels;
			transform.localScale = Vector3.one;
			Debug.Log ("\tpanelID: " + this.panelID + " | gridPosID: " + GridManagerOrtho.Instance.gridPositions [this.panelID].id);
			transform.position = new Vector3 (GridManagerOrtho.Instance.gridPositions [this.panelID].center.x, GridManagerOrtho.Instance.gridPositions [this.panelID].center.y, 10);
			EaseCurve.Instance.Scl (transform, transform.localScale, transform.localScale * 0.9f, 0.25f, 0, EaseCurve.Instance.linear);
			ScreenManager.Instance.MoveToLayer (transform, LayerMask.NameToLayer ("UserInit"));
			Debug.Log ("\tpanelGridPos: " + this.panelGridPos);
			EventsManager.Instance.UserKioskOpenRequest (this.panelGridPos, environment);
			StartCoroutine (MovePanelToKiosk ((int)this.panelGridPos.x));
		}


		//panel in idle context that is background or is animating should activate blank kiosk
		if (panelContext == PanelContext.Idle 
			&& (panelView == PanelView.Background || panelState == PanelState.Animating)) 
		{
			Vector2 tappedGridPos = GridManagerOrtho.Instance.CalculateColRowFromScreenPos (tapGesture.ScreenPosition);
			EventsManager.Instance.UserKioskOpenRequest (tappedGridPos);
		}


		//
		if (panelContext == PanelContext.Kiosk && panelState == PanelState.Active) {
			Debug.Log ("\trotate 360 ");
			panelState = PanelState.Animating;
			//SetAsThumbnail (); //this should happen elsewhere
			EaseCurve.Instance.Rot (transform, transform.localRotation, 180f, transform.up, 0.5f, 0f, EaseCurve.Instance.linear, PanelFlipped);
			//EaseCurve.Instance.Rot (transform, transform.localRotation, 360f, transform.up, 1f, 0f, EaseCurve.Instance.linear);
			//EaseCurve.Instance.Scl (transform, transform.localScale, transform.localScale*0.5f, 1f, 0f, EaseCurve.Instance.linear);
		}

		//panel in kiosk context that is background should close active kiosk
		if (panelContext == PanelContext.Kiosk && panelView == PanelView.Background) {

		}

		//panel in kiosk context that is a thumbnail should activate
		if (panelContext == PanelContext.Kiosk && panelView == PanelView.Thumbnail) {
			//first check if another panel is active, and hide that one
		}
	}

	private void transformStartedHandler(object sender, EventArgs e)
	{
		//Debug.Log (transform.name+" transformStartedHandler");

	}

	private void transformedHandler(object sender, EventArgs e)
	{
		if (panelView == PanelView.Background || panelContext == PanelContext.Idle)
			return;

		transform.position += transformGesture.DeltaPosition;
		myKiosk.menuFollowPanel = true;

		transform.localScale *= transformGesture.DeltaScale;
		if (transform.localScale.x > 1)
			transform.localScale = Vector3.one;
		if (transform.localScale.x < 0.5f)
			transform.localScale = Vector3.one * 0.5f;
	}

	private void transformCompletedHandler(object sender, EventArgs e)
	{
		if (panelView == PanelView.Background || panelContext == PanelContext.Idle)
			return;

		if (myKiosk != null) {
			myKiosk.menuFollowPanel = false;
		}
	}
	#endregion

	IEnumerator MovePanelToKiosk(int _col)
	{
		yield return new WaitForSeconds (1f);
		Debug.Log ("[MovePanelToKiosk] UserKiosk_" + _col);
		GameObject kiosk = GameObject.Find ("/Kiosks/UserKiosk_" + _col);
		transform.parent = kiosk.transform;
		transform.localPosition = Vector3.zero + Vector3.forward * 25f;
		ScreenManager.Instance.MoveToLayer (transform, LayerMask.NameToLayer ("Default"));
		panelContext = PanelContext.Kiosk;
		panelState = PanelState.Active;
		myKiosk = kiosk.GetComponent<UserKiosk> ();
		myKiosk.activePanel = transform;
	}


}
