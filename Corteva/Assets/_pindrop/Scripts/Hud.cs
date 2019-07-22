using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hud : MonoBehaviour {

	public Text widthText;
	public Text heightText;
	public Text logText;

	public GameObject admin;
	private bool adminOn = true;

	public Vector2 screenActualPx;
	public Vector3 screenCalcIn;
	public float detectedDPI;

	private static Hud _instance;
	public static Hud Instance { get { return _instance; } }
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

		detectedDPI = Screen.dpi;
		Log ("DPI: " + detectedDPI);

		//QualitySettings.vSyncCount = 0;
		Log ("vSync: " + QualitySettings.vSyncCount);

		Log ("graphicsDeviceType: " + SystemInfo.graphicsDeviceType);
		Log ("graphicsDeviceName: " + SystemInfo.graphicsDeviceName);
		Log ("graphicsDeviceID: " + SystemInfo.graphicsDeviceID);

		ToggleAdmin ();
	}

	void Update () {
		screenActualPx = new Vector2 (Screen.width, Screen.height);
		screenCalcIn = new Vector2 (screenActualPx.x / detectedDPI, screenActualPx.y / detectedDPI);
		widthText.text = screenActualPx.x.ToString()+"px";
		//widthText.text += "\n"+screenCalcIn.x.ToString()+"in";
		heightText.text = screenActualPx.y.ToString()+"px";
		//heightText.text += "\n"+screenCalcIn.y.ToString()+"in";


		if (Input.GetKeyDown (KeyCode.M)) {
			ToggleAdmin ();
		}
	}

	public void ToggleAdmin(){
		//logText.gameObject.SetActive (false);
		admin.SetActive (!admin.activeSelf);
		logText.gameObject.SetActive (!logText.gameObject.activeSelf);
		adminOn = !adminOn;
	}

	public void Log(string _txt){
		Debug.Log (_txt);
		logText.text += "\n"+_txt;
	}
}
