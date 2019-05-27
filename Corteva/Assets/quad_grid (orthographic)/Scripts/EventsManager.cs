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

	//subscribe/unsibscribe to events
	//EventsManager.Instance.OnAssetsFinishedLoading += assetsLoadedHandler;
	//EventsManager.Instance.OnAssetsFinishedLoading -= assetsLoadedHandler;
	//assetsLoadedHandler(){}

	//dispatch event
	//EventsManager.Instance.AssetsFinishedLoading();

	public delegate void AssetsFinishedLoadingEvent();
	public event AssetsFinishedLoadingEvent OnAssetsFinishedLoading;
	public void AssetsFinishedLoading(){ OnAssetsFinishedLoading (); }

	public delegate void SceneFinishedLoadingEvent(string _scene);
	public event SceneFinishedLoadingEvent OnSceneFinishedLoading;
	public void SceneFinishedLoading(string _scene){ OnSceneFinishedLoading (_scene); }

	public delegate void UserKioskOpenRequestEvent(Vector2 _gridPos, Vector2 _screenPos, Environment _env = null, Transform _panel = null);
	public event UserKioskOpenRequestEvent OnUserKioskOpenRequest;
	public void UserKioskOpenRequest(Vector2 _gridPos, Vector2 _screenPos, Environment _env = null, Transform _panel = null){ OnUserKioskOpenRequest (_gridPos, _screenPos, _env, _panel); }

	public delegate void UserKioskCloseRequestEvent(Vector2 _gridPos, bool _closeImmediately);
	public event UserKioskCloseRequestEvent OnUserKioskCloseRequest;
	public void UserKioskCloseRequest(Vector2 _gridPos, bool _closeImmediately){ OnUserKioskCloseRequest (_gridPos, _closeImmediately); }

	public delegate void UserKioskActivatePanelInGrid();
	public event UserKioskActivatePanelInGrid OnUserKioskActivatePanelInGrid;
	public void UserKioskActivatePanelInGridRequest(){ OnUserKioskActivatePanelInGrid (); }

	public delegate void EnvironmentSwitch(int _env);
	public event EnvironmentSwitch OnEnvironmentSwitch;
	public void EnvironmentSwitchRequest(int _env){ OnEnvironmentSwitch (_env); }

	public delegate void ClearEverything();
	public event ClearEverything OnClearEverything;
	public void ClearEverythingRequest(){ OnClearEverything (); }
}
