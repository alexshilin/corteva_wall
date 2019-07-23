using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TouchScript.Gestures.TransformGestures;

public class RotateGlobe : MonoBehaviour
{
    public TransformGesture transformGesture;
    void OnEnable()
    {
        transformGesture = GetComponent<TransformGesture>();

        transformGesture.Transformed += transformedHandler;
    }

    void OnDisable()
    {
        transformGesture.Transformed -= transformedHandler;
    }

    private void transformedHandler(object sender, System.EventArgs e)
    {
        //Vector3 newrot = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + transformGesture.DeltaPosition.x * 100f);
        //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newrot), 0.3f);
		transform.RotateAround (Vector3.down, transformGesture.DeltaPosition.x);
		transform.RotateAround (Vector3.right, transformGesture.DeltaPosition.y);
        //transform.RotateAround(rotateAround.transform.eulerAngles, transformGesture.DeltaPosition.x * 0.5f);
    }
}
