using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GA : MonoBehaviour {

	[HideInInspector]
	public GoogleAnalyticsV4 Tracking;
	public string testTrackingCode;

	private static GA _instance;
	public static GA Instance { get { return _instance; } }
	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		} else {
			_instance = this;
		}

		Tracking = GetComponent<GoogleAnalyticsV4> ();

		#if UNITY_EDITOR
		if(testTrackingCode!=""){
			Debug.Log("using TEST GA ID for unity editor: "+testTrackingCode);
			Tracking.otherTrackingCode = testTrackingCode;
		}
		#endif
	}
}
