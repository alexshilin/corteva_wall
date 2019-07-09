using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SimpleJSON;
using UnityEngine.UI;

public class PinDropThankScreen : MonoBehaviour {

    public PinDropMenu menu;

    public Image ring1, ring2;
    public TextMeshPro percent1, percent2;
    public float perc1, perc2;
    public TextMeshPro txt1, txt2;//roles & challenges
    float t;
    public bool startPlaying;

    public GameObject back;
	// Use this for initialization
	void Start () {
        FetchAnswers();
	}
	
	// Update is called once per frame
	void Update () {
        if (startPlaying)
        {
            if (t < 1)
            {
                t += Time.deltaTime;
            }
            else if (t >= 1)
                t = 1;


            ring1.fillAmount = Mathf.Lerp(0, perc1, t);
            ring2.fillAmount = Mathf.Lerp(0, perc2, t);
        }

	}

    public void FetchAnswers()
    {
        txt1.text = menu.q1a + "s";
        txt2.text = menu.q2a;
        ring1.fillAmount = 0;
        ring2.fillAmount = 0;
        foreach (string key in menu.rolesPercent.Keys)
        {
            if (key == menu.q1a)
            {
                int p1 = int.Parse(menu.rolesPercent[key]);
                perc1 = (float)p1 / 100;
                percent1.text = menu.rolesPercent[key] + "%";

            }
        }

        foreach (string key in menu.challengesPercent.Keys)
        {
            if (key == menu.q2a)
            {
                int p2 = int.Parse(menu.challengesPercent[key]);
                perc2 = (float)p2 / 100;
                percent2.text = menu.challengesPercent[key] + "%";
            }
        }

        //reset();
    }

    public void reset()
    {
        t = 0;
        startPlaying = false;
        //ring1.fillAmount = 0;
        //ring2.fillAmount = 0;
    }

    public void restart()
    {
        t = 0;
        startPlaying = true;
        //ring1.fillAmount = 0;
        //ring2.fillAmount = 0;
    }
}
