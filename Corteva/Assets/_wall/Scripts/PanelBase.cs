using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;
using TMPro;
using SimpleJSON;


public class PanelBase : MonoBehaviour {

	[Header("Components")]
	//references to the container game objects within the panelbase game object
	public Transform front;
	public Transform back;
	public Transform thumbnail;
	public Collider collide;

	//enums for panel states
	public enum PanelContext
	{
		None,
		Idle,
		Kiosk
	}
	public enum PanelState
	{
		Ready,
		Animating,
		Active,
		Hidden
	}
	public enum PanelView
	{
		Blank,
		Background,
		Thumbnail,
		Front,
		Back
	}

	//settings for position and rotation of panel views when the panel gets flipped betwen views
	private Vector3 forwardPos = new Vector3 (0, 0, -0.01f);
	private Vector3 forwardRot = new Vector3 (0, 0, 0);
	private Vector3 awayPos = new Vector3 (0, 0, 0.01f);
	private Vector3 awayRot = new Vector3 (0, 180, 0);



	[Header("Dont Edit in Inspector")]
	//ref for current panel view
	public PanelView currViewFacingForward;
	public PanelView currViewFacingAway;
	//panel states
	public PanelState panelState;
	public PanelContext panelContext = PanelContext.None;
	public PanelView panelView = PanelView.Blank;

	//environment data for this panel
	public Environment environment{ get; set; }
	//id from JSON
	public string panelID;
	public string panelName;
	//gridID is to reference its physical position in idle state
	public int gridID;
	//ref to panels kiosk parent
	public UserKiosk myKiosk;
	//col,row of panel in idle state
	public Vector2 panelGridPos;

	//gesture component references
	private TapGesture tapGesture;
	private TransformGesture transformGesture;
	private FlickGesture flickGesture;

	//ref to pmp where panel module prefabs are referenced
	//used for assembling various templates
	private PanelModulePool PMP;

	private AssetManager AM;


	void Awake(){
		panelState = PanelState.Ready;
	}


	private void OnEnable()
	{
		//get gesture components
		tapGesture = GetComponent<TapGesture> ();
		transformGesture = GetComponent<TransformGesture> ();
		flickGesture = GetComponent<FlickGesture> ();

		//register for their events
		tapGesture.Tapped += tappedHandler;

		transformGesture.TransformStarted += transformStartedHandler;
		transformGesture.Transformed += transformedHandler;
		transformGesture.TransformCompleted += transformCompletedHandler;

		flickGesture.Flicked += flickHandler;

		//get ref to pmp
		PMP = PanelModulePool.Instance;
		AM = AssetManager.Instance;
	}

	private void OnDisable()
	{
		//unregidter from events
		tapGesture.Tapped -= tappedHandler;

		transformGesture.TransformStarted -= transformStartedHandler;
		transformGesture.Transformed -= transformedHandler;
		transformGesture.TransformCompleted -= transformCompletedHandler;

		flickGesture.Flicked -= flickHandler;
	}

	void Start(){
		
	}


	void Update(){
		
	}

	/// <summary>
	/// Step 1 in panel assembly. Instantiates the required components based on templates/data received from JSON
	/// </summary>
	/// <param name="_panelData">JSON panel data.</param>
	public void Assemble(JSONNode _panelData)
	{
		//Debug.Log (_panelData);
		Debug.Log ("[Assemble] " + _panelData ["reference_title"] + ": " + ((_panelData ["views"]["front"].Count > 0) ? "front" : "nil") + " , " + ((_panelData ["views"]["back"].Count > 0) ? "back" : "nil") + " , " + ((_panelData ["views"]["thumbnail"].Count > 0) ? "thumb" : "nil"));
		//it is not necessary for a panel to have all three views
		//this only generates those specified
		if (_panelData["views"]["front"].Count > 0) {
			AssembleView (_panelData ["views"]["front"], PanelView.Front);
		}
		if (_panelData ["views"]["back"].Count > 0) {
			AssembleView (_panelData ["views"]["back"], PanelView.Back);
		}
		if (_panelData ["views"]["thumbnail"].Count > 0) {
			AssembleView (_panelData ["views"]["thumbnail"], PanelView.Thumbnail);
		}
	}

	public void AssembleBeauty(JSONNode _panelData)
	{
		//Debug.Log (_panelData);
		Debug.Log ("[AssembleBeauty] " + _panelData ["reference_title"] + ": " + ((_panelData ["front"].Count > 0) ? "front" : "nil") );
		//it is not necessary for a panel to have all three views
		//this only generates those specified
		if (_panelData["front"].Count > 0) {
			AssembleView (_panelData ["front"], PanelView.Front);
		}
	}

