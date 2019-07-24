using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TouchScript.Gestures;

public class SeedPipeline : MonoBehaviour {

	public VideoPlayer video;
	public List<SeedPipelineBtn> btns = new List<SeedPipelineBtn>();
	private List<TapGesture> btnTaps = new List<TapGesture> ();
	private int currStep = 0;
	private int nextStep = -1;
	private int skipStep = -1;
	bool vidReady = false;
	float rewindSpeed = 7f;
	private bool tOut = false;
	private bool isTransitioning = false;
	private bool playAfterSeek = false;
	private long currSeekFrame = -1;
	private bool waitingOnSeek = false;

	void OnEnable(){
		video.Prepare ();
		video.prepareCompleted += videoReady;
		video.seekCompleted += videoSeeked;
	}

	void OnDisable(){
		video.prepareCompleted -= videoReady;
		video.seekCompleted -= videoSeeked;
	}

	public void Tap(int _id){
		
		//Debug.Log ("!!!"+(vidReady && !video.isPlaying)+" "+_id);
		if (vidReady && !video.isPlaying && !isTransitioning) {
			Debug.Log ("!!! " + _id);
			for (int i = 0; i < btns.Count; i++) {
				if (i == _id) {
					btns [i].Toggle (true);
				} else {
					btns [i].Toggle (false);
				}
			}
			currStep = nextStep;
			nextStep = _id;
			tOut = true;
			//Debug.Log ("   LEAVING " + currStep + " to " + nextStep);
			video.Play ();
			isTransitioning = true;
		}

	}

	private void videoReady(VideoPlayer _source){
		vidReady = true;
		nextStep = -1;
		_source.frame = 0;
		_source.Pause ();
		Tap (0);
	}

	private void videoSeeked(VideoPlayer _source){
		//Debug.Log (" ^ ^ ^ " + waitingOnSeek + "|" + (video.frame == currSeekFrame) + "|" + playAfterSeek);
		if (waitingOnSeek && video.frame == currSeekFrame && playAfterSeek) {
			waitingOnSeek = false;
			playAfterSeek = false;
			currSeekFrame = -1;
			isTransitioning = true;
			_source.Play ();
		}
	}

	private void SeekTo(long _frame, bool _continue){
		//Debug.Log (" > > > " + _frame + " (" + _continue + ")");
		video.Pause ();
		isTransitioning = false;
		waitingOnSeek = true;
		video.frame = currSeekFrame = _frame;
		playAfterSeek = _continue;
	}

	void Update () {
		if (isTransitioning) {
			//Debug.Log (video.frame + "/" + video.frameCount + " || " + video.time);
			if (currStep == 0 && tOut && video.frame >= 150) { //out of 0
				//Debug.Log ("      OUT of 0");
				tOut = false;
				if (nextStep == 2) {
					//Debug.Log ("        SKIP to 2");
					SeekTo (300, true);
				} else {
					//Debug.Log ("        STAY for 1");
				}
			}

			if (currStep == 1 && tOut && video.frame >= 300) { //out of 1
				Debug.Log ("      OUT of 1");
				tOut = false;
				if (nextStep == 0) {
					//Debug.Log ("        SKIP to 0");
					SeekTo (0, true);
				} else {
					//Debug.Log ("        STAY for 1");
				}
			}

			if (currStep == 2 && tOut &&  video.frame >= 450) { //out of 2
				Debug.Log ("      OUT of 2");
				tOut = false;
				if (nextStep == 1) {
					//Debug.Log ("        SKIP to 1");
					SeekTo (150, true);
				} else {
					//Debug.Log ("        STAY for 0");
				}
			}


			if (nextStep == 0 && video.frame >= 125 && video.frame <= 270) {
				//Debug.Log ("STOP at 0");
				tOut = false;
				SeekTo (135, false);
			}
			if (nextStep == 0 && video.frame >= 450) {
				//Debug.Log ("JUMP to Start");
				SeekTo (0, true);
			}

			if (nextStep == 1 && video.frame >= 240 && video.frame <= 400) {
				//Debug.Log ("STOP at 1");
				tOut = false;
				SeekTo (280, false);
			}
			if (nextStep == 2 && video.frame >= 355) {
				//Debug.Log ("STOP at 2");
				tOut = false;
				SeekTo (430, false);
			}





			/*
			if (nextStep == 0 && video.frame >= 125 && video.frame <= 270) {
				video.Pause ();
				video.frame = 135;
				Debug.Log ("STOP 0");
			}
			if (skipStep == 0 && video.frame >= 125 && video.frame <= 300) {
				video.frame = 135;
				skipStep = -1;
				video.playbackSpeed = 1f;
			}

			if (nextStep == 1 && video.frame >= 240 && video.frame <= 400) {
				video.Pause ();
				video.frame = 280;
			}
			if (skipStep == 1 && video.frame >= 240 && video.frame <= 400) {
				video.frame = 280;
				skipStep = -1;
				video.playbackSpeed = 1f;
			}

			if (nextStep == 2 && video.frame >= 355) {
				video.Pause ();
				video.frame = 430;
			}
			if (skipStep == 2 && video.frame >= 355) {
				video.frame = 430;
				skipStep = -1;
				video.playbackSpeed = 1f;
			}

			if (nextStep == 0 && video.frame >= 445) {
				video.frame = 0;
			}

			if (nextStep == 1 && video.frame >= 445) {
				video.frame = 0;
			}
			*/
		}
	}
}

