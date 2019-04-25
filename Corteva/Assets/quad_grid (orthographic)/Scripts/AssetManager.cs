using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.IO;
/*
{
	"environment": {
		"title": "Global",
		"summary": "Global Summary",
		"backgrounds": [
			{
				"type": "image", 
				"path": "file_path"
			},
		],
		"beauty_panels": [
			{
				"type": "video", 
				"path": "file_path"
			},
		],
		"content_panels":[
			{
				"type": "",
				"asset_path": "file_path",
				"message": "",
				"headline": "",
				"body": "", 
				"data": {
					"data_point_a": ,
					"data_point_b": 
				}
			},
		]
	}
}
*/


[System.Serializable]
public class Environment
{
	public int envID;
	public Color32 envColor;
	public Texture2D envIcon;
	public string envTitle;
	public string envSummary;
	public List<string> envBackgroundVideos;
}

public class AssetManager : MonoBehaviour {
	#region class variables
	[HideInInspector]
	public string basePath;
	private List<string> filesToLoad1x1 = new List<string>();
	private List<string> filesToLoad1x2 = new List<string>();
	[HideInInspector]
	public List<Texture2D> loadedTextures1x1 = new List<Texture2D>();
	[HideInInspector]
	public List<Texture2D> loadedTextures1x2 = new List<Texture2D>();
	[HideInInspector]
	public List<string> videoFiles = new List<string> ();
	private List<string> usedVideoFiles = new List<string> ();
	[HideInInspector]
	public List<string> hdVideoFiles = new List<string> ();
	private List<string> usedHdVideoFiles = new List<string> ();
	[HideInInspector]
	public List<string> videoFiles329 = new List<string> ();
	private List<string> usedVideoFiles329 = new List<string> ();
	#endregion


	#region game object references
	[Header("GameObject references")]
	public Camera mainCamera;
	public Camera userInitCamera;
	public Camera userPanelCamera;
	public Transform panels;
	public Transform cams;
	public Transform kiosks;
	public PanelObject bgPanel1;
	public PanelObject bgPanel2;
	#endregion


	#region prefabs
	[Header("Prefabs")]
	public GameObject panelPrefab;
	public GameObject cellCameraPrefab;
	public GameObject userKioskPrefab;
	public List<GameObject> panelPool = new List<GameObject> ();
	public List<Environment> environments = new List<Environment>();
	#endregion


	#region events
//	public delegate void AssetsFinishedLoadingEvent();
//	public event AssetsFinishedLoadingEvent OnAssetsFinishedLoading;
//
//	public delegate void SceneFinishedLoadingEvent(string _scene);
//	public event SceneFinishedLoadingEvent OnSceneFinishedLoading;
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

	//TODO
	//read the initializing JSON file
	//preload all assets
	//poll JSON file for new changes
		//load new assets if needed

	void Start(){
		//TEMP
		Environment e = new Environment ();
		e.envTitle = "Globe";
		e.envColor = new Color32 (0, 114, 206, 255);
		environments.Add (e);
		e = new Environment ();
		e.envTitle = "Farm";
		e.envColor = new Color32 (0, 191, 111, 255);
		environments.Add (e);
		e = new Environment ();
		e.envTitle = "Plant";
		e.envColor = new Color32 (252, 76, 2, 255);
		environments.Add (e);
		e = new Environment ();
		e.envTitle = "Seed";
		e.envColor = new Color32 (32, 32, 32, 255);
		environments.Add (e);
	}




	#region PUBLIC methods
	public void LoadAssets(){
		StartCoroutine (LoadImages ());
	}

	public void LoadScene(string _scene){
		StartCoroutine (LoadNewScene (_scene));
	}

