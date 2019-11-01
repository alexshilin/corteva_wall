using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures.TransformGestures;
using TouchScript.Gestures;
using System;
using SimpleJSON;

public class PinDropEarth : MonoBehaviour {

	public PinDrop PD;
	public Transform earthSphere;
	public Camera cam;
	public Transform pinContainer;
	public Transform pin;

	private bool spinning = false;
	private float spinSpeed = 3f;
	private bool flicking = true;
	private float spinVelocity = 0f;
	private Vector3 spinAxis = Vector3.up;
	private float spinDamp = 0.9f;

	private RaycastHit hit;
	private Texture2D tex;

	private JSONNode pins;

	public TransformGesture transformGesture;
	public TransformGesture twoFingerTransformGesture;
	private FlickGesture flickGesture;
	private TapGesture tapGesture;
	private PressGesture pressGesture;

	private bool twoFingerRotate = false;

	public GameObject newUserPin;

    public float maxTimeToWait = 5f;
	public float timeWaited = 0f;
	public bool idling = true;


	void Awake () {
		tex = earthSphere.GetComponent<Renderer> ().material.GetTexture ("_CloudAndNightTex") as Texture2D;
		spinning = true;

		if (GetComponentInParent<Camera> ()) {
			cam = GetComponentInParent<Camera> ();
		}
	}

	void Start(){
		//Invoke("LoadPins", 1f);
		StartCoroutine(LoadPins());
	}

	void OnEnable(){
		transformGesture.AddFriendlyGesture (twoFingerTransformGesture);

		flickGesture = GetComponent<FlickGesture> ();
		tapGesture = GetComponent<TapGesture> ();
		pressGesture = GetComponent<PressGesture> ();

		transformGesture.TransformStarted += transformStartedHandler;
		transformGesture.Transformed += transformedHandler;
		transformGesture.TransformCompleted += transformCompletedHandler;

		twoFingerTransformGesture.TransformStarted += twoFingerTransformStartHandler;
		twoFingerTransformGesture.Transformed += twoFingerTransformHandler;
		twoFingerTransformGesture.TransformCompleted += twoFingerTransformEndHandler;

		flickGesture.Flicked += flickedHandler;

		tapGesture.Tapped += tapHandler;

		pressGesture.Pressed += pressedHandler;
	}

	void OnDisable(){

		transformGesture.TransformStarted -= transformStartedHandler;
		transformGesture.Transformed -= transformedHandler;
		transformGesture.TransformCompleted -= transformCompletedHandler;

		twoFingerTransformGesture.TransformStarted -= twoFingerTransformStartHandler;
		twoFingerTransformGesture.Transformed -= twoFingerTransformHandler;
		twoFingerTransformGesture.TransformCompleted -= twoFingerTransformEndHandler;

		flickGesture.Flicked -= flickedHandler;

		tapGesture.Tapped -= tapHandler;

		pressGesture.Pressed += pressedHandler;
	}

	public IEnumerator LoadPins(){
		yield return new WaitForSeconds (0.5f);
		pins = PinData.Instance.pinData;

		Debug.Log ("[PinDropEarth] " + pins ["pins"].Count+" pins"); 

		Dictionary<string, string> lookup = new Dictionary<string, string> ();
		for (int i = 0; i < pins ["roles"].Count; i++) {
			lookup.Add (pins ["roles"] [i] ["machine_name"], pins ["roles"] [i] ["title"]);
		}
		for (int i = 0; i < pins ["challenges"].Count; i++) {
			lookup.Add (pins ["challenges"] [i] ["machine_name"], pins ["challenges"] [i] ["title"]);
		}

		//int maxPins = pins ["pins"].Count > 100 ? 100 : pins ["pins"].Count;
		int maxPins = pins["pins"].Count;
		for (int i = 0; i < maxPins; i++) { 
			//Debug.Log (i+": "+pins ["pins"] [i] ["role"]+" "+pins ["pins"] [i] ["challenge"]);

			//NOTE: do a check to make sure both role and chellenge are not blank

			string role = lookup [pins ["pins"] [i] ["role"]];
			string chal = lookup [pins ["pins"] [i] ["challenge"]];
			string txt = "<b>" + chal + "</b><br>" + role;
			PlacePin (new Vector2(pins ["pins"][i]["lat"].AsFloat, pins ["pins"][i]["lon"].AsFloat), txt);
		}

		pinContainer.localPosition = Vector3.zero;
	}

	public Vector2 XYZtoLatLon(Vector3 _v3){
		float r = Mathf.Sqrt (_v3.x * _v3.x + _v3.y * _v3.y + _v3.z * _v3.z); 
		float lat = Mathf.Asin (_v3.z / r) * Mathf.Rad2Deg;
		float lon = Mathf.Atan2 (_v3.y, _v3.x) * Mathf.Rad2Deg * -1;
		Debug.Log ("[XYZtoLatLong] ("+_v3+") > ("+ lat + ", " + lon +" )");
		LatLonToXYZ (new Vector2 (lat, lon));
		return new Vector2 (lat, lon);
	}


