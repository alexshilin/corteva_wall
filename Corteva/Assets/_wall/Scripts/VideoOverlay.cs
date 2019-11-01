using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TMPro;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;

public class VideoOverlay : MonoBehaviour {

	public Camera cam;
	public GameObject videoPlayerParent;
	public VideoPlayer videoPlayer;
	public AudioSource audioSource;
	public Renderer overlayBg;

	[Header("Video Controls")]
	public TextMeshPro timeTxt;
	public TextMeshPro durationTxt;
	public SpriteRenderer seekBg;
	public SpriteRenderer seekFg;
	public Transform seekHandle;
	public SpriteRenderer volBg;
	public SpriteRenderer volFg;
	public Transform volHandle;
	public GameObject soundOn;
	public GameObject soundOff;
	public GameObject playBtn;
	public GameObject pauseBtn;
	public Transform controls;

	public TapGesture barTapGesture;
	public TransformGesture barTransformGesture;
	public TapGesture volTapGesture;
	public TransformGesture volTransformGesture;
	public TapGesture soundTapGesture;
	public TapGesture seek15bTapGesture;
	public TapGesture seek15fTapGesture;
	public TapGesture playPauseTapGesture;
	public TapGesture closeTapGesture;
	public TransformGesture controlsTransformGesture;
	public TapGesture videoTapGesture;

	private bool preparing;
	private double totalTime;
	private string totalTimeStr;
	private string timeStr;
	private float timePct;
	private bool muted = false;
	private float lastVol;
	private float controlsIdleTime = 7f;
	private float controlCurrIdle = 0;

	[Header("Enabler")]
	public Transform leftControls;
	public Transform leftEnableControl;
	public Transform leftEnabler;

	public TapGesture leftEnablerTapGesture;
	public TapGesture leftLoadVideoTapGesture;
	public TapGesture leftCloseLoadVideoTapGesture;

	public Transform rightControls;
	public Transform rightEnableControl;
	public Transform rightEnabler;

	public TapGesture rightEnablerTapGesture;
	public TapGesture rightLoadVideoTapGesture;
	public TapGesture rightCloseLoadVideoTapGesture;

	private Vector3 leftEnablerOffPos = new Vector3(-0.6f, 0,0);
	private Vector3 leftEnablerControlsOffPos = new Vector3(-1.6f, 0,0);

	private Vector3 rightEnablerOffPos = new Vector3(0.6f, 0,0);
	private Vector3 rightEnablerControlsOffPos = new Vector3(1.6f, 0,0);

	private float enablerIdleTime = 5f;
	private float enablerCurrIdle;
	private bool enablerOpen = false;
	private bool controlsLeft = true;

	void Start(){
		controls.gameObject.SetActive (false);
		videoPlayerParent.SetActive (false);
		preparing = true;
		leftControls.localPosition = new Vector3 (-((GridManagerOrtho.Instance.desiredFullScreenAspect.x / 2) + 0.2f), leftControls.localPosition.y, leftControls.localPosition.z);
		rightControls.localPosition = new Vector3 (((GridManagerOrtho.Instance.desiredFullScreenAspect.x / 2) + 0.2f), rightControls.localPosition.y, rightControls.localPosition.z);
	}

	void OnEnable(){
		barTapGesture.Tapped += barTappedHandler;
		barTransformGesture.Transformed += barTransformedGesture;

		volTapGesture.Tapped += volTappedHandler;
		volTransformGesture.Transformed += volTransformedGesture;

		soundTapGesture.Tapped += soundTappedHandler;

		seek15bTapGesture.Tapped += seek15bTappedHandler;
		seek15fTapGesture.Tapped += seek15fTappedHandler;

		playPauseTapGesture.Tapped += playPauseTappedHandler;

		closeTapGesture.Tapped += closeTappedHandler;

		controlsTransformGesture.Transformed += controlsTransformedGesture;

		videoTapGesture.Tapped += videoTappedHandler;

		leftEnablerTapGesture.Tapped += leftEnablerTapTappedHandler;
		leftLoadVideoTapGesture.Tapped += leftLoadVideoTappedHandler;
		leftCloseLoadVideoTapGesture.Tapped += leftCloseLoadVideoTappedHandler;

		rightEnablerTapGesture.Tapped += rightEnablerTapTappedHandler;
		rightLoadVideoTapGesture.Tapped += rightLoadVideoTappedHandler;
		rightCloseLoadVideoTapGesture.Tapped += leftCloseLoadVideoTappedHandler;
	}

