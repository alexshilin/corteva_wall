using UnityEngine;
using UnityEngine.UI;

public class LatLong : MonoBehaviour
{
	public Transform sphere;
	public Transform markerA;
	public Transform markerB;
	public Transform markerC;
	public Transform markerD;
	float radius = 1;
	public float latitude = 51.5072f;
	public float longitude = 0.1275f;

	float lat;
	float lon;
	float xPos;
	float yPos;
	float zPos;

	public Text TextDisplay;

	void Start()
	{
		radius = sphere.localScale.x * 0.5f;
	}

	//
	//_v3 must be in local space of sphere object
	//
	public void XYZtoLatLong(Vector3 _v3){
		float r = Mathf.Sqrt (_v3.x * _v3.x + _v3.y * _v3.y + _v3.z * _v3.z); 
		float lat = Mathf.Asin (_v3.z / r) * Mathf.Rad2Deg;
		float lon = Mathf.Atan2 (_v3.y, _v3.x) * Mathf.Rad2Deg * -1;
//		Debug.Log (lat + ", " + -lon);
		//TextDisplay.text = "lat: "+lat+"\nlon: "+lon;
		latitude = lat;
		longitude = lon;
	}

	void Update(){
//		lat = latitude * Mathf.Deg2Rad;
//		lon = longitude * Mathf.Deg2Rad;
//
//		xPos = radius * Mathf.Cos (lon) * Mathf.Cos (lat);
//		yPos = radius * Mathf.Cos (lon) * Mathf.Sin (lat);
//		zPos = radius * Mathf.Sin (lon);
//
//		markerA.localPosition = new Vector3(xPos, yPos, zPos);
//
//
//
//
//		lat = latitude;
//		lon = longitude;
//		Vector3 origin = new Vector3 (sphere.localScale.x*0.5f, 0, 0);
//		//build a quaternion using euler angles for lat,lon
//		Quaternion rotation = Quaternion.Euler(0f,lon,lat);
//		//transform our reference vector by the rotation. Easy-peasy!
//		Vector3 point = rotation*origin;
//		markerB.localPosition = point;
//
//
//
//
//		lat = latitude;
//		lon = longitude;
//		float a = radius * Mathf.Cos(lat);
//		xPos = a * Mathf.Cos(lon);
//		yPos = radius * Mathf.Sin(lat);
//		zPos = a * Mathf.Sin(lon);
//
//		markerC.localPosition = new Vector3(xPos, yPos, zPos);



		var earthRadius = 6367; 
		lat = latitude * Mathf.Deg2Rad;
		lon = longitude * Mathf.Deg2Rad;
		xPos = earthRadius * Mathf.Cos (lat) * Mathf.Cos (lon);
		yPos = earthRadius * Mathf.Cos (lat) * Mathf.Sin (lon);
		zPos = earthRadius * Mathf.Sin (lat);
		markerD.localPosition = new Vector3 (xPos, zPos, yPos).normalized * radius;
		markerD.rotation = Quaternion.LookRotation((markerD.position - sphere.position), sphere.forward);
	}

}