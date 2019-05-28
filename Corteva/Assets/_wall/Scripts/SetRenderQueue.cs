using UnityEngine;

[AddComponentMenu("Rendering/SetRenderQueue")]

public class SetRenderQueue : MonoBehaviour {

	protected void Awake() {
		Renderer[] renders = GetComponentsInChildren<Renderer>();
		Debug.Log (name + " " + renders.Length);
		foreach (Renderer rendr in renders){
			rendr.material.renderQueue = 2002; // set their renderQueue
		}
	}
}