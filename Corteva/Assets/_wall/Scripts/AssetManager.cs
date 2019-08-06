using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.IO;
using SimpleJSON;
using YamlDotNet.RepresentationModel;

[System.Serializable]
public class Environment
{
	public int envID;
	public string envKey;
	public string envTitle;
	public string envSummary;
	public string envIconPath;
	public string envKioskBg;
	public string envBg;
	public Color32 envColor;
	public GameObject envBgVid;
	public JSONNode envPanelData = new JSONArray();
	public JSONNode btyPanelData;
	public JSONNode bty1x1PanelData = new JSONArray ();
	public JSONNode bty1x2PanelData = new JSONArray ();
	public List<int> bty1x1Indeces = new List<int> ();
	public List<int> bty1x2Indeces = new List<int> ();
	public int env1x1Count;
}


public class AssetManager : MonoBehaviour {
	#region class variables
	private string basePath;
	//private string filePrefix;
	private int relativeFolderOffset;
	private string assetsFolder;

	[HideInInspector]
	public string displayName;


	[Header("Loaded Assets")]
	public List<string> imageFiles = new List<string>();
	[HideInInspector]
	public List<Texture2D> imageTextures = new List<Texture2D>();


	public List<string> videoFiles = new List<string> ();
	public List<string> videoFilesInUse = new List<string> ();
	private List<string> usedVideoFiles = new List<string> ();

	public JSONNode pins;
	#endregion


	#region game object references
	[Header("GameObject references")]
	public Camera mainCamera;
	public Camera userInitCamera;
	public Camera tapToStartCamera;
	public Camera userPanelCamera;
	public Transform panels;
	public Transform cams;
	public Transform kiosks;
	public Transform idleBackgrounds;
	#endregion


	#region prefabs
	[Header("Prefabs")]
	public GameObject panelPrefab;
	public GameObject NEWpanelPrefab;
	public GameObject cellCameraPrefab;
	public GameObject userKioskPrefab;
	public GameObject tapToStart;
	#endregion


	#region lists
	[Header("List references")]
	public List<string> baseTemplateNames = new List<string> ();
	public List<string> customTemplateNames = new List<string> ();
	public List<Environment> environments = new List<Environment>();
	#endregion


