using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures;

public class ClickforNextPage : MonoBehaviour
{
    public Transform nextPage;
    public GameObject Menu;
    private TapGesture tapGesture;

    void OnEnable()
    {
        tapGesture = GetComponent<TapGesture>();

        tapGesture.Tapped += tapHandler;
    }

    void OnDisable()
    {
        tapGesture.Tapped -= tapHandler;
    }

    void tapHandler(object sender, System.EventArgs e)
    {
        Menu.GetComponent<PinDropMenu>().ShowPage(nextPage);
    }
}