	/// <summary>
	/// Alternate to Assemble. This is used for one off templates that dont get definied in the JSON.
	/// (e.g. backgrounds, titles, etc)
	/// </summary>
	/// <param name="_template">Template string name</param>
	/// <param name="_path">Path to relevant image/view asset (if applicable)</param>
	public void AssembleBasic(string _template, string _path = ""){
		GameObject t;
		Renderer panelRenderer;

		if (_template == "16x9_bg") {
			t = LoadModule ("1x1_texture", PanelView.Front);
			t.name = "1x1_texture";
			VideoPlayer vid = t.transform.Find ("TextureQuad").GetComponent<VideoPlayer> ();
			vid.url = AssetManager.Instance.GetVideo (_path);
			vid.enabled = true;
			vid.Prepare ();
			//vid.Play ();
			return;
		}

		if (_template == "32x9_bg") {
			t = LoadModule ("2x1_texture", PanelView.Front);
			t.name = "2x1_texture";
			VideoPlayer vid = t.transform.Find ("TextureQuad").GetComponent<VideoPlayer> ();
			vid.url = AssetManager.Instance.GetVideo (_path);
			vid.enabled = true;
			vid.Prepare ();
			//vid.Play ();
			return;
		}

		if (_template == "kiosk_bg") {
			t = LoadModule ("1x1_texture", PanelView.Front);
			t.name = "1x1_texture";
			panelRenderer = t.transform.Find("TextureQuad").GetComponent<Renderer> ();
			panelRenderer.material.mainTexture = AssetManager.Instance.GetTexture (_path);
			return;
		}

		if (_template == "title_idle") {
			t = LoadModule ("1x1_texture_color_02", PanelView.Front);
			t.transform.Find ("ColorQuad").GetComponent<Renderer> ().material.color = environment.envColor;
			panelRenderer = t.transform.Find("TextureQuad").GetComponent<Renderer> ();
			panelRenderer.material.mainTexture = AssetManager.Instance.GetTexture (environment.envIconPath);
			return;
		}

		if (_template == "title_kiosk") {
			t = LoadModule ("1x1_kiosk_title", PanelView.Front);
			t.name = "1x1_kiosk_title";
			t.transform.Find("Bg").GetComponent<Renderer> ().material.SetColor ("_color1", environment.envColor);
			t.transform.Find("Icon").GetComponent<Renderer> ().material.mainTexture = AssetManager.Instance.GetTexture (environment.envIconPath);
			t.transform.Find ("Title").GetComponent<TextMeshPro> ().text = environment.envTitle;
			t.transform.Find ("Body").GetComponent<TextMeshPro> ().text = environment.envSummary;
			return;
		}
	}

