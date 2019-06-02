using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class FoodWaste : MonoBehaviour {

	public GameObject blocker;
	public List<VideoPlayer> videos = new List<VideoPlayer>();
	public List<FoodWasteBtn> btns = new List<FoodWasteBtn> ();
	public List<GameObject> infos = new List<GameObject> ();
	private int currId = 0;
	private bool first = true;
	private int level = 0;

	void OnEnable(){
		foreach (VideoPlayer v in videos) {
			v.Prepare ();
			v.prepareCompleted += videoReady;
		}
		UpdateLevel (0);
	}

	IEnumerator hideBlocker(){
		yield return new WaitForSeconds(0.6f);
		blocker.SetActive(false);
	}

	void OnDisable(){
		first = true;
		level = 0;
		blocker.SetActive(true);
		foreach (VideoPlayer v in videos) {
			v.prepareCompleted -= videoReady;
		}
	}

	private void videoReady(VideoPlayer _source){
		_source.Play ();
		if (first) {
			StartCoroutine(hideBlocker());
			first = false;
		}
	}

	public void UpdateLevel(int _amt){
		level += _amt;
		for (int i = 0; i < videos.Count; i++) {
			if (i == level) {
				videos [i].frame = 0;
				videos [i].transform.localPosition = Vector3.zero;
			} else {
				videos [i].frame = 0;
				videos [i].transform.localPosition = Vector3.zero + Vector3.forward * 0.01f;
			}
		}
	}

	public void UpdateCard(int _id){
		for (int i = 0; i < infos.Count; i++) {
			if (infos [i] != null) {
				infos [i].SetActive (false);
			}
		}
		if(_id!=0)
			infos [_id].SetActive (true);
	}
}
