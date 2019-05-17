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
	public string envTitle;
	public string envSummary;
	public string envIconPath;
	public string kioskBg;
	public Color32 envColor;
	public List<GameObject> envBackgroundPanels = new List<GameObject>();
	public JSONNode envPanelData;
	public JSONNode btyPanelData;
	public List<int> bty1x1Indeces = new List<int> ();
	public List<int> bty1x2Indeces = new List<int> ();
	public int env1x1Count;
}
/*
[System.Serializable]
public class Panel
{
	public string panelID;
	public List<PanelSide> panelSide = new List<PanelSide>();
}
[System.Serializable]
public class PanelSide
{
	public PanelBase.PanelView view; 
	public string template;
	public List<PanelAsset> panelAsset = new List<PanelAsset>();
}
[System.Serializable]
public class PanelAsset
{
	public string assetPath;
	public string assetType;
	public string assetFormat;
}
*/
public class AssetManager : MonoBehaviour {
	#region class variables
	private string basePath;
	private string filePrefix;
	private int relativeFolderOffset;
	private string assetsFolder;


	private List<string> texturesToLoad = new List<string>();
	[HideInInspector]
	public List<Texture2D> loadedTextures = new List<Texture2D>();


	[HideInInspector]
	public List<string> videoFiles = new List<string> ();
	private List<string> usedVideoFiles = new List<string> ();
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

	void Start(){
		
	}

	public void Init(){
		Debug.Log ("AssetManager [Init]");
		SetUpAssetPaths ();
		LoadAssets ();
		//ParseAppData ();
	}