	/// <summary>
	/// Step 2 of panel assembly. This grabs the necessary modules required for a template, assembles them, and populates with data.
	/// </summary>
	/// <param name="_templateData">Template data from JSON.</param>
	/// <param name="_view">The view (front, back, thumb) this template belongs to.</param>
	public void AssembleView(JSONNode _templateData, PanelView _view)
	{
		GameObject t;
		Renderer panelRenderer;
		TextMeshPro text;
		Color txtColor;
		Color bgColor;



		string template = _templateData ["template"];
		string bgPath = AM.ParsePath (AM.rootDir + _templateData ["content"] ["bg_path"]);
		//Debug.Log ("[AssembleView] "+template +" "+ _view+" "+bgPath);

		if (template == "beauty_1x1") {
			t = LoadModule ("1x1_texture_color", _view);

			bool isVideo = _templateData ["content"] ["bg_type"] == "video" ? true : false;
			if (isVideo) {
				VideoPlayer vid = t.transform.Find ("TextureQuad").GetComponent<VideoPlayer> ();
				vid.url = AssetManager.Instance.GetVideo (bgPath);
				vid.enabled = true;
				vid.Prepare ();
				if (_view == PanelView.Front)
					vid.Play ();
			} else {
				panelRenderer = t.transform.Find ("TextureQuad").GetComponent<Renderer> ();
				panelRenderer.material.mainTexture = AssetManager.Instance.GetTexture (bgPath);
			}
			return;
		}



		if (template == "beauty_1x2") {
			t = LoadModule ("1x2_beauty", _view);

			bool isVideo = _templateData ["content"] ["bg_type"] == "video" ? true : false;
			if (isVideo) {
				VideoPlayer vid = t.transform.Find ("TextureQuad").GetComponent<VideoPlayer> ();
				vid.url = AssetManager.Instance.GetVideo (bgPath);
				vid.enabled = true;
				vid.Prepare ();
				if (_view == PanelView.Front)
					vid.Play ();
			} else {
				panelRenderer = t.transform.Find ("TextureQuad").GetComponent<Renderer> ();
				panelRenderer.material.mainTexture = AssetManager.Instance.GetTexture (bgPath);
			}
			return;
		}



		if (template == "template_01") {
			//
			t = LoadModule ("1x1_texture_color", _view);

			bool isVideo = _templateData ["content"] ["bg_type"] == "video" ? true : false;
			if (isVideo) {
				VideoPlayer vid = t.transform.Find ("TextureQuad").GetComponent<VideoPlayer> ();
				vid.url = AssetManager.Instance.GetVideo (bgPath);
				vid.enabled = true;
				vid.Prepare ();
				vid.Play ();
			} else {
				bool isImage = _templateData ["content"] ["bg_type"] == "image" ? true : false;
				if (isImage) {
					panelRenderer = t.transform.Find ("TextureQuad").GetComponent<Renderer> ();
					panelRenderer.material.mainTexture = AssetManager.Instance.GetTexture (bgPath);
				} else {
					if (_templateData ["content"]["bg_color"].Count == 3) {
						t.transform.Find ("TextureQuad").GetComponent<Renderer> ().material.color = new Color32 ((byte)_templateData ["content"]["bg_color"][0].AsInt, (byte)_templateData ["content"]["bg_color"][1].AsInt, (byte)_templateData ["content"]["bg_color"][2].AsInt, 255);;
					} else {
						t.transform.Find ("TextureQuad").GetComponent<Renderer> ().material.color = environment.envColor;
					}
				}
			}

			//
			t = LoadModule ("1x1_txt_layout_02", _view);

			txtColor = Color.white;
			if (_templateData ["content"]["txt_color"].Count == 3) {
				txtColor = new Color32 ((byte)_templateData ["content"]["txt_color"][0].AsInt, (byte)_templateData ["content"]["txt_color"][1].AsInt, (byte)_templateData ["content"]["txt_color"][2].AsInt, 255);
			}
			t.GetComponent<PanelText> ().SetText ("", _templateData ["content"]["title"], _templateData ["content"]["body"], txtColor);
			t.transform.localPosition += transform.forward * -0.01f;

			//
			t = LoadModule ("1x1_extras", _view);

			t.GetComponent<PanelExtras> ().ColorBtns (environment.envColor, Color.white);

			t.transform.localPosition += transform.forward * -0.02f;

			return;
		}

		if (template == "template_02_tl" || 
			template == "template_02_tr") 
		{
			//
			string imgModule = (template == "template_02_tl") ? "1x2_texture_color_r" : "1x2_texture_color_l";
			t = LoadModule (imgModule, _view);
		
			bool isVideo = _templateData ["content"] ["bg_type"] == "video" ? true : false;
			if (isVideo) {
				VideoPlayer vid = t.transform.Find ("TextureQuad").GetComponent<VideoPlayer> ();
				vid.url = AssetManager.Instance.GetVideo (bgPath);
				vid.enabled = true;
				vid.Prepare ();
				if (_view == PanelView.Front)
					vid.Play ();
			} else {
				panelRenderer = t.transform.Find ("TextureQuad").GetComponent<Renderer> ();
				panelRenderer.material.mainTexture = AssetManager.Instance.GetTexture (bgPath);
			}

			if (_templateData ["content"]["bg_color"].Count == 3) {
				t.transform.Find ("ColorQuad").GetComponent<Renderer> ().material.color = new Color32 ((byte)_templateData ["content"]["bg_color"][0].AsInt, (byte)_templateData ["content"]["bg_color"][1].AsInt, (byte)_templateData ["content"]["bg_color"][2].AsInt, 255);;
			} else {
				t.transform.Find ("ColorQuad").GetComponent<Renderer> ().material.color = environment.envColor;
			}


			//
			string txtModule = (template == "template_02_tl") ? "1x2_txt_layout_l" : "1x2_txt_layout_r";
			t = LoadModule (txtModule, _view);

			txtColor = Color.white;
			if (_templateData ["content"]["txt_color"].Count == 3) {
				txtColor = new Color32 ((byte)_templateData ["content"]["txt_color"][0].AsInt, (byte)_templateData ["content"]["txt_color"][1].AsInt, (byte)_templateData ["content"]["txt_color"][2].AsInt, 255);
			}
			t.GetComponent<PanelText> ().SetText ("", _templateData ["content"]["title"], _templateData ["content"]["body"], txtColor);
			t.transform.localPosition += transform.forward * -0.01f;


			//
			t = LoadModule ("1x1_extras", _view);
			if ((template == "template_02_tl" && _view == PanelView.Front)
				|| (template == "template_02_tr" && _view == PanelView.Back)) 
			{
				//buttons are image-side
				t.GetComponent<PanelExtras> ().ColorBtns (environment.envColor, Color.white);
			} else if ((template == "template_02_tl" && _view == PanelView.Back)
						|| (template == "template_02_tr" && _view == PanelView.Front)) 
			{
				//buttons are text-siz\de
				t.GetComponent<PanelExtras> ().ColorBtns (new Color32(230, 231, 232, 255), environment.envColor);
			}
			t.transform.localPosition += transform.forward * -0.02f;

			return;
		}


	
		if (template == "template_03") {
			//
			t = LoadModule ("1x1_texture_color", _view);

			bool isVideo = _templateData ["content"] ["bg_type"] == "video" ? true : false;
			if (isVideo) {
				VideoPlayer vid = t.transform.Find ("TextureQuad").GetComponent<VideoPlayer> ();
				vid.url = AssetManager.Instance.GetVideo (bgPath);
				vid.enabled = true;
				vid.Prepare ();
				vid.Play ();
			} else {
				bool isImage = _templateData ["content"] ["bg_type"] == "image" ? true : false;
				if (isImage) {
					panelRenderer = t.transform.Find ("TextureQuad").GetComponent<Renderer> ();
					panelRenderer.material.mainTexture = AssetManager.Instance.GetTexture (bgPath);
				} else {
					if (_templateData ["content"]["bg_color"].Count == 3) {
						t.transform.Find ("TextureQuad").GetComponent<Renderer> ().material.color = new Color32 ((byte)_templateData ["content"]["bg_color"][0].AsInt, (byte)_templateData ["content"]["bg_color"][1].AsInt, (byte)_templateData ["content"]["bg_color"][2].AsInt, 255);;
					} else {
						t.transform.Find ("TextureQuad").GetComponent<Renderer> ().material.color = environment.envColor;
					}
				}
			}

			//
			t = LoadModule ("1x1_txt_layout_03", _view);

			txtColor = Color.white;
			if (_templateData ["content"]["txt_color"].Count == 3) {
				txtColor = new Color32 ((byte)_templateData ["content"]["txt_color"][0].AsInt, (byte)_templateData ["content"]["txt_color"][1].AsInt, (byte)_templateData ["content"]["txt_color"][2].AsInt, 255);
			}
			t.GetComponent<PanelText> ().SetText ("", _templateData ["content"]["title"], _templateData ["content"]["body"], txtColor);
			t.transform.localPosition += transform.forward * -0.01f;


			//
			t = LoadModule ("1x1_extras", _view);

			t.GetComponent<PanelExtras> ().ColorBtns (environment.envColor, Color.white);
			t.transform.localPosition += transform.forward * -0.02f;

			return;
		}



		//standard back view template
		if (template == "template_04") {
			t = LoadModule ("1x1_texture_color", _view);
			if (_templateData ["content"]["bg_color"].Count == 3) {
				t.transform.Find ("TextureQuad").GetComponent<Renderer> ().material.color = new Color32 ((byte)_templateData ["content"]["bg_color"][0].AsInt, (byte)_templateData ["content"]["bg_color"][1].AsInt, (byte)_templateData ["content"]["bg_color"][2].AsInt, 255);
			} else {
				t.transform.Find ("TextureQuad").GetComponent<Renderer> ().material.color = environment.envColor;
			}

			t = LoadModule ("1x1_txt_layout_04", _view);

			txtColor = Color.white;
			if (_templateData ["content"]["txt_color"].Count == 3) {
				txtColor = new Color32 ((byte)_templateData ["content"]["txt_color"][0].AsInt, (byte)_templateData ["content"]["txt_color"][1].AsInt, (byte)_templateData ["content"]["txt_color"][2].AsInt, 255);
			}
			t.GetComponent<PanelText> ().SetText ("", _templateData ["content"]["title"], _templateData ["content"]["body"], txtColor);
			t.transform.localPosition += transform.forward * -0.01f;

			t = LoadModule ("1x1_extras", _view);

			//t.GetComponent<PanelExtras> ().ColorBtns (environment.envColor, Color.white);
			t.GetComponent<PanelExtras> ().ColorBtns (new Color32(230, 231, 232, 255), environment.envColor);
			t.transform.localPosition += transform.forward * -0.02f;

			return;
		}




		//image with infocard
		if (template == "template_05") {
			//
			t = LoadModule ("1x1_texture_color", _view);

			bool isVideo = _templateData ["content"] ["bg_type"] == "video" ? true : false;
			if (isVideo) {
				VideoPlayer vid = t.transform.Find ("TextureQuad").GetComponent<VideoPlayer> ();
				vid.url = AssetManager.Instance.GetVideo (bgPath);
				vid.enabled = true;
				vid.Prepare ();
				vid.Play ();
			} else {
				bool isImage = _templateData ["content"] ["bg_type"] == "image" ? true : false;
				if (isImage) {
					panelRenderer = t.transform.Find ("TextureQuad").GetComponent<Renderer> ();
					panelRenderer.material.mainTexture = AssetManager.Instance.GetTexture (bgPath);
				} else {
					if (_templateData ["content"]["bg_color"].Count == 3) {
						t.transform.Find ("TextureQuad").GetComponent<Renderer> ().material.color = new Color32 ((byte)_templateData ["content"]["bg_color"][0].AsInt, (byte)_templateData ["content"]["bg_color"][1].AsInt, (byte)_templateData ["content"]["bg_color"][2].AsInt, 255);;
					} else {
						t.transform.Find ("TextureQuad").GetComponent<Renderer> ().material.color = environment.envColor;
					}
				}
			}

			//
			t = LoadModule ("1x1_txt_layout_05", _view);
			txtColor = Color.white;
			if (_templateData ["content"]["txt_color"].Count == 3) {
				txtColor = new Color32 ((byte)_templateData ["content"]["txt_color"][0].AsInt, (byte)_templateData ["content"]["txt_color"][1].AsInt, (byte)_templateData ["content"]["txt_color"][2].AsInt, 255);
			}
			bgColor = environment.envColor;
			if (_templateData ["content"]["bg_color"].Count == 3) {
				bgColor = new Color32 ((byte)_templateData ["content"]["bg_color"][0].AsInt, (byte)_templateData ["content"]["bg_color"][1].AsInt, (byte)_templateData ["content"]["bg_color"][2].AsInt, 255);;
			}
			t.GetComponentInChildren<InfoCard> ().SetText (_templateData ["content"]["title"], _templateData ["content"]["body"], bgColor, new Color(0,0,0,0.75f), txtColor);
			t.transform.localPosition += transform.forward * -0.01f;


			//
			t = LoadModule ("1x1_extras", _view);

			t.GetComponent<PanelExtras> ().ColorBtns (environment.envColor, Color.white);
			t.transform.localPosition += transform.forward * -0.02f;

			return;
		}

		if (template == "flows_of_trade") {
			t = LoadModule ("flows_of_trade", _view);

			t = LoadModule ("1x1_extras", _view);

			t.GetComponent<PanelExtras> ().ColorBtns (environment.envColor, Color.white);
			t.transform.localPosition += transform.forward * -0.02f;

			return;
		}

		if (template == "food_waste") {
			//
			t = LoadModule ("food_waste", _view);

			//
			t = LoadModule ("1x1_extras", _view);

			t.GetComponent<PanelExtras> ().ColorBtns (environment.envColor, Color.white);
			t.transform.localPosition += transform.forward * -0.02f;

			return;
		}

		if (template == "pearl_millet") {
			//
			t = LoadModule ("1x2_txt_layout_r", _view);

			txtColor = Color.white;
			if (_templateData ["content"]["txt_color"].Count == 3) {
				txtColor = new Color32 ((byte)_templateData ["content"]["txt_color"][0].AsInt, (byte)_templateData ["content"]["txt_color"][1].AsInt, (byte)_templateData ["content"]["txt_color"][2].AsInt, 255);
			}
			t.GetComponent<PanelText> ().SetText ("", _templateData ["content"]["title"], _templateData ["content"]["body"], txtColor);
			t.transform.localPosition += transform.forward * -0.01f;

			//
			t = LoadModule ("pearl_millet", _view);
			t.transform.localPosition += transform.forward * -0.02f;

			//
			t = LoadModule ("1x1_extras", _view);

			t.GetComponent<PanelExtras> ().ColorBtns (new Color32(230, 231, 232, 255), environment.envColor);
			t.transform.localPosition += transform.forward * -0.03f;

			return;
		}

		if (template == "food_consumption") {
			t = LoadModule ("food_consumption", _view);

			t = LoadModule ("1x1_extras", _view);

			t.GetComponent<PanelExtras> ().ColorBtns (environment.envColor, Color.white);
			t.transform.localPosition += transform.forward * -0.02f;

			return;
		}

		if (template == "crop_pipeline") {
			//
			t = LoadModule ("crop_pipeline", _view);

			//
			t = LoadModule ("1x1_txt_layout_06", _view);

			txtColor = Color.white;
			if (_templateData ["content"]["txt_color"].Count == 3) {
				txtColor = new Color32 ((byte)_templateData ["content"]["txt_color"][0].AsInt, (byte)_templateData ["content"]["txt_color"][1].AsInt, (byte)_templateData ["content"]["txt_color"][2].AsInt, 255);
			}
			t.GetComponent<PanelText> ().SetText ("", _templateData ["content"]["title"], _templateData ["content"]["body"], txtColor);
			t.transform.localPosition += transform.forward * -0.01f;

			//
			t = LoadModule ("1x1_extras", _view);

			t.GetComponent<PanelExtras> ().ColorBtns (new Color32(230, 231, 232, 255), environment.envColor);
			t.transform.localPosition += transform.forward * -0.03f;

			return;
		}

		if (template == "crispr_why") {
			//
			t = LoadModule ("crispr_why", _view);

			//
			t = LoadModule ("1x1_txt_layout_06", _view);

			txtColor = Color.white;
			if (_templateData ["content"]["txt_color"].Count == 3) {
				txtColor = new Color32 ((byte)_templateData ["content"]["txt_color"][0].AsInt, (byte)_templateData ["content"]["txt_color"][1].AsInt, (byte)_templateData ["content"]["txt_color"][2].AsInt, 255);
			}
			t.GetComponent<PanelText> ().SetText ("", _templateData ["content"]["title"], _templateData ["content"]["body"], txtColor);
			t.transform.localPosition += transform.forward * -0.01f;

			//
			t = LoadModule ("1x1_extras", _view);

			t.GetComponent<PanelExtras> ().ColorBtns (new Color32(230, 231, 232, 255), environment.envColor);
			t.transform.localPosition += transform.forward * -0.03f;

			return;
		}

		if (template == "crispr_what") {
			//
			t = LoadModule ("crispr_what", _view);

			//
			t = LoadModule ("1x1_txt_layout_06", _view);

			txtColor = Color.white;
			if (_templateData ["content"]["txt_color"].Count == 3) {
				txtColor = new Color32 ((byte)_templateData ["content"]["txt_color"][0].AsInt, (byte)_templateData ["content"]["txt_color"][1].AsInt, (byte)_templateData ["content"]["txt_color"][2].AsInt, 255);
			}
			t.GetComponent<PanelText> ().SetText ("", _templateData ["content"]["title"], _templateData ["content"]["body"], txtColor);
			t.transform.localPosition += transform.forward * -0.01f;

			//
			t = LoadModule ("1x1_extras", _view);

			t.GetComponent<PanelExtras> ().ColorBtns (new Color32(230, 231, 232, 255), environment.envColor);
			t.transform.localPosition += transform.forward * -0.03f;

			return;
		}

		if (template == "drone_innovation") {
			//
			t = LoadModule ("drone_innovation", _view);

			//
			t = LoadModule ("1x1_txt_layout_06", _view);

			txtColor = Color.white;
			if (_templateData ["content"]["txt_color"].Count == 3) {
				txtColor = new Color32 ((byte)_templateData ["content"]["txt_color"][0].AsInt, (byte)_templateData ["content"]["txt_color"][1].AsInt, (byte)_templateData ["content"]["txt_color"][2].AsInt, 255);
			}
			t.GetComponent<PanelText> ().SetText ("", _templateData ["content"]["title"], _templateData ["content"]["body"], txtColor);
			t.transform.localPosition += transform.forward * -0.01f;

			//
			t = LoadModule ("1x1_extras", _view);

			t.GetComponent<PanelExtras> ().ColorBtns (environment.envColor, Color.white);
			t.transform.localPosition += transform.forward * -0.03f;

			return;
		}

		if (template == "plant_health") {
			//
			t = LoadModule ("stressed_plant", _view);

			//
			t = LoadModule ("1x1_txt_layout_06", _view);

			txtColor = Color.white;
			if (_templateData ["content"]["txt_color"].Count == 3) {
				txtColor = new Color32 ((byte)_templateData ["content"]["txt_color"][0].AsInt, (byte)_templateData ["content"]["txt_color"][1].AsInt, (byte)_templateData ["content"]["txt_color"][2].AsInt, 255);
			}
			t.GetComponent<PanelText> ().SetText ("", _templateData ["content"]["title"], _templateData ["content"]["body"], txtColor);
			t.transform.localPosition += transform.forward * -0.01f;

			//
			t = LoadModule ("1x1_extras", _view);

			t.GetComponent<PanelExtras> ().ColorBtns (environment.envColor, Color.white);
			t.transform.localPosition += transform.forward * -0.03f;

			return;
		}
	}


