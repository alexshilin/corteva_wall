using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TouchScript.Gestures;
using TMPro;

public class SatelliteButton : MonoBehaviour {

	public SatelliteController SC;
    public Transform myInfo, myGlobeObj;
    public TextMeshPro number, txt;

    public Transform modelC, modelBW;
    public Transform ring;
    public bool isSelected;
    private TapGesture tapGesture;

    private void OnEnable()
    {
        tapGesture = GetComponent<TapGesture>();
        tapGesture.Tapped += tapHandler;
    }

    private void OnDisable()
    {
        tapGesture.Tapped -= tapHandler;
    }

    private void tapHandler(object sender, System.EventArgs e)
    {
        Debug.Log("Being clicked");
        //Select myself
        SC.ClickOnSatellite(transform);
        //ring animate
        //ring.GetComponent<Image>().fillAmount = 0;
        //setactive colored model
        //modelC.gameObject.SetActive(isSelected);
        //modelBW.gameObject.SetActive(!isSelected);
    }

//    public void ShowRing(bool _show)
//    {
//        ring.GetComponent<SpriteRenderer>().color = _show ? 
//        
//        if (_show)
//        {
//            StartCoroutine(LerpRingFill(0, 1));
//        }
//        else
//        {
//            ring.GetComponent<Image>().fillAmount = 0;
//        }
//    }

//    IEnumerator LerpRingFill(float _from, float _to)
//    {
//        float t = 0f;
//        while(t < 1)
//        {
//            t += Time.deltaTime * 2;
//            ring.GetComponent<SpriteRenderer>().color = Color.Lerp(_from, _to, t);
//            yield return null;
//        }
//    }


}
