using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TouchScript.Gestures;

public class SeedPipeline : MonoBehaviour {

	public VideoPlayer video;
	public List<SeedPipelineBtn> btns = new List<SeedPipelineBtn>();
	private List<TapGesture> btnTaps = new List<TapGesture> ();
	private int step = 0;
	bool vidReady = false;
	bool rewind = false;
	float rewindSpeed = 7f;

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
		if (vidReady && !video.isPlaying && !rewind) {
			if (Mathf.Abs (_id - step) == 1) {
				for (int i = 0; i < btns.Count; i++) {
					if (i == _id) {
						btns [i].Toggle (true);
					} else {
						btns [i].Toggle (false);
					}
				}
				if (_id > step) {
					step = _id;
					video.Play ();
				}
				if (_id < step) {
					step = _id;
					rewind = true;
					video.time -= Time.deltaTime * rewindSpeed;
				}
			}
		}
	}

	private void videoReady(){
		vidReady = true;
		step = -1;
		video.frame = 0;
		video.Pause ();
		Tap (0);
	}

	private void videoSeeked(VideoPlayer _source){
		if (rewind) {
			video.time -= Time.deltaTime * rewindSpeed;
			//video.frame--;
			//Debug.Log ("<<<<<: " + video.frame + "/" + video.frameCount + " || " + video.time);
			if (step == 1 && video.frame <= 260) {
				//video.Pause ();
				video.frame = 270;
				rewind = false;
				//step--;
				//Debug.Log ("rewound to step 1");
			}

			if (step == 0 && video.frame <= 135) {
				//video.Pause ();
				video.frame = 135;
				rewind = false;
				//step--;
				//Debug.Log ("rewound to step 0");
			}
		}
	}

	void Update () {
		if (video.isPlaying) {
			//Debug.Log (">>>>>: " + video.frame + "/" + video.frameCount + " || " + video.time);
			if (step == 0 && video.frame >= 135) {
				video.Pause ();
				video.frame = 135;
				//step++;
				//Debug.Log ("end step 0");
			}

			if (step == 1 && video.frame >= 260) {
				video.Pause ();
				video.frame = 260;
				//step++;
				//Debug.Log ("end step 1");
			}

			if (step == 2 && video.frame >= 370) {
				video.Pause ();
				video.frame = 370;
				//step++;
				//Debug.Log ("end step 2");
			}
		}
	}
}