	private Transform GetViewContainer(PanelView _view){
		if (_view == PanelView.Front) return front;
		if (_view == PanelView.Back) return back;
		if (_view == PanelView.Thumbnail) return thumbnail;
		return front;
	}
	private GameObject LoadModule(string _type, PanelView _view)
	{
		//Debug.Log("\t[LoadModule] '"+_type+"' into "+_view);

		//determine parent container
		Transform viewParent = GetViewContainer(_view);

		//instantiate module
		GameObject module = null;
		try{
		 	module = Instantiate (PMP.modules.Find (x => x.name == _type).prefab, viewParent);
		}catch(System.NullReferenceException e){
			throw new NullReferenceException ("You might be missing a reference to a \"panel module\" prefab in the PanelModulePool.");
		}
		return module;
	}


	public void ActivateView(PanelView _viewToShow, bool _faceAway)
	{
		//Debug.Log ("[ActivateView] "+_viewToShow+" to "+(_faceAway ? "away" : "toward"));

		Transform viewToShow = front;
		if (_viewToShow == PanelView.Front) viewToShow = front;
		if (_viewToShow == PanelView.Back) viewToShow = back;
		if (_viewToShow == PanelView.Thumbnail) viewToShow = thumbnail;

		if (_faceAway) {
			viewToShow.transform.localPosition = awayPos;
			viewToShow.transform.localEulerAngles = awayRot;
			viewToShow.gameObject.SetActive (true);
			currViewFacingAway = _viewToShow;
		} else {
			viewToShow.transform.localPosition = forwardPos;
			viewToShow.transform.localEulerAngles = forwardRot;
			viewToShow.gameObject.SetActive (true);
			currViewFacingForward = _viewToShow;
			panelView = _viewToShow;
		}

		//Debug.Log ((viewToShow.GetComponentInChildren<VideoPlayer> ()?"YES video player":"NO video player"));
		if (viewToShow.GetComponentInChildren<VideoPlayer> ()) {
			viewToShow.GetComponentInChildren<VideoPlayer> ().Play ();
		}

		//make sure this panel is in kiosk and is active panel
		if (panelContext == PanelContext.Kiosk) {
			//check if this view has panel buttons
			if (viewToShow.GetComponentInChildren<PanelExtras> ()) {
				//if this is a front view and there is a back view
				if (_viewToShow == PanelView.Front && back.childCount > 0) {
					//enable the close and more buttons
					viewToShow.GetComponentInChildren<PanelExtras> ().ToggleBtns (true, false, true);
				}
				//if this is the back view
				if (_viewToShow == PanelView.Back) {
					//enabled the close and back buttons
					viewToShow.GetComponentInChildren<PanelExtras> ().ToggleBtns (true, true, false);
				}
			}
		}

		if (panelContext == PanelContext.Idle && _viewToShow == PanelView.Front) {
			collide.transform.localPosition = new Vector3 (0, 0, -1);
		}
			
		if (currViewFacingAway != PanelView.Front && currViewFacingForward != PanelView.Front) {
			front.gameObject.SetActive (false);
		}
		if (currViewFacingAway != PanelView.Back && currViewFacingForward != PanelView.Back) {
			back.gameObject.SetActive (false);
		}
		if (currViewFacingAway != PanelView.Thumbnail && currViewFacingForward != PanelView.Thumbnail) {
			thumbnail.gameObject.SetActive (false);
		}
	}

