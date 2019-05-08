using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TMPro;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;
using RenderHeads.Media.AVProVideo;

public class PanelObject : MonoBehaviour {

	public Transform pivot;
	public GameObject viz;
	public Color currentColor = Color.black;
	[HideInInspector]
	public Environment env;

	[Header("Front Full Panel 32:9")]
	public Transform frontFullPanel329;
	public Transform frontFullPanelTexture329;
	public MediaPlayer videoPlayer329;

	private bool using329video = false;

	[Header("Front Full Panel ")]
	public Transform frontFullPanel;
	public Transform frontFullPanelColor;
	public Transform frontFullPanelTexture;


	public TextMeshPro envHeadlineText;
	public TextMeshPro envSummaryText;

	public VideoPlayer videoPlayer;

	[Header("Front Split Panel ")]
	public Transform frontSplitPanel;
	public Transform frontSplitPanelColor;
	public Transform frontSplitPanelTexture;

	[Header("Back Panel")]
	public Transform backPanel;
	public Transform backPanelColor;
	public Transform backPanelTexture;


	//gesture vars
	public Transform colliders;

	private TapGesture tapGesture;
	private TransformGesture transformGesture;

	[Header("--")]
	public Vector2 panelGridPos;
	public bool canBeMasked = true;

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

	public PanelState panelState;
	public PanelContext panelContext = PanelContext.None;
	public PanelView panelMode = PanelView.Blank;
	public UserKiosk myKiosk;

	private Vector3 baseScale;
	private Vector3 basePos;

	public int panelID;

	void Awake(){
		foreach(TextMeshPro tmp in transform.GetComponentsInChildren<TextMeshPro>()){
			tmp.enableCulling = true;
		}
		foreach (Transform child in transform) {
			if(child != colliders)
				child.gameObject.SetActive (false);
		}
		panelState = PanelState.Ready;
	}

	void Start () {
		baseScale = transform.localScale;
		//frontPanelTexture.gameObject.SetActive (false);
	}

	private void OnEnable()
	{
		tapGesture = GetComponent<TapGesture> ();
		tapGesture.Tapped += tappedHandler;

		transformGesture = GetComponent<TransformGesture> ();
		transformGesture.TransformStarted += transformStartedHandler;
		transformGesture.Transformed += transformedHandler;
		transformGesture.TransformCompleted += transformCompletedHandler;

		//if (videoPlayer329.gameObject.activeSelf) {
			videoPlayer329.Events.AddListener (OnMediaPlayerEvent);
		//}
	}

	private void OnDisable()
	{
		tapGesture.Tapped -= tappedHandler;
		transformGesture.TransformStarted -= transformStartedHandler;
		transformGesture.Transformed -= transformedHandler;
		transformGesture.TransformCompleted -= transformCompletedHandler;

		//if (videoPlayer329.gameObject.activeSelf) {
			videoPlayer329.Events.RemoveListener (OnMediaPlayerEvent);
			//videoPlayer329.CloseVideo ();
		//}
	}


	#region assemble panel

	#endregion


	#region touch handlers
	private void tappedHandler(object sender, EventArgs e)
	{
		Debug.Log ("[tappedHandler] "+ tapGesture.ScreenPosition+ " "+ transform.name);
		Debug.Log ("\t" + panelState + " | " + panelContext);


		//panels in the Idle context can only be tapped, and should only activate user kiosks
		if (panelContext == PanelContext.Idle 
			&& panelMode == PanelView.Front 
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
			EventsManager.Instance.UserKioskOpenRequest (this.panelGridPos, tapGesture.ScreenPosition, env);
			StartCoroutine (MovePanelToKiosk ((int)this.panelGridPos.x));
		}


		//panel in idle context that is background or is animating should activate blank kiosk
		if (panelContext == PanelContext.Idle 
			&& (panelMode == PanelView.Background || panelState == PanelState.Animating)) 
		{
			Vector2 tappedGridPos = GridManagerOrtho.Instance.CalculateColRowFromScreenPos (tapGesture.ScreenPosition);
			EventsManager.Instance.UserKioskOpenRequest (tappedGridPos, tapGesture.ScreenPosition);
		}


		//
		if (panelContext == PanelContext.Kiosk && panelState == PanelState.Active) {
			Debug.Log ("\trotate 360 ");
			SetAsThumbnail (); //this should happen elsewhere
			EaseCurve.Instance.Rot (transform, transform.localRotation, 180f, transform.up, 0.5f, 0f, EaseCurve.Instance.linear);
			//EaseCurve.Instance.Rot (transform, transform.localRotation, 360f, transform.up, 1f, 0f, EaseCurve.Instance.linear);
			//EaseCurve.Instance.Scl (transform, transform.localScale, transform.localScale*0.5f, 1f, 0f, EaseCurve.Instance.linear);
		}

		//panel in kiosk context that is background should close active kiosk
		if (panelContext == PanelContext.Kiosk && panelMode == PanelView.Background) {

		}

		//panel in kiosk context that is a thumbnail should activate
		if (panelContext == PanelContext.Kiosk && panelMode == PanelView.Thumbnail) {
			//first check if another panel is active, and hide that one
		}
	}

	private void transformStartedHandler(object sender, EventArgs e)
	{
		//Debug.Log (transform.name+" transformStartedHandler");

	}

	private void transformedHandler(object sender, EventArgs e)
	{
		if (panelMode == PanelView.Background || panelContext == PanelContext.Idle)
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
		if (panelMode == PanelView.Background || panelContext == PanelContext.Idle)
			return;

		if (myKiosk != null) {
			myKiosk.menuFollowPanel = false;
		}
	}
	#endregion