	private void SetUpAssetPaths(){
		string userRoot;
		string rootPath;
		string rootYamlDocName = "corteva.config.yaml";
		string environmentsJsonDocName = "environments.json";
		string contentJsonDocName = "content_items.json";
		string filesJsonDocName = "files.json";
		string lastupdateJsonDocName = "last_update.json";
		string messagingJsonDocName = "messaging_buckets.json";
		string presentationsJsonDocName = "presentations.json";

		/*
		//get ref to user root
		userRoot = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal)+"/";
		ScreenManager.Instance.Log ("user root: " + userRoot);

		//load root YAML file
		var input = new StringReader(File.ReadAllText (userRoot+rootYamlDocName));
		var yaml = new YamlStream();
		yaml.Load(input);
		var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

		//get document paths from YAML
		rootPath = mapping.Children [new YamlScalarNode ("root_path")] + "/";
		ScreenManager.Instance.Log ("\troot_path: " + rootPath);

		*/
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

	private void ParseAppData(){
		string dataAsJson = File.ReadAllText (assetsFolder+"data.json");

		var N = JSON.Parse(dataAsJson);

		string assetPath;
		if (Application.platform == RuntimePlatform.WindowsPlayer) {
			assetPath = N ["asset_path_win"];
		} else {
			assetPath = N ["asset_path_mac"];
		}

		ScreenManager.Instance.Log("assets path: "+assetPath);

		Debug.Log ("Total Environemtns: "+N["environments"].Count);

		Environment e;
		for (int i = 0; i < N ["environments"].Count; i++) {
			Debug.Log("\t"+N ["environments"] [i] ["title"]);
			e = new Environment ();
			e.envID = i;
			e.envTitle = N ["environments"] [i] ["title"];
			e.envSummary = N ["environments"] [i] ["summary"];
			e.envColor = new Color32 ((byte)N ["environments"] [i] ["colorRGB"][0].AsInt, (byte)N ["environments"] [i] ["colorRGB"][1].AsInt, (byte)N ["environments"] [i] ["colorRGB"][2].AsInt, 255);
			e.envIconPath = assetPath + N ["environments"] [i] ["iconPath"];
			e.btyPanelData = N ["environments"] [i] ["beauty_panels"];
			e.envPanelData = N ["environments"] [i] ["content_panels"];
			e.kioskBg = N ["environments"] [i] ["kiosk_background_image_16x9"];

			if (ScreenManager.Instance.currAspect == ScreenManager.Aspect.is169) {
				GameObject panelBaseGO = Instantiate (NEWpanelPrefab, idleBackgrounds);
				PanelBase panelBase = panelBaseGO.GetComponent<PanelBase> ();
				panelBase.panelID = e.envTitle + "_bg";
				panelBaseGO.name = panelBase.panelID;
				panelBase.environment = e;
				panelBase.AssembleBasic ("16x9_bg", N ["environments"] [i] ["idle_background_video_16x9"]);
				e.envBackgroundPanels.Add (panelBaseGO);
			} else {
				GameObject panelBaseGO = Instantiate (NEWpanelPrefab, idleBackgrounds);
				PanelBase panelBase = panelBaseGO.GetComponent<PanelBase> ();
				panelBase.panelID = e.envTitle + "_bg";
				panelBaseGO.name = panelBase.panelID;
				panelBase.environment = e;
				panelBase.AssembleBasic ("32x9_bg", N ["environments"] [i] ["idle_background_video_32x9"]);
				e.envBackgroundPanels.Add (panelBaseGO);
			}

			e.env1x1Count = e.envPanelData.Count;
			for (int a = 0; a < e.btyPanelData.Count; a++) {
				if (e.btyPanelData [a]["front"]["template"] == "beauty_1x2") {
					e.bty1x2Indeces.Add (a);
				} else {
					e.bty1x1Indeces.Add (a);
				}
			}
				
			environments.Add (e);
		}

		IdleStateController.Instance.Init ();
	}


	#region PUBLIC methods
	public void LoadAssets(){
		StartCoroutine (LoadImages ());
	}

	public void LoadScene(string _scene){
		StartCoroutine (LoadNewScene (_scene));
	}

	public Texture GetTexture(string _name){
		//Debug.Log ("Getting: " + _name);
		int i = texturesToLoad.FindIndex (x => x.Contains (_name));
		return loadedTextures [i];
	}

	public Texture GetRandomTexture(){
		return loadedTextures [Random.Range (0, loadedTextures.Count)];
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

	private IEnumerator LoadImages () {
		//check which platform we're on
		//Application.platform


		//macOS

		//will return root user folder on mac
		//System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)


		Debug.Log ("system memory size: " + SystemInfo.systemMemorySize+"MB");
		Debug.Log ("graphics memory size: " + SystemInfo.graphicsMemorySize+"MB");
//		System.GC.Collect();
//		Debug.Log ("using [" + UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong () + "] of [" + UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong () + "]");

		//TEMP
		//save paths of files from the following directories
		DirectoryInfo dir = new DirectoryInfo(assetsFolder+"images/");
		FileInfo[] info = dir.GetFiles("*.jpg");
		foreach (FileInfo fi in info){
			texturesToLoad.Add (filePrefix + fi.FullName);
		}

		dir = new DirectoryInfo(assetsFolder+"misc/");
		info = dir.GetFiles("*.png");
		foreach (FileInfo fi in info){
			texturesToLoad.Add (filePrefix + fi.FullName);
		}

		dir = new DirectoryInfo(assetsFolder+"videos/");
		info = dir.GetFiles("*.mp4");
		foreach (FileInfo fi in info){
			videoFiles.Add (filePrefix + fi.FullName);
		}

		//preload images to textures
		ScreenManager.Instance.Log("Loading textures...");
		foreach(string file in texturesToLoad)
		{
			ScreenManager.Instance.Log("  loading: " + file);
			WWW www = new WWW(file);
			while (!www.isDone)
			{
				yield return null;
			}
			Texture2D t = new Texture2D(2, 2, TextureFormat.DXT5, false);
			www.LoadImageIntoTexture(t);
			loadedTextures.Add (t);
		}

		ScreenManager.Instance.Log(" texture loading DONE"); 
		ScreenManager.Instance.ToggleAdmin ();

		ParseAppData ();

		//EventsManager.Instance.AssetsFinishedLoading();

		yield return null;
	}

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
}