	void SetRender(Transform _go, bool _enable){
		foreach (Renderer r in _go.GetComponentsInChildren<Renderer>()) {
			r.enabled = _enable;
		}
	}

	void PanelFlipped(){
		UpdatePanelView ();
		myKiosk.somePanelIsAnimating = false;
		panelState = PanelState.Active;
	}
	void PanelMovedToUserGrid(){
		UpdatePanelView ();
		if (transform.parent != myKiosk.userGrid) {
			transform.parent = myKiosk.userGrid;
		}
		//transform.SetParent (myKiosk.userGrid, false);
		//transform.localScale = Vector3.one;
		myKiosk.somePanelIsAnimating = false;
		myKiosk.activePanel = null;
		panelState = PanelState.Ready;
	}
	void PanelMovedToUserKiosk(){
		UpdatePanelView ();
		myKiosk.somePanelIsAnimating = false;
		panelState = PanelState.Active;
	}
	void UpdatePanelView(){
		PanelView prevViewFacingFront = currViewFacingForward;
		PanelView prevViewFacingAway = currViewFacingAway;
		currViewFacingForward = prevViewFacingAway;
		currViewFacingAway = prevViewFacingFront;

		if (Mathf.Abs(Mathf.RoundToInt(transform.localEulerAngles.y)) == 180) {
			awayPos = new Vector3 (0, 0, -0.01f);
			awayRot = new Vector3 (0, 0, 0);
			forwardPos = new Vector3 (0, 0, 0.01f);
			forwardRot = new Vector3 (0, 180, 0);
		}else{
			forwardPos = new Vector3 (0, 0, -0.01f);
			forwardRot = new Vector3 (0, 0, 0);
			awayPos = new Vector3 (0, 0, 0.01f);
			awayRot = new Vector3 (0, 180, 0);;
		}

		//hide away view
		if (currViewFacingAway == PanelView.Front) {
			front.gameObject.SetActive (false);
			//SetRender(front, false);
		}
		if (currViewFacingAway == PanelView.Back) {
			back.gameObject.SetActive (false);
			//SetRender(back, false);
		}
		if (currViewFacingAway == PanelView.Thumbnail) {
			thumbnail.gameObject.SetActive (false);
			//SetRender(thumbnail, false);
		}

		panelView = currViewFacingForward;

		if(panelView == PanelView.Front && back.childCount == 0){
			front.GetComponentInChildren<PanelExtras> ().ToggleBtns (true, false, false);
		}

		Debug.Log ("[PanelFlipped] " + prevViewFacingFront + " | " + prevViewFacingAway + " >> " + currViewFacingForward + " | " + currViewFacingAway);
	}


