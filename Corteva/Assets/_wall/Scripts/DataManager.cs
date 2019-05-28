using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SimpleJSON;
using YamlDotNet.RepresentationModel;

public class DataManager : MonoBehaviour {

	private ScreenManager SM;

	private string userRoot;
	private string rootDir;
	private string dataDir;
	private string mediaDir;

	private string filePrefix;

	const string rootYamlDocName = "corteva.config.yaml";
	const string environmentsJsonDocName = "environments.json";
	const string contentJsonDocName = "content_items.json";
	const string filesJsonDocName = "files.json";
	const string lastupdateJsonDocName = "last_update.json";
	const string messagingJsonDocName = "messaging_buckets.json";
	const string presentationsJsonDocName = "presentations.json";

	[HideInInspector]
	public List<Texture2D> imageTextures = new List<Texture2D>();
	private List<string> imageFiles = new List<string>();

	[HideInInspector]
	public List<string> videoFiles = new List<string> ();

	public List<Environment> environments = new List<Environment>();

	void Start(){
		//Init ();
	}

	public void Init(){
		SM = ScreenManager.Instance;
		filePrefix = (Application.platform == RuntimePlatform.WindowsPlayer) ? "" : "file://";
		//ParseYamlConfig ();
	}
		
	public string ParsePath(string _path){
		return (Application.platform == RuntimePlatform.WindowsPlayer) ? _path.Replace ("/", "\\") : _path;
	}

	private void ParseYamlConfig(){
		//get ref to user root
		userRoot = ParsePath(System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal)+"/");
		SM.Log ("user root: " + userRoot);

		//load root YAML file
		var input = new StringReader(File.ReadAllText (userRoot+rootYamlDocName));
		var yaml = new YamlStream();
		yaml.Load(input);

		//get document paths from YAML
		var rootNode = (YamlMappingNode)yaml.Documents[0].RootNode;
		rootDir = ParsePath (rootNode.Children [new YamlScalarNode ("root_path")] + "/");
		SM.Log ("\trootDir: " + rootDir);

		var dataNode = (YamlMappingNode)rootNode.Children [new YamlScalarNode ("data")];
		dataDir = ParsePath (rootDir + dataNode.Children [new YamlScalarNode ("directory")] + "/");
		SM.Log ("\tdataDir: " + dataDir);

		var mediaNode = (YamlMappingNode)rootNode.Children [new YamlScalarNode ("media")];
		mediaDir = ParsePath (rootDir + mediaNode.Children [new YamlScalarNode ("directory")] + "/");
		SM.Log ("\tmediaDir: " + mediaDir);

