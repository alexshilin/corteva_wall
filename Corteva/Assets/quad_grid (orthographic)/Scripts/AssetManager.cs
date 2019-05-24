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


	public List<string> imageFiles = new List<string>();
	[HideInInspector]
	public List<Texture2D> imageTextures = new List<Texture2D>();


	public List<string> videoFiles = new List<string> ();
	private List<string> usedVideoFiles = new List<string> ();

	public JSONNode pins;
	#endregion


	#region game object references
	[Header("GameObject references")]
	public Camera mainCamera;
	public Camera userInitCamera;
	public Camera userPanelCamera;
	public Transform panels;
	public Transform cams;
	public Transform kiosks;
	public Transform idleBackgrounds;
	public PanelObject bgPanel1;
	public PanelObject bgPanel2;
	#endregion


	#region prefabs
	[Header("Prefabs")]
	public GameObject panelPrefab;
	public GameObject NEWpanelPrefab;
	public GameObject cellCameraPrefab;
	public GameObject userKioskPrefab;
	public List<GameObject> panelPool = new List<GameObject> ();
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
			environments [eI].envPanelData.Add(Ncp ["data"] [i].ToString());
		}

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
			Debug.Log ("- "+environments [i].btyPanelData.Count);
			for (int a = 0; a < environments[i].btyPanelData.Count; a++) {
				if (environments[i].btyPanelData [a]["front"]["template"] == "beauty_1x2") {
					environments[i].bty1x2Indeces.Add (a);
				} else {
					environments[i].bty1x1Indeces.Add (a);
				}
			}
			Debug.Log ("\t- "+environments[i].bty1x1Indeces.Count);
			Debug.Log ("\t- "+environments[i].bty1x2Indeces.Count);
		}

		ScreenManager.Instance.ToggleAdmin ();
		IdleStateController.Instance.Init ();
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
		int i = videoFiles.FindIndex (x => x.Contains (_name));
		return videoFiles [i];
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