	void OnDisable(){
		barTapGesture.Tapped -= barTappedHandler;
		barTransformGesture.Transformed -= barTransformedGesture;

		volTapGesture.Tapped -= volTappedHandler;
		volTransformGesture.Transformed -= volTransformedGesture;

		soundTapGesture.Tapped -= soundTappedHandler;

		seek15bTapGesture.Tapped -= seek15bTappedHandler;
		seek15fTapGesture.Tapped -= seek15fTappedHandler;

		playPauseTapGesture.Tapped -= playPauseTappedHandler;

		closeTapGesture.Tapped -= closeTappedHandler;

		controlsTransformGesture.Transformed -= controlsTransformedGesture;

		videoTapGesture.Tapped -= videoTappedHandler;

		rightEnablerTapGesture.Tapped -= rightEnablerTapTappedHandler;
		rightLoadVideoTapGesture.Tapped -= leftLoadVideoTappedHandler;
		rightCloseLoadVideoTapGesture.Tapped -= leftCloseLoadVideoTappedHandler;
	}

	void barTappedHandler(object sender, System.EventArgs e){
		controlCurrIdle = 0f;
		float seekPct = seekBg.transform.InverseTransformPoint(cam.ScreenToWorldPoint(barTapGesture.ScreenPosition)).x / seekBg.size.x;
		SeekTo (seekPct);
	}
	void barTransformedGesture(object sender, System.EventArgs e){
		controlCurrIdle = 0f;
		float seekPct = seekBg.transform.InverseTransformPoint(cam.ScreenToWorldPoint(barTransformGesture.ScreenPosition)).x / seekBg.size.x;
		SeekTo (seekPct);
	}

	void volTappedHandler(object sender, System.EventArgs e){
		controlCurrIdle = 0f;
		float volPct = volBg.transform.InverseTransformPoint(cam.ScreenToWorldPoint(volTapGesture.ScreenPosition)).x / volBg.size.x;
		SetVolume (volPct);
	}
	void volTransformedGesture(object sender, System.EventArgs e){
		controlCurrIdle = 0f;
		float volPct = volBg.transform.InverseTransformPoint(cam.ScreenToWorldPoint(volTransformGesture.ScreenPosition)).x / volBg.size.x;
		SetVolume (volPct);
	}

	void soundTappedHandler(object sender, System.EventArgs e){
		controlCurrIdle = 0f;
		if (muted) {
			SetVolume (lastVol);
			muted = false;
		} else {
			lastVol = audioSource.volume;
			SetVolume (0);
			muted = true;
		}
	}

	void seek15bTappedHandler(object sender, System.EventArgs e){
		controlCurrIdle = 0f;
		SkipBy (15f, false);
	}
	void seek15fTappedHandler(object sender, System.EventArgs e){
		controlCurrIdle = 0f;
		SkipBy (15f);
	}

	void playPauseTappedHandler(object sender, System.EventArgs e){
		controlCurrIdle = 0f;
		PlayPause ();
	}

	private void PlayPause(){
		if (videoPlayer.isPlaying) {
			videoPlayer.Pause ();
			playBtn.SetActive (true);
			pauseBtn.SetActive (false);
		} else {
			videoPlayer.Play ();
			playBtn.SetActive (false);
			pauseBtn.SetActive (true);
		}
	}

	void closeTappedHandler(object sender, System.EventArgs e){
		StartCoroutine (DoClose ());
	}

	void controlsTransformedGesture(object sender, System.EventArgs e){
		controlCurrIdle = 0f;
		controls.localPosition += controlsTransformGesture.DeltaPosition;
	}

	void videoTappedHandler(object sender, System.EventArgs e){
		ShowControls ();
	}

	void ShowControls(){
		controls.gameObject.SetActive (true);
		controlCurrIdle = 0f;
	}