	public Texture GetRandomTexture(){
		return loadedTextures1x1 [Random.Range (0, loadedTextures1x1.Count)];
	}
	public Texture GetRandomTexture1x2(){
		return loadedTextures1x2 [Random.Range (0, loadedTextures1x2.Count)];
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

	public string GetRandomHDVideo(){
		return GetRandomVideo ();
//		int r = Random.Range (0, hdVideoFiles.Count);
//		string vid = hdVideoFiles [r];
//		hdVideoFiles.RemoveAt (r);
//		if (hdVideoFiles.Count == 0) {
//			hdVideoFiles = new List<string> (usedHdVideoFiles);
//			usedHdVideoFiles.Clear ();
//		}
//		usedHdVideoFiles.Add (vid);
//		return vid;
	}

	public string GetRandom329Video(){
		int r = Random.Range (0, videoFiles329.Count);
		string vid = videoFiles329 [r];
		videoFiles329.RemoveAt (r);
		if (videoFiles329.Count == 0) {
			videoFiles329 = new List<string> (usedVideoFiles329);
			usedVideoFiles329.Clear ();
		}
		usedVideoFiles329.Add (vid);
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

		//file prefix
		string f = "file://";
		int offset = -2;
		//returns absolute path of app on hd, backing up 2 directories to reach the folder containing this app. 
		basePath = Application.dataPath;

		ScreenManager.Instance.Log ("platform: "+Application.platform.ToString());
		if (Application.platform == RuntimePlatform.WindowsPlayer) {
			//offset = -1;
			f = "";
		}
		basePath = basePath.Substring (0, GetNthIndex (basePath, char.Parse("/"), offset));
		ScreenManager.Instance.Log("app path: " + Application.dataPath);

		//asset folder name
		string assetsFolder = basePath + "/assets/";
		if (Application.isEditor) {
			assetsFolder = basePath + "/_builds/assets/";
		}
		ScreenManager.Instance.Log("assets path: "+assetsFolder);

		Debug.Log ("system memory size: " + SystemInfo.systemMemorySize+"MB");
		Debug.Log ("graphics memory size: " + SystemInfo.graphicsMemorySize+"MB");
//		System.GC.Collect();
//		Debug.Log ("using [" + UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong () + "] of [" + UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong () + "]");

		//TEMP
		//save paths of files from the following directories
		DirectoryInfo dir = new DirectoryInfo(assetsFolder+"images_1x1/");
		FileInfo[] info = dir.GetFiles("*.jpg");
		foreach (FileInfo fi in info){
			filesToLoad1x1.Add (f + fi.FullName);
		}

		dir = new DirectoryInfo(assetsFolder+"images_1x2/");
		info = dir.GetFiles("*.jpg");
		foreach (FileInfo fi in info){
			filesToLoad1x2.Add (f + fi.FullName);
		}

		dir = new DirectoryInfo(assetsFolder+"videos/");
		info = dir.GetFiles("*.mp4");
		foreach (FileInfo fi in info){
			videoFiles.Add (f + fi.FullName);
		}

		dir = new DirectoryInfo(assetsFolder+"videos_HD/");
		info = dir.GetFiles("*.mp4");
		foreach (FileInfo fi in info){
			hdVideoFiles.Add (f + fi.FullName);
		}

		dir = new DirectoryInfo(assetsFolder+"videos_HD_329/");
		info = dir.GetFiles("*.mp4");
		foreach (FileInfo fi in info){
			videoFiles329.Add (f + fi.FullName);
		}


		//preload images to textures
		ScreenManager.Instance.Log("Loading textures...");
		foreach(string file in filesToLoad1x1)
		{
			ScreenManager.Instance.Log("  loading: " + file);
			WWW www = new WWW(file);
			while (!www.isDone)
			{
				yield return null;
			}
			Texture2D t = new Texture2D(2, 2);
			www.LoadImageIntoTexture(t);
			loadedTextures1x1.Add (t);
		}

		foreach(string file in filesToLoad1x2)
		{
			ScreenManager.Instance.Log("  loading: " + file);
			WWW www = new WWW(file);
			while (!www.isDone)
			{
				yield return null;
			}
			Texture2D t = new Texture2D(2, 2);
			www.LoadImageIntoTexture(t);
			loadedTextures1x2.Add (t);
		}

		ScreenManager.Instance.Log(" texture loading DONE"); 
		ScreenManager.Instance.ToggleAdmin ();

		EventsManager.Instance.AssetsFinishedLoading();

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