		ParseJsonConfigs ();
	}

	private void ParseJsonConfigs(){
		//all media files
		string filesJSON = File.ReadAllText (dataDir + filesJsonDocName);
		var Nfiles = JSON.Parse(filesJSON);
		SM.Log ("filesJSON: (" + Nfiles.Count + ") " + (dataDir + filesJsonDocName));

		for (int i = 0; i < Nfiles.Count; i++) 
		{
			if (Nfiles [i] ["type"] == "image") 
			{
				string path = ParsePath (rootDir + Nfiles [i] ["path"]);
				if (!imageFiles.Contains (path))
				{
					imageFiles.Add (path);
					//SM.Log ("\t" + path);
				} else {
					//SM.Log ("\tDUPE "+path);
				}
			}

			if (Nfiles [i] ["type"] == "video") 
			{
				string path = ParsePath (rootDir + Nfiles [i] ["path"]);
				if (!videoFiles.Contains (path)) 
				{
					videoFiles.Add (path);
					SM.Log ("\t" + path);
				}
			}
		}

		//all environemts
		string environmentsJSON = File.ReadAllText (dataDir + environmentsJsonDocName);
		var Nenv = JSON.Parse(environmentsJSON);
		SM.Log ("environmentsJSON: (" + Nenv["data"].Count + ") " + (dataDir + environmentsJsonDocName));

		Environment e;
		for (int i = 0; i < Nenv["data"].Count; i++) {
			//Debug.Log("\t"+Nenv["data"] [i] ["title"]);
			e = new Environment ();
			e.envID = i;
			string _key = Nenv ["data"] [i] ["_key"];
			e.envKey = Nenv ["data"] [i] [_key];
			e.envTitle = Nenv["data"] [i] ["title"];
			e.envSummary = Nenv ["data"] [i] ["summary"];
			e.envColor = new Color32 ((byte)Nenv ["data"] [i] ["colorRGB"] [0].AsInt, (byte)Nenv ["data"] [i] ["colorRGB"] [1].AsInt, (byte)Nenv ["data"] [i] ["colorRGB"] [2].AsInt, 255);
			e.envIconPath = ParsePath (rootDir + Nenv ["data"] [i] ["icon"] ["path"]);
			e.envKioskBg = ParsePath (rootDir + Nenv ["data"] [i] ["kiosk_background_image"] ["path"]);
			e.envBg = ParsePath (rootDir + Nenv ["data"] [i] ["idle_background_video"] ["path"]);
			environments.Add (e);
		}

		//all content panels
		string contentPanelsJSON = File.ReadAllText (dataDir + contentJsonDocName);
		var Ncp = JSON.Parse(contentPanelsJSON);
		SM.Log ("contentPanelsJSON: (" + Ncp["data"].Count + ") " + (dataDir + contentJsonDocName));

		for (int i = 0; i < Ncp ["data"].Count; i++) {
			string envKey = Ncp ["data"] [i] ["environment"];
			int eI = environments.FindIndex (x => x.envKey == envKey);
			environments [eI].envPanelData.Add(Ncp ["data"] [i]);
		}

		//TODO match content panels to their environments

		//presentations (which content panels / beauty panels to use in the idle state for each environment)
		string presentationsJSON = File.ReadAllText (dataDir + presentationsJsonDocName);
		var Npresentations = JSON.Parse(presentationsJSON);
		JSONNode scenes = Npresentations ["data"] [0] ["scenes"];
		SM.Log ("presentationsJSON: (" + scenes.Count + ") " + (dataDir + presentationsJsonDocName));
		for (int i = 0; i < scenes.Count; i++) 
		{
			SM.Log ("\t" + scenes [i] ["environment"] +
					": " + scenes [i] ["content_panels"].Count + " content panels" +
					", " + scenes [i] ["beauty_panels"].Count + " beauty panels");
			string envKey = scenes [i] ["environment"];
			int eI = environments.FindIndex (x => x.envKey == envKey);
			environments [eI].btyPanelData = scenes [i] ["beauty_panels"];
		}

		StartCoroutine(LoadImages ());
	}

	private IEnumerator LoadImages () {
		//preload images to textures
		SM.Log("START loading textures...");

		foreach(string file in imageFiles)
		{
			SM.Log("\tloading: " + filePrefix + file);
			WWW www = new WWW(filePrefix + file);
			while (!www.isDone)
			{
				yield return null;
			}
			Texture2D t = new Texture2D(2, 2, TextureFormat.DXT5, false);
			www.LoadImageIntoTexture(t);
			imageTextures.Add (t);
		}

		SM.Log("FINISHED loading textures.");

		Prepare ();

		yield return null;
	}

	private void Prepare(){
		for (int i = 0; i < environments.Count; i++) {
			GameObject panelBaseGO = Instantiate (AssetManager.Instance.NEWpanelPrefab, AssetManager.Instance.idleBackgrounds);
			PanelBase panelBase = panelBaseGO.GetComponent<PanelBase> ();
			panelBase.panelID = environments[i].envTitle + "_bg";
			panelBaseGO.name = panelBase.panelID;
			panelBase.environment = environments[i];
			string bgTemplate = (ScreenManager.Instance.currAspect == ScreenManager.Aspect.is169) ? "16x9_bg" : "32x9_bg";
			panelBase.AssembleBasic (bgTemplate, environments[i].envBg);
			environments[i].envBgVid = panelBaseGO;

			environments[i].env1x1Count = environments[i].envPanelData.Count;
			for (int a = 0; a < environments[i].btyPanelData.Count; a++) {
				if (environments[i].btyPanelData [a]["front"]["template"] == "beauty_1x2") {
					environments[i].bty1x2Indeces.Add (a);
				} else {
					environments[i].bty1x1Indeces.Add (a);
				}
			}
		}
	}
}