	public void LoadVideo(string _source){
		Debug.Log ("[VideoOverlay] LoadVideo (" + _source + ")");
		overlayBg.transform.localScale = new Vector3 (GridManagerOrtho.Instance.desiredFullScreenAspect.x, 9, 1);
		videoPlayerParent.SetActive (true);
		overlayBg.material.color = new Color32 (0, 0, 0, 0);
		videoPlayer.transform.localPosition += Vector3.up * videoPlayer.transform.localScale.y;
		videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
		videoPlayer.controlledAudioTrackCount = 1;
		videoPlayer.EnableAudioTrack(0, true);
		videoPlayer.SetTargetAudioSource(0, audioSource);
		videoPlayer.url = _source;
		videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
		videoPlayer.Prepare ();
		preparing = true;
		videoPlayer.prepareCompleted += VideoPrepared;
		videoPlayer.seekCompleted += SeekComplete;
		videoPlayer.loopPointReached += VideoComplete;

		StartCoroutine (DoOverlay ());
	}

	IEnumerator DoOverlay(){
		EaseCurve.Instance.Vec3 (leftEnableControl, leftEnableControl.localPosition, leftEnablerControlsOffPos, 0.3f, 0, EaseCurve.Instance.easeIn, null, "local");
		EaseCurve.Instance.Vec3 (rightEnableControl, rightEnableControl.localPosition, rightEnablerControlsOffPos, 0.3f, 0, EaseCurve.Instance.easeIn, null, "local");
		EaseCurve.Instance.MatColor (overlayBg.material, new Color32 (0, 0, 0, 0), new Color32 (0, 0, 0, 255), 1f, 0, EaseCurve.Instance.linear);
		yield return new WaitForSeconds (1f);
		videoPlayer.transform.localPosition = Vector3.zero;
		videoPlayer.gameObject.SetActive (true);
		PlayPause ();
		if (controlsLeft) {
			controls.localPosition = new Vector3 (-(GridManagerOrtho.Instance.desiredFullScreenAspect.x / 2) + 1.5f, 0, 0);
		} else {
			controls.localPosition = new Vector3 ((GridManagerOrtho.Instance.desiredFullScreenAspect.x / 2) - 1.5f, 0, 0);
		}
		ShowControls ();
	}

	public void SeekTo(float _pct){
		videoPlayer.frame = (long)(videoPlayer.frameCount * _pct);
	}

	public void SkipBy(float _sec, bool _fwd = true){
		float skipTime = _fwd ? _sec : -_sec;
		videoPlayer.time = videoPlayer.time + skipTime;
	}

	public void SetVolume(float _pct){
		audioSource.volume = _pct;
		if (audioSource.volume < 0.1f) {
			audioSource.volume = 0;
			soundOn.SetActive (false);
			soundOff.SetActive (true);
		} else {
			soundOn.SetActive (true);
			soundOff.SetActive (false);
		}
	}

	void VideoPrepared(VideoPlayer _player){
		totalTime = _player.frameCount / _player.frameRate;
		System.TimeSpan VideoUrlLength = System.TimeSpan.FromSeconds (totalTime);
		string minutes = (VideoUrlLength.Minutes).ToString ("00");
		string seconds = (VideoUrlLength.Seconds).ToString ("00");
		totalTimeStr = minutes + ":" + seconds;
		preparing = false;
	}

	void SeekComplete(VideoPlayer _player){
		//Debug.Log ("seeked to: " + _player.frame);
	}

	void VideoComplete(VideoPlayer _player){
		StartCoroutine (DoClose ());
	}

	IEnumerator DoClose(){
		preparing = true;
		videoPlayer.Stop ();
		//Destroy (videoPlayer.gameObject);
		//Destroy (controls.gameObject);
		videoPlayer.gameObject.SetActive (false);
		controls.gameObject.SetActive (false);
		//IdleStateController.Instance.activeTransitionLoop = true;
		EaseCurve.Instance.MatColor (overlayBg.material, new Color32 (0, 0, 0, 255), new Color32 (0, 0, 0, 0), 1f, 0, EaseCurve.Instance.linear);
		yield return new WaitForSeconds (1f);
		videoPlayerParent.SetActive (false);
		leftEnabler.localPosition = Vector3.zero;
		rightEnabler.localPosition = Vector3.zero;
		//Destroy (gameObject);
	}

