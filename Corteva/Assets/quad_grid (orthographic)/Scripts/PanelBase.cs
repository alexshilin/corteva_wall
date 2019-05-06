﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;
using TMPro;


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

		PMP = PanelModulePool.Instance;
		string temp = UnityEngine.Random.Range (0, 2) == 0 ? "template_01" : "template_02";
		AssemblePanel (temp);
	}

	private void OnDisable()
	{
		tapGesture.Tapped -= tappedHandler;
		transformGesture.TransformStarted -= transformStartedHandler;
		transformGesture.Transformed -= transformedHandler;
		transformGesture.TransformCompleted -= transformCompletedHandler;
	}

	void Start(){
		
	}

	void Update(){
		if (Input.GetKeyDown (KeyCode.A)) {
			panelState = PanelState.Active;
			panelContext = PanelContext.Kiosk;
			AssemblePanel ("");
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
	public void AssemblePanel(string _template)//(Environment _environment, int _id)
	{
//		environment = _environment;
//		panelID = _id;
		Debug.Log("[AssemblePanel]");
//		string _template = "01";
		GameObject t;
		Renderer panelRenderer;
		TextMeshPro text;



		if (_template == "329bg") {
			t = LoadModule ("1x2_texture", PanelView.Front);
			//populate the module
			//panelRenderer = t.transform.Find("TextureQuad").GetComponent<Renderer> ();
			//panelRenderer.material.mainTexture = AssetManager.Instance.GetTexture (_img);
			ActivateView (PanelView.Front, true);
			return;
		}




		if (_template == "template_01") {
			//THUMBNAIL
			t = LoadModule ("1x1_texture_color", PanelView.Thumbnail);
			panelRenderer = t.transform.Find("TextureQuad").GetComponent<Renderer> ();
			panelRenderer.material.mainTexture = AssetManager.Instance.GetTexture ("test_thumbnail");

			//FRONT
			t = LoadModule ("1x1_texture_color", PanelView.Front);

			bool isVideo = UnityEngine.Random.Range (0, 2) == 0 ? true : false;
			if (isVideo) {
				VideoPlayer vid = t.transform.Find ("TextureQuad").GetComponent<VideoPlayer> ();
				vid.url = AssetManager.Instance.GetRandomVideo ();
				vid.enabled = true;
				vid.Prepare ();
				vid.Play ();
			} else {
				panelRenderer = t.transform.Find ("TextureQuad").GetComponent<Renderer> ();
				panelRenderer.material.mainTexture = AssetManager.Instance.GetTexture ("sample_07");
			}

			t = LoadModule ("1x1_txt_layout_02", PanelView.Front);
			text = t.transform.Find("Title").GetComponent<TextMeshPro> ();
			text.text = "Front";
			t.transform.Find ("Body").gameObject.SetActive (false);
			t.transform.localPosition += transform.forward * -0.01f;

			//BACK
			t = LoadModule ("1x1_texture_color", PanelView.Back);
			t.transform.Find ("TextureQuad").GetComponent<Renderer> ().material.color = new Color32 (50, 50, 50, 255);

			t = LoadModule ("1x1_txt_layout_02", PanelView.Back);
			text = t.transform.Find("Title").GetComponent<TextMeshPro> ();
			text.text = "Back";
			t.transform.Find ("Body").gameObject.SetActive (false);
			t.transform.localPosition += transform.forward * -0.01f;
			return;
		}

		if (_template == "template_02") {
			//THUMBNAIL
			t = LoadModule ("1x1_texture_color", PanelView.Thumbnail);
			panelRenderer = t.transform.Find("TextureQuad").GetComponent<Renderer> ();
			panelRenderer.material.mainTexture = AssetManager.Instance.GetTexture ("test_thumbnail");

			//FRONT
			t = LoadModule ("1x1_texture_color", PanelView.Front);
			t.transform.Find ("TextureQuad").GetComponent<Renderer> ().material.color = new Color32 (50, 50, 50, 255);

			t = LoadModule ("1x1_viz_earth", PanelView.Front);
			t.transform.localPosition += transform.forward * -0.01f;

			t = LoadModule ("1x1_txt_layout_02", PanelView.Front);
			text = t.transform.Find("Title").GetComponent<TextMeshPro> ();
			text.text = "Front";
			t.transform.Find ("Body").gameObject.SetActive (false);
			t.transform.localPosition += transform.forward * -0.02f;

			//BACK
			t = LoadModule ("1x1_texture_color", PanelView.Back);
			t.transform.Find ("TextureQuad").GetComponent<Renderer> ().material.color = new Color32 (50, 50, 50, 255);

			t = LoadModule ("1x1_txt_layout_02", PanelView.Back);
			text = t.transform.Find("Title").GetComponent<TextMeshPro> ();
			text.text = "Back";
			t.transform.Find ("Body").gameObject.SetActive (false);
			t.transform.localPosition += transform.forward * -0.01f;
			return;
		}





		t = LoadModule ("1x1_texture_color", PanelView.Thumbnail);
		panelRenderer = t.transform.Find("TextureQuad").GetComponent<Renderer> ();
		panelRenderer.material.mainTexture = AssetManager.Instance.GetTexture ("test_thumbnail");

		t = LoadModule ("1x1_texture_color", PanelView.Front);
		panelRenderer = t.transform.Find("TextureQuad").GetComponent<Renderer> ();
		panelRenderer.material.mainTexture = AssetManager.Instance.GetTexture ("test_front");

		t = LoadModule ("1x1_texture_color", PanelView.Back);
		panelRenderer = t.transform.Find("TextureQuad").GetComponent<Renderer> ();
		panelRenderer.material.mainTexture = AssetManager.Instance.GetTexture ("test_back");

//		bool flip = UnityEngine.Random.Range (0, 2) == 0 ? true : false;
//		ActivateView (PanelView.Thumbnail, flip);
//		ActivateView (PanelView.Front, !flip);
	}
	private GameObject LoadModule(string _type, PanelView _view)
	{
		Debug.Log("\t[LoadModule] '"+_type+"' into "+_view);

		//determine parent container
		Transform viewParent = front;
		if (_view == PanelView.Front) viewParent = front;
		if (_view == PanelView.Back) viewParent = back;
		if (_view == PanelView.Thumbnail) viewParent = thumbnail;

		//instantiate module
		GameObject module = Instantiate (PMP.modules.Find (x => x.name == _type).prefab, viewParent);
		return module;
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
			//viewToShow.gameObject.SetActive (true);
			SetRender(viewToShow, true);
			currViewFacingAway = _viewToShow;
		} else {
			viewToShow.transform.localPosition = forwardPos;
			viewToShow.transform.localEulerAngles = forwardRot;
			//viewToShow.gameObject.SetActive (true);
			SetRender(viewToShow, true);
			currViewFacingForward = _viewToShow;
			panelView = _viewToShow;
		}

		if (currViewFacingAway != PanelView.Front && currViewFacingForward != PanelView.Front) {
			//front.gameObject.SetActive (false);
			SetRender(front, false);
		}
		if (currViewFacingAway != PanelView.Back && currViewFacingForward != PanelView.Back) {
			//back.gameObject.SetActive (false);
			SetRender(back, false);
		}
		if (currViewFacingAway != PanelView.Thumbnail && currViewFacingForward != PanelView.Thumbnail) {
			//thumbnail.gameObject.SetActive (false);
			SetRender(thumbnail, false);
		}
	}

	void SetRender(Transform _go, bool _enable){
		foreach (Renderer r in _go.GetComponentsInChildren<Renderer>()) {
			r.enabled = _enable;
		}
	}

	void PanelFlipped(){
		UpdatePanelView ();
		myKiosk.somePanelIsAnimating = false;
		panelState = PanelState.Active;
	}
	void PanelMovedToUserGrid(){
		UpdatePanelView ();
		transform.parent = myKiosk.userGrid;
		transform.localScale = Vector3.one;
		myKiosk.somePanelIsAnimating = false;
		myKiosk.activePanel = null;
		panelState = PanelState.Ready;
	}
	void PanelMovedToUserKiosk(){
		UpdatePanelView ();
		myKiosk.somePanelIsAnimating = false;
		panelState = PanelState.Active;
	}
	void UpdatePanelView(){
		PanelView prevViewFacingFront = currViewFacingForward;
		PanelView prevViewFacingAway = currViewFacingAway;
		currViewFacingForward = prevViewFacingAway;
		currViewFacingAway = prevViewFacingFront;

		if (Mathf.Abs(Mathf.RoundToInt(transform.localEulerAngles.y)) == 180) {
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

		//hide away view
		if (currViewFacingAway == PanelView.Front) {
			//front.gameObject.SetActive (false);
			SetRender(front, false);
		}
		if (currViewFacingAway == PanelView.Back) {
			//back.gameObject.SetActive (false);
			SetRender(back, false);
		}
		if (currViewFacingAway == PanelView.Thumbnail) {
			//thumbnail.gameObject.SetActive (false);
			SetRender(thumbnail, false);
		}

		panelView = currViewFacingForward;
		Debug.Log ("[PanelFlipped] " + prevViewFacingFront + " | " + prevViewFacingAway + " >> " + currViewFacingForward + " | " + currViewFacingAway);
	}
	#endregion

	#region touch handlers
	private void tappedHandler(object sender, EventArgs e)
	{
		Debug.Log ("[tappedHandler] "+ tapGesture.ScreenPosition+ " "+ transform.name);
		Debug.Log ("\t" + panelContext + " | " + panelView + " | " + panelState);

		//panel in idle context that is background or is animating should activate blank kiosk
		if (panelContext == PanelContext.Idle 
			&& (panelView == PanelView.Background || panelState == PanelState.Animating)) 
		{
			Vector2 tappedGridPos = GridManagerOrtho.Instance.CalculateColRowFromScreenPos (tapGesture.ScreenPosition);
			EventsManager.Instance.UserKioskOpenRequest (tappedGridPos);
		}

		//if the panel is animating, dont execute following
		if (panelState == PanelState.Animating) 
		{
			Debug.Log ("\t panel is animating [end]");
			return;
		}

		//panels in the Idle context can only be tapped, and should only activate user kiosks
		if (panelContext == PanelContext.Idle 
			&& (panelView == PanelView.Front || panelView == PanelView.Thumbnail)
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

		if(panelContext == PanelContext.Kiosk && myKiosk.somePanelIsAnimating)
		{
			Debug.Log ("\t kiosk is animating [end]");
			return;
		}
			

		//
		if (panelContext == PanelContext.Kiosk && panelState == PanelState.Active) {
			//FlipAround ();
			if (currViewFacingForward == PanelView.Front) 
			{
				ActivateView (PanelView.Back, true);
			}
			if (currViewFacingForward == PanelView.Back) 
			{
				ActivateView (PanelView.Front, true);
			}
			FlipAround ();
			//BackToGrid();
		}

		//panel in kiosk context that is background should close active kiosk
		if (panelContext == PanelContext.Kiosk && panelView == PanelView.Background) {

		}

		//panel in kiosk context that is a thumbnail should activate
		if (panelContext == PanelContext.Kiosk && panelView == PanelView.Thumbnail && panelState == PanelState.Ready) {
			//first check if another panel is active, and hide that one
			//EventsManager.Instance.UserKioskActivatePanelInGridRequest();
			if (myKiosk.activePanel) {
				Debug.Log("\t has active panel, closing that panel first");
				myKiosk.activePanel.GetComponent<PanelBase> ().BackToGrid ();
				//ActivateFromGrid (true);
			} else {
				Debug.Log("\t no active panel, opening thumbnail");
				ActivateFromGrid (false);
			}

		}
	}

	private void FlipAround()
	{
		Debug.Log ("\t[FlipAround]");
		panelState = PanelState.Animating;
		myKiosk.somePanelIsAnimating = true;
		//SetAsThumbnail (); //this should happen elsewhere
		EaseCurve.Instance.Rot (transform, transform.localRotation, 180f, transform.up, 0.5f, 0f, EaseCurve.Instance.easeOut, PanelFlipped);
		//EaseCurve.Instance.Rot (transform, transform.localRotation, 360f, transform.up, 1f, 0f, EaseCurve.Instance.linear);
		//EaseCurve.Instance.Scl (transform, transform.localScale, transform.localScale*0.5f, 0.4f, 0f, EaseCurve.Instance.easeBack);
	}

	public void BackToGrid()
	{
		
			Debug.Log ("[BackToGrid] " + name);
			if (currViewFacingForward != PanelView.Thumbnail) 
			{
				ActivateView (PanelView.Thumbnail, true);
			}
			panelState = PanelState.Animating;
			myKiosk.somePanelIsAnimating = true;
			Vector3 goTo = myKiosk.GetComponentInChildren<UserGrid> ().transform.TransformPoint(myKiosk.GetComponentInChildren<UserGrid> ().emptySpot);
			EaseCurve.Instance.Vec3 (transform, transform.position, goTo, 0.5f, 0, EaseCurve.Instance.easeOut);
			EaseCurve.Instance.Rot (transform, transform.localRotation, 180f, transform.up, 0.7f, 0f, EaseCurve.Instance.easeOutBack);
			EaseCurve.Instance.Scl (transform, transform.localScale, Vector3.one*0.3f, 0.8f, 0f, EaseCurve.Instance.easeOutBack, PanelMovedToUserGrid);
	}

	private void ActivateFromGrid(bool _waitForActiveToClose){
		Debug.Log ("[ActivateFromGrid] " + name);
		float delay = 0f;
		if (_waitForActiveToClose)
			delay = 0.6f;
		ActivateView (PanelView.Front, true);
		panelState = PanelState.Animating;
		myKiosk.somePanelIsAnimating = true;
		myKiosk.activePanel = transform;
		myKiosk.userGrid.GetComponent<UserGrid> ().emptySpot = transform.localPosition;
		Vector3 goTo = Vector3.forward * 25f;
		transform.parent = myKiosk.transform;
		EaseCurve.Instance.Vec3 (transform, transform.localPosition, goTo, 0.5f, delay, EaseCurve.Instance.easeOut, null, "local");
		EaseCurve.Instance.Rot (transform, transform.localRotation, 180f, transform.up, 0.5f, delay, EaseCurve.Instance.easeOut);
		EaseCurve.Instance.Scl (transform, transform.localScale, Vector3.one * 0.9f, 0.6f, delay, EaseCurve.Instance.easeOut, PanelMovedToUserKiosk);
	}

	private void transformStartedHandler(object sender, EventArgs e)
	{
		//Debug.Log (transform.name+" transformStartedHandler");

	}

	private void transformedHandler(object sender, EventArgs e)
	{
		if (panelView == PanelView.Background
		    || panelContext == PanelContext.Idle
		    || (panelContext == PanelContext.Kiosk && panelState == PanelState.Ready))
		{
			return;
		}

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
		if (panelView == PanelView.Thumbnail) {
			ActivateView (PanelView.Front, true);
			FlipAround ();
		}
	}

	public void SetAs329Video(bool _playWhenReady = false){
		AssemblePanel ("329bg");
	}

//	public void SetAs329Video(bool _playWhenReady = false){
//		frontFullPanelTexture329.gameObject.SetActive (true);
//		frontFullPanel329.gameObject.SetActive (true);
//
//		LoadVideo(AssetManager.Instance.GetRandom329Video(), _playWhenReady);
//		videoPlayer329.Control.MuteAudio (true);
//		videoPlayer329.enabled = true;
//
//		using329video = true;
//		if(_playWhenReady)
//			videoPlayer329.Control.Play ();
//	}


}
