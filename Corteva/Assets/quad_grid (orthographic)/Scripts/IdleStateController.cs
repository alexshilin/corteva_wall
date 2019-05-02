using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * this class is responsible for:
 * generating layout configurations for the grid's idle state
 * instantiating the cell cameras and panels for each layout
 * managing the animations and transitions between idle states
*/
public class IdleStateController : MonoBehaviour {


	[System.Serializable]
	public class CellAction{
		public int row;
		public int col;
		public string name;
		public Vector2 panelType;
		public Vector3 fromPos;
		public Vector3 toPos;
		public Vector3 direction;
		public GameObject panel;
		public GameObject cellCam;
		public float distanceToOrigin;
	}

	#region manager references
	private ScreenManager SM;
	private AssetManager AM;
	private GridManagerOrtho GM;
	#endregion


	#region class vars
	int[][] layoutGrid;
	public List<CellAction> idleSequence = new List<CellAction>();
	int startColumn;
	Vector2 startPanelPos;
	Vector3 previousPanelPos = Vector3.zero;


	List<PanelObject> bgPanels = new List<PanelObject> ();
	int currBg = 0;

	public List<int> kioskColumns = new List<int> ();
	public List<int> kioskColumnsToClose = new List<int> ();

	List<Environment> environments;
	int currEnv = -1;

	string[] availableTypes = new string[]{"1x1","1x1","1x1","1x1","1x2","2x2"};
	List<string> usedTypes = new List<string>();

	Transform currTitleCam;
	Transform currTitlePanel;


	float timeElapsedSinceLastTransition;
	float timeToNextTransition = 10f;
	bool activeTransitionLoop = false;

	bool panelsInTransition = false;

	bool titleHidden = false;
	bool hideTitleAfterInroTransition = false;

	private float panelDepth = 50f;
	#endregion

