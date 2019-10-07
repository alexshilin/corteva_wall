using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using TMPro;

public class PinDropResults : MonoBehaviour {

	public PinDropMenu menu;
	public GameObject back;
	[Header("Left")]
	public Transform barHolder;
	public GameObject barPrefab;
	[Header("Right")]
	public Transform ringHolder;
	public GameObject ringPrefab;
	public TextMeshPro pctTxt;
	public Transform keysHolder;
	public GameObject keyPrefab;

	private JSONNode pins;
	private Color barHighlight = new Color32 (255, 193, 0, 255);
	private Vector3 keysHolderStartPos;

	void Awake(){
		keysHolderStartPos = keysHolder.localPosition;
	}

	public void FetchAnswers()
	{
		reset ();

		//grab pin json data
		pins = PinData.Instance.pinData;

		//sort roles alphabetically
		SortedDictionary<string, int> roles = new SortedDictionary<string, int> ();
		for (int i = 0; i < pins ["roles"].Count; i++) {
			roles.Add (pins ["roles"] [i] ["title"], pins ["roles"] [i] ["funfact"] ["percentage_number"].AsInt);
		}
		//make bar graphs
		int a = 0;
		foreach(KeyValuePair<string, int> role in roles){
			GameObject go = Instantiate (barPrefab, barHolder);
			go.transform.localPosition = new Vector3 (0, a * -0.29f, 0);
			a++;
			bool h = (role.Key == menu.q1a) ? true : false;
			go.GetComponent<PinDropResultsBar> ().SetBar (h, role.Key, role.Value, h ? barHighlight : Color.white);
		}

		//make ring graphs
		float ringStartOffset = 0;
		float ringPadding = 2f;
		List<PinDropResultsKey> keys = new List<PinDropResultsKey> ();
		for (int i = 0; i < pins ["challenges"].Count; i++) {
			//build rings
			GameObject go = Instantiate (ringPrefab, ringHolder);
			ringStartOffset += ringPadding/2f;
			float ringFillAmt = pins ["challenges"] [i] ["funfact"] ["percentage_number"].AsFloat - ringPadding;
			float pctRotateAmt = ringStartOffset + (pins ["challenges"] [i] ["funfact"] ["percentage_number"].AsFloat * 0.5f);
			bool h = (pins ["challenges"][i] ["title"] == menu.q2a) ? true : false;
			Color challenegeColor = new Color32 ((byte)pins ["challenges"] [i] ["funfact"] ["color"] [0], (byte)pins ["challenges"] [i] ["funfact"] ["color"] [1], (byte)pins ["challenges"] [i] ["funfact"] ["color"] [2], 255);
			go.GetComponent<PinDropResultsRing> ().SetRing (h, pins ["challenges"] [i] ["funfact"] ["percentage_number"].AsInt, ringFillAmt, ringStartOffset, pctRotateAmt, challenegeColor);
			if (h) {
				pctTxt.text = pins ["challenges"] [i] ["funfact"] ["percentage_number"] + "%";
			}
			ringStartOffset += ringFillAmt + (ringPadding/2f);

			//build keys
			GameObject gok = Instantiate (keyPrefab, keysHolder);
			//sets the label and save for layout
			keys.Add(gok.GetComponent<PinDropResultsKey>());

			keys[i].SetLabel (pins ["challenges"] [i] ["title"], challenegeColor );
		}



		//layout labels
		//we're going to arrange them in pairs, from the outside in
		//e.g:
		//longest + shortest
		//2nd longest + 2nd shortest
		//etc.

		//first sort them from longest to shortest
		keys.Sort ((k1, k2) => k1.labelWidth.CompareTo (k2.labelWidth));
		keys.Reverse ();

		float longestPairWidth = 0;
		float horizontalPad = 0.2f;
		float verticalPad = 0.15f;
		float longestLabel = keys [0].labelWidth;

		//since we're consuming two items per row, we only need to iterate through half the list
		//but in case the list is an odd number, we want to account for that
		float keyCount = (keys.Count%2==0) ? keys.Count / 2 : Mathf.Ceil(keys.Count / 2);

		for (int i = 0; i < keys.Count; i++) {
			//position left key
			keys [i].transform.localPosition = new Vector3 (0, i * -0.15f, 0);

			//pairs are odd, and this is the last key
			if (keys.Count % 2 != 0 && i == Mathf.Ceil (keys.Count / 2))
				break;

			//position right key
			keys [keys.Count - (1 + i)].transform.localPosition = new Vector3 (horizontalPad + longestLabel, i * -0.15f, 0);

			//calculate pair width
			float currPairWidth = keys [i].labelWidth + keys [keys.Count - (1 + i)].labelWidth;
			if (currPairWidth > longestPairWidth) {
				longestPairWidth = currPairWidth;
			}

			//pairs are even and were halfway through
			if (keys.Count % 2 == 0 && i == (keys.Count / 2) - 1)
				break;
		}

		//reposition the keys container so its centered
		keysHolder.localPosition += Vector3.left * ((longestPairWidth + horizontalPad) / 2f);
	}

	public void reset(){
		foreach(Transform child in barHolder.transform){
			Destroy (child.gameObject);
		}
		foreach(Transform child in ringHolder.transform){
			Destroy (child.gameObject);
		}
		foreach(Transform child in keysHolder.transform){
			Destroy (child.gameObject);
		}
		keysHolder.localPosition = keysHolderStartPos;
	}
}
