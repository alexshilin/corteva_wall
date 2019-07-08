using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TouchScript.Gestures;

public class ClickableObject : MonoBehaviour
{

    private TapGesture tapGesture;

    [Serializable]
    public class MyOwnEvent : UnityEvent { }

    [SerializeField]
    private MyOwnEvent OnClick = new MyOwnEvent();
    public MyOwnEvent onMyOwnEvent { get { return OnClick; } set { OnClick = value; } }

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
        onMyOwnEvent.Invoke();
    }

}