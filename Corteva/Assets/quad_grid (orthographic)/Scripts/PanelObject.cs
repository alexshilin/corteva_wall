using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TMPro;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;
using RenderHeads.Media.AVProVideo;
	
//PANELS
/*
	environment
		environmentColor
	panel
		panelType


	panelTypes
		envHeaderPanel -the one at the top of a column
		envAttractPanel	-the one just below envHeaderPanel
		beautyImgPanel -idle state only
		beautyVidPanel -idle state only
		imagePanel
		videoPanel
		vizPanel

	panelElements
		bgColor (based on env)
		img (optional)
		vid (optional)
		txt
			envHeadline
			envSummary
			pnlHeadline
			pnlSummary
			pnlBody
			pnlMessageType
*/

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


	//gesture vars
	public Transform colliders;
	private TapGesture tapGesture;
	private TransformGesture transformGesture;

	[Header("--")]
	public Vector2 panelGridPos;
	public bool canBeMasked = true;
	public enum PanelState
	{
		Ready,
		Animating,
		Active,
		Hidden
	}
	public enum PanelMode
	{
		Blank,
		Background,
		Thumbnail,
		Front,
		Back

	}
	public enum PanelContext
	{
		None,
		Idle,
		Kiosk
	}
	public PanelState panelState;
	public PanelContext panelContext = PanelContext.None;
	public PanelMode panelMode = PanelMode.Blank;
	public bool isUserActive = false;

	private Vector3 baseScale;
	private Vector3 basePos;

	public int panelID;

	public bool canInteract = true;

	// GET module and POPULATE its content
	// @ grid position DO something

	public void SetAsNonInteractive(){
		canInteract = false;
		OnDisable ();
	}

	void Awake(){
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


	#region touch
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

	private void tappedHandler(object sender, EventArgs e)
	{
		Debug.Log ("[tappedHandler] "+ tapGesture.ScreenPosition+ " "+ transform.name);
		Debug.Log ("\t" + panelState + " | " + panelContext);


		//panels in the Idle state can only be tapped, and should only activate user kiosks
		if (panelContext == PanelContext.Idle 
			&& panelMode == PanelMode.Front 
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
			EventsManager.Instance.UserKioskOpenRequest (this.panelGridPos, env);
			StartCoroutine (MovePanelToKiosk ((int)this.panelGridPos.x));
		}



		if (panelContext == PanelContext.Idle 
			&& (panelMode == PanelMode.Background || panelState == PanelState.Animating)) 
		{
				//background/beauty panels are non interactable and should not remain when tapped
				//Debug.Log("tapped at: "+tapGesture.ScreenPosition);
				//Debug.Log ("from: " + );
				//tapGesture
				Vector2 tappedGridPos = GridManagerOrtho.Instance.CalculateColRowFromScreenPos (tapGesture.ScreenPosition);
				EventsManager.Instance.UserKioskOpenRequest (tappedGridPos);
		}
	}


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

	IEnumerator MovePanelToKiosk(int _col){
		yield return new WaitForSeconds (1f);
		Debug.Log ("[MovePanelToKiosk] UserKiosk_" + _col);
		GameObject kiosk = GameObject.Find ("/Kiosks/UserKiosk_" + _col);
		transform.parent = kiosk.transform;
		transform.localPosition = Vector3.zero + Vector3.forward * 25f;
		ScreenManager.Instance.MoveToLayer (transform, LayerMask.NameToLayer ("Default"));
		panelContext = PanelContext.Kiosk;
		//kiosk.GetComponent<UserManager> ().activePanel = transform;
	}

	private void transformStartedHandler(object sender, EventArgs e)
	{
		//Debug.Log (transform.name+" transformStartedHandler");

	}

	private void transformedHandler(object sender, EventArgs e)
	{
		if (panelMode == PanelMode.Background || panelContext == PanelContext.Idle)
			return;

		transform.position += transformGesture.DeltaPosition;

		transform.localScale *= transformGesture.DeltaScale;
		if (transform.localScale.x > 1)
			transform.localScale = Vector3.one;
		if (transform.localScale.x < 0.5f)
			transform.localScale = Vector3.one * 0.5f;
	}

	private void transformCompletedHandler(object sender, EventArgs e)
	{
		//Debug.Log (transform.name+" transformCompletedHandler");
	}
	#endregion




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
	#endregion





}
