using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenManager : MonoBehaviour {

	public Text widthText;
	public Text heightText;
	public Text logText;
	public Text setSingleScreenWidthText;
	public Text setSingleScreenheightText;
	public Text setGridXText;
	public Text setGridYText;
	public Text idleTimerText;

	public GameObject admin;
	private bool adminOn = true;
	private bool dualDisplayMode = false;

	public Vector2 screenActualPx;
	public Vector3 screenCalcIn;
	public float detectedDPI;
	public enum Aspect{ is169, is329 }
	public Aspect currAspect = Aspect.is169;

	private static ScreenManager _instance;
	public static ScreenManager Instance { get { return _instance; } }
	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		} else {
			_instance = this;
		}
	}

	void Start () {
		Debug.Log ("ScreenManager [Start]");
		screenActualPx = new Vector2 ((float)Screen.width, (float)Screen.height);
		Log ("current resolution: " + screenActualPx);

		float camAspect = AssetManager.Instance.mainCamera.aspect;
		Log ("cam aspect: " + camAspect);
		if (camAspect > 1.7f && camAspect < 1.8f) {
			currAspect = Aspect.is169;
		}
		if (camAspect > 3.5 && camAspect < 3.6f) {
			currAspect = Aspect.is329;
		}
		Log ("current aspect: " + currAspect);

		detectedDPI = Screen.dpi;
		Log ("DPI: " + detectedDPI);

		//QualitySettings.vSyncCount = 0;
		Log ("vSync: " + QualitySettings.vSyncCount);

		Log ("graphicsDeviceType: " + SystemInfo.graphicsDeviceType);
		Log ("graphicsDeviceName: " + SystemInfo.graphicsDeviceName);
		Log ("graphicsDeviceID: " + SystemInfo.graphicsDeviceID);

		GridManagerOrtho.Instance.Init ();
	}

	string getAspect (Vector2 _resolution){
		float aspect = (_resolution.x / _resolution.y);
		string a = aspect.ToString("0.0");
		if (a == "1.8") {
			a = "16:9";
		}else if (a == "3.5") {
			a = "32:9";
		}
		return a;
	}

	void Update () {
		screenActualPx = new Vector2 (Screen.width, Screen.height);
		screenCalcIn = new Vector2 (screenActualPx.x / detectedDPI, screenActualPx.y / detectedDPI);
		widthText.text = screenActualPx.x.ToString()+"px";
		widthText.text += "\n"+screenCalcIn.x.ToString()+"in";
		heightText.text = screenActualPx.y.ToString()+"px";
		heightText.text += "\n"+screenCalcIn.y.ToString()+"in";


		if (Input.GetKeyDown (KeyCode.M)) {
			ToggleAdmin ();
		}

		if (Input.GetKeyDown (KeyCode.L)) {
			logText.gameObject.SetActive (!logText.gameObject.activeSelf);
		}


		//toggle fullscreen
		if (Input.GetKeyDown (KeyCode.F)) {
			Screen.fullScreen = !Screen.fullScreen;
		}

		//switch to windowed and resize to 16:9 aspect resolution
		if (Input.GetKeyDown (KeyCode.Alpha6)) {
			Screen.fullScreen = false;
			int newHeight = Mathf.RoundToInt((float)Screen.width / (16f/9f));
			Screen.SetResolution (Screen.width, newHeight, false);

			if (currAspect != Aspect.is169) {
				EventsManager.Instance.ClearEverythingRequest ();
				currAspect = Aspect.is169;
			}

		}

		//switch to windowed and resize to 32:9 aspect resolution
		if (Input.GetKeyDown (KeyCode.Alpha7)) {
			Screen.fullScreen = !Screen.fullScreen;
			int newHeight = Mathf.RoundToInt((float)Screen.width / (32f/9f));
			Screen.SetResolution (Screen.width, newHeight, false);

			if (currAspect != Aspect.is329) {
				EventsManager.Instance.ClearEverythingRequest ();
				currAspect = Aspect.is329;
			}
		}
	}


	//this method is called from a UI button
	public void SetResolutionByScreens(){
		string w = setSingleScreenWidthText.text;
		string h = setSingleScreenheightText.text;
		string x = setGridXText.text;
		string y = setGridYText.text;
		if (w != "" && h != "" && x != "" && y != "") {
			Screen.SetResolution (int.Parse (w), int.Parse (h), false);
			Vector2 setRes = new Vector2 (int.Parse (w), int.Parse (h));
			Vector2 setGrd = new Vector2 (int.Parse (x), int.Parse (y));
			logText.text += "\n" + "setting app size to: " + w + " x " + h + " width a " + x + " x " + y + " grid";
		} else {
			Screen.SetResolution (3840, 2160, false);
		}
	}

	public void MoveToLayer(Transform root, int layer) {
		root.gameObject.layer = layer;
		foreach(Transform child in root)
			MoveToLayer(child, layer);
	}

	public void ToggleAdmin(){
		logText.gameObject.SetActive (false);
		admin.SetActive (!admin.activeSelf);
		adminOn = !adminOn;
	}

	public void Log(string _txt){
		Debug.Log (_txt);
		logText.text += "\n"+_txt;
	}
}