	#region touch handlers
	private void tappedHandler(object sender, EventArgs e)
	{
		Debug.Log ("[tappedHandler] "+ tapGesture.ScreenPosition+ " "+ transform.name);
		Debug.Log ("\t" + panelContext + " | " + panelView + " | " + panelState);

		//panel in idle context that is background or is animating should activate blank kiosk
		if (panelContext == PanelContext.Idle 
			&& (panelView == PanelView.Background || panelState == PanelState.Animating)) 
		{
			if (IdleStateController.Instance.layoutGrid == null) {
				IdleStateController.Instance.StartIdleLoop ();
			} else {
				Vector2 tappedGridPos = GridManagerOrtho.Instance.CalculateColRowFromScreenPos (tapGesture.ScreenPosition);
				EventsManager.Instance.UserKioskOpenRequest (tappedGridPos, tapGesture.ScreenPosition);
				//Track
				GA.Instance.Tracking.LogEvent(new EventHitBuilder()
					.SetEventCategory(AM.displayName)
					.SetEventAction("Kiosk > Open")
					.SetEventLabel("Kiosk "+(tappedGridPos.x+1)));
			}
		}

		//if the panel is animating, dont execute following
		if (panelState == PanelState.Animating) 
		{
			Debug.Log ("\t panel is animating [end]");
			return;
		}

		//panels in the Idle context can only be tapped, and should only activate user kiosks
		if (panelContext == PanelContext.Idle 
			&& (panelView == PanelView.Front || panelView == PanelView.Thumbnail)
			&& panelState == PanelState.Active) 
		{
			//content panels are interactable, and should remain when tapped
			collide.transform.localPosition = Vector3.zero;
			transform.parent = AssetManager.Instance.panels;
			transform.localScale = Vector3.one;
			Debug.Log ("\tgridCell: " + this.gridID + " | gridPosID: " + GridManagerOrtho.Instance.gridPositions [this.gridID].id);
			transform.position = new Vector3 (GridManagerOrtho.Instance.gridPositions [this.gridID].center.x, GridManagerOrtho.Instance.gridPositions [this.gridID].center.y, 10);
			EaseCurve.Instance.Scl (transform, transform.localScale, transform.localScale * 0.9f, 0.25f, 0, EaseCurve.Instance.linear);
			ScreenManager.Instance.MoveToLayer (transform, LayerMask.NameToLayer ("UserInit"));
			Debug.Log ("\tpanelGridPos: " + this.panelGridPos);
			EventsManager.Instance.UserKioskOpenRequest (this.panelGridPos, tapGesture.ScreenPosition, environment, transform);
			StartCoroutine (MovePanelToKiosk ((int)this.panelGridPos.x));
			//Track
			GA.Instance.Tracking.LogEvent(new EventHitBuilder()
				.SetEventCategory(AM.displayName)
				.SetEventAction("Kiosk > Open")
				.SetEventLabel("Kiosk "+(this.panelGridPos.x+1)));
			//Track
			GA.Instance.Tracking.LogEvent(new EventHitBuilder()
				.SetEventCategory(AM.displayName)
				.SetEventAction("Panel > Open")
				.SetEventLabel("["+panelID+"] "+panelName));
		}

		//the rest are all kiosk related
		//so make sure this panels has a kiosk reference
		if (!myKiosk)
			return;

		if(panelContext == PanelContext.Kiosk && myKiosk.somePanelIsAnimating)
		{
			Debug.Log ("\t kiosk is animating [end]");
			return;
		}
			

		//
		if (panelContext == PanelContext.Kiosk && panelState == PanelState.Active) {
			//FlipAround ();
		}


		if (panelContext == PanelContext.Kiosk && panelView == PanelView.Background) {
			//make sure the kiosk isnt in the middle of animating something 
			if (!myKiosk.somePanelIsAnimating) {
				/*
				//panel in kiosk context that is background should close active kiosk
				//first check if another panel is active, and hide that one
				if (myKiosk.activePanel) {
					Debug.Log ("\t has active panel, closing that panel first");
					myKiosk.activePanel.GetComponent<PanelBase> ().BackToGrid ();
				}
				*/
				if (!myKiosk.activePanel) {

				}
			}
		}

		//panel in kiosk context that is a thumbnail should activate
		if (panelContext == PanelContext.Kiosk && panelView == PanelView.Thumbnail && panelState == PanelState.Ready) {
			//make sure the kiosk isnt in the middle of animating something
			if (!myKiosk.somePanelIsAnimating) {
				//first check if another panel is active, and hide that one
				if (myKiosk.activePanel) {
					//Debug.Log ("\t has active panel, closing that panel first");
					//myKiosk.activePanel.GetComponent<PanelBase> ().BackToGrid ();
				} else {
					Debug.Log ("\t no active panel, opening thumbnail");
					ActivateFromGrid (false);
					//Track
					GA.Instance.Tracking.LogEvent(new EventHitBuilder()
						.SetEventCategory(AM.displayName)
						.SetEventAction("Panel > Open")
						.SetEventLabel("["+panelID+"] "+panelName));
				}
			}
		}
	}

