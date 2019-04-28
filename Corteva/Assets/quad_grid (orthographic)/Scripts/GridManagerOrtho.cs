using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class GridManagerOrtho : MonoBehaviour {

	[System.Serializable]
	public class GridItem{
		public int id;
		public int row;
		public int col;
		public Vector2 center;
		public Vector2 topLeft;
		public Vector2 topRight;
		public Vector2 bottomLeft;
		public Vector2 bottomRight;
	}
		
	[HideInInspector]
	public Vector2 desiredGrid = new Vector2 (6, 3);
	[HideInInspector]
	public List<GridItem> gridPositions = new List<GridItem>();

	private Vector2 desiredFullScreenAspect = new Vector2 (32, 9);
	private Vector2 desiredGridElementAspect = new Vector2 (16, 9);
	private Vector2 calculatedGridElementSize;



	//Instance
	private static GridManagerOrtho _instance;
	public static GridManagerOrtho Instance { get { return _instance; } }
	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		} else {
			_instance = this;
		}
	}

	void Start () {
		EventsManager.Instance.OnAssetsFinishedLoading += assetsLoaded;
		EventsManager.Instance.OnClearEverything += recalculateGrid;
	}

	void OnDisabled(){
		EventsManager.Instance.OnClearEverything -= recalculateGrid;
	}

	void assetsLoaded(){
		EventsManager.Instance.OnAssetsFinishedLoading -= assetsLoaded;
		Init ();
	}

	void recalculateGrid(){
		//Init ();
	}

	void Init(){
		Debug.Log ("[Init]");
		AssetManager.Instance.mainCamera.orthographicSize = desiredFullScreenAspect.y * 0.5f;
		AssetManager.Instance.userInitCamera.orthographicSize = desiredFullScreenAspect.y * 0.5f;

		//EventsManager.Instance.OnSceneFinishedLoading += sceneLoaded;
		//AssetManager.Instance.LoadScene("earth_bg");
		//AssetManager.Instance.LoadScene("user_space");

		if (ScreenManager.Instance.currAspect == ScreenManager.Aspect.is169) {
			Debug.Log ("[16:9] SETTING 3x3 GRID");
			desiredGrid = new Vector2 (3, 3);
			desiredFullScreenAspect = new Vector2 (16, 9);
		}
		if (ScreenManager.Instance.currAspect == ScreenManager.Aspect.is329) {
			Debug.Log ("[32:9] SETTING 6x3 GRID");
			desiredGrid = new Vector2 (6, 3);
			desiredFullScreenAspect = new Vector2 (32, 9);
		}

		CalculateGridPositions ();

		IdleStateController.Instance.Prepare ();
	}

	void sceneLoaded(string _scene){
		if (_scene == "earth_bg") {
			//AssetManager.Instance.OnSceneFinishedLoading -= sceneLoaded;
			AssetManager.Instance.mainCamera.clearFlags = CameraClearFlags.Depth;
		}
	}

	void OnDrawGizmos(){
		for (int i = 0; i < gridPositions.Count; i++) {
			Gizmos.color = Color.cyan;
			Vector3 tlV3 = gridPositions [i].topLeft;
			tlV3.z = 5f;
			Gizmos.DrawSphere (tlV3, 0.1f);

			Gizmos.color = Color.magenta;
			Vector3 trV3 = gridPositions [i].topRight;
			trV3.z = 6f;
			Gizmos.DrawSphere (trV3, 0.1f);

			Gizmos.color = Color.white;
			Vector3 cV3 = gridPositions [i].center;
			cV3.z = 5f;
			Gizmos.DrawSphere (cV3, 0.2f);

			Gizmos.color = Color.yellow;
			Vector3 blV3 = gridPositions [i].bottomLeft;
			blV3.z = 6f;
			Gizmos.DrawSphere (blV3, 0.1f);

			Gizmos.color = Color.green;
			Vector3 brV3 = gridPositions [i].bottomRight;
			blV3.z = 6f;
			Gizmos.DrawSphere (brV3, 0.1f);
		}
	}

	void CalculateGridPositions(){
		float totalPanels = desiredGrid.x * desiredGrid.y;
		int xx = 0;
		int yy = 0;
		calculatedGridElementSize = desiredGridElementAspect / desiredGrid.y;
		Vector3 startPos = new Vector3 (desiredFullScreenAspect.x*-0.5f, desiredFullScreenAspect.y*0.5f, 40);
		Vector3 startSize = new Vector3 (calculatedGridElementSize.x, calculatedGridElementSize.y, 1);
		for (int i = 0; i < totalPanels; i++) {
			Vector3 step = new Vector3 (startSize.x * xx, startSize.y * yy, 0);
			Vector3 centerOffset = new Vector3 (startSize.x * 0.5f, startSize.y * -0.5f, 0);
			Vector3 topRightOffset = new Vector3 (startSize.x, 0, 0);
			Vector3 bottomLeftOffset = new Vector3 (0, -startSize.y, 0);
			Vector3 bottomRightOffset = new Vector3 (startSize.x, -startSize.y, 0);
			Vector3 panelPos = startPos + step + centerOffset;
			GridItem gi = new GridItem();
			gi.id = i;
			gi.row = yy + 1;
			gi.col = xx + 1;
			gi.topLeft = startPos + step;
			gi.topRight = startPos + step + topRightOffset;
			gi.center = startPos + step + centerOffset;
			gi.bottomLeft = startPos + step + bottomLeftOffset;
			gi.bottomRight = startPos + step + bottomRightOffset;
			gridPositions.Add (gi);
			xx++;
			if (xx == desiredGrid.x) {
				xx = 0;
				yy--;
			}
		}
	}

	public Vector2 CalculateColRowFromScreenPos(Vector2 _screenPos){
		//calculate row/col from screen position
		float row = _screenPos.y/(Screen.height/desiredGrid.y);//(1/desiredGrid.y);
		float col = _screenPos.x/(Screen.width/desiredGrid.x);
		Debug.Log ("CalculateColRowFromScreenPos: "+_screenPos+" > "+(int)row+", "+(int)col);
		row = (int)row;
		col = (int)col;
		return new Vector2 (col, row);
	}


	/*
	Rect CreateCameraCell(int _row, int _col, Vector2 _size, Vector3 _dir){
		Rect camRect;
		float cellW = 1 / desiredGrid.x;
		float cellH = 1 / desiredGrid.y;
		float x = cellW * _col;
		if (_size.x == 2 && _dir == Vector3.left)
			x = cellW * (_col - 1);
		float y = cellH * ((desiredGrid.y - 1) - _row);
		if (_size.x == 2)
			y = cellH * ((desiredGrid.y - 1) - (_row + 1));
		float w = cellW * _size.x;
		float h = cellH * _size.y;
		camRect = new Rect (x, y, w, h);
		return camRect;
	}


	#region masked layout
	int[][] layoutGrid;
	float negSpacePercentage = 1;
	string[] availableTypes = new string[]{"1x1","1x1","1x1","1x1","1x2","2x2"};
	List<string> usedTypes = new List<string>();
	public List<PanelAction> idleSequence = new List<PanelAction>();
	private List<PanelAction> idleSequenceTmp = new List<PanelAction>();
	Vector3 previousPanelPos = Vector3.zero;
	Vector2 startPanelPos;
	Vector3 previousBranch = Vector3.zero;
	int prevDir;

	void PlanLayout(){
		//generate blank layout grids
		if (desiredGrid.x == 6) {
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

		//reset planning vars
		usedTypes.Clear ();
		prevDir = 0;
		previousPanelPos = Vector3.zero;
		previousBranch = Vector3.zero;
//		if (idleSequence.Count > 0) {
//			idleSequenceTmp = new List<PanelAction>(idleSequence);
//		}
		idleSequence.Clear ();

		//pick random cell in top row
		int startColumn = Random.Range (1, (int)desiredGrid.x-1);
		startPanelPos = new Vector2 (0, startColumn);
		Debug.Log ("[PlantLayout] STARTING at column [" + startColumn+"]");

		//place ENV. TITLE panel there
		SetPanel (0, startColumn, "1x1", Vector3.down);

		//should be: go DOWN (because currently in top row)
		SetPanel (1, startColumn, "1x1", Vector3.down);

		//in mid row, go outwards
		CheckLeft (1, startColumn);
		CheckRight (1, startColumn);

		Debug.Log ("ANIMATING PANELS");
		//StartCoroutine(PlacePanels ());
//		if (idleSequenceTmp.Count > 0) {
//			UnPlaceCameras ();
//		}
		PlaceCameras();
	}

	void CheckLeft(int _row, int _col){
		//how many cells remain open to the left of this cell
		int colsRemainL = _col;
		Debug.Log ("\t[CheckLeft] from ["+_row+","+_col+"] (" + colsRemainL + " open)");
		if (colsRemainL == 0) {
			//end of row (screen)
			//place something in bottom row?
			int r = Random.Range (0, 2);
			if (r == 1)
				CheckDown (_row, _col);
			return;
		}
		int allowedTypes = colsRemainL == 1 ? 2 : 3;
		//once cell remains, can place 1x1 or 1x2 panels only
		//iterate to next cell
		int nextCol = _col - 1;
		//check which types of panels we have remaning in this environemnt
		string p = CheckAvailable (_row, nextCol, allowedTypes);
		//check which of those can be used based on layout rules (TBD)
		//TestFit ();
		//place the chosen panel in adjacent cell
		SetPanel (_row, nextCol, p, Vector3.left);

		//if we happened to have placed a 2x2 cell, iterate one cell further than next
		if (p == "2x2")
			nextCol--;
		//recur
		CheckLeft (_row, nextCol);
	}

	void CheckRight(int _row, int _col){
		//same as CheckLeft but in the other direction
		int colsRemainR = (int)desiredGrid.x - (_col + 1);
		Debug.Log ("\t[CheckRight] from ["+_row+","+_col+"] (" + colsRemainR + " open)");
		if (colsRemainR == 0) {
			int r = Random.Range (0, 2);
			if (r == 1)
				CheckDown (_row, _col);
			return;
		}
		int allowedTypes = colsRemainR == 1 ? 2 : 3;
		int nextCol = _col + 1; 
		string p = CheckAvailable (_row, nextCol, allowedTypes);
		//TestFit ();
		SetPanel (_row, nextCol, p, Vector3.right);
		if (p == "2x2")
			nextCol++;
		CheckRight (_row, nextCol);
	}

	void CheckDown(int _row, int _col){
		Debug.Log ("\t[CheckDown] from ["+_row+","+_col+"]");
		int nextRow = _row + 1;
		string p = CheckAvailable (nextRow, _col, 1); 
		bool ok = TestFit (nextRow, _col, "1x1", Vector3.down); 
		if(ok)
			SetPanel (nextRow, _col, "1x1", Vector3.down);
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
		float total = desiredGrid.x * desiredGrid.y;
		for (int i = 0; i < layoutGrid.Length; i++) {
			for (int ii = 0; ii < layoutGrid [i].Length; ii++) {
				if (layoutGrid [i] [ii] != 0)
					occupied++;
			}
		}
		float occupiedPercent = occupied / total;
		float emptyPercent = 1 - occupiedPercent;
		Debug.Log ("[CheckNegativeSpacePercentage] " + RoundDecimal (occupiedPercent, 1) + ":" + RoundDecimal (emptyPercent, 1));
		return RoundDecimal (emptyPercent, 1);
	}
	float RoundDecimal(float _value, int _decimals){
		float rounder = 10f * _decimals;
		return Mathf.Round(_value * rounder) / rounder;
	}

	bool TestFit(int _row, int _col, string _type, Vector3 _dir){
		bool canPlace = true;
		Debug.Log("\t\t[TestFit] "+_type+" in cell ["+_row+","+_col+"]");

		//rule 1: make sure cell isnt already occupied
		if (layoutGrid [_row] [_col] != 0) {
			return false;
		}

		//rule 2: no full columns (reserved for user kiosk)
		if (_row == 2 && _type=="1x1") {
			bool colAlmostFull = (layoutGrid [1] [_col] !=0 && layoutGrid [0] [_col] !=0) ? true : false;
			if (colAlmostFull) {
				Debug.Log ("\t\t[TestFit] failed, column too full");
				return false;
			}
		}

		return true;
	}

	void SetPanel(int _row, int _col, string _type, Vector3 _dir){
		//calculate 1Darray index(i) from 2Darray point(x,y)
		//this is used to grap world position coordinates from List previously calculated by GridManager
		int id = _col + (int)desiredGrid.x * _row;
		Debug.Log ("\t\t\t[PlacePanel] "+_type+" into " + _row + "," + _col + " [" + id + "]");

		//set up panel action
		Vector2 gridPos = Vector3.zero;
		Vector3 finalPos = Vector3.zero;
		PanelAction pa = new PanelAction();

		string panelName = _type+" @ ["+_row+","+_col+"]";
	
		pa.row = _row;
		pa.col = _col;

		//if panel should be 2x2
		if (_type == "2x2") {
			int cellDir = 0;
			//based on direction, use bottom right/left corner of open cell to center a 2x2 panel
			if (_dir == Vector3.right) {
				gridPos = gridPositions [id].bottomRight;
				cellDir = 1;
			}
			if (_dir == Vector3.left) {
				gridPos = gridPositions [id].bottomLeft;
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
				gridPos = gridPositions [id].topRight;
			if (_dir == Vector3.left)
				gridPos = gridPositions [id].topRight;

			finalPos = new Vector3 (gridPos.x, gridPos.y, panelDepth);

			pa.panelType = new Vector2(1,2);
			pa.toPos = finalPos;

			layoutGrid [_row] [_col] = 2;
			layoutGrid [_row - 1] [_col] = 2; 
		} else {
			//place panel
			finalPos = new Vector3 (gridPositions [id].center.x, gridPositions [id].center.y, panelDepth);

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

		idleSequence.Add (pa);

		//update previous panelPos
		previousPanelPos = finalPos;

		CheckNegativeSpacePercentage ();
	}

	static int SortByDistanceFromOrigin(PanelAction pa1, PanelAction pa2)
	{
		return pa1.distanceToOrigin.CompareTo(pa2.distanceToOrigin);
	}

	IEnumerator PlacePanels(){
		for (int r = 0; r < layoutGrid.Length; r++) {
			for(int c = 0; c<layoutGrid[r].Length; c++){
				if (layoutGrid [r] [c] == 0) {
					int id = c + (int)desiredGrid.x * r;
					GameObject m = Instantiate (panelMaskPrefab);
					m.transform.position = gridPositions [id].center;
					m.transform.parent = masks;
				}
			}
		}
		masks.transform.position += Vector3.forward;

		idleSequence.Sort(SortByDistanceFromOrigin);
		float zDepth = 50;
		float layerDepth = 0;
		float group = 2;
		for (int i = 0; i < idleSequence.Count; i++) {
			GameObject panel = Instantiate (panelPrefab);
			//panel.SetActive (false);
			//panel.name = panelName;

			idleSequence[i].fromPos.z = zDepth + layerDepth;
			idleSequence[i].toPos.z = zDepth + layerDepth;
			panel.transform.position = idleSequence[i].fromPos;

			if (idleSequence[i].panelType == new Vector2(2,2)) {
				panel.transform.localScale *= 2;
			} else if (idleSequence[i].panelType == new Vector2(1,2)) {
				panel.transform.localScale *= 2;
			}else{
			}

			layerDepth += 2;

			//panel.GetComponent<PanelObject> ().panelID = id;
			gridPanels.Add (panel.GetComponent<PanelObject> ());
			panel.transform.parent = panels;

			if (idleSequence[i].panelType == new Vector2(1,2)) {
				panel.GetComponent<PanelObject> ().SetAsImage1x2 ();
			} else {
				int r = Random.Range (0, 3);
				if (r == 1) {
					panel.GetComponent<PanelObject> ().SetAs3dViz ();
				} else if (r == 2) {
					//panel.GetComponent<PanelObject> ().SetAsVideo ();
					panel.GetComponent<PanelObject> ().SetAsImage ();
				} else {
					panel.GetComponent<PanelObject> ().SetAsImage ();
				}
			}
//			panel.transform.position = idleSequence [i].toPos;
//			yield return null;
			//panel.SetActive (true);


			float speed = group * 0.1f;
			float wait = group * 0.1f;
			float speedMult = 1;
			if (idleSequence [i].panelType == new Vector2 (2, 2))
				speedMult = 2;
			EaseCurve.Instance.Vec3 (panel.transform, idleSequence[i].fromPos, idleSequence[i].toPos, speed * speedMult, 0f); //, EaseCurve.Instance.custom
			if (i < idleSequence.Count-1) {
				if (idleSequence [i + 1].distanceToOrigin == idleSequence [i].distanceToOrigin) {
					yield return null;
				} else {
					group++;
					yield return new WaitForSeconds (wait * speedMult);
				}
			} else {
				yield return null;
			}
		}
	}

	void PlaceCameras(){
		idleSequence.Sort(SortByDistanceFromOrigin);
		float zDepth = 50;
		float layerDepth = 0;
		float group = 2;
		for (int i = 0; i < idleSequence.Count; i++) {
			GameObject ccGo = Instantiate (cellCamera);
			ccGo.SetActive (true);
			Camera ccCam = ccGo.GetComponentInChildren<Camera> ();
			Rect rec = CreateCameraCell (idleSequence[i].row, idleSequence[i].col, idleSequence[i].panelType, idleSequence[i].direction);
			ccCam.rect = rec;
			ccGo.transform.parent = cams;
			ccGo.transform.localPosition = new Vector3 (ccGo.transform.position.x + (idleSequence [i].col * 40f), ccGo.transform.position.y - (idleSequence [i].row * 20), 0);
			ccGo.name = idleSequence[i].row + ", " + idleSequence[i].col;
			GameObject panel = Instantiate (panelPrefab);
			panel.transform.parent = ccGo.transform.Find ("Container");
			panel.transform.localPosition = Vector3.zero;
			panel.transform.localScale = Vector3.one;
			panel.transform.localPosition = Vector3.Scale( -idleSequence [i].direction, new Vector3(6f, 3.5f, 0f));
			Vector3 toPos = Vector3.zero;
			if (idleSequence[i].panelType == new Vector2(1,2)) {
				panel.GetComponent<PanelObject> ().SetAsImage1x2 ();
				toPos.x += (5.33333f / 4f);
			} else {
				int r = Random.Range (0, 3);
				if (r == 1) {
					panel.GetComponent<PanelObject> ().SetAs3dViz ();
				} else if (r == 2) {
					panel.GetComponent<PanelObject> ().SetAsVideo ();
				} else {
					panel.GetComponent<PanelObject> ().SetAsImage ();
				}
			}
			idleSequence [i].fromPos = panel.transform.localPosition;
			idleSequence [i].toPos = toPos;
			idleSequence [i].panel = panel;
			idleSequence [i].cellCam = ccGo;
			float speed = group * 0.5f;
			float wait = group * 0.1f;
			if (i < idleSequence.Count-1)
				if (idleSequence [i + 1].distanceToOrigin != idleSequence [i].distanceToOrigin)
					group++;
			EaseCurve.Instance.Vec3 (panel.transform, panel.transform.localPosition, toPos, speed, wait, EaseCurve.Instance.custom, null, "local");
			Material mat = ccGo.transform.Find ("Container").Find ("Quad").GetComponent<Renderer> ().material; 
			Color32 toCol = mat.color;
			toCol.a = 175;
			EaseCurve.Instance.MatColor (mat, mat.color, toCol, speed, wait, EaseCurve.Instance.custom);
		}
	}

	void UnPlaceCameras(){
		float zDepth = 50;
		float layerDepth = 0;
		float group = 2;
		for (int i = idleSequence.Count - 1; i >= 0; i--) {
			float speed = group * 0.5f;
			float wait = group * 0.1f;
			if (i > 1)
			if (idleSequence [i - 1].distanceToOrigin != idleSequence [i].distanceToOrigin)
				group++;
			idleSequence [i].cellCam.transform.Find ("Container").Find ("Quad").gameObject.SetActive (false);
			if (i > 0) {
				EaseCurve.Instance.Vec3 (idleSequence [i].panel.transform, idleSequence [i].toPos, idleSequence [i].fromPos, speed, wait, EaseCurve.Instance.custom, null, "local");
			} else {
				EaseCurve.Instance.Vec3 (idleSequence [i].panel.transform, idleSequence [i].toPos, idleSequence [i].fromPos, speed, wait, EaseCurve.Instance.custom, nextSequence, "local");
			}
		}
	}

	public void nextSequence(){
		ClearPanels ();
		PlanLayout ();
	}
	#endregion











	int[][][] layouts3x3 = new int[][][]
	{ 
		new int[][]{	
			new int[]{0, 0, 1},
			new int[]{1, 1, 1},
			new int[]{0, 0, 1}
		},
		new int[][]{	
			new int[]{1, 0, 0},
			new int[]{1, 1, 1},
			new int[]{0, 0, 1}
		},
		new int[][]{	
			new int[]{0, 1, 0},
			new int[]{1, 1, 1},
			new int[]{0, 1, 0}
		}
	};
//	int[][][] layouts6x3 = new int[][][]
//	{ 
//		new int[][]{	
//			new int[]{0, 0, 1, 0, 0, 0},
//			new int[]{1, 1, 1, 4, 0, 1},
//			new int[]{0, 1, 1, 0, 0, 0}
//		}
//	};
	void MakeGrid (Vector2 _desiredGrid){
		float totalPanels = _desiredGrid.x * _desiredGrid.y;
		float xx = 0;
		float yy = 0;
		Vector3 startPos = new Vector3 (desiredFullScreenAspect.x*-0.5f, desiredFullScreenAspect.y*0.5f, panelDepth);
		Vector3 startSize = new Vector3 (calculatedGridElementSize.x, calculatedGridElementSize.y, 1);
		gridPanels.Clear ();
		int l = Random.Range(0, layouts3x3.Length);
		int i=0;
		for (int y = 0; y < layouts3x3[l].Length; y++) {
			for (int x = 0; x < layouts3x3 [l] [y].Length; x++) {
				if (layouts3x3 [l] [y] [x] == 1) {
					GameObject panel = Instantiate (panelPrefab);
					Vector3 step = new Vector3 (startSize.x * x, startSize.y * -y, 0);
					Vector3 centerOffset = new Vector3 (startSize.x * 0.5f, startSize.y * -0.5f, 0);
					Vector3 panelPos = startPos + step + centerOffset;
					Vector3 panelSize = startSize;
					panelSize.z = 0.05f;
					panel.transform.position = panelPos;
					//panel.transform.rotation = Quaternion.Euler (new Vector3 (0, 180, 0));
					panel.GetComponent<PanelObject> ().panelID = i;
					gridPanels.Add (panel.GetComponent<PanelObject> ());
					panel.transform.parent = panels;
					int r = Random.Range (0, 3);
					if (r == 1) {
						panel.GetComponent<PanelObject> ().SetAs3dViz ();
					} else if (r == 2) {
						panel.GetComponent<PanelObject> ().SetAsVideo ();
					} else {
						panel.GetComponent<PanelObject> ().SetAsImage ();
					}
				} else {
					gridPanels.Add (null);
				}
				i++;
			}
		}
		panelDepth--;
	}

	void MakeGridOld (Vector2 _desiredGrid){
		float totalPanels = _desiredGrid.x * _desiredGrid.y;
		float xx = 0;
		float yy = 0;
		Vector3 startPos = new Vector3 (desiredFullScreenAspect.x*-0.5f, desiredFullScreenAspect.y*0.5f, panelDepth);
		Vector3 startSize = new Vector3 (calculatedGridElementSize.x, calculatedGridElementSize.y, 1);
		gridPanels.Clear ();


		for (int i = 0; i < totalPanels; i++) {
			int rng = Random.Range (0, 2);
			if (rng > 0) {
				GameObject panel = Instantiate (panelPrefab);
				Vector3 step = new Vector3 (startSize.x * xx, startSize.y * yy, 0);
				Vector3 centerOffset = new Vector3 (startSize.x * 0.5f, startSize.y * -0.5f, 0);
				Vector3 panelPos = startPos + step + centerOffset;
				Vector3 panelSize = startSize;
				panelSize.z = 0.05f;
				panel.transform.position = panelPos;
				//panel.transform.rotation = Quaternion.Euler (new Vector3 (0, 180, 0));
				panel.GetComponent<PanelObject> ().panelID = i;
				gridPanels.Add (panel.GetComponent<PanelObject> ());
				panel.transform.parent = panels;
				int r = Random.Range (0, 3);
				if (r == 1) {
					panel.GetComponent<PanelObject> ().SetAs3dViz ();
				} else if (r == 2) {
					panel.GetComponent<PanelObject> ().SetAsVideo ();
				} else {
					panel.GetComponent<PanelObject> ().SetAsImage ();
				}
			} else {
				gridPanels.Add (null);
			}
			xx++;
			if (xx == desiredGrid.x) {
				xx = 0;
				yy--;
			}
		}
		panelDepth--;
	}




	//NOTES
	//steps:
	//1. create panel
	//	panel type
	//	panel assets
	//	panel layer (z-pos)
	//2. transtion in
	//	start settings from:(grid position, size, etc)
	//	end settings to:(grid position, size, transition type, etc)
	//3. transition out (optional)
	//	could simply get obscured by another panel
	//		option: have each panel perform a raycast toward camera to check if they're being obscured and selfdestruct if so
	//	end settings to:(transition type, etc)
	PanelObject lastZoom3;
	PanelObject lastZoom2;
	PanelObject lastZoom;
	void MakePanel (int _gridPosition){
		GameObject panel = Instantiate (panelPrefab);
		Vector3 panelPos = gridPositions [_gridPosition].center;
		panelPos.z = panelDepth;
		panelDepth --;
		panel.transform.position = panelPos;
		panel.transform.localScale = Vector3.one * 0.1f;
		panel.GetComponent<PanelObject>().SetAsImage();
		panel.transform.parent = panels;
		panel.GetComponent<PanelObject> ().ZoomToCell ();
		if (lastZoom != null) {
			lastZoom.ZoomToFull ();
			if (lastZoom2 != null) {
				lastZoom2.ZoomToOver ();
				if (lastZoom3 != null) {
					lastZoom3.ZoomToOver2 ();
				}
				lastZoom3 = lastZoom2;
			}
			lastZoom2 = lastZoom;
		}
		lastZoom = panel.GetComponent<PanelObject> ();
	}

	public void ZoomOne(){
		MakePanel(8);
	}




	//animate panel being pushed
	int bobPanel;
	void DoBob(){
		bobPanel = Random.Range(0, gridPanels.Count);
		gridPanels [bobPanel].BobDown ();
	}
	void DoUnbob(){
		gridPanels [bobPanel].BobUp ();
	}




	//load panel behind another panel then spin around to show new panel
	void MakeReversePanel (int _gridPosition){
		bool panelExists = (gridPanels [_gridPosition] != null) ? true : false;
		Vector3 gridPos;
		if(panelExists){
			gridPos = gridPanels [_gridPosition].transform.position;

			int fall = Random.Range (0, 2);
			if (fall > 0) {
				FallAway (gridPanels [_gridPosition].gameObject);
				gridPanels [_gridPosition] = null;
				return;
			}

		}else{
			gridPos = gridPositions [_gridPosition].center;
			gridPos.z = panelDepth;
		}

		GameObject spinner = new GameObject ();
		spinner.transform.position = gridPos;
		spinner.transform.parent = panels;

		GameObject panel = Instantiate (panelPrefab);
		Vector3 panelPos = gridPos;
		panelPos.z = gridPos.z + 0.1f;
		panel.transform.position = panelPos;
		panel.transform.rotation = Quaternion.Euler (0, 180, 0);
		int r = Random.Range (0, 3);
		if (r == 1) {
			panel.GetComponent<PanelObject> ().SetAs3dViz ();
		} else if (r == 2) {
			panel.GetComponent<PanelObject> ().SetAsVideo ();
		} else {
			panel.GetComponent<PanelObject> ().SetAsImage ();
		}
		if(panelExists)
			gridPanels [_gridPosition].transform.parent = spinner.transform;
		panel.transform.parent = spinner.transform;


		Hashtable paramsHash = new Hashtable();
		paramsHash.Add("_tmpParent", spinner);
		paramsHash.Add("_keep", panel);
		//paramsHash.Add("_remove", gridPanels [_gridPosition].gameObject);
		paramsHash.Add("_gridPosition", _gridPosition);
		iTween.RotateAdd (spinner, iTween.Hash("y", 180, "easetype", "easeInOutBack", "time", 1f, "oncompletetarget", this.gameObject, "oncomplete", "CleanUpReversePanel", "oncompleteparams", paramsHash));
	}
	//void CleanUpReversePanel(GameObject _tmpParent, GameObject _keep, GameObject _remove, int _gridPosition){
	void CleanUpReversePanel(object hashObj){
		Hashtable hash = (Hashtable)hashObj;
		GameObject _keep = (GameObject)hash ["_keep"];
		gridPanels [(int)hash["_gridPosition"]] = _keep.GetComponent<PanelObject> ();
		//Destroy ((GameObject)hash["_remove"]);
		_keep.transform.localPosition = Vector3.zero;
		_keep.transform.parent = panels;
		Destroy ((GameObject)hash["_tmpParent"]);
	}
	[ContextMenu("RevealNewPanel")]
	void RevealNewPanel(){
		MakeReversePanel (Random.Range (0, gridPanels.Count));
	}
	void FallAway(GameObject _go){
		Destroy (_go);
	}




	//premade animation of a grid reveal
	[ContextMenu("GridReveal01")]
	public void GridReveal01(){
		PlanLayout ();
		//MakeGrid (desiredGrid);
		//StartCoroutine (GridReveal01Timed());
	}

	IEnumerator GridReveal01Timed(){
		for (int i = 0; i < gridPanels.Count; i++) {
			if(gridPanels[i]!=null)
				gridPanels [i].Spin (2f, 180f);
			yield return new WaitForSecondsRealtime (0.1f);
		}
		yield return null;
	}



	//clear all panels and reset depth
	void ClearPanels(){
		Debug.Log ("clearing " + panels.childCount + " panels");
		foreach (Transform child in panels) {
			GameObject.Destroy(child.gameObject);
		}
		foreach (Transform child in masks) {
			GameObject.Destroy(child.gameObject);
		}
		foreach (Transform child in cams) {
			GameObject.Destroy(child.gameObject);
		}
		panelDepth = 50f;
	}
		


	RaycastHit hit;
	void Update(){
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			ZoomOne ();
		}
		if (Input.GetKeyDown (KeyCode.Alpha0)) {
			GridReveal01 ();
		}
		if (Input.GetKeyDown (KeyCode.Alpha9)) {
			UnPlaceCameras ();
		}
		if (Input.GetKeyDown (KeyCode.Backspace)) {
			ClearPanels ();
		}
		if (Input.GetKeyDown (KeyCode.DownArrow)) {
			DoBob ();
		}
		if (Input.GetKeyUp (KeyCode.DownArrow)) {
			DoUnbob ();
		}

		if (Input.GetKeyUp (KeyCode.P)) {
			//StartCoroutine(SwapRandom ());
			InvokeRepeating ("RevealNewPanel", 0, 1.5f);
		}
			
		if (Input.GetMouseButtonDown (0)) {
			if (Physics.Raycast (mainCamera.ScreenPointToRay (Input.mousePosition), out hit)) {
				//check if we're interacting with a panel
				if (hit.transform.name == "Front") {
					//check if that panel should be interacted with
					if (hit.transform.GetComponentInParent<PanelObject> ().canInteract) {
						//has this user been activated yet?
						if (!openUser) {
							//this user area has not been activated yet...
							PanelObject panel = hit.transform.GetComponentInParent<PanelObject> ();
							//move it slightly closer to camera so there are no hittest conflicts
//							panel.transform.localPosition += Vector3.back;
							//separate this panel by moving it to a new layer from the "idle" group so it can be interacted with
							ScreenManager.Instance.MoveToLayer (panel.transform, LayerMask.NameToLayer ("UserInit"));
							iTween.ScaleTo (panel.gameObject, panel.transform.localScale * 0.95f, 1f);
							//activate user area
							OpenUser (panel.panelID);
						} else {
							//user area has been activated...
							//check if the panel we're interacting with now is the current users active panel
							if (hit.transform.GetComponentInParent<PanelObject> ().transform == GameObject.Find ("UserManager").GetComponent<UserManager> ().activePanel.transform) {
								//if yes, let us drag it around the user area
//								UserManager um = GameObject.Find ("UserManager").GetComponent<UserManager> ();
								//make the nav follow it
//								um.setPanelFollow (Input.mousePosition);
							}
						}
					}
				}
			}
		}
//		if (openUser) {
//			gotoRect = iTween.RectUpdate (userPanelCamera.rect, new Rect (.1666666f, 0, .1666666f, 1), 5f);
//			userPanelCamera.rect = gotoRect;
//		}
	}
		

	bool openUser = false;
	Rect gotoRect;
	void OpenUser(int _id){
		if (!openUser) {
			//userPanelCamera.gameObject.SetActive (true);
			//openUser = true;
			//SceneManager.LoadScene("user_space", LoadSceneMode.Additive);
			GameObject user = GameObject.Find ("UserManager");
			UserManager um = user.GetComponent<UserManager> ();
			um.activePanel = gridPanels [_id];
			um.SetCam (desiredGrid.x, (float)gridPositions [_id].col - 1);
			um.Open ();
			openUser = true;
		}
	}
	*/
}
