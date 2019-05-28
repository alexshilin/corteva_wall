using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EarthContols : MonoBehaviour {

	public Transform earthSphere;
	public Transform satelliteLookAt;
	public Text displayText;
	public bool active = false;

	Rigidbody rb;
	Renderer rend;
	Texture2D tex;
	public Camera cam;

	RaycastHit hit;

	Vector3 handle;
	Vector3 dirFromCenterToHandle;

	float sphereRadius;

	float idleRotationSpeed = 0.5f;
	float dragSpeed = 20f;
	bool started = false;
	bool touchingWorld = false;
	bool transition = false;

	Vector3 transitionTo;

	Vector3 mousePosStartDrag;
	Vector3 mousePosEndDrag;
	Vector3 dragVel;
	float dragTime;
	float dampMod = 0.5f;
	Vector3 swipeDir;
	Vector3 swipeRotAxis;
	float swipeDis;
	float swipeSpd;

	void Start () {
		rb = GetComponent<Rigidbody> ();
		rb.centerOfMass = transform.position;
		rend = earthSphere.GetComponent<Renderer> ();
		tex = rend.material.GetTexture ("_CloudAndNightTex") as Texture2D;
		sphereRadius = earthSphere.localScale.x * 0.5f;
	}  

	void Update () {
		if (Input.GetMouseButton (0)) {
			if (Physics.Raycast (cam.ScreenPointToRay (Input.mousePosition), out hit)) {
				//Debug.Log (hit.transform.name);
				//if (hit.transform.gameObject == this.gameObject && !touchingWorld) {
				if (!touchingWorld) {
					mousePosStartDrag = Input.mousePosition;
					dragTime = 0;
					rb.angularVelocity = Vector3.zero;

					handle = hit.point;
					dirFromCenterToHandle = (handle - transform.position).normalized;
					touchingWorld = true;
					started = true;
					GetComponent<LatLong> ().XYZtoLatLong (earthSphere.InverseTransformPoint(hit.point));
					getTextureColor (hit.textureCoord);
				}
				if (hit.transform.name == "Sphere") {
					//getTextureColor (hit.textureCoord);
				}
			} else {
				handle = Vector3.zero;
				dirFromCenterToHandle = Vector3.zero;
			}
			if (touchingWorld) {
				float rotX = Input.GetAxis ("Mouse X") * dragSpeed * Mathf.Deg2Rad;
				float rotY = Input.GetAxis ("Mouse Y") * dragSpeed * Mathf.Deg2Rad;
				transform.RotateAround (Vector3.up, -rotX);
				transform.RotateAround (Vector3.right, rotY);
			}
		}
		if (Input.GetMouseButtonUp (0)) {
			touchingWorld = false;

			//TODO could be smarter
			//if i swiped but then didnt move for a while, it shouldnt be considered a "throw" action
			mousePosEndDrag = Input.mousePosition;
			swipeDir = mousePosEndDrag - mousePosStartDrag;
			swipeDis = Vector3.Distance (mousePosStartDrag, mousePosEndDrag);
			swipeSpd = swipeDis / dragTime;

			swipeRotAxis = Vector3.Cross (swipeDir, Vector3.forward);
			Debug.Log (swipeDir+" , "+swipeDis+" , "+swipeSpd);
			if (swipeSpd > 400 && swipeSpd < Mathf.Infinity) {
				rb.AddTorque (swipeRotAxis * swipeSpd, ForceMode.VelocityChange);
			}

		}

		if (Input.GetKeyDown (KeyCode.Space)) {
			started = true;
			transition = true;
		}
		if (touchingWorld) {
			dragTime += Time.deltaTime;
		}
	}

	void FixedUpdate () {
		if (!started) {
			transform.Rotate (transform.up, -idleRotationSpeed * Time.fixedDeltaTime);
		}
		if (!active)
			return;
		if (transition) {
			
			//rotate camera to look at pin
			Vector3 direction = satelliteLookAt.position - cam.transform.position;
			Quaternion toRotation = Quaternion.FromToRotation(cam.transform.up, satelliteLookAt.parent.forward) * Quaternion.LookRotation (direction, cam.transform.up);
			cam.transform.rotation = Quaternion.Lerp (cam.transform.rotation, toRotation, .1f * Time.fixedDeltaTime);



			//move camera to 1unit above pin
			transitionTo = satelliteLookAt.position + ((satelliteLookAt.position - transform.position).normalized * 3f);
			cam.transform.position = Vector3.Lerp (cam.transform.position, transitionTo, .2f * Time.fixedDeltaTime);

		}
	}

	void OnDrawGizmos(){
		Gizmos.color = Color.black;
		Gizmos.DrawLine (transform.position, transform.position + dirFromCenterToHandle * (sphereRadius + 1f));
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere (transitionTo, 0.1f);
	}

	//hittest needs to happen on the sphere itself, not its parent container as it is now.
	void getTextureColor(Vector2 texCoord){
		Vector2 pixelPos = texCoord;

		pixelPos.x *= tex.width;
		pixelPos.y *= tex.height;
		//adjust for texture offset
		pixelPos.x = pixelPos.x - 512f;
		if (pixelPos.x < 0)
			pixelPos.x = tex.width + pixelPos.x;
		//adjust for vertical flip
		//pixelPos.y = tex.height - pixelPos.y;

		Color pixCol = tex.GetPixel ((int)pixelPos.x, (int)pixelPos.y);
		float colAverage = (pixCol.r + pixCol.g + pixCol.b) / 3f;
		Debug.Log (texCoord+" > "+pixelPos+" > "+pixCol+" ("+colAverage+")");
		if (colAverage > 0.9f) {
			//displayText.text += "\nwater";
		} else {
			//displayText.text += "\nland";
		}

	}
}
