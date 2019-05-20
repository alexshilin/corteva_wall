using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures.TransformGestures;
using TouchScript.Gestures;
using System;
using SimpleJSON;

public class PinDropEarth : MonoBehaviour {

	public Transform earthSphere;
	public Camera cam;
	public Transform pinContainer;
	public Transform pin;

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

	private bool twoFingerRotate = false;

	// Use this for initialization
	void Start () {
		tex = earthSphere.GetComponent<Renderer> ().material.GetTexture ("_CloudAndNightTex") as Texture2D;

		string dataAsJson = System.IO.File.ReadAllText ("/Users/user/Documents/WORK/Baji/Corteva/_repo/_builds/assets/"+"pins.json");
		pins = JSON.Parse(dataAsJson);

		Debug.Log ("[PinDropEarth] " + pins ["pins"].Count+" pins"); 

		for (int i = 0; i < pins ["pins"].Count; i++) {
			PlacePin (pins ["pins"][i]["lat"].AsFloat, pins ["pins"][i]["lon"].AsFloat);
		}
	}


	public void XYZtoLatLon(Vector3 _v3){
		float r = Mathf.Sqrt (_v3.x * _v3.x + _v3.y * _v3.y + _v3.z * _v3.z); 
		float lat = Mathf.Asin (_v3.z / r) * Mathf.Rad2Deg;
		float lon = Mathf.Atan2 (_v3.y, _v3.x) * Mathf.Rad2Deg * -1;
		Debug.Log ("[XYZtoLatLong] ("+_v3+") > ("+ lat + ", " + lon +" )");
		LatLonToXYZ (lat, lon);
	}


	public Vector3 LatLonToXYZ(float _lat, float _lon){
		float earthRadius = 6367; 
		float radius = earthSphere.localScale.x * 0.5f;
		float lat = _lat * Mathf.Deg2Rad;
		float lon = _lon * Mathf.Deg2Rad;
		float xPos = earthRadius * Mathf.Cos (lat) * Mathf.Cos (lon);
		float yPos = earthRadius * Mathf.Cos (lat) * Mathf.Sin (lon);
		float zPos = earthRadius * Mathf.Sin (lat);
		Vector3 pinPos = new Vector3 (xPos, zPos, yPos).normalized * radius;
		//pin.localPosition = pinPos;
		//pin.rotation = Quaternion.LookRotation((pin.position - earthSphere.position), earthSphere.forward);
		//Debug.Log ("[LatLonToXYZ] (" + _lat+", "+ _lon+") > (" + pinPos +")"+" | ("+pin.position+")");
		return pinPos;
	}


	void LandOrWater(Vector2 texCoord){
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

	}

	void PlacePin(float _lat, float _lon){
		GameObject p = Instantiate (pin.gameObject, pinContainer);
		p.transform.localPosition = LatLonToXYZ (_lat, _lon);
		p.transform.rotation = Quaternion.LookRotation((p.transform.position - earthSphere.position), earthSphere.forward);
	}

	void OnEnable(){
		if (GetComponentInParent<Camera> ()) {
			cam = GetComponentInParent<Camera> ();
		}

		transformGesture.AddFriendlyGesture (twoFingerTransformGesture);

		flickGesture = GetComponent<FlickGesture> ();
		tapGesture = GetComponent<TapGesture> ();

		transformGesture.TransformStarted += transformStartedHandler;
		transformGesture.Transformed += transformedHandler;
		transformGesture.TransformCompleted += transformCompletedHandler;

		twoFingerTransformGesture.TransformStarted += twoFingerTransformStartHandler;
		twoFingerTransformGesture.Transformed += twoFingerTransformHandler;
		twoFingerTransformGesture.TransformCompleted += twoFingerTransformEndHandler;

		flickGesture.Flicked += flickedHandler;

		tapGesture.Tapped += tapHandler;
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
	}

	private void transformStartedHandler(object sender, EventArgs e){
		flicking = false;
		spinVelocity = 0;
	}

	private void transformedHandler(object sender, EventArgs e){
		if (!twoFingerRotate) {
			transform.RotateAround (Vector3.down, transformGesture.DeltaPosition.x);
			transform.RotateAround (Vector3.right, transformGesture.DeltaPosition.y);
		}
	}

	private void transformCompletedHandler(object sender, EventArgs e){
	}

	private void twoFingerTransformStartHandler(object sender, EventArgs e){
		twoFingerRotate = true;
	}

	private void twoFingerTransformHandler(object sender, System.EventArgs e)
	{
		//apply rotation on global z axis
		transform.localRotation *= Quaternion.AngleAxis(twoFingerTransformGesture.DeltaRotation, Vector3.forward);

		//apply scaling
		transform.localScale *= twoFingerTransformGesture.DeltaScale;
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
		spinVelocity = flickGesture.ScreenFlickTime * 500;
		spinAxis = new Vector3(flickGesture.ScreenFlickVector.y, -flickGesture.ScreenFlickVector.x, 0);
		flicking = true;
		//Debug.Log ("FLICK " + spinAxis + " " + spinVelocity);
	}

	private void tapHandler(object sender, EventArgs e){
		if (Physics.Raycast (cam.ScreenPointToRay (tapGesture.ScreenPosition), out hit)) {
			Debug.Log ("[tapHandler] "+hit.transform.name+" (" + hit.point + ") | (" + earthSphere.InverseTransformPoint (hit.point) + " )");
			XYZtoLatLon (earthSphere.InverseTransformPoint(hit.point));
			LandOrWater (hit.textureCoord);
		} 
	}
	
	void Update () {
		if(flicking){
			if (spinVelocity > 0) {
				transform.RotateAround (transform.position, spinAxis, spinVelocity);
				spinVelocity *= spinDamp;
			}
		}
	}
}