	public Vector3 LatLonToXYZ(Vector2 _latlon){
		float earthRadius = 6367; 
		float radius = earthSphere.localScale.x * 0.5f;
		float lat = _latlon.x * Mathf.Deg2Rad;
		float lon = _latlon.y * Mathf.Deg2Rad;
		float xPos = earthRadius * Mathf.Cos (lat) * Mathf.Cos (lon);
		float yPos = earthRadius * Mathf.Cos (lat) * Mathf.Sin (lon);
		float zPos = earthRadius * Mathf.Sin (lat);
		Vector3 pinPos = new Vector3 (xPos, zPos, yPos).normalized * radius;
		//pin.localPosition = pinPos;
		//pin.rotation = Quaternion.LookRotation((pin.position - earthSphere.position), earthSphere.forward);
		//Debug.Log ("[LatLonToXYZ] (" + _lat+", "+ _lon+") > (" + pinPos +")"+" | ("+pin.position+")");
		return pinPos;
	}


	bool IsItLand(Vector2 texCoord){
		bool isLand = false;
		Vector2 pixelPos = texCoord;

		Debug.Log ("[LandOrWater] (" + pixelPos + ") | (" + tex.width + ", " + tex.height + ")");

		pixelPos.x *= tex.width;
		pixelPos.y *= tex.height;

		//adjust for texture offset
		float textureOffset = (tex.width * earthSphere.GetComponent<Renderer> ().material.GetTextureOffset("_CloudAndNightTex").x);
		//Debug.Log (tex.width + " (" + pixelPos.x + " + "+textureOffset+") = " + (pixelPos.x + textureOffset));
		pixelPos.x = pixelPos.x + textureOffset;
		if (pixelPos.x < 0)
			pixelPos.x = tex.width + pixelPos.x;

		Color pixCol = tex.GetPixel ((int)pixelPos.x, (int)pixelPos.y);
		float colAverage = (pixCol.r + pixCol.g + pixCol.b) / 3f;
		//Debug.Log (texCoord+" > "+pixelPos+" > "+pixCol+" ("+colAverage+")");
		if (colAverage < 0.9f) {
			isLand = true;
		}
		Debug.Log ("[LandOrWater] " + (isLand ? "land" : "water"));
		return isLand;

	}

	void PlacePin(Vector2 _latlon, string _text, bool _newUser = false){
		if (newUserPin != null) {
			Destroy (newUserPin);
			//PD.menu.instruct.text = "Tap to drop a pin in your home location";
		}
		GameObject p = Instantiate (pin.gameObject, pinContainer);
		p.transform.localPosition = LatLonToXYZ (_latlon);

        //Debug.Log("place pin ");
        //hideOtherPins(p.transform);
        //p.transform.localPosition = new Vector3(Mathf.Round(LatLonToXYZ(_latlon).x * 50f) / 50f, LatLonToXYZ(_latlon).y, Mathf.Round(LatLonToXYZ(_latlon).z * 50f) / 50f);

		p.GetComponent<Pin> ().SetPinText (_text);
		p.GetComponent<Pin> ().latLon = _latlon;
        p.GetComponent<Pin>().info.gameObject.SetActive(true);
        PD.menu.undecidedPin = p;


		if (_newUser) {
			p.GetComponent<Pin> ().SetConfirm ();
			p.GetComponent<Pin> ().SetPinColor (new Color32 (252, 76, 2, 255));
			p.GetComponent<Pin> ().baseSize *= 2f;
			newUserPin = p;
			//PD.menu.ToggleWelcome (0);
			PD.menu.icons.SetActive (false);
			PD.menu.instruct.text = "Tap CONFIRM to continue";
		}

		//p.transform.rotation = Quaternion.LookRotation((p.transform.position - earthSphere.position), earthSphere.forward);
	}


    private void hideOtherPins(Transform _pin){
        float distlimit = 3f;

        foreach (Transform p in pinContainer)
        {
            if (Vector3.Distance(p.localPosition, _pin.localPosition) < distlimit && p != _pin)
            {
                Debug.Log("HAS FOUND");
                p.GetComponent<Pin>().info.gameObject.SetActive(false);
            }

        }


    }

	private void transformStartedHandler(object sender, EventArgs e){
		flicking = false;
		spinning = false;
		spinVelocity = 0;
		//PD.menu.ToggleWelcome (0);
		//PD.menu.icons.SetActive (false);
		//if(newUserPin==null)
			//PD.menu.instruct.text = "Tap to drop a pin in your home location";
	}

	private void transformedHandler(object sender, EventArgs e){
		if (!twoFingerRotate) {
			
			//scale 0.5, mod 0.5
			//scale 1, mod 0.25
			//scale 2, mod 0.1
			float mod = 0.2553933f*Mathf.Pow(transform.localScale.x, -1.011058f);
			//Debug.Log((transform.localScale.x * 0.05f));
			transform.RotateAround (Vector3.down, transformGesture.DeltaPosition.x * mod);
			transform.RotateAround (Vector3.right, transformGesture.DeltaPosition.y * mod);

		}
	}

	private void transformCompletedHandler(object sender, EventArgs e){
	}