/*
public VideoPlayer video;
public List<SeedPipelineBtn> btns = new List<SeedPipelineBtn>();
private List<TapGesture> btnTaps = new List<TapGesture> ();
private int currStep = 0;
private int nextStep = -1;
private int skipStep = -1;
bool vidReady = false;
float rewindSpeed = 7f;

void OnEnable(){
	video.Prepare ();
	video.prepareCompleted += videoReady;
}

void OnDisable(){
	video.prepareCompleted -= videoReady;
}

public void Tap(int _id){
	//Debug.Log ("!!!"+(vidReady && !video.isPlaying)+" "+_id);
	if (vidReady && !video.isPlaying) {
		for (int i = 0; i < btns.Count; i++) {
			if (i == _id) {
				btns [i].Toggle (true);
			} else {
				btns [i].Toggle (false);
			}
		}
		currStep = nextStep;
		if ((currStep == 0 && _id == 2) || (currStep == 1 && _id == 0) || (currStep == 2 && _id == 1)) {
			video.playbackSpeed = 2f;
			if (currStep == 0)
				skipStep = 1;
			if (currStep == 1)
				skipStep = 2;
			if (currStep == 2)
				skipStep = 0;
		} else {
			video.playbackSpeed = 1f;
		}
		nextStep = _id;
		video.Play ();
	}

}

private void videoReady(VideoPlayer _source){
	vidReady = true;
	nextStep = -1;
	_source.frame = 0;
	_source.Pause ();
	Tap (0);
}

void Update () {
	if (video.isPlaying) {
		//Debug.Log ("\t" + video.frame + "/" + video.frameCount + " || " + video.time);
		if (nextStep == 0 && video.frame >= 125 && video.frame <= 270) {
			video.Pause ();
			video.frame = 135;
			Debug.Log ("STOP 0");
		}
		if (skipStep == 0 && video.frame >= 125 && video.frame <= 300) {
			video.frame = 135;
			skipStep = -1;
			video.playbackSpeed = 1f;
		}

		if (nextStep == 1 && video.frame >= 240 && video.frame <= 400) {
			video.Pause ();
			video.frame = 280;
		}
		if (skipStep == 1 && video.frame >= 240 && video.frame <= 400) {
			video.frame = 280;
			skipStep = -1;
			video.playbackSpeed = 1f;
		}

		if (nextStep == 2 && video.frame >= 355) {
			video.Pause ();
			video.frame = 430;
		}
		if (skipStep == 2 && video.frame >= 355) {
			video.frame = 430;
			skipStep = -1;
			video.playbackSpeed = 1f;
		}

		if (nextStep == 0 && video.frame >= 445) {
			video.frame = 0;
		}

		if (nextStep == 1 && video.frame >= 445) {
			video.frame = 0;
		}
	}
}
*/