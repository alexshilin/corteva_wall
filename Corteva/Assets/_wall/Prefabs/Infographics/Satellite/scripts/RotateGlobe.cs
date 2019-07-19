using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures.TransformGestures;
using TouchScript.Gestures;
using System;

public class RotateGlobe : MonoBehaviour
{

    private TapGesture tapGesture;
    public TransformGesture transformGesture;
    void OnEnable()
    {
        tapGesture = GetComponent<TapGesture>();
        transformGesture = GetComponent<TransformGesture>();

        transformGesture.TransformStarted += transformStartedHandler;
        transformGesture.Transformed += transformedHandler;
        transformGesture.TransformCompleted += transformCompletedHandler;


        tapGesture.Tapped += tapHandler;
    }

    void OnDisable()
    {
        transformGesture.TransformStarted -= transformStartedHandler;
        transformGesture.Transformed -= transformedHandler;
        transformGesture.TransformCompleted -= transformCompletedHandler;


        tapGesture.Tapped -= tapHandler;
    }
    // Use this for initialization
    void Start()
    {

    }

    private void transformStartedHandler(object sender, EventArgs e)
    {
    }

    private void transformCompletedHandler(object sender, EventArgs e)
    {
    }


    private void tapHandler(object sender, EventArgs e)
    {
        Debug.Log("rOTATING");
    }

    private void transformedHandler(object sender, EventArgs e)
    {
        Vector3 newrot = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + transformGesture.DeltaPosition.x * 30f);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newrot), 0.3f);
        //transform.RotateAround(rotateAround.transform.eulerAngles, transformGesture.DeltaPosition.x * 0.5f);
    }
}
