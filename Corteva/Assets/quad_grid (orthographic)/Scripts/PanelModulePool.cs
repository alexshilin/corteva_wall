using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PanelModule
{
	public string name;
	public GameObject prefab;
}

[Serializable]
public class AssembledPanel
{
	public Environment environment;
	public int id;
	public GameObject panel;
}

public class PanelModulePool : MonoBehaviour {

	public List<PanelModule> modules = new List<PanelModule>();

	[HideInInspector]
	public List<AssembledPanel> recycler = new List<AssembledPanel> (); 

	private static PanelModulePool _instance;
	public static PanelModulePool Instance { get { return _instance; } }
	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		} else {
			_instance = this;
		}
	}


}