	#region instance
	private static IdleStateController _instance;
	public static IdleStateController Instance { get { return _instance; } }
	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		} else {
			_instance = this;
		}
	}
	#endregion

	void Start(){
		//get references to manager instances
		AM = AssetManager.Instance;
		GM = GridManagerOrtho.Instance;
		SM = ScreenManager.Instance;

		//get environemnts
		environments = new List<Environment>(AM.environments);

		//start event listenered
		EventsManager.Instance.OnUserKioskOpenRequest += KioskOpenResponse;
		EventsManager.Instance.OnUserKioskCloseRequest += KioskCloseResponse;
		EventsManager.Instance.OnClearEverything += ClearEverything;
	}

	//NOTE: consider putting kiosk activation logic in this class.

	void OnDisable(){
		//stop event listeneres
		EventsManager.Instance.OnUserKioskOpenRequest -= KioskOpenResponse;
		EventsManager.Instance.OnUserKioskCloseRequest += KioskCloseResponse;
		EventsManager.Instance.OnClearEverything -= ClearEverything;
	}

	void Update(){
		//idle loop timer
		if (activeTransitionLoop && !panelsInTransition) {
			SM.idleTimerText.text = Mathf.Round(timeToNextTransition - timeElapsedSinceLastTransition) + "\n" + (activeTransitionLoop?"looping":"static") + " + " + (panelsInTransition?"busy":"available");
			timeElapsedSinceLastTransition += Time.deltaTime;
			if (timeElapsedSinceLastTransition >= timeToNextTransition) {
				UnPlaceCameras ();
			}
		} else {
			SM.idleTimerText.text = (activeTransitionLoop?"looping":"static") + " + " + (panelsInTransition?"busy":"available");
		}

		//TEMP keyboard commands
		if (Input.GetKeyDown (KeyCode.Alpha0)) {
			PlanLayout ();
		}
		if (Input.GetKeyDown (KeyCode.Alpha9)) {
			StartIdleLoop ();
		}
		if (Input.GetKeyDown (KeyCode.Backspace)) {
			ClearCellCams ();
		}
		if (Input.GetKeyDown (KeyCode.Alpha8)) {
			ZoomBG ();
		}

	}

	public void Prepare(){
		Debug.Log ("[Prepare] idle grid for " + GM.desiredGrid.x + " columns.");

		ClearKiosks ();
			
		MakeBackgroundPanels();
	}



	void MakeBackgroundPanels(){
		//get video files based on aspect ratio
		List<string> bgVideos = new List<string> (SM.currAspect==ScreenManager.Aspect.is169 ? AM.videoFiles : AM.videoFiles329);
		//preload all background videos
		for (int i = 0; i < bgVideos.Count; i++) {
			GameObject bgPanel = Instantiate (AM.panelPrefab);
			bgPanel.SetActive (true);
			//place all bg panels off-screen so they dont get rendered
			bgPanel.transform.position = new Vector3 (0f, i == 0 ? 0f : 100f, 100f);
			bgPanel.transform.localScale *= GM.desiredGrid.x;
			if (SM.currAspect==ScreenManager.Aspect.is169) {
				bgPanel.GetComponent<PanelObject> ().SetAsVideo (false, i == 0);
			} else {
				bgPanel.GetComponent<PanelObject> ().SetAs329Video (i == 0);
			}
			bgPanel.GetComponent<PanelObject> ().panelContext = PanelObject.PanelContext.Idle;
			bgPanel.GetComponent<PanelObject> ().panelMode = PanelObject.PanelView.Background;
			//bgPanel.GetComponent<PanelObject> ().SetAsNonInteractive ();
			bgPanels.Add (bgPanel.GetComponent<PanelObject> ());
		}
		currBg = 0;
	}

	#region cleanup
	private void ClearKiosks(){
		Debug.Log ("[ClearKiosks]");
		kioskColumns.Clear ();
		for (int i = 0; i < GM.desiredGrid.x; i++) {
			kioskColumns.Add (0);
			kioskColumnsToClose.Add (0);
		}
	}
		
	private void ClearCellCams(){
		Debug.Log ("[ClearCellCams] " + AM.cams.childCount);
		foreach (Transform child in AM.cams) {
			if (currTitleCam != null) {
				if (child != currTitleCam)
					GameObject.Destroy (child.gameObject);
			} else {
				GameObject.Destroy (child.gameObject);
			}
		}
		panelDepth = 50f;
	}

	private void ClearEverything(){
		Debug.Log ("[ClearEverything]");
		ClearKiosks ();
		ClearCellCams ();
		idleSequence.Clear ();
	}
	#endregion

	#region event listener reactions
	private void KioskOpenResponse (Vector2 _gridPos, Environment _env){
		Debug.Log ("!![KioskOpenResponse] at col " + _gridPos.x);

		//update which columns have kiosks
		kioskColumns [(int)_gridPos.x] = 1;
		//do we have any panels open?
		if (idleSequence.Count > 0) {
			//is there an active title cellcam and panel?
			if (idleSequence [0].cellCam.activeSelf && idleSequence [0].panel != null) {
				//hide it
				HideTitlePanel ();
			}
		}
	}
	private void KioskCloseResponse(Vector2 _gridPos, bool _now){
		Debug.Log ("!![KioskCloseResponse] at col " + _gridPos.x + " " + (_now ? "now" : "prepare"));
		//a kiosk is being closed
		if (_now) {
			//do something after the kiosks have been closed
			for (int i = 0; i < kioskColumns.Count; i++) {
				if (i==(int)_gridPos.x && kioskColumns [i] == 1){
					Debug.Log ("\tclosing kiosk at col " + i);
					kioskColumns [i] = 0;
				}
			}
		}
	}
	#endregion



	void HideTitlePanel(){
		Debug.Log ("[HideTitlePanel] " + (titleHidden ? "hidden" : "visible") + " | " + (idleSequence [0].panel.GetComponent<PanelBase> ().panelState == PanelBase.PanelState.Animating ? "animating" : "static"));
		if (!titleHidden) {
			if (idleSequence [0].panel.GetComponent<PanelBase> ().panelState != PanelBase.PanelState.Animating) {
				titleHidden = true;
				EaseCurve.Instance.Vec3 (idleSequence [0].panel.transform, idleSequence [0].panel.transform.localPosition, idleSequence [0].fromPos, 0.5f, 0.25f, EaseCurve.Instance.custom2, null, "local");
			} else {
				hideTitleAfterInroTransition = true;
			}
		}
	}


	void AddKioskToClose(int _col){
		if(kioskColumns[_col] == 1)
			kioskColumnsToClose [_col] = 1;
	}
	void CloseKiosks (){
		Debug.Log ("[CheckKiosksToClose]");
		bool doResumeLoop = kioskColumns.Contains(0) ? false : true;
		for (int i = 0; i < kioskColumns.Count; i++) {
			if (kioskColumns [i] == 1 && kioskColumnsToClose [i] == 1) {
				Debug.Log ("\tclosing kiosk at col " + i);
				EventsManager.Instance.UserKioskCloseRequest (new Vector2(i,0), true);
				kioskColumns [i] = 0;
				kioskColumnsToClose [i] = 0;
			}
		}
		if (doResumeLoop) {
			StartIdleLoop ();
		}
	}

	void StartIdleLoop(){
		Debug.Log ("[StartIdleLoop]");
		timeElapsedSinceLastTransition = timeToNextTransition;
		panelsInTransition = false;
		activeTransitionLoop = true;
	}



	void PlanLayout(){
		Debug.Log ("----------------------------------------------------------");
		Debug.Log ("[PlanLayout]");
		//generate blank layout grids
		if (GM.desiredGrid.x == 6) {
			layoutGrid = new int[][] {
				new int[]{ 0, 0, 0, 0, 0, 0 },
				new int[]{ 0, 0, 0, 0, 0, 0 },
				new int[]{ 0, 0, 0, 0, 0, 0 }
			};
		} else {
			layoutGrid = new int[][] {
				new int[]{ 0, 0, 0 },
				new int[]{ 0, 0, 0 },
				new int[]{ 0, 0, 0 }
			};
		}

		//reset vars
		usedTypes.Clear ();
		previousPanelPos = Vector3.zero;
		idleSequence.Clear ();

		//if wall is totally clear or were at the last environment
		if (currEnv == -1 || currEnv == environments.Count - 1) {
			//choose new start column
			currEnv = 0;
			if (GM.desiredGrid.x == 6) {
				startColumn = Random.Range (1, (int)GM.desiredGrid.x - 1);
			} else {
				startColumn = Random.Range (0, (int)GM.desiredGrid.x);
			}
		} else {
			//we're still iterating through environments, keep start position
			currEnv++;
		}
		//if a kiosk is open, overwrite previous rules
		if (kioskColumns.Contains(1)) {
			if(!kioskColumns.Contains(0)){
				//all kiosks are open
				Debug.Log("\tno columns left. ending idle loop.");
				activeTransitionLoop = false;
				return;
			}
			//create a new list of colmns which do not have a kiosk in them
			List<int> kCols = new List<int> ();
			for (int i = 0; i < kioskColumns.Count; i++) {
				if (kioskColumns [i] == 0) {
					kCols.Add (i);
				}
			}
			//chose a random column from that list
			startColumn = kCols[Random.Range (0, kCols.Count)];
		}

		Debug.Log (environments.Count);
		Debug.Log ("\tENV (" + currEnv + " of "+environments.Count+"): " + environments [currEnv].envTitle);

		//set starting panel position
		startPanelPos = new Vector2 (0, startColumn);
		Debug.Log ("\tSTARTING at column [" + startColumn+"]");

		//place ENV. TITLE panel there
		AllocatePanelPlacement (0, startColumn, "1x1", Vector3.down);

		//should be: go DOWN (because currently in top row)
		AllocatePanelPlacement (1, startColumn, "1x1", Vector3.down);

		//in mid row, go outwards
		CheckLeft (1, startColumn);
		CheckRight (1, startColumn);

		StartCoroutine(PlaceCellCameras());
	}



	#region layout logic
	void CheckLeft(int _row, int _col){
//		Debug.Log ("\t[CheckLeft] from ["+_row+","+_col+"] (" + colsRemainL + " open)");

		//how many cells remain open to the left of this cell
		int colsRemainL = _col;

		if (colsRemainL == 0) {
			//end of row (screen)
			//place something in bottom row?
			int r = Random.Range (0, 2);
			if (r == 1)
				CheckDown (_row, _col);
			return;
		}

		//decide allowed panel types based on remaining cells
		int allowedTypes = colsRemainL == 1 ? 2 : 3;
		//if theres a kiosk open, ignore previous, only allow one type
		if (kioskColumns.Contains(1))
			allowedTypes = 1;
		
		//once cell remains, can place 1x1 or 1x2 panels only
		//iterate to next cell
		int nextCol = _col - 1;
		//check which types of panels we have remaning in this environemnt based on the type of panel allowed
		string p = CheckAvailable (_row, nextCol, allowedTypes);

		//place the chosen panel in adjacent cell
		AllocatePanelPlacement (_row, nextCol, p, Vector3.left);

		//if we happened to have placed a 2x2 cell, step one cell further
		if (p == "2x2")
			nextCol--;

		//recur
		CheckLeft (_row, nextCol);
	}

	void CheckRight(int _row, int _col){
//		Debug.Log ("\t[CheckRight] from ["+_row+","+_col+"] (" + colsRemainR + " open)");

		//same as CheckLeft but in the other direction
		int colsRemainR = (int)GM.desiredGrid.x - (_col + 1);

		if (colsRemainR == 0) {
			int r = Random.Range (0, 2);
			if (r == 1)
				CheckDown (_row, _col);
			return;
		}

		int allowedTypes = colsRemainR == 1 ? 2 : 3;
		if (kioskColumns.Contains(1))
			allowedTypes = 1;
		
		int nextCol = _col + 1; 

		string p = CheckAvailable (_row, nextCol, allowedTypes);

		AllocatePanelPlacement (_row, nextCol, p, Vector3.right);

		if (p == "2x2")
			nextCol++;

		CheckRight (_row, nextCol);
	}

	void CheckDown(int _row, int _col){
//		Debug.Log ("\t[CheckDown] from ["+_row+","+_col+"]");

		if (!kioskColumns.Contains(1)) {
			int nextRow = _row + 1;
			string p = CheckAvailable (nextRow, _col, 1); 
			bool ok = TestFit (nextRow, _col, "1x1", Vector3.down); 
			if (ok)
				AllocatePanelPlacement (nextRow, _col, "1x1", Vector3.down);
		}
	}

	string CheckAvailable(int _row, int _col, int _panelType){
		string r;
		if (_panelType == 3) {
			r = availableTypes [Random.Range (0, availableTypes.Length)];
		} else if (_panelType == 2) {
			r = availableTypes [Random.Range (0, availableTypes.Length - 1)];
		} else {
			r = availableTypes [Random.Range (0, availableTypes.Length - 2)];
		}
		//r = availableTypes [Random.Range (0, availableTypes.Length - 2)];
		//TODO make sure there is AT LEAST + AT MOST 1 instance of a 1x2 and 2x2 panel type
		//this takes care of the AT MOST part
		if ((r == "1x2" && usedTypes.Contains ("1x2")) || (r == "2x2" && usedTypes.Contains ("2x2"))) {
			r = "1x1";
		}

		usedTypes.Add (r);
		return r;
	}

	float CheckNegativeSpacePercentage(){
		float occupied = 0;
		float total = GM.desiredGrid.x * GM.desiredGrid.y;
		for (int i = 0; i < layoutGrid.Length; i++) {
			for (int ii = 0; ii < layoutGrid [i].Length; ii++) {
				if (layoutGrid [i] [ii] != 0)
					occupied++;
			}
		}
		float occupiedPercent = occupied / total;
		float emptyPercent = 1 - occupiedPercent;
//		Debug.Log ("[CheckNegativeSpacePercentage] " + RoundDecimal (occupiedPercent, 1) + ":" + RoundDecimal (emptyPercent, 1));
		return RoundDecimal (emptyPercent, 1);
	}
	float RoundDecimal(float _value, int _decimals){
		float rounder = 10f * _decimals;
		return Mathf.Round(_value * rounder) / rounder;
	}

	bool TestFit(int _row, int _col, string _type, Vector3 _dir){
//		Debug.Log("\t\t[TestFit] "+_type+" in cell ["+_row+","+_col+"]");

		bool canPlace = true;

		//rule 1: make sure cell isnt already occupied
		if (layoutGrid [_row] [_col] != 0) {
			return false;
		}

		//rule 2: no full columns (reserved for user kiosk)
		if (_row == 2 && _type=="1x1") {
			bool colAlmostFull = (layoutGrid [1] [_col] !=0 && layoutGrid [0] [_col] !=0) ? true : false;
			if (colAlmostFull) {
//				Debug.Log ("\t\t[TestFit] failed, column too full");
				return false;
			}
		}

		return true;
	}

	void AllocatePanelPlacement(int _row, int _col, string _type, Vector3 _dir){
		//calculate 1Darray index(i) from 2Darray point(x,y)
		//this is used to grap world position coordinates from List previously calculated by GridManager
		int id = _col + (int)GM.desiredGrid.x * _row;
//		Debug.Log ("\t\t\t[PlacePanel] "+_type+" into " + _row + "," + _col + " [" + id + "]");

		Vector2 gridPos = Vector3.zero;
		Vector3 finalPos = Vector3.zero;

		//set up cell action
		CellAction pa = new CellAction();

		string panelName = _type+" @ ["+_row+","+_col+"]";

		pa.row = _row;
		pa.col = _col;

		//if panel should be 2x2
		if (_type == "2x2") {
			int cellDir = 0;
			//based on direction, use bottom right/left corner of open cell to center a 2x2 panel
			if (_dir == Vector3.right) {
				gridPos = GM.gridPositions [id].bottomRight;
				cellDir = 1;
			}
			if (_dir == Vector3.left) {
				gridPos = GM.gridPositions [id].bottomLeft;
				cellDir = -1;
			}
			finalPos = new Vector3 (gridPos.x, gridPos.y, panelDepth);

			//update sequence
			pa.panelType = new Vector2(2,2);
			pa.toPos = finalPos;

			//update layout grid to inidicate these cells as being occupied
			layoutGrid [_row] [_col] = 4; 
			layoutGrid [_row] [_col + cellDir] = 4;
			layoutGrid [_row + 1] [_col] = 4; 
			layoutGrid [_row + 1] [_col + cellDir] = 4;
		} else if (_type == "1x2") {
			//based on direction, use bottom right/left corner of open cell to center a 2x2 panel
			if (_dir == Vector3.right)
				gridPos = GM.gridPositions [id].topRight;
			if (_dir == Vector3.left)
				gridPos = GM.gridPositions [id].topRight;

			finalPos = new Vector3 (gridPos.x, gridPos.y, panelDepth);

			pa.panelType = new Vector2(1,2);
			pa.toPos = finalPos;

			layoutGrid [_row] [_col] = 2;
			layoutGrid [_row - 1] [_col] = 2; 
		} else {
			//place panel
			finalPos = new Vector3 (GM.gridPositions [id].center.x, GM.gridPositions [id].center.y, panelDepth);

			pa.panelType = new Vector2(1,1);
			pa.toPos = finalPos;

			//update layout grid with newly occupied cell
			layoutGrid [_row] [_col] = 1;
		}

		Vector3 calcFromPanelPos = finalPos;
		if (_dir == Vector3.right || _dir == Vector3.left){
			calcFromPanelPos += (_dir * pa.panelType.x * 5.33333f * -1f);
		}
		if(_dir==Vector3.down || _dir == Vector3.up)
			calcFromPanelPos += (_dir * pa.panelType.y * 3f * -1f);
		
		pa.fromPos = calcFromPanelPos;
		pa.direction = _dir;
		pa.distanceToOrigin = Vector2.Distance (startPanelPos, new Vector2(_row, _col));

		panelName += " ("+pa.distanceToOrigin+")";
		pa.name = panelName;

		idleSequence.Add (pa);

		//update previous panelPos
		previousPanelPos = finalPos;

		CheckNegativeSpacePercentage ();
	}

	static int SortByDistanceFromOrigin(CellAction pa1, CellAction pa2)
	{
		return pa1.distanceToOrigin.CompareTo(pa2.distanceToOrigin);
	}
	#endregion



	IEnumerator PlaceCellCameras(){
		Debug.Log ("[PlaceCellCameras]");
		idleSequence.Sort(SortByDistanceFromOrigin);
		float zDepth = 50;
		float layerDepth = 0;
		float group = 2;
		float titlePauseTime = 0;
		//int activePanels = 0;
		int i = 0;

		panelsInTransition = true;

		//instantiate new cell cams
		while (i < idleSequence.Count) {
			GameObject ccGo;

			if (i == 1) {
				Debug.Log ("\twaited " + (Time.time - titlePauseTime) + " after title");
			}

			if ((i == 0 && currEnv > 0) && !kioskColumns.Contains (1) && currTitlePanel != null) {
				//this is the title panel
//				Debug.Log (currEnv+"...adjusting current title panel in existing title cam");
				currTitlePanel.position += Vector3.forward * 10;
				Destroy (currTitlePanel.gameObject, 1f);
				ccGo = currTitleCam.gameObject;
			} else {
				ccGo = Instantiate (AM.cellCameraPrefab);
				ccGo.SetActive (true);
				Camera ccCam = ccGo.GetComponentInChildren<Camera> ();
				ccCam.GetComponent<TouchScript.Layers.StandardLayer> ().Name = "CellCamera " + idleSequence [i].col + ", " + idleSequence [i].row;
				Rect rec = CreateCameraCell (idleSequence [i].row, idleSequence [i].col, idleSequence [i].panelType, idleSequence [i].direction);
				ccCam.rect = rec;
				ccGo.transform.parent = AM.cams;
				ccGo.transform.localPosition = new Vector3 (ccGo.transform.position.x + (idleSequence [i].col * 40f), ccGo.transform.position.y - (idleSequence [i].row * 20), 0);
				ccGo.name = idleSequence [i].col + ", " + idleSequence [i].row;
			}
			GameObject panel = Instantiate (AM.NEWpanelPrefab);

			panel.transform.parent = ccGo.transform.Find ("Container");
			panel.transform.localPosition = Vector3.zero;
			panel.transform.localScale = Vector3.one;
			panel.transform.localPosition = Vector3.Scale (-idleSequence [i].direction, new Vector3 (6f, 3.5f, 0f));
			Vector3 toPos = Vector3.zero;

			PanelBase po = panel.GetComponent<PanelBase> ();

			po.panelID = idleSequence [i].col + (int)GM.desiredGrid.x * idleSequence [i].row;
			po.panelContext = PanelBase.PanelContext.Idle;
			po.panelState = PanelBase.PanelState.Animating;
			po.panelView = PanelBase.PanelView.Front;
			po.panelGridPos = new Vector2 (idleSequence [i].col, idleSequence [i].row);
			po.environment = environments [currEnv];

			bool flip = UnityEngine.Random.Range (0, 2) == 0 ? true : false;
			po.ActivateView (PanelBase.PanelView.Thumbnail, flip);
			po.ActivateView (PanelBase.PanelView.Front, !flip);

			panel.name = "Panel " + idleSequence [i].col + ", " + idleSequence [i].row;

			/*po.SetPanelColors (environments [currEnv].envColor);
			if (i == 0) {
				po.SetAsTitle (environments [currEnv].envTitle);
				po.panelMode = PanelObject.PanelView.Background;
			} else if (idleSequence [i].panelType == new Vector2 (1, 2)) {
				po.SetAsImage1x2 ();
				po.panelMode = PanelObject.PanelView.Background;
				//panel.GetComponent<PanelObject> ().SetAsNonInteractive ();
				toPos.x += (5.33333f / 4f);
			} else {
				int r = Random.Range (0, 3);
				if (r == 1) {
					po.SetAs3dViz ();
				} else if (r == 2) {
					po.SetAsVideo (false, true);
					//panel.GetComponent<PanelObject> ().SetAsImage ();
				} else {
					po.SetAsImage ();
				}
			}*/

			idleSequence [i].fromPos = panel.transform.localPosition;
			idleSequence [i].toPos = toPos;
			idleSequence [i].panel = panel;
			idleSequence [i].cellCam = ccGo;


			//animate the panel
			float speed = group * 0.5f;
			float wait = group * 0.1f;
			if (i < idleSequence.Count - 1) {
				if (idleSequence [i + 1].distanceToOrigin != idleSequence [i].distanceToOrigin)
					group++;
			}
			if ((i == 0 && currEnv < environments.Count - 1) && !kioskColumns.Contains (1)) {
//					Debug.Log (currEnv + "...saving current title cam");
				//save current title panel for transitions
				currTitleCam = ccGo.transform;
				currTitlePanel = panel.transform;
			}


			if (i == 0) {
				EaseCurve.Instance.Vec3 (panel.transform, panel.transform.localPosition, toPos, speed, wait, EaseCurve.Instance.custom, null, "local");
				Invoke ("ZoomBG", 0.5f);
				titlePauseTime = Time.time;
				yield return new WaitForSecondsRealtime (1f);
			} else {
				//Debug.Log (i + " < " + (idleSequence.Count - 1));
				if (i < idleSequence.Count - 1) {
					EaseCurve.Instance.Vec3 (panel.transform, panel.transform.localPosition, toPos, speed, wait, EaseCurve.Instance.custom, null, "local");
				} else {
					EaseCurve.Instance.Vec3 (panel.transform, panel.transform.localPosition, toPos, speed, wait, EaseCurve.Instance.custom, PlacingFinished, "local");
				}
				Material mat = ccGo.transform.Find ("Container").Find ("Quad").GetComponent<Renderer> ().material; 
				Color32 fromColor = environments [currEnv].envColor;//ChangeColorBrightness(environments [currEnv].envColor, -1f);
				fromColor.a = 0;
				mat.color = fromColor;
				Color32 toCol = environments [currEnv].envColor;
				toCol.a = 175;
				EaseCurve.Instance.MatColor (mat, fromColor, toCol, speed, wait, EaseCurve.Instance.custom);
			}
			i++;
		}
		if(currEnv == environments.Count - 1 || kioskColumns.Contains(1)){
//			Debug.Log (currEnv+"...UNsaving current title cam");
			currTitleCam = null;
			currTitlePanel = null;
		}
		yield return null;
	}

	void TitleDown(){
		Debug.Log ("[TitleDown] title should hide " + hideTitleAfterInroTransition);
		if (hideTitleAfterInroTransition) {
			titleHidden = true;
			hideTitleAfterInroTransition = false;
			idleSequence [0].panel.GetComponent<PanelBase> ().panelState = PanelBase.PanelState.Hidden;
			EaseCurve.Instance.Vec3 (idleSequence [0].panel.transform, idleSequence [0].panel.transform.localPosition, idleSequence [0].fromPos, 2f*0.5f, 2f*0.1f, EaseCurve.Instance.custom, null, "local");
		}
	}

	void PlacingFinished(){
		Debug.Log ("[PlacingFinished]");
		foreach (CellAction p in idleSequence) {
			p.panel.GetComponent<PanelBase>().panelState = PanelBase.PanelState.Active;
		}
		timeElapsedSinceLastTransition = 0;
		panelsInTransition = false;
		titleHidden = false;
	}

	void UnPlaceCameras(){
		Debug.Log ("[UnPlaceCameras]");
		panelsInTransition = true;
		float zDepth = 50;
		float layerDepth = 0;
		float group = 2;
		float duration = 0;
		float delay = 0;

		if (idleSequence.Count > 0) {
			for (int i = idleSequence.Count - 1; i > 0; i--) {
				duration = group * 0.2f;
				delay = group * 0.05f;
				if (i > 1) {
					if (idleSequence [i - 1].distanceToOrigin != idleSequence [i].distanceToOrigin)
						group++;
				}

				//if a panel was tapped on in the idle state, it gets moved to the kiosk
				//since it no longer part of the idleSequence, we dont want to animate it
				//but not animating <something> will throw off the timing of the remaining panels in the idleSequence
				//in that case, we use the color quad as a stand-in for the panel object
				Transform p = idleSequence [i].panel.transform;
				if (p.parent.name != "Container") {
					p = idleSequence [i].cellCam.transform.Find ("Container");
				} else {
					idleSequence [i].cellCam.transform.Find ("Container").Find ("Quad").gameObject.SetActive (false);
					idleSequence [i].panel.GetComponent<PanelBase> ().panelState = PanelBase.PanelState.Animating;
				}

				if (i == 1) {
					EaseCurve.Instance.Vec3 (p, idleSequence [i].toPos, idleSequence [i].fromPos, duration, delay, EaseCurve.Instance.custom2, nextSequence, "local");
				} else {
					EaseCurve.Instance.Vec3 (p, idleSequence [i].toPos, idleSequence [i].fromPos, duration, delay, EaseCurve.Instance.custom2, null, "local");
				}
			}

			if ((currEnv == environments.Count - 1 || kioskColumns.Contains (1)) && !titleHidden) {
				idleSequence [0].panel.GetComponent<PanelObject> ().panelState = PanelObject.PanelState.Animating;
				titleHidden = true;
				idleSequence [0].cellCam.transform.Find ("Container").Find ("Quad").gameObject.SetActive (false);
				EaseCurve.Instance.Vec3 (idleSequence [0].panel.transform, idleSequence [0].toPos, idleSequence [0].fromPos, duration, delay+0.2f, EaseCurve.Instance.custom2, null, "local");
			}
		} else {
			nextSequence ();
		}
	}

	public void nextSequence(){
		Debug.Log ("[nextSequence]");
		panelsInTransition = false;
		if (kioskColumns.Contains (1)) {
			//displatch event that we've switched environments
			EventsManager.Instance.EnvironmentSwitchRequest ();
		}
		//CloseKiosks ();
		ClearCellCams ();
		PlanLayout ();
	}

	Rect CreateCameraCell(int _row, int _col, Vector2 _size, Vector3 _dir){
		Rect camRect;
		int col = _col;
		int row = _row;

		float cellW = 1f / GM.desiredGrid.x;
		float cellH = 1f / GM.desiredGrid.y;
		float x = cellW * _col;
		if (_size.x == 2f && _dir == Vector3.left)
			x = cellW * (_col - 1f);
		float y = cellH * ((GM.desiredGrid.y - 1f) - _row);
		if (_size.x == 2f)
			y = cellH * ((GM.desiredGrid.y - 1f) - (_row + 1f));
		float w = cellW * _size.x;
		float h = cellH * _size.y;

		camRect = new Rect (x, y, w, h);
		return camRect;
	}
		
	void ZoomBG(){
		int nextBGi = currBg+1;
		if (nextBGi == bgPanels.Count)
			nextBGi = 0;
		bgPanels [nextBGi].transform.position = nextPosition;
		bgPanels [nextBGi].PlayVideo ();
		Material bgMat;
		if (ScreenManager.Instance.currAspect == ScreenManager.Aspect.is329) {
			bgMat = bgPanels[currBg].frontFullPanelTexture329.GetComponent<Renderer> ().material;
			bgPanels [nextBGi].frontFullPanelTexture329.GetComponent<Renderer> ().material.color = baseColor;
		} else {
			bgMat = bgPanels[currBg].frontFullPanelTexture.GetComponent<Renderer> ().material;
			bgPanels [nextBGi].frontFullPanelTexture.GetComponent<Renderer> ().material.color = baseColor;
		}
		Color32 toColor = bgMat.color;
		toColor.a = 0;
		EaseCurve.Instance.Scl (bgPanels[currBg].transform, bgPanels[currBg].transform.localScale, bgPanels[currBg].transform.localScale * 1.5f, 0.5f, 0, EaseCurve.Instance.linear, UpdateBG);
		EaseCurve.Instance.MatColor (bgMat, baseColor, toColor, 0.5f, 0, EaseCurve.Instance.linear);
	}


	Vector3 baseScale;
	Vector3 activePosition = new Vector3 (0, 0, 100);
	Vector3 nextPosition = new Vector3 (0, 0, 105);
	Vector3 readyPosition = new Vector3 (0, 100, 100);
	Color32 baseColor = new Color32 (255, 255, 255, 255);


	void UpdateBG(){
//		PanelObject activeBg;
//		PanelObject readyBg;
//		if (currBg == 1) {
//			activeBg = AM.bgPanel2;
//			readyBg = AM.bgPanel1;
//			currBg = 2;
//		} else {
//			activeBg = AM.bgPanel1;
//			readyBg = AM.bgPanel2;
//			currBg = 1;
//		}
		//move move current bg to back on the line
		baseScale = new Vector3 (GM.desiredGrid.x, GM.desiredGrid.x, GM.desiredGrid.x);

//		if (SM.currAspect == ScreenManager.Aspect.is329) {
//			bgPanels[currBg].frontFullPanelTexture329.GetComponent<Renderer> ().material.color = baseColor;
//		} else {
//			bgPanels[currBg].frontFullPanelTexture.GetComponent<Renderer> ().material.color = baseColor;
//		}
		bgPanels[currBg].PauseVideo ();
		//bgPanels[currBg].gameObject.SetActive(false);
		bgPanels[currBg].transform.position = readyPosition;
		bgPanels[currBg].transform.localScale = baseScale;
		//set next bg as current
		currBg++;
		if (currBg == bgPanels.Count)
			currBg = 0;
		bgPanels[currBg].transform.position = activePosition;
		//set up on deck
//		int nextBGi = currBg<bgPanels.Count-1?currBg+1:0;
//		bgPanels[nextBGi].transform.position = nextPosition;
		//bgPanels[nextBGi].gameObject.SetActive (true);


		//

//		activeBg.transform.position = activePosition;
//		readyBg.transform.position = readyPosition;
//		readyBg.transform.localScale = baseScale;
//		if (ScreenManager.Instance.currAspect == ScreenManager.Aspect.is329) {
//			readyBg.frontFullPanelTexture329.GetComponent<Renderer> ().material.color = baseColor;
//			readyBg.SetAs329Video (false);
//		} else {
//			readyBg.frontFullPanelTexture.GetComponent<Renderer> ().material.color = baseColor;
//			readyBg.SetAsVideo (true, false);
//		}
	}
}
