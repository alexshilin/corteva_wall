using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventsManager : MonoBehaviour {

	private static EventsManager _instance;
	public static EventsManager Instance { get { return _instance; } }
	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		} else {
			_instance = this;
		}
	}

	//subscribe/unsibscript to events
	//EventsManager.Instance.OnAssetsFinishedLoading += assetsLoaded;
	//EventsManager.Instance.OnAssetsFinishedLoading -= assetsLoaded;

	public delegate void AssetsFinishedLoadingEvent();
	public event AssetsFinishedLoadingEvent OnAssetsFinishedLoading;
	public void AssetsFinishedLoading(){ OnAssetsFinishedLoading (); }

	public delegate void SceneFinishedLoadingEvent(string _scene);
	public event SceneFinishedLoadingEvent OnSceneFinishedLoading;
	public void SceneFinishedLoading(string _scene){ OnSceneFinishedLoading (_scene); }

	public delegate void UserKioskRequestEvent(Vector2 _gridPos, bool _doOpen);
	public event UserKioskRequestEvent OnUserKioskRequest;
	public void UserKioskRequest(Vector2 _gridPos, bool _doOpen){ OnUserKioskRequest (_gridPos, _doOpen); }
}
