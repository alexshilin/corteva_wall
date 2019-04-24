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
	public class PanelAction{
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
	private float panelDepth = 50f;
	int[][] layoutGrid;
	float negSpacePercentage = 1;
	string[] availableTypes = new string[]{"1x1","1x1","1x1","1x1","1x2","2x2"};
	List<string> usedTypes = new List<string>();
	public List<PanelAction> idleSequence = new List<PanelAction>();
	private List<PanelAction> idleSequenceTmp = new List<PanelAction>();
	Vector3 previousPanelPos = Vector3.zero;
	Vector2 startPanelPos;
	int[] kioskColumns;

	float timeElapsedSinceLastTransition;
	float timeToNextTransition = 10f;
	bool activeTransitionLoop = false;
	#endregion


	List<Environment> environments;
	int currEnv = -1;
	int startColumn;

	int currBg = 1;

	Transform currTitleCam;
	Transform currTitlePanel;

	bool aKioskIsOpen = false;
	bool panelsInTransition = false;
	IEnumerator mainLoop;

	void Start(){
		AM = AssetManager.Instance;
		GM = GridManagerOrtho.Instance;

		EventsManager.Instance.OnUserKioskRequest += updateFromKioskRequest;

		if (GM.desiredGrid.x == 6) {
			kioskColumns = new[]{ 0, 0, 0, 0, 0, 0 };
		} else {
			kioskColumns = new[]{ 0, 0, 0 };
		}

		environments = new List<Environment>(AssetManager.Instance.environments);
	}

	//NOTE: consider putting kiosk activation logic in this class.

	void OnDisable(){
		EventsManager.Instance.OnUserKioskRequest -= updateFromKioskRequest;
	}

	void Update(){
		if (Input.GetKeyDown (KeyCode.Alpha0)) {
			PlanLayout ();
		}
		if (Input.GetKeyDown (KeyCode.Alpha9)) {
			//InvokeRepeating("UnPlaceCameras", 0f, 10f);
			StartIdleLoop ();
		}
		if (Input.GetKeyDown (KeyCode.Backspace)) {
			ClearPanels ();
		}
		if (Input.GetKeyDown (KeyCode.Alpha8)) {
			ZoomBG ();
		}
		if (activeTransitionLoop && !panelsInTransition) {
			timeElapsedSinceLastTransition += Time.deltaTime;
			if (timeElapsedSinceLastTransition >= timeToNextTransition) {
				UnPlaceCameras ();
			}
		}
	}

	void StartIdleLoop(){
		Debug.Log ("[StartIdleLoop]");
		timeElapsedSinceLastTransition = timeToNextTransition;
		panelsInTransition = false;
		activeTransitionLoop = true;
//		mainLoop = LoopIdle ();
//		StartCoroutine(mainLoop);
//		UnPlaceCameras ();
	}
//	IEnumerator LoopIdle(){
//		while (true) {
//			yield return new WaitForSeconds (5f + 8f);
//		}
//	}

	void updateFromKioskRequest (Vector2 _gridPos, bool _doOpen, Environment _env){
		aKioskIsOpen = true;
		Vector2 gridPos = _gridPos;
		bool doOpen = _doOpen;

		//2 display
//		if (gridPos.x > 2) {
//			gridPos.x -= 3;
//		}
		//end
		if (doOpen) {
			kioskColumns [(int)gridPos.x] = 1;
			Debug.Log (kioskColumns.Length + " " + (int)gridPos.x);
			if (idleSequence.Count > 0) {
				PanelAction pa = idleSequence.Find (x => x.row == gridPos.y && x.col == gridPos.x);
				if (pa != null) {
					pa.panel = null;
					HideTitlePanel ();
				} else {
					if (idleSequence [0].panel != null) {
						HideTitlePanel ();
					} else {
						StartIdleLoop ();
					}
				}
			} else {
				StartIdleLoop ();
			}
		} else {
			Debug.Log ("close kiosk " + _gridPos.x);
			//TODO
			//animate kiosk out
			//then animate replacement panel in
			//update kiosk list
			kioskColumns [(int)gridPos.x] = 0;
		}
//		if(mainLoop!=null)
//			StopCoroutine (mainLoop);

	}

	void HideTitlePanel(){
		if (!panelsInTransition) {
			panelsInTransition = true;
			EaseCurve.Instance.Vec3 (idleSequence [0].panel.transform, idleSequence [0].toPos, idleSequence [0].fromPos, 0.5f, 0.25f, EaseCurve.Instance.custom2, StartIdleLoop, "local");
		}
	}

	void PlanLayout(){
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

		if (currEnv == -1) {
			//wall is totally clear
			currEnv = 0;
			if (GM.desiredGrid.x == 6) {
				startColumn = Random.Range (1, (int)GM.desiredGrid.x - 1);
			} else {
				startColumn = Random.Range (0, (int)GM.desiredGrid.x);
			}
		} else if (currEnv == environments.Count - 1) {
			//we're at the last environment
			//choose new start position
			currEnv = 0;
			if (GM.desiredGrid.x == 6) {
				startColumn = Random.Range (1, (int)GM.desiredGrid.x - 1);
			} else {
				startColumn = Random.Range (0, (int)GM.desiredGrid.x);
			}
		} else {
			//we're still iterating through environments
			//keep start position
			currEnv++;
		}
		if (aKioskIsOpen) {
			//if a kiosk is open, overwrite idle rules
			//pick new start position each time, excluding in columns with kiosks

			//Debug.Log("kiosk columns: "+System.String.Join("", new List<int>(kioskColumns).ConvertAll(i => i.ToString()).ToArray()));
			List<int> kCols = new List<int> ();
			for (int i = 0; i < kioskColumns.Length; i++) {
				if (kioskColumns [i] == 0) {
					kCols.Add (i);
				} else {
//					Debug.Log ("EXcluding column " + i + " from start options");
				}
			}
			//Debug.Log ("open columns: " + System.String.Join("", kCols.ConvertAll(i => i.ToString()).ToArray()));
			startColumn = kCols[Random.Range (0, kCols.Count)];
		}

		Debug.Log ("\tENV (" + currEnv + " of "+environments.Count+"): " + environments [currEnv].envTitle);

		//reset vars
		usedTypes.Clear ();
		previousPanelPos = Vector3.zero;
//		if (idleSequence.Count > 0) {
//			idleSequenceTmp = new List<PanelAction>(idleSequence);
//		}
		idleSequence.Clear ();

		//pick random cell in top row
		//int startColumn = Random.Range (1, (int)GM.desiredGrid.x-1);
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

	void CheckLeft(int _row, int _col){
		//how many cells remain open to the left of this cell
		int colsRemainL = _col;
//		Debug.Log ("\t[CheckLeft] from ["+_row+","+_col+"] (" + colsRemainL + " open)");
		if (colsRemainL == 0) {
			//end of row (screen)
			//place something in bottom row?
			int r = Random.Range (0, 2);
			if (r == 1)
				CheckDown (_row, _col);
			return;
		}
		int allowedTypes = colsRemainL == 1 ? 2 : 3;
		if (aKioskIsOpen)
			allowedTypes = 1;
		//once cell remains, can place 1x1 or 1x2 panels only
		//iterate to next cell
		int nextCol = _col - 1;
		//check which types of panels we have remaning in this environemnt
		string p = CheckAvailable (_row, nextCol, allowedTypes);
		//check which of those can be used based on layout rules (TBD)
		//TestFit ();
		//place the chosen panel in adjacent cell
		AllocatePanelPlacement (_row, nextCol, p, Vector3.left);

		//if we happened to have placed a 2x2 cell, iterate one cell further than next
		if (p == "2x2")
			nextCol--;
		//recur
		CheckLeft (_row, nextCol);
	}

	void CheckRight(int _row, int _col){
		//same as CheckLeft but in the other direction
		int colsRemainR = (int)GM.desiredGrid.x - (_col + 1);
//		Debug.Log ("\t[CheckRight] from ["+_row+","+_col+"] (" + colsRemainR + " open)");
		if (colsRemainR == 0) {
			int r = Random.Range (0, 2);
			if (r == 1)
				CheckDown (_row, _col);
			return;
		}
		int allowedTypes = colsRemainR == 1 ? 2 : 3;
		if (aKioskIsOpen)
			allowedTypes = 1;
		int nextCol = _col + 1; 
		string p = CheckAvailable (_row, nextCol, allowedTypes);
		//TestFit ();
		AllocatePanelPlacement (_row, nextCol, p, Vector3.right);
		if (p == "2x2")
			nextCol++;
		CheckRight (_row, nextCol);
	}

	void CheckDown(int _row, int _col){
//		Debug.Log ("\t[CheckDown] from ["+_row+","+_col+"]");
		if (!aKioskIsOpen) {
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
		bool canPlace = true;
//		Debug.Log("\t\t[TestFit] "+_type+" in cell ["+_row+","+_col+"]");

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

		//set up panel action
		PanelAction pa = new PanelAction();

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

	static int SortByDistanceFromOrigin(PanelAction pa1, PanelAction pa2)
	{
		return pa1.distanceToOrigin.CompareTo(pa2.distanceToOrigin);
	}
		

	IEnumerator PlaceCellCameras(){
		Debug.Log ("[PlaceCellCameras]");
		idleSequence.Sort(SortByDistanceFromOrigin);
		float zDepth = 50;
		float layerDepth = 0;
		float group = 2;
		float titlePauseTime = 0;
		int i = 0;
		//for (int i = 0; i < idleSequence.Count; i++) {
		while(i<idleSequence.Count){
			GameObject ccGo;

			if (i == 1) {
				Debug.Log ("\twaited " + (Time.time - titlePauseTime) + " after title");
			}

			if ((i==0 && currEnv > 0) && !aKioskIsOpen) {
				//this is the title panel
//				Debug.Log (currEnv+"...adjusting current title panel in existing title cam");
				currTitlePanel.position += Vector3.forward * 10;
				Destroy (currTitlePanel.gameObject, 1f);
				ccGo = currTitleCam.gameObject;
			} else {
				ccGo = Instantiate (AM.cellCameraPrefab);
				ccGo.SetActive (true);
				Camera ccCam = ccGo.GetComponentInChildren<Camera> ();
				ccCam.GetComponent<TouchScript.Layers.StandardLayer> ().Name = "CellCamera "+idleSequence[i].col+", "+idleSequence[i].row;
				//two displays
//				if (idleSequence [i].col > 2) {
//					ccCam.targetDisplay = 1;
//				} else {
//					ccCam.targetDisplay = 0;
//				}
				Rect rec = CreateCameraCell (idleSequence[i].row, idleSequence[i].col, idleSequence[i].panelType, idleSequence[i].direction);
				ccCam.rect = rec;
				ccGo.transform.parent = AM.cams;
				ccGo.transform.localPosition = new Vector3 (ccGo.transform.position.x + (idleSequence [i].col * 40f), ccGo.transform.position.y - (idleSequence [i].row * 20), 0);
				ccGo.name = idleSequence[i].col + ", " + idleSequence[i].row;
			}
			GameObject panel = Instantiate (AM.panelPrefab);

			panel.transform.parent = ccGo.transform.Find ("Container");
			panel.transform.localPosition = Vector3.zero;
			panel.transform.localScale = Vector3.one;
			panel.transform.localPosition = Vector3.Scale( -idleSequence [i].direction, new Vector3(6f, 3.5f, 0f));
			Vector3 toPos = Vector3.zero;

			panel.GetComponent<PanelObject>().panelID = idleSequence[i].col + (int)GM.desiredGrid.x * idleSequence[i].row;
			panel.GetComponent<PanelObject> ().panelState = "Idle";
			panel.GetComponent<PanelObject> ().panelGridPos = new Vector2 (idleSequence [i].col, idleSequence [i].row);
			panel.GetComponent<PanelObject> ().env = environments [currEnv];

			panel.name = "Panel "+idleSequence [i].col+", "+idleSequence [i].row;

			if(idleSequence [i].panelType != new Vector2 (1, 1)){
				panel.GetComponent<PanelObject> ().canInteract = false;
			}
				
			panel.GetComponent<PanelObject> ().SetPanelColors (environments [currEnv].envColor);
			if (i == 0) {
				panel.GetComponent<PanelObject> ().SetAsTitle (environments [currEnv].envTitle);
			}else if (idleSequence[i].panelType == new Vector2(1,2)) {
				panel.GetComponent<PanelObject> ().SetAsImage1x2 ();
				//panel.GetComponent<PanelObject> ().SetAsNonInteractive ();
				toPos.x += (5.33333f / 4f);
			}else {
				int r = Random.Range (0, 3);
				if (r == 1) {
					panel.GetComponent<PanelObject> ().SetAs3dViz ();
				} else if (r == 2) {
					panel.GetComponent<PanelObject> ().SetAsVideo (false, true);
					//panel.GetComponent<PanelObject> ().SetAsImage ();
				} else {
					panel.GetComponent<PanelObject> ().SetAsImage ();
				}
			}

			idleSequence [i].fromPos = panel.transform.localPosition;
			idleSequence [i].toPos = toPos;
			idleSequence [i].panel = panel;
			idleSequence [i].cellCam = ccGo;

			if (kioskColumns [idleSequence [i].col] == 1) {
				//theres a kiosk here, place the cell cam/panel without animating and disable
				panel.transform.localPosition = toPos;
				ccGo.SetActive (false);
			} else {
				//animate the panel
				float speed = group * 0.5f;
				float wait = group * 0.1f;
				if (i < idleSequence.Count - 1) {
					if (idleSequence [i + 1].distanceToOrigin != idleSequence [i].distanceToOrigin)
						group++;
				}
				//if (i == 0 && currEnv < environments.Count - 1) {
				if ((i == 0 && currEnv < environments.Count - 1) && !aKioskIsOpen) {
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
			}
			i++;
		}
		if(currEnv == environments.Count - 1){
//			Debug.Log (currEnv+"...UNsaving current title cam");
			currTitleCam = null;
			currTitlePanel = null;
		}
		yield return null;
	}

	void PlacingFinished(){
		Debug.Log ("[PlacingFinished]");
		timeElapsedSinceLastTransition = 0;
		panelsInTransition = false;
	}

	void UnPlaceCameras(){
		Debug.Log ("[UnPlaceCameras]");
		panelsInTransition = true;
		float zDepth = 50;
		float layerDepth = 0;
		float group = 2;
		float speed = 0;
		float wait = 0;

		//yuck, fix this!
		if (!aKioskIsOpen) {
			for (int i = idleSequence.Count - 1; i > 0; i--) {
				speed = group * 0.2f;
				wait = group * 0.05f;
				if (i > 1) {
					if (idleSequence [i - 1].distanceToOrigin != idleSequence [i].distanceToOrigin)
						group++;
				}
				idleSequence [i].cellCam.transform.Find ("Container").Find ("Quad").gameObject.SetActive (false);
				if (idleSequence [i].panel != null) {
					if (i == 1) {
						EaseCurve.Instance.Vec3 (idleSequence [i].panel.transform, idleSequence [i].toPos, idleSequence [i].fromPos, speed, wait, EaseCurve.Instance.custom2, nextSequence, "local");
					} else {
				
						EaseCurve.Instance.Vec3 (idleSequence [i].panel.transform, idleSequence [i].toPos, idleSequence [i].fromPos, speed, wait, EaseCurve.Instance.custom2, null, "local");
					}
				}
			}
			if (currEnv == environments.Count - 1) {
				idleSequence [0].cellCam.transform.Find ("Container").Find ("Quad").gameObject.SetActive (false);
				EaseCurve.Instance.Vec3 (idleSequence [0].panel.transform, idleSequence [0].toPos, idleSequence [0].fromPos, speed, wait, EaseCurve.Instance.custom2, null, "local");
			}
		} else {
			if (idleSequence.Count > 0) {
				int alreadyRemoved = (idleSequence [0].panel.transform.localPosition != idleSequence [0].fromPos) ? 0 : 1;
				for (int i = idleSequence.Count - 1; i >= alreadyRemoved; i--) {
					if (idleSequence [i].cellCam.activeSelf) {
						speed = group * 0.2f;
						wait = group * 0.05f;
						if (i > alreadyRemoved) {
							if (idleSequence [i - 1].distanceToOrigin != idleSequence [i].distanceToOrigin)
								group++;
						}
						idleSequence [i].cellCam.transform.Find ("Container").Find ("Quad").gameObject.SetActive (false);
						if (idleSequence [i].panel != null) {
							if (i == alreadyRemoved) {
								EaseCurve.Instance.Vec3 (idleSequence [i].panel.transform, idleSequence [i].toPos, idleSequence [i].fromPos, speed, wait, EaseCurve.Instance.custom2, nextSequence, "local");
							} else {
								EaseCurve.Instance.Vec3 (idleSequence [i].panel.transform, idleSequence [i].toPos, idleSequence [i].fromPos, speed, wait, EaseCurve.Instance.custom2, null, "local");
							}
						}
					}
				}
			} else {
				nextSequence ();
			}
		}
	}

	public void nextSequence(){
		ClearPanels ();
		PlanLayout ();
	}



	//clear all panels and reset depth
	void ClearPanels(){
		Debug.Log ("[ClearPanels] " + AM.cams.childCount + " panels");
//		foreach (Transform child in AM.panels) {
//			GameObject.Destroy(child.gameObject);
//		}
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

	Rect CreateCameraCell(int _row, int _col, Vector2 _size, Vector3 _dir){
		Rect camRect;
		int col = _col;
		int row = _row;

		/*
		float cellW = 1f / 3f;
		float cellH = 1f / GM.desiredGrid.y;
		if (col > 2) {
			col -= 3; //last 3 cols on second display, restart at 0
		}
		float x = cellW * col;
		if (_size.x == 2 && _dir == Vector3.left)
			x = cellW * (col - 1);
		float y = cellH * ((GM.desiredGrid.y - 1) - row);
		if (_size.x == 2)
			y = cellH * ((GM.desiredGrid.y - 1) - (row + 1));
		float w = cellW * _size.x;
		float h = cellH * _size.y;
		*/


		//single 32:9 display
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
		PanelObject activeBg;
		PanelObject readyBg;
		if (currBg == 1) {
			activeBg = AM.bgPanel1;
			readyBg = AM.bgPanel2;
		} else {
			activeBg = AM.bgPanel2;
			readyBg = AM.bgPanel1;
		}
		readyBg.PlayVideo ();
		Material bgMat = activeBg.frontFullPanelTexture329.GetComponent<Renderer> ().material;
		Color32 toColor = bgMat.color;
		toColor.a = 0;
		//EaseCurve.Instance.Scl (AM.bgPanel2.transform, AM.bgPanel2.transform.localScale * 2, AM.bgPanel1.transform.localScale, 0.5f, 0, EaseCurve.Instance.linear);
		EaseCurve.Instance.Scl (activeBg.transform, activeBg.transform.localScale, activeBg.transform.localScale * 1.5f, 0.5f, 0, EaseCurve.Instance.linear, UpdateBG);
		EaseCurve.Instance.MatColor (bgMat, bgMat.color, toColor, 0.5f, 0, EaseCurve.Instance.linear);
	}
	Vector3 baseScale;// = new Vector3 (GM.desiredGrid.x, GM.desiredGrid.x, GM.desiredGrid.x);
	Vector3 activePosition = new Vector3 (0, 0, 100);
	Vector3 readyPosition = new Vector3 (0, 0, 110);
	Color32 baseColor = new Color32 (255, 255, 255, 255);
	void UpdateBG(){
		PanelObject activeBg;
		PanelObject readyBg;
		if (currBg == 1) {
			activeBg = AM.bgPanel2;
			readyBg = AM.bgPanel1;
			currBg = 2;
		} else {
			activeBg = AM.bgPanel1;
			readyBg = AM.bgPanel2;
			currBg = 1;
		}
		baseScale = new Vector3 (GM.desiredGrid.x, GM.desiredGrid.x, GM.desiredGrid.x);
		activeBg.transform.position = activePosition;
		readyBg.transform.position = readyPosition;
		readyBg.transform.localScale = baseScale;
		readyBg.frontFullPanelTexture329.GetComponent<Renderer> ().material.color = baseColor;
		if (ScreenManager.Instance.currAspect == ScreenManager.Aspect.is329) {
			readyBg.SetAs329Video (false);
		} else {
			readyBg.SetAsVideo (true, false);
		}
	}
}