	private static AssetManager _instance;
	public static AssetManager Instance { get { return _instance; } }
	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		} else {
			_instance = this;
		}
	}


	private ScreenManager SM;

	private string userRoot;
	public string rootDir;
	public string dataDir;
	public string mediaDir;
	public string inUseDir;

	public string filePrefix;

	const string rootYamlDocName = "corteva.config.yaml";
	string environmentsJsonDocName = "environments.json";
	string contentJsonDocName = "content_items.json";
	string filesJsonDocName = "files.json";
	string lastupdateJsonDocName = "last_update.json";
	string messagingJsonDocName = "messaging_buckets.json";
	string presentationsJsonDocName = "presentations.json";

	public string ParsePath(string _path){
		return (Application.platform == RuntimePlatform.WindowsPlayer) ? _path.Replace ("/", "\\") : _path;
	}

	private IEnumerator CheckForUpdates(){
		yield return new WaitForSecondsRealtime (5f * 60f);
	}

	public void Init(){
		SM = ScreenManager.Instance;
		filePrefix = (Application.platform == RuntimePlatform.WindowsPlayer) ? "" : "file://";
		ParseYamlConfig ();
	}


	private void ParseYamlConfig(){
		//FUTURE NOTE: if you're updatng these, make sure to make updates to PinData.cs as well.

		//get ref to user root
		userRoot = ParsePath(System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal)+"/");
		SM.Log ("user root: " + userRoot);

		//make ref to temp inuse folder to hold video files that are currenty being streamed by the app.
		inUseDir = ParsePath (userRoot + "corteva_inuse" +"/");

		//load root YAML file
		var input = new StringReader(File.ReadAllText (userRoot+rootYamlDocName));
		var yaml = new YamlStream();
		yaml.Load(input);

		//get document paths from YAML
		var rootNode = (YamlMappingNode)yaml.Documents[0].RootNode;
		rootDir = ParsePath (rootNode.Children [new YamlScalarNode ("root_path")] + "/");
		SM.Log ("\trootDir: " + rootDir);

		if ((string)rootNode.Children [new YamlScalarNode ("nickname")] != "") {
			displayName = rootNode.Children [new YamlScalarNode ("location")] + " (" + rootNode.Children [new YamlScalarNode ("nickname")] + ")";
		} else {
			displayName = (string)rootNode.Children [new YamlScalarNode ("location")];
		}

		var dataNode = (YamlMappingNode)rootNode.Children [new YamlScalarNode ("data")];
		dataDir = ParsePath (rootDir + dataNode.Children [new YamlScalarNode ("directory")] + "/");
		SM.Log ("\tdataDir: " + dataDir);

		lastupdateJsonDocName = (string)dataNode.Children [new YamlScalarNode ("last_updated_file")];

		var mediaNode = (YamlMappingNode)rootNode.Children [new YamlScalarNode ("media")];
		mediaDir = ParsePath (rootDir + mediaNode.Children [new YamlScalarNode ("directory")] + "/");
		SM.Log ("\tmediaDir: " + mediaDir);

		var filesNode = (YamlMappingNode)dataNode.Children [new YamlScalarNode ("files")];
		environmentsJsonDocName = (string)filesNode.Children [new YamlScalarNode ("environments")];
		contentJsonDocName = (string)filesNode.Children [new YamlScalarNode ("content_items")];
		filesJsonDocName = (string)filesNode.Children [new YamlScalarNode ("media")];
		messagingJsonDocName = (string)filesNode.Children [new YamlScalarNode ("messaging_buckets")];
		presentationsJsonDocName = (string)filesNode.Children [new YamlScalarNode ("presentations")];

		ParseJsonConfigs ();
	}

	private void ParseJsonConfigs(){
		//all media files
		string filesJSON = File.ReadAllText (dataDir + filesJsonDocName);
		var Nfiles = JSON.Parse(filesJSON);
		SM.Log ("filesJSON: (" + Nfiles.Count + ") " + (dataDir + filesJsonDocName));

		if (imageTextures.Count > 0) {
			//we've already loaded some textures, this must be an update
		}

		//create new directory to hold copies of BG videos while they're in use
		if(Directory.Exists(inUseDir)){
			//delete everything inside it
			Directory.Delete(inUseDir, true);
		}
		Directory.CreateDirectory (inUseDir);


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
					//copy all video files to new directory so that sync program can work in background
					//otherwise if the video files are in use by unity, the sunc fails

					//recreate path structure within "inuse" folder
					//grab base path
					string pth = ParsePath (Nfiles [i] ["path"]);
					//split by slash
					string[] p = (Application.platform == RuntimePlatform.WindowsPlayer) ? pth.Split (char.Parse("\\")) : pth.Split (char.Parse("/"));
					//keep track of dir length
					string currDir = inUseDir;
					//create directories as needed
					for (int n = 0; n < p.Length - 1; n++) {
						string newDir = ParsePath (currDir + p [n] + "/");
						if (!Directory.Exists (newDir)) {
							Directory.CreateDirectory (newDir);
						}
						currDir = newDir;
					}
					//update new path
					string newBGPath = ParsePath (currDir + Nfiles [i]  ["filename"]);
					//copy file to that path
					File.Copy (ParsePath (rootDir + Nfiles [i]  ["path"]), newBGPath);
					Debug.Log ("\t* " + newBGPath);

					//save old and new path references 
					videoFiles.Add (path);
					videoFilesInUse.Add (newBGPath);
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
			//check that the desired template is actually available
			if (baseTemplateNames.Contains (Ncp ["data"] [i] ["views"] ["front"] ["template"]) || customTemplateNames.Contains (Ncp ["data"] [i] ["views"] ["front"] ["template"])) {
				string envKey = Ncp ["data"] [i] ["environment"];
				int eI = environments.FindIndex (x => x.envKey == envKey);
				environments [eI].envPanelData.Add (Ncp ["data"] [i].ToString ());
			} else {
				SM.Log ("\t --[" + Ncp ["data"] [i] ["nid"] +"] "+Ncp ["data"] [i] ["reference_title"] + " (\"" + Ncp ["data"] [i] ["views"] ["front"] ["template"] + "\" TEMPLATE UNAVALABLE)");
			}
		}

		//presentations (which content panels / beauty panels to use in the idle state for each environment)
		string presentationsJSON = File.ReadAllText (dataDir + presentationsJsonDocName);
		var Npresentations = JSON.Parse(presentationsJSON);
		JSONNode scenes = Npresentations ["data"] [0] ["scenes"];
		SM.Log ("presentationsJSON: (" + scenes.Count + ") " + (dataDir + presentationsJsonDocName));
		for (int i = 0; i < scenes.Count; i++) 
		{
			if (scenes [i] ["scene_type"] == "welcome") {


				//welcome scene exception
				e = new Environment ();
				e.envID = -1;
				e.envKey = "welcome";
				e.envTitle = scenes [i] ["title"];
				e.envSummary = "";
				e.envColor = new Color32 ((byte)scenes [i] ["colorRGB"] [0].AsInt, (byte)scenes [i] ["colorRGB"] [1].AsInt, (byte)scenes [i] ["colorRGB"] [2].AsInt, 255);
				//e.envIconPath = ParsePath (rootDir + scenes [i] ["icon"] ["path"]);
				//e.envKioskBg = ParsePath (rootDir + scenes [i] ["kiosk_background_image"] ["path"]);
				e.envBg = ParsePath (rootDir + scenes [i] ["idle_background_video"] ["path"]);

				for (int ii = 0; ii < scenes [i] ["welcome_panels"].Count; ii++) {
					e.envPanelData.Add (scenes [i] ["welcome_panels"] [ii].ToString ());
				}

				e.btyPanelData = scenes [i] ["beauty_panels"];
				environments.Insert (0, e);
			
			
			} else {


				SM.Log ("\t" + scenes [i] ["environment"] +
				": " + scenes [i] ["content_panels"].Count + " content panels" +
				", " + scenes [i] ["beauty_panels"].Count + " beauty panels");

				//match "scene" environment from json and match to environment object
				string envKey = scenes [i] ["environment"];
				int eI = environments.FindIndex (x => x.envKey == envKey);
				environments [eI].btyPanelData = scenes [i] ["beauty_panels"];
			}


			/*
			for (int n = 0; n < scenes [i] ["beauty_panels"].Count; n++) {
				
				if (scenes [i] ["beauty_panels"] [n] ["front"] ["template"] == "beauty_1x1") 
				{
					environments [eI].bty1x1PanelData.Add (scenes [i] ["beauty_panels"] [n].ToString ());
				}

				if (scenes [i] ["beauty_panels"] [n] ["front"] ["template"] == "beauty_1x2") 
				{
					environments [eI].bty1x2PanelData.Add (scenes [i] ["beauty_panels"] [n].ToString ());
				}
			
			}
			Debug.Log (">>>>");
			Debug.Log (environments [eI].bty1x1PanelData.Count+" | "+environments [eI].bty1x2PanelData.Count);
			*/
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
			//the mipmap flag being set to true may throw an assertion error, but app will run fine.
			//this seems to be a unity bug.
			//Texture2D t = new Texture2D(2, 2, TextureFormat.DXT5, true);
			Texture2D t = new Texture2D(2, 2, TextureFormat.ARGB32, true);
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
			//Debug.Log ("- "+environments [i].btyPanelData.Count);
			for (int a = 0; a < environments[i].btyPanelData.Count; a++) {
				if (environments[i].btyPanelData [a]["front"]["template"] == "beauty_1x2") {
					environments[i].bty1x2Indeces.Add (a);
					environments [i].bty1x2PanelData.Add (environments[i].btyPanelData [a].ToString());
				} else {
					environments[i].bty1x1Indeces.Add (a);
					environments [i].bty1x1PanelData.Add (environments[i].btyPanelData [a].ToString());
				}
			}
			//Debug.Log ("\t- "+environments[i].bty1x1Indeces.Count);
			//Debug.Log ("\t- "+environments[i].bty1x2Indeces.Count);
		}

		//START APP IDLE LOOP
		ScreenManager.Instance.ToggleAdmin ();
		IdleStateController.Instance.Init ();
		IdleStateController.Instance.StartIdleLoop ();
	}

	#region PUBLIC methods

	public Texture GetTexture(string _name){
		//Debug.Log ("Getting: " + _name);
		int i = imageFiles.FindIndex (x => x.Contains (_name));
		return imageTextures [i];
	}

	public Texture GetRandomTexture(){
		return imageTextures [Random.Range (0, imageTextures.Count)];
	}


	public string GetVideo(string _name){
		//find file based on original path from json
		int i = videoFiles.FindIndex (x => x.Contains (_name));

		//return reference to same file but in the "inuse" directory
		return videoFilesInUse[i];

		//return videoFiles [i];
	}

	public string GetRandomVideo(){
		int r = Random.Range (0, videoFiles.Count);
		string vid = videoFiles [r];
		videoFiles.RemoveAt (r);
		if (videoFiles.Count == 0) {
			videoFiles = new List<string> (usedVideoFiles);
			usedVideoFiles.Clear ();
		}
		usedVideoFiles.Add (vid);
		return vid;
	}
	#endregion














	/*


	public void Init(){
		Debug.Log ("AssetManager [Init]");
		SetUpAssetPaths ();
		LoadAssets ();
		//ParseAppData ();
	}

	private void SetUpAssetPaths(){
		//file prefix
		filePrefix = "file://";
		relativeFolderOffset = -2;
		//returns absolute path of app on hd, backing up 2 directories to reach the folder containing this app. 
		basePath = Application.dataPath;
		ScreenManager.Instance.Log("app path: " + Application.dataPath);

		ScreenManager.Instance.Log ("platform: "+Application.platform.ToString());
		if (Application.platform == RuntimePlatform.WindowsPlayer) {
			relativeFolderOffset = -1;
			filePrefix = "";
		}
		basePath = basePath.Substring (0, GetNthIndex (basePath, char.Parse("/"), relativeFolderOffset));

		//asset folder name
		assetsFolder = basePath + "/assets/";
		if (Application.isEditor) {
			assetsFolder = basePath + "/_builds/assets/";
		}
		ScreenManager.Instance.Log("assets folder: "+assetsFolder);
	}


	/// <summary>
	/// Returns the index of the nth instance of a char in a string.
	/// </summary>
	/// <returns>The nth index.</returns>
	/// <param name="s">string to parse</param>
	/// <param name="t">char to find [try char.Parse(<string>)]</param>
	/// <param name="n">pos int nth, neg int for nth from last</param>
	public int GetNthIndex(string s, char t, int n){
		int count = 0;
		if (n > 0) {
			for (int i = 0; i < s.Length; i++) {
				if (s [i] == t) {
					count++;
					if (count == n) {
						return i;
					}
				}
			}
		} else if (n < 0) {
			count = 0;
			for (int i = s.Length - 1; i > 0; i--) {
				if (s [i] == t) {
					count--;
					if (count == n) {
						return i;
					}
				}
			}
		}
		return -1;
	}



	#region PRIVATE methods

	private IEnumerator LoadNewScene(string _scene) {
		ScreenManager.Instance.Log("Loading scene: "+_scene);

		AsyncOperation async = SceneManager.LoadSceneAsync(_scene, LoadSceneMode.Additive);

		while (!async.isDone) {
			yield return null;
		}

		ScreenManager.Instance.Log(" "+_scene+" scene loaded.");

		EventsManager.Instance.SceneFinishedLoading (_scene);
	}
	#endregion

*/
}