	private void twoFingerTransformStartHandler(object sender, EventArgs e){
		flicking = false;
		spinning = false;
		twoFingerRotate = true;
		//PD.menu.ToggleWelcome (0);
		PD.menu.icons.SetActive (false);
		//if(newUserPin==null)
			//PD.menu.instruct.text = "Tap to drop a pin in your home location";
	}

	private void twoFingerTransformHandler(object sender, System.EventArgs e)
	{
		//apply rotation on global z axis
		//transform.Rotate(Vector3.forward, twoFingerTransformGesture.DeltaRotation, Space.World);

		//apply scaling
		transform.localScale *= twoFingerTransformGesture.DeltaScale;

		//set scaling limits
		if (transform.localScale.x > 2f)
			transform.localScale = Vector3.one * 2f;
		if (transform.localScale.x < .5f)
			transform.localScale = Vector3.one * .5f;
	}

    private void scaleWithMouse(){
        //apply scaling
        transform.localScale += Vector3.one * Input.mouseScrollDelta.y * 0.05f;

        //set scaling limits
        if (transform.localScale.x > 2f)
            transform.localScale = Vector3.one * 2f;
        if (transform.localScale.x < .5f)
            transform.localScale = Vector3.one * .5f;
        

    }

	private void twoFingerTransformEndHandler(object sender, EventArgs e){
		twoFingerRotate = false;
	}

	private void flickedHandler(object sender, EventArgs e){
		spinVelocity = flickGesture.ScreenFlickTime * 200;
		//spinAxis = new Vector3(flickGesture.ScreenFlickVector.y, -flickGesture.ScreenFlickVector.x, 0);
//		spinAxis = new Vector3(0, -flickGesture.ScreenFlickVector.x, 0);
//		spinning = false;
//		flicking = true;

		//PD.menu.ToggleWelcome (0);
		//PD.menu.icons.SetActive (false);
		//if(newUserPin==null)
			//PD.menu.instruct.text = "Tap to drop a pin in your home location";
		//Debug.Log ("FLICK " + spinAxis + " " + spinVelocity);
	}

	private void tapHandler(object sender, EventArgs e){
        if (Physics.Raycast (cam.ScreenPointToRay (tapGesture.ScreenPosition), out hit)) {
            PD.menu.icons.SetActive(false);
            PD.menu.instruct.text = "";

			spinning = false;
			Debug.Log ("[tapHandler] "+hit.transform.name+" (" + hit.point + ") | (" + earthSphere.InverseTransformPoint (hit.point) + " )");
			XYZtoLatLon (earthSphere.InverseTransformPoint(hit.point));
			if (IsItLand (hit.textureCoord)) {
                PD.menu.HideAllPages ();

                if(PD.menu.questionCompleted){
                    PlacePin(XYZtoLatLon(earthSphere.InverseTransformPoint(hit.point)), "CONFIRM", true);
                }
				
			} else {
				if (newUserPin != null) {
					Destroy (newUserPin);
                    PD.menu.HideAllPages ();
					//PD.menu.instruct.text = "Tap to drop a pin in your home location";
				}
			}
		} 
	}

	private void pressedHandler(object sender, EventArgs e){
		timeWaited = 0f;
		idling = false;
	}

	private void GoIdle(){//Close all other menus

        if(PD.menu.CurrentPage == PD.menu.welcome)
        {
            Debug.Log("NO IDLE");
            return;
        }

        //GA--Idle timer reset

        Debug.Log("!!!!GoIdle");
		idling = true;
		PD.menu.hidePins = false;
		Destroy (newUserPin);

        PD.menu.ShowPage(PD.menu.welcome.transform);
        PD.menu.instruct.text = "";
        //      PD.menu.HideAllPages ();
        //PD.menu.ToggleWelcome (1, 0f);
        //PD.menu.icons.SetActive (true);
        //PD.menu.instruct.text = "Explore and Drop a Pin";
        EaseCurve.Instance.RotTo (transform, Quaternion.Euler (0, -10f, 0), 2f, 0f, EaseCurve.Instance.easeIn, GoIdle2);
        EaseCurve.Instance.Scl (transform, transform.localScale, Vector3.one * PD.initGlobeSize, 1.5f, 0f, EaseCurve.Instance.easeIn);
    }

	void GoIdle2(){
		spinning = true;
	}
		
	void Update () {
		if (Input.GetKey (KeyCode.UpArrow)) {
			if (transform.localScale.x > 0.5f) {
				transform.localScale *= 0.9f;
			}
		}
		if (Input.GetKey (KeyCode.DownArrow)) {
			if (transform.localScale.x < 2f) {
				transform.localScale *= 1.1f;
			}
		}

		if (!idling) {
			timeWaited += Time.deltaTime;
			if (timeWaited > maxTimeToWait) {
				GoIdle ();
			}
		}

		if(flicking){
			if (spinVelocity > 0) {
				transform.RotateAround (transform.position, spinAxis, spinVelocity);
				spinVelocity *= spinDamp;
			}
		}

		if (spinning) {
			transform.Rotate (transform.up, -spinSpeed * Time.deltaTime);
		}

		if (Input.mouseScrollDelta.y != 0) {
			scaleWithMouse ();
		}
	}
}