	public void FlipAround()
	{
		Debug.Log ("\t[FlipAround]");
		if (currViewFacingForward == PanelView.Front) 
		{
			ActivateView (PanelView.Back, true);
		}
		if (currViewFacingForward == PanelView.Back) 
		{
			ActivateView (PanelView.Front, true);
		}
		panelState = PanelState.Animating;
		myKiosk.somePanelIsAnimating = true;
		//SetAsThumbnail (); //this should happen elsewhere
		EaseCurve.Instance.Rot (transform, transform.localRotation, 180f, transform.up, 0.5f, 0f, EaseCurve.Instance.easeOut, PanelFlipped);
		//EaseCurve.Instance.Rot (transform, transform.localRotation, 360f, transform.up, 1f, 0f, EaseCurve.Instance.linear);
		//EaseCurve.Instance.Scl (transform, transform.localScale, transform.localScale*0.5f, 0.4f, 0f, EaseCurve.Instance.easeBack);
	}

	public void BackToGrid()
	{
		Debug.Log ("[BackToGrid] " + name);
		if (!myKiosk.somePanelIsAnimating) {
			if (currViewFacingForward != PanelView.Thumbnail) {
				ActivateView (PanelView.Thumbnail, true);
			}
			panelState = PanelState.Animating;
			myKiosk.somePanelIsAnimating = true;
			myKiosk.ToggleTint (false);
			Vector3 goTo = myKiosk.GetComponentInChildren<UserGrid> ().transform.TransformPoint (myKiosk.GetComponentInChildren<UserGrid> ().emptySpot);
			float scaleTo = myKiosk.GetComponentInChildren<UserGrid> ().emptySize;
			EaseCurve.Instance.Vec3 (transform, transform.position, goTo, 0.5f, 0, EaseCurve.Instance.easeOut);
			EaseCurve.Instance.Rot (transform, transform.localRotation, 180f, transform.up, 0.7f, 0f, EaseCurve.Instance.easeOutBack);
			EaseCurve.Instance.Scl (transform, transform.localScale, Vector3.one * scaleTo, 0.8f, 0f, EaseCurve.Instance.easeOutBack, PanelMovedToUserGrid);
			//Track
			GA.Instance.Tracking.LogEvent(new EventHitBuilder()
				.SetEventCategory(AssetManager.Instance.displayName)
				.SetEventAction("Panel > Close")
				.SetEventLabel("["+panelID+"] "+panelName));
		}
	}

