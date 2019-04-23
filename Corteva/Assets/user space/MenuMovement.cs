/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using UnityEngine;
using TouchScript.Behaviors;
using TouchScript.Gestures.TransformGestures;
using System;


	/// <exclude />
	public class MenuMovement : MonoBehaviour
	{

		private TransformGesture gesture;
		private Transformer transformer;
		private Rigidbody rb;

		private void OnEnable()
		{
			// The gesture
			gesture = GetComponent<TransformGesture>();
			// Transformer component actually MOVES the object
			transformer = GetComponent<Transformer>();
			rb = GetComponent<Rigidbody>();

			transformer.enabled = false;
			//rb.isKinematic = false;

			// Subscribe to gesture events
			gesture.TransformStarted += transformStartedHandler;
			gesture.TransformCompleted += transformCompletedHandler;
		}

		private void OnDisable()
		{
			// Unsubscribe from gesture events
			gesture.TransformStarted -= transformStartedHandler;
			gesture.TransformCompleted -= transformCompletedHandler;
		}

		private void transformStartedHandler(object sender, EventArgs e)
		{
			// When movement starts we need to tell physics that now WE are moving this object manually
			//rb.isKinematic = true;
			Debug.Log("hi");
			transformer.enabled = true;
		}

		private void transformCompletedHandler(object sender, EventArgs e)
		{
			transformer.enabled = false;
			//rb.isKinematic = false;
			rb.WakeUp();
		}





//		private TransformGesture gesture;
//
//		private void OnEnable()
//		{
//			gesture = GetComponent<TransformGesture>();
//			gesture.Transformed += transformedHandler;
//		}
//
//		private void OnDisable()
//		{
//			gesture.Transformed -= transformedHandler;
//		}
//
//		private void transformedHandler(object sender, System.EventArgs e)
//		{
//			//transform.localRotation *= Quaternion.AngleAxis(gesture.DeltaRotation, -gesture.RotationAxis);
//			Debug.Log(gesture.DeltaPosition);
//			transform.localPosition = Vector3.Lerp (transform.localPosition, gesture.DeltaPosition, 2f * Time.deltaTime);
//		}
	}