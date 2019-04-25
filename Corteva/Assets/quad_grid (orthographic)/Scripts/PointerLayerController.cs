using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TouchManager has its own layering for touches. These touch layers are attached to camera obejcts.
/// Since we instantiate cameras constantly, this class watches for new touch layers (cameras) and sorts
/// the TouchManager's layers to match the depth's of the cameras.
/// </summary>
public class PointerLayerController : MonoBehaviour {

	private int layerCount = 0;

	void Update () {
		//check if layer count has changed
		if (TouchScript.LayerManager.Instance.LayerCount == layerCount)
			return;

		//sort TouchManager Pointer Layers by camera depth
		List<TouchScript.Layers.TouchLayer> layers = new List<TouchScript.Layers.TouchLayer> (TouchScript.LayerManager.Instance.Layers);
		layers.Sort (SortByCameraDepth);
		layers.Reverse ();
		for (int i = 0; i < layers.Count; i++) {
			for (int n = 0; n < TouchScript.LayerManager.Instance.Layers.Count; n++) {
				if (layers [i].transform == TouchScript.LayerManager.Instance.Layers [n].transform) {
					TouchScript.LayerManager.Instance.ChangeLayerIndex (n, i);
				}
			}
		}

		//update new layer count
		layerCount = TouchScript.LayerManager.Instance.LayerCount;
	}

	static int SortByCameraDepth(TouchScript.Layers.TouchLayer cam1, TouchScript.Layers.TouchLayer cam2)
	{
		return cam1.GetComponent<Camera>().depth.CompareTo(cam2.GetComponent<Camera>().depth);
	}
}
