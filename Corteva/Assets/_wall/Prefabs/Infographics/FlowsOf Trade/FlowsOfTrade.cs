using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class FlowsOfTrade : MonoBehaviour {

	public GameObject blocker;
	public List<VideoPlayer> videos = new List<VideoPlayer>();
	public List<FlowsOfTradeBtn> btns = new List<FlowsOfTradeBtn> ();
	private int currId = 0;
	private bool first = true;

	void OnEnable(){
		foreach (VideoPlayer v in videos) {
			v.Prepare ();
			v.prepareCompleted += videoReady;
		}
	}

	IEnumerator hideBlocker(){
		yield return new WaitForSeconds(0.6f);
		blocker.SetActive(false);
	}

	void OnDisable(){
		first = true;
		blocker.SetActive(true);
		foreach (VideoPlayer v in videos) {
			v.prepareCompleted -= videoReady;
		}
	}

	private void videoReady(VideoPlayer _source){
		_source.Play ();
		if (_source.clip.name == videos [currId].clip.name) {
			_source.frame = 0;
			btns [currId].ToggleButton (true);
		} else {
			_source.frame = 0;
		}
		if (first) {
			StartCoroutine(hideBlocker());
			first = false;
		}
	}

	public void PlayClip(int _id){
		if (_id != currId) {
			for (int i = 0; i < videos.Count; i++) {
				if (i == _id) {
					currId = _id;
					btns [i].ToggleButton (true);
					videos [i].frame = 0;
					videos [i].transform.localPosition = Vector3.zero;
				} else {
					btns [i].ToggleButton (false);
					videos [i].frame = 0;
					videos [i].transform.localPosition = Vector3.zero + Vector3.forward * 0.01f;
				}
			}
		}
	}
}
