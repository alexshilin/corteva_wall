﻿using System.Collections;
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

	public GameObject admin;
	private bool adminOn = true;
	private bool dualDisplayMode = false;

	public Vector2 screenActualPx;
	public Vector3 screenCalcIn;
	public float detectedDPI;

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
//		Resolution[] resolutions = Screen.resolutions;
//		Log ("available resolutions: ");
//		foreach (var res in resolutions){
//			Log("  "+res.width + "x" + res.height + " : " + res.refreshRate+"hz");
//			hasAspect(new Vector2((float)res.width, (float)res.height), new Vector2(16f, 9f));
//		}
//		if (Display.displays.Length > 1) {
//			Log("Multiple displays detected ["+Display.displays.Length+"]");
//			Vector2 multiDisplayRes = Vector2.zero;
//			for(int i=0; i<Display.displays.Length; i++) {
//				Display.displays[i].Activate ();
//				multiDisplayRes.x += (float)Display.displays[i].renderingWidth;
//				multiDisplayRes.y += (float)Display.displays[i].renderingHeight;
//				Log("\tDisplay "+i+" ["+Display.displays[i].renderingWidth+" x "+Display.displays[i].renderingHeight+"] "+getAspect (new Vector2 ((float)Display.displays[i].renderingWidth, (float)Display.displays[i].renderingHeight)));
//			}
//			Log("\t\tTotal ["+multiDisplayRes.x+" x "+multiDisplayRes.y+"] "+getAspect (new Vector2 (multiDisplayRes.x, multiDisplayRes.y)));
//			screenActualPx = multiDisplayRes;
//			if (dualDisplayMode) {
//				Log ("Dual display mode actve");
//				screenActualPx = multiDisplayRes;
//			} else {
//				Log ("Dual display mode inactve, must be turned on manually.");
//			}
//		} else {
//			Log ("Single display");
//			screenActualPx = new Vector2 ((float)Screen.width, (float)Screen.height);
//		}
//		Log("\tTotal resolution ["+screenActualPx.x+" x "+screenActualPx.y+"] "+getAspect (new Vector2 (screenActualPx.x, screenActualPx.y)));
		screenActualPx = new Vector2 ((float)Screen.width, (float)Screen.height);
		Log ("current resolution: " + screenActualPx);
		string aspect = getAspect (new Vector2 ((float)Screen.width, (float)Screen.height));
		Log ("current aspect: " + aspect +" | "+AssetManager.Instance.mainCamera.aspect);
		detectedDPI = Screen.dpi;
		Log ("DPI: " + detectedDPI);
		//QualitySettings.vSyncCount = 0;
		Log ("vSync: " + QualitySettings.vSyncCount);

		Log ("graphicsDeviceType: " + SystemInfo.graphicsDeviceType);
		Log ("graphicsDeviceName: " + SystemInfo.graphicsDeviceName);
		Log ("graphicsDeviceID: " + SystemInfo.graphicsDeviceID);

//		Log ("displays detected: " + Display.displays.Length);
//		foreach (Display d in Display.displays) {
//				d.Activate ();
//				Log ("\t"+ d.systemWidth + ", " + d.systemHeight + " | " + d.renderingWidth + ", " + d.renderingHeight);
//		}


		AssetManager.Instance.LoadAssets ();

		//logText.text = "...";

		//ToggleAdmin ();

		//move user cameras to top touch layers
		//		int currentLayer = 0;
		//		foreach (TouchScript.Layers.TouchLayer l in TouchScript.LayerManager.Instance.Layers) {
		//			Debug.Log ("layer: " + l.Name);
		//			if (l.Name == "UserCam") {
		//				Debug.Log ("moving " + l.Name + "from layer: " + currentLayer + " to " + 0);
		//				TouchScript.LayerManager.Instance.ChangeLayerIndex (currentLayer, 0);
		//			}
		//			currentLayer++;
		//		}
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
	}



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
		if (adminOn) {
			admin.SetActive (false);
			adminOn = false;
		}else{
			admin.SetActive (true);
			adminOn = true;
		}
	}

	public void Log(string _txt){
		Debug.Log (_txt);
		logText.text += "\n"+_txt;
	}
}
