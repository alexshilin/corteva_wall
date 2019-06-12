using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatsList
{
	public List<FoodConsumptionStat> stat = new List<FoodConsumptionStat> ();
}
public class FoodConsumption : MonoBehaviour {

	public List<FoodConsumptionBtn> btns = new List<FoodConsumptionBtn>();
	public List<Renderer> maps = new List<Renderer> ();
	public List<StatsList> stats = new List<StatsList> ();

	private Color hidden = new Color32 (1, 1, 1, 0);
	private int currId = -1;

	public void Toggle(int _id){
		if (_id != currId) {
			for (int i = 0; i < btns.Count; i++) {
				if (i == _id) {
					currId = _id;
					btns [i].ToggleBtn (true);
					EaseCurve.Instance.MatColor (maps [i].material, maps [i].material.color, Color.white, 0.3f, 0.15f, EaseCurve.Instance.linear);
					foreach (FoodConsumptionStat stat in stats[i].stat) {
						stat.gameObject.SetActive (true);
						stat.Show ();
					}
				
				} else {
					btns [i].ToggleBtn (false);
					EaseCurve.Instance.MatColor (maps [i].material, maps [i].material.color, hidden, 0.15f, 0.0f, EaseCurve.Instance.linear);
					foreach (FoodConsumptionStat stat in stats[i].stat) {
						stat.gameObject.SetActive (false);
					}
				}
			}
		}
	}


}