	private void ActivateFromGrid(bool _waitForActiveToClose){
		Debug.Log ("[ActivateFromGrid] " + name);
		float delay = 0f;
		if (_waitForActiveToClose)
			delay = 0.6f;
		ActivateView (PanelView.Front, true);
		panelState = PanelState.Animating;
		myKiosk.somePanelIsAnimating = true;
		myKiosk.activePanel = transform;
		myKiosk.userGrid.GetComponent<UserGrid> ().emptySpot = transform.localPosition;
		myKiosk.userGrid.GetComponent<UserGrid> ().emptySize = transform.localScale.x < 1.1f ? 0.3f : 0.606f;
		Vector3 goTo = myKiosk.menu.localPosition;
		goTo.z = 25f;
		goTo.x = 0.16f;
		transform.parent = myKiosk.transform;
		EaseCurve.Instance.Vec3 (transform, transform.localPosition, goTo, 0.5f, delay, EaseCurve.Instance.easeOut, null, "local");
		EaseCurve.Instance.Rot (transform, transform.localRotation, 180f, transform.up, 0.5f, delay, EaseCurve.Instance.easeOut);
		EaseCurve.Instance.Scl (transform, transform.localScale, Vector3.one * 0.9f, 0.6f, delay, EaseCurve.Instance.easeOut, PanelMovedToUserKiosk);
		myKiosk.ToggleTint (true);
	}

	private void transformStartedHandler(object sender, EventArgs e)
	{
		//Debug.Log (transform.name+" transformStartedHandler");

	}

	private void transformedHandler(object sender, EventArgs e)
	{
		if (panelView == PanelView.Background || panelContext == PanelContext.Idle)
		{
			return;
		}

		if (panelContext == PanelContext.Kiosk && panelState == PanelState.Ready && myKiosk.activePanel == null) {
			myKiosk.dragDelta = transformGesture.DeltaPosition;
			myKiosk.dragGrid = true;
		}

		if (panelContext == PanelContext.Kiosk && panelState == PanelState.Active && myKiosk.activePanel == transform) {
			transform.position += transformGesture.DeltaPosition;
			if (transform.localPosition.y < -3f)
				transform.localPosition = new Vector3 (transform.localPosition.x, -3f, transform.localPosition.z);
			if (transform.localPosition.y > 0f)
				transform.localPosition = new Vector3 (transform.localPosition.x, -0, transform.localPosition.z);
			if (transform.localPosition.x < -1f)
				transform.localPosition = new Vector3 (-1, transform.localPosition.y, transform.localPosition.z);
			if (transform.localPosition.x > 1f)
				transform.localPosition = new Vector3 (1, transform.localPosition.y, transform.localPosition.z);
			//
			myKiosk.menuFollowPanel = true;
			//reset idle clock
			myKiosk.timeSinceLastTouch = 0f;

			/*
			transform.localScale *= transformGesture.DeltaScale;
			if (transform.localScale.x > 1)
				transform.localScale = Vector3.one;
			if (transform.localScale.x < 0.5f)
				transform.localScale = Vector3.one * 0.5f;
			*/
		}
	}

	private void transformCompletedHandler(object sender, EventArgs e)
	{
		if (panelView == PanelView.Background || panelContext == PanelContext.Idle)
			return;

		if (myKiosk.dragGrid) {
			myKiosk.dragGrid = false;
		}
		
		if (myKiosk != null) {
			myKiosk.menuFollowPanel = false;
		}
	}

	private void flickHandler(object sender, EventArgs e){
		if (panelContext == PanelContext.Kiosk && panelState == PanelState.Ready && myKiosk.activePanel == null) {
			//fling grid
		}
	}
	#endregion

	IEnumerator MovePanelToKiosk(int _col)
	{
		yield return new WaitForSeconds (0.4f);
		Debug.Log ("[MovePanelToKiosk] UserKiosk_" + _col);
		GameObject kiosk = GameObject.Find ("/Kiosks/UserKiosk_" + _col);
		Vector3 posInPurgatory = kiosk.transform.position;
		posInPurgatory.z = 0;
		transform.position = posInPurgatory + (Vector3.up * transform.position.y) + (Vector3.forward * 25f);
		transform.parent = kiosk.transform;
		ScreenManager.Instance.MoveToLayer (transform, LayerMask.NameToLayer ("Default"));
		panelContext = PanelContext.Kiosk;
		panelState = PanelState.Active;
		myKiosk = kiosk.GetComponent<UserKiosk> ();
		myKiosk.activePanel = transform;
		myKiosk.ToggleTint (true);
		if (panelView == PanelView.Thumbnail) {
			ActivateView (PanelView.Front, true);
			FlipAround ();
		} else {
			if (front.GetComponentInChildren<PanelExtras> ()) {
				if (panelView == PanelView.Front && back.childCount == 0) {
					front.GetComponentInChildren<PanelExtras> ().ToggleBtns (true, false, false);
				} else {
					front.GetComponentInChildren<PanelExtras> ().ToggleBtns (true, false, true);
				}
			}
		}
	}
		

	public void PlayBgVideo(){
		transform.GetComponentInChildren<VideoPlayer> ().Play ();
	}
	public void PauseBgVideo(){
		transform.GetComponentInChildren<VideoPlayer> ().Pause ();
	}


}