	void VideoTime(){
		System.TimeSpan VideoUrlLength = System.TimeSpan.FromSeconds(videoPlayer.time);
		string minutes = (VideoUrlLength.Minutes).ToString("00");
		string seconds = (VideoUrlLength.Seconds).ToString("00");
		timeStr = minutes + ":" + seconds;

		timeTxt.text = timeStr;
		durationTxt.text = totalTimeStr;
	}

	void VideoBar(){
		timePct = (float)videoPlayer.frame / (float)videoPlayer.frameCount;
		seekFg.size = new Vector2 (seekFg.size.y + (seekBg.size.x * timePct), seekFg.size.y);
		seekHandle.localPosition = new Vector3 ((seekFg.transform.localPosition.x - (seekFg.size.y * seekFg.transform.localScale.x)) + (seekFg.size.x * seekFg.transform.localScale.x),
												 seekFg.transform.localPosition.y, 
												 seekHandle.localPosition.z);
	}

	void VolumeBar(){
		volFg.size = new Vector2 (volFg.size.y + (volBg.size.x * audioSource.volume), volFg.size.y);
		volHandle.localPosition = new Vector3 ((volFg.transform.localPosition.x - (volFg.size.y * volFg.transform.localScale.x)) + (volFg.size.x * volFg.transform.localScale.x),
												volFg.transform.localPosition.y, 
												volHandle.localPosition.z);
	}

	void Update(){
		if (!preparing) {
			VideoTime ();
			VideoBar ();
			VolumeBar ();
			if (controlCurrIdle >= controlsIdleTime) {
				controls.gameObject.SetActive (false);
			} else {
				controlCurrIdle += Time.deltaTime;
			}
		}
		if (enablerOpen) {
			if (enablerCurrIdle >= enablerIdleTime) {
				closeEnablers ();
			} else {
				enablerCurrIdle += Time.deltaTime;
			}
		}
	}

	void leftEnablerTapTappedHandler(object sender, System.EventArgs e){
		enablerOpen = true;
		enablerCurrIdle = 0;
		leftEnabler.localPosition = leftEnablerOffPos;
		EaseCurve.Instance.Vec3 (leftEnableControl, leftEnableControl.localPosition, Vector3.zero, 0.3f, 0, EaseCurve.Instance.easeIn, null, "local");
	}
	void rightEnablerTapTappedHandler(object sender, System.EventArgs e){
		enablerOpen = true;
		enablerCurrIdle = 0;
		rightEnabler.localPosition = rightEnablerOffPos;
		EaseCurve.Instance.Vec3 (rightEnableControl, rightEnableControl.localPosition, Vector3.zero, 0.3f, 0, EaseCurve.Instance.easeIn, null, "local");
	}

	void leftLoadVideoTappedHandler(object sender, System.EventArgs e){
		controlsLeft = true;
		StartVideo ();
	}
	void rightLoadVideoTappedHandler(object sender, System.EventArgs e){
		controlsLeft = false;
		StartVideo ();
	}
	void StartVideo(){
		StartCoroutine(IdleStateController.Instance.CloseTapToStart (true));
		LoadVideo (AssetManager.Instance.onDemandVideoFile);
	}

	void leftCloseLoadVideoTappedHandler(object sender, System.EventArgs e){
		closeEnablers ();
	}
	void closeEnablers(){
		enablerOpen = false;
		leftEnabler.localPosition = Vector3.zero;
		EaseCurve.Instance.Vec3 (leftEnableControl, leftEnableControl.localPosition, leftEnablerControlsOffPos, 0.3f, 0, EaseCurve.Instance.easeIn, null, "local");
		rightEnabler.localPosition = Vector3.zero;
		EaseCurve.Instance.Vec3 (rightEnableControl, rightEnableControl.localPosition, rightEnablerControlsOffPos, 0.3f, 0, EaseCurve.Instance.easeIn, null, "local");
	}
}