	#region video handlers
	private Queue<string> _eventLog = new Queue<string>(8);
	private float _eventTimer = 1f;
	public void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
	{
		if (mp == videoPlayer329) {
			switch (et) {
			case MediaPlayerEvent.EventType.ReadyToPlay:
				break;
			case MediaPlayerEvent.EventType.Started:
				break;
			case MediaPlayerEvent.EventType.FirstFrameReady:
				break;
			case MediaPlayerEvent.EventType.MetaDataReady:
				break;
			case MediaPlayerEvent.EventType.FinishedPlaying:
				break;
			}

			AddEvent (et);
		}
	}
	private void AddEvent(MediaPlayerEvent.EventType et)
	{
		_eventLog.Enqueue(et.ToString());
		if (_eventLog.Count > 5)
		{
			_eventLog.Dequeue();
			_eventTimer = 1f;
		}
	}
	private void LoadVideo(string filePath, bool autoPlay = false)
	{
		if (!videoPlayer329.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, filePath, autoPlay))
		{
			Debug.LogError("Failed to open video!");
		}
	}
	private static bool VideoIsReady(MediaPlayer mp)
	{
		return (mp != null && mp.TextureProducer != null && mp.TextureProducer.GetTextureFrameCount() <= 0);
	}
	public void PlayVideo(){
		if (using329video) {
			videoPlayer329.Control.Play ();
		} else {
			if (videoPlayer.isPrepared && !videoPlayer.isPlaying)
				videoPlayer.Play ();
		}
	}
	public void PauseVideo(){
		if (using329video) {
			videoPlayer329.Control.Pause ();
		} else {
			if (videoPlayer.isPrepared && !videoPlayer.isPlaying)
				videoPlayer.Pause ();
		}
	}
	#endregion



	IEnumerator MovePanelToKiosk(int _col){
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



	#region variants
	public void Reset(){
		envHeadlineText.gameObject.SetActive (false);

		frontFullPanelTexture.gameObject.SetActive (false);
		frontFullPanelColor.gameObject.SetActive (false);
		frontFullPanel.gameObject.SetActive (false);
	}

	public void SetPanelColors (Color32 _color){
		frontFullPanelColor.GetComponent<Renderer> ().material.color = _color;
		backPanelColor.GetComponent<Renderer> ().material.color = _color;
	}

	public void SetAsTitle(string _title){
		Reset ();
		envHeadlineText.text = _title;
		envHeadlineText.gameObject.SetActive (true);
		envSummaryText.gameObject.SetActive (true);
		frontFullPanelColor.gameObject.SetActive (true);
		frontFullPanel.gameObject.SetActive (true);
	}

	public void SetAsImage(){
		Renderer panelRenderer = frontFullPanelTexture.GetComponent<Renderer> ();
		panelRenderer.material.mainTexture = AssetManager.Instance.GetRandomTexture ();
		frontFullPanelTexture.gameObject.SetActive (true);
		frontFullPanelColor.gameObject.SetActive (false);
		frontFullPanel.gameObject.SetActive (true);
	}
	public void SetAsImage1x2(){
		Renderer panelRenderer = frontSplitPanelTexture.GetComponent<Renderer> ();
		panelRenderer.material.mainTexture = AssetManager.Instance.GetRandomTexture1x2 ();
		frontFullPanel.gameObject.SetActive (false);

		frontSplitPanel.gameObject.SetActive (true);
		frontSplitPanelTexture.gameObject.SetActive (true);
		frontSplitPanelColor.gameObject.SetActive (false);
	}
		
	public void SetAs329Video(bool _playWhenReady = false){
		frontFullPanelTexture329.gameObject.SetActive (true);
		frontFullPanel329.gameObject.SetActive (true);

		LoadVideo(AssetManager.Instance.GetRandom329Video(), _playWhenReady);
		videoPlayer329.Control.MuteAudio (true);
		videoPlayer329.enabled = true;

		using329video = true;
		if(_playWhenReady)
			videoPlayer329.Control.Play ();
	}

	public void SetAsVideo(bool _hd, bool _playWhenReady){
		VideoPanel (_hd, _playWhenReady);
	}
	private void VideoPanel(bool _hd, bool _playWhenReady){
		if (_hd) {
			videoPlayer.url = AssetManager.Instance.GetRandomHDVideo ();
		} else {
			videoPlayer.url = AssetManager.Instance.GetRandomVideo ();
		}
		videoPlayer.enabled = true;
		videoPlayer.Prepare ();
		frontFullPanelTexture.gameObject.SetActive (true);
		frontFullPanelColor.gameObject.SetActive (false);
		frontFullPanel.gameObject.SetActive (true);
		if(_playWhenReady)
			videoPlayer.Play ();
	}

	public void SetAs3dViz(){
		viz.SetActive (true);
		frontFullPanelTexture.gameObject.SetActive (false);
		frontFullPanelColor.gameObject.SetActive (true);
		frontFullPanel.gameObject.SetActive (true);
	}


	public void SetAsThumbnail(){
		Renderer panelRenderer = backPanelTexture.GetComponent<Renderer> ();
		panelRenderer.material.mainTexture = AssetManager.Instance.GetRandomTexture ();
		backPanelTexture.gameObject.SetActive (true);
		backPanelColor.gameObject.SetActive (false);
		backPanel.gameObject.SetActive (true);
	}

//	public void SetAsThumbnail(){
//		Renderer panelRenderer = backPanelTexture.GetComponent<Renderer> ();
//		panelRenderer.material.mainTexture = AssetManager.Instance.GetRandomTexture ();
//		backPanelTexture.gameObject.SetActive (true);
//		backPanelColor.gameObject.SetActive (false);
//		backPanel.gameObject.SetActive (true);
//	}
	#endregion





}
