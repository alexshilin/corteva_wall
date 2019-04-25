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

//	public class PanelElement{
//		public string type;
//		public string img;
//		public string env;
//		public Color envColor;
//		public PanelObject panelObj;
//	}

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

	public bool isUserActive = false;

	private Vector3 baseScale;
	private Vector3 basePos;

	public int panelID;

	public Transform colliders;
	public bool canInteract = true;

	private TapGesture tapGesture;
	private TransformGesture transformGesture;

	[Header("--")]
	public Vector2 panelGridPos;
	public bool canBeMasked = true;
	public string panelState = "";

	// GET module and POPULATE its content
	// @ grid position DO something

	public void SetAsNonInteractive(){
		canInteract = false;
		OnDisable ();
	}

	void Awake(){
//		if (canBeMasked) {
//			Renderer[] renders = GetComponentsInChildren<Renderer> ();
//			foreach (Renderer rendr in renders) {
//				rendr.material.renderQueue = 2002; // set their renderQueue
//			}
//		}
		foreach (Transform child in transform) {
			if(child != colliders)
				child.gameObject.SetActive (false);
		}
		
	}


	// Use this for initialization
	void Start () {
		
		baseScale = transform.localScale;
		//frontPanelTexture.gameObject.SetActive (false);
	}

	// Update is called once per frame
	void Update () {

	}

	public void PlayVideo(){
		if (using329video) {
			//if (videoPlayer329.isPrepared && !videoPlayer329.isPlaying)
			//if(videoPlayer329
			videoPlayer329.Control.Play ();
		} else {
			if (videoPlayer.isPrepared && !videoPlayer.isPlaying)
				videoPlayer.Play ();
		}
	}

	public void PauseVideo(){
		if (using329video) {
			//if (videoPlayer329.isPrepared && !videoPlayer329.isPlaying)
			//if(videoPlayer329
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

		if (videoPlayer329.gameObject.activeSelf) {
			videoPlayer329.Events.AddListener (OnMediaPlayerEvent);
		}
	}

	private void OnDisable()
	{
		tapGesture.Tapped -= tappedHandler;
		transformGesture.TransformStarted -= transformStartedHandler;
		transformGesture.Transformed -= transformedHandler;
		transformGesture.TransformCompleted -= transformCompletedHandler;

		if (videoPlayer329.gameObject.activeSelf) {
			videoPlayer329.Events.RemoveListener (OnMediaPlayerEvent);
			videoPlayer329.CloseVideo ();
		}
	}

	private void tappedHandler(object sender, EventArgs e)
	{
		Debug.Log ("[tappedHandler] "+ tapGesture.ScreenPosition+ " "+ transform.name+" "+panelState+" | "+canInteract);


		//panels in the Idle state can only be tapped, and should only activate user kiosks
		if (panelState == "Idle") {
			//content panels are interactable, and should remain when tapped
			if (canInteract) {
				transform.parent = AssetManager.Instance.panels;
				transform.localScale = Vector3.one;
				Debug.Log ("panelID: " + this.panelID);
				Debug.Log ("\tgridPosID: " + GridManagerOrtho.Instance.gridPositions [this.panelID].id);
				transform.position = new Vector3(GridManagerOrtho.Instance.gridPositions[this.panelID].center.x, GridManagerOrtho.Instance.gridPositions[this.panelID].center.y, 10);
				EaseCurve.Instance.Scl (transform, transform.localScale, transform.localScale * 0.9f, 0.25f, 0, EaseCurve.Instance.linear);
				ScreenManager.Instance.MoveToLayer (transform, LayerMask.NameToLayer ("UserInit"));
				Debug.Log ("panelGridPos: " + this.panelGridPos);
				EventsManager.Instance.UserKioskRequest (this.panelGridPos, true, env);
				StartCoroutine (MoveToKiosk ((int)this.panelGridPos.x));
			} else {
				//background/beauty panels are non interactable and should not remain when tapped
				//Debug.Log("tapped at: "+tapGesture.ScreenPosition);
				//Debug.Log ("from: " + );
				//tapGesture
				Vector2 tappedGridPos = GridManagerOrtho.Instance.CalculateColRowFromScreenPos (tapGesture.ScreenPosition);
				EventsManager.Instance.UserKioskRequest (tappedGridPos, true);
			}
		}
		if (isUserActive) {
			//Spin (1f, 180f);
		}
		if (!isUserActive) {
			//isUserActive = true;
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
				//GatherProperties ();
				break;
			case MediaPlayerEvent.EventType.FinishedPlaying:
				break;
			}

			AddEvent (et);
		}
	}
	private void AddEvent(MediaPlayerEvent.EventType et)
	{
		//Debug.Log("[SimpleController] Event: " + et.ToString());
		if (et.ToString () == "ResolutionChanged") {
			//Debug.Log ("\t" + videoPlayer329.Info.GetVideoWidth() + " " + videoPlayer329.Info.GetVideoHeight());
		}
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

	IEnumerator MoveToKiosk(int _col){
		yield return new WaitForSeconds (1f);
		Debug.Log ("Looking for: UserKiosk_" + _col);
		GameObject kiosk = GameObject.Find ("/Kiosks/UserKiosk_" + _col);
		transform.parent = kiosk.transform;
		transform.localPosition = Vector3.zero + Vector3.forward * 10f;
		ScreenManager.Instance.MoveToLayer (transform, LayerMask.NameToLayer ("Default"));
		//kiosk.GetComponent<UserManager> ().activePanel = transform;
	}

	private void transformStartedHandler(object sender, EventArgs e)
	{
		//Debug.Log (transform.name+" transformStartedHandler");

	}

	private void transformedHandler(object sender, EventArgs e)
	{
		//Debug.Log (transform.name+" transformedHandler "+transformGesture.DeltaPosition);
		//if (isUserActive) {
			transform.position += transformGesture.DeltaPosition;

			transform.localScale *= transformGesture.DeltaScale;
			if (transform.localScale.x > 1)
				transform.localScale = Vector3.one;
			if (transform.localScale.x < 0.5f)
				transform.localScale = Vector3.one * 0.5f;
		//}
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
		
	public void SetAs329Video(bool _playWhenReady){
		//videoPlayer329.m_VideoPath = AssetManager.Instance.GetRandom329Video ();
		bool autoStart = false;
		if (_playWhenReady)
			autoStart = true;

		frontFullPanelTexture329.gameObject.SetActive (true);
		frontFullPanel329.gameObject.SetActive (true);

		//videoPlayer329.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, AssetManager.Instance.GetRandom329Video(), autoStart);
		LoadVideo(AssetManager.Instance.GetRandom329Video(), autoStart);
		videoPlayer329.Control.MuteAudio (true);
		//videoPlayer329.url = AssetManager.Instance.GetRandom329Video ();
		videoPlayer329.enabled = true;
		//videoPlayer329.Prepare ();

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

	public void SetAsInfograph(){

	}
	#endregion




	#region animations
	public void ZoomToCell(){
		//iTween.RotateAdd (transform.gameObject, iTween.Hash ("x", 360f, "easeType", "easeOutCubic", "time", 0.75f));
		iTween.ScaleTo (transform.gameObject, iTween.Hash ("x", 1f, "y", 1f, "easeType", "easeOutCubic", "time", 0.5f));
	}
	public void ZoomToFull(){
		iTween.ScaleTo (transform.gameObject, iTween.Hash ("x", 3f, "y", 3f, "easeType", "easeOutCubic", "time", 0.7f));
	}
	public void ZoomToOver(){
		iTween.ScaleTo (transform.gameObject, iTween.Hash ("x", 5f, "y", 5f, "easeType", "easeOutCubic", "time", 0.9f));
	}
	public void ZoomToOver2(){
		iTween.ScaleTo (transform.gameObject, iTween.Hash ("x", 10f, "y", 10f, "easeType", "easeOutCubic", "time", 1.1f));
	}

	public void BobDown(){
		transform.localPosition += Vector3.back * 1;
		baseScale = transform.localScale;
		Vector3 toScale = transform.localScale * 0.8f;
		iTween.ScaleTo(transform.gameObject, iTween.Hash("scale", toScale, "easeType", "easeOutQuad", "time", 0.5f));
	}
	public void BobUp(){
		Vector3 toScale = baseScale;
		iTween.ScaleTo(transform.gameObject, iTween.Hash("scale", toScale, "easeType", "easeOutElastic", "time", 1f));
	}



	/// <summary>
	/// Spin at specified speed along this object's local up axis
	/// </summary>
	/// <param name="speed">Speed.</param>
	public void Spin(float speed){
		
	}

	/// <summary>
	/// Spin at specified speed along specified axis.
	/// </summary>
	/// <param name="speed">Speed.</param>
	/// <param name="axis">Axis.</param>
	public void Spin(float speed, Vector3 axis){

	}

	/// <summary>
	/// Spin the specified speed for maxDegrees along this objects local up axis
	/// </summary>
	/// <param name="speed">Speed.</param>
	/// <param name="maxDegrees">Max degrees.</param>
	public void Spin(float speed, float maxDegrees){
		//iTween.RotateAdd (transform.gameObject, new Vector3 (0, maxDegrees, 0), speed);
		iTween.RotateAdd (transform.gameObject, iTween.Hash("y", maxDegrees, "easetype", "easeInOutCubic", "time", speed));
	}

	/// <summary>
	/// Spin at specified speed for maxDegrees on axis.
	/// </summary>
	/// <param name="speed">Speed.</param>
	/// <param name="maxDegrees">Max degrees.</param>
	/// <param name="axis">Axis.</param>
	public void Spin(float speed, float maxDegrees, Vector3 axis){

	}
	#endregion
}
