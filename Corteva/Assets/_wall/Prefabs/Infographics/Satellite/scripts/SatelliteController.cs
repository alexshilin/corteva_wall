using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SatelliteController : MonoBehaviour {
   // private static SatelliteController singleton;
    //public static SatelliteController s { get { return singleton; } }

//    private void Awake()
//    {
//        singleton = this;
//    }
    public GameObject globe;
    public Transform icons;
    public Transform firstSatellite;
    public Color cblue;

    void Start () {
        ClickOnSatellite(firstSatellite);
	}
	
	// Update is called once per frame
	void Update () {
		globe.transform.Rotate(transform.forward, -5 * Time.deltaTime, Space.Self);
    }

    public void ClickOnSatellite(Transform _satellite)
    {
        foreach(Transform kid in icons)
        {
            SatelliteButton kidscript = kid.GetComponent<SatelliteButton>();
            if (kid == _satellite)
            {
                kidscript.isSelected = true;
                kidscript.myInfo.gameObject.SetActive(true);
                kidscript.myGlobeObj.gameObject.SetActive(true);
                kidscript.modelC.gameObject.SetActive(true);
                kidscript.modelBW.gameObject.SetActive(false);
                kidscript.number.color = cblue;
                kidscript.txt.color = cblue;
//                kidscript.ShowRing(true);
                kidscript.ring.GetComponent<SpriteRenderer>().color = cblue;
            }
            else
            {
                kidscript.isSelected = false;
                kidscript.myInfo.gameObject.SetActive(false);
                kidscript.myGlobeObj.gameObject.SetActive(false);
                kidscript.modelC.gameObject.SetActive(false);
                kidscript.modelBW.gameObject.SetActive(true);
                kidscript.number.color = Color.black;
                kidscript.txt.color = Color.black;
                kidscript.ring.GetComponent<SpriteRenderer>().color = Color.black;
//                kidscript.ShowRing(false);
            }
        }
    }
}
