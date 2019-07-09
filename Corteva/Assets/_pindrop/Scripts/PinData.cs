using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SimpleJSON;
using YamlDotNet.RepresentationModel;

public class PinData : MonoBehaviour {

	const string rootYamlDocName = "corteva.config.yaml";
	private string userRoot;
	private string rootDir;
	private string newPinsSave;
	public string oldPinsLoad;
	public JSONNode pinData;

	[HideInInspector]
	public string displayName;

	private static PinData _instance;
	public static PinData Instance { get { return _instance; } }
	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		} else {
			_instance = this;
		}

		ParseYamlConfig ();
	}

	public string ParsePath(string _path){
		return (Application.platform == RuntimePlatform.WindowsPlayer) ? _path.Replace ("/", "\\") : _path;
	}

	private void ParseYamlConfig(){
		//get ref to user root
		userRoot = ParsePath(System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal)+"/");

		//load root YAML file
		var input = new StringReader(File.ReadAllText (userRoot+rootYamlDocName));
		var yaml = new YamlStream();
		yaml.Load(input);

		var rootNode = (YamlMappingNode)yaml.Documents[0].RootNode;
		var dataNode = (YamlMappingNode)rootNode.Children [new YamlScalarNode ("data")];
		var filesNode = (YamlMappingNode)dataNode.Children [new YamlScalarNode ("files")];

		//get display name for GA tracking
		if ((string)rootNode.Children [new YamlScalarNode ("nickname")] != "") {
			displayName = rootNode.Children [new YamlScalarNode ("location")] + " (" + rootNode.Children [new YamlScalarNode ("nickname")] + ")";
		} else {
			displayName = (string)rootNode.Children [new YamlScalarNode ("location")];
		}

		//get file path where to save pins
		newPinsSave = ParsePath ((string)rootNode.Children [new YamlScalarNode ("new_dropped_pins")]);

		//get document paths from YAML
		rootDir = ParsePath (rootNode.Children [new YamlScalarNode ("root_path")] + "/");
		string dataDir = ParsePath (rootDir + dataNode.Children [new YamlScalarNode ("directory")] + "/");

		oldPinsLoad = dataDir + filesNode.Children [new YamlScalarNode ("pindrop")];

		ParseJsonConfig ();
	}

	private void ParseJsonConfig(){
		string filesJSON = File.ReadAllText (oldPinsLoad);
		pinData = JSON.Parse(filesJSON);
	}

	public void SavePin(Vector2 _latlong, string _person, string _interest){
		Debug.Log ("SAVING: (" + _latlong.x+", "+_latlong.y + ") " + _person + ": " + _interest);

		string saveString = _person + "," + _interest + "," + _latlong.x + "," + _latlong.y + "\n";

		if (File.Exists (newPinsSave)) 
		{
			File.AppendAllText(newPinsSave, saveString);
		} else {
			File.WriteAllText(newPinsSave, saveString);
		}
	}

}
