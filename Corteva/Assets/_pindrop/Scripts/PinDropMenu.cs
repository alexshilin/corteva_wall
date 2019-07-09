using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SimpleJSON;

public class PinDropMenu : MonoBehaviour {

	public PinDrop PD;

    public GameObject Pages;

    [Header("Pages")]
    public GameObject welcome;
    public GameObject welcomeChild;
    public TextMeshPro welcomeTxt;
    public GameObject q1, q2, q3, thankYou;
    public GameObject CurrentPage;
    [Header("Instructions")]
    public GameObject GestureInstructions;
	public TextMeshPro instruct;
    public GameObject icons;
    public GameObject bg;
    [Header("Buttons")]
	public GameObject backBtn;
	public GameObject closeBtn;
    public GameObject infoBtn;
    public GameObject gpsBtn;
	public GameObject btn;

	private List<GameObject> Q1B = new List<GameObject>();
	private List<GameObject> Q2B = new List<GameObject>();

    public Dictionary<string, string> rolesPercent = new Dictionary<string, string>();
    public Dictionary<string, string> challengesPercent = new Dictionary<string, string>();

    public string q1a;
    public string q2a;

    public bool questionCompleted;

    [Header("Pins")]
    public GameObject lastPin, undecidedPin;

    void OnEnable(){

	}

	void Start(){
        ShowPage(welcome.transform);
        backBtn.SetActive(PD.myKiosk != null);
        //Debug.Log("IS KYOSK NULL " + PD.myKiosk != null);
        MakeButtons();
    }

    //------PAGES TOGGLE CONTROL-------

    public void ShowPage(Transform _page){//not for welcome page
        //Debug.Log("Showing page " + _page);
        foreach(Transform kid in Pages.transform){
            if (kid == _page){
                kid.gameObject.SetActive(true);
                CurrentPage = kid.gameObject;
                if(_page == welcome.transform){
                    ShowBackground(true);
                    ToggleWelcome(1, 0);
                    //Debug.Log("Fading in background page " + _page);
                }else if(_page == thankYou.transform){
                    ShowBackground(true, 0f);
                }
                else{
                    ShowBackground(true,0);
                }

                if(kid == q1.transform || kid == q2.transform || kid == thankYou.transform){
                    PD.globe.maxTimeToWait =30;
                }else{
                    PD.globe.maxTimeToWait = 15;
                }

               

            }
            else{
                kid.gameObject.SetActive(false);
            }
        }
        //Debug.Log("Showing page " + CurrentPage);
        if (CurrentPage == welcome)
        {
            PD.globe.idling = true;
            closeBtn.SetActive(false);
        }
        else
        {
            PD.globe.idling = false;
            closeBtn.SetActive(true);
        }

        GestureInstructions.SetActive(false);

        infoBtn.SetActive(false);
        gpsBtn.SetActive(false);
        PD.globe.timeWaited = 0;
    }

    public void HideAllPages(){
        //Debug.Log("hide all pages");

        if(CurrentPage == q1)
        {
            //GA--user closes question one with 'X' button
        }else if(CurrentPage == q2)
        {
            //GA--user closes question one with 'X' button
        }

        foreach (Transform kid in Pages.transform)
        {
            kid.gameObject.SetActive(false);
        }
        ShowBackground(false,0);
        closeBtn.SetActive(false);
        infoBtn.SetActive(true);
        gpsBtn.SetActive(true);
        GestureInstructions.SetActive(true);
        PD.globe.timeWaited = 0;
    }


    public void ContinueExploring(){
        //GA--user taps 'continue exploring' button
        thankYou.GetComponent<PinDropThankScreen>().reset();
        HideAllPages();
    }

    //------TEXT CONTROL-------

    private void easeInText(TextMeshPro _txt, float _delay = 0, bool _easeIn = true){
        if(_easeIn){
            _txt.alpha = 0;
            EaseCurve.Instance.TextAlpha(_txt, _txt.alpha, 1f, 1f, _delay, EaseCurve.Instance.linear, null);
        }else{
            _txt.alpha = 1;
            EaseCurve.Instance.TextAlpha(_txt, _txt.alpha, 0f, 1f, _delay, EaseCurve.Instance.linear, null);
        }
       
    }

    //------WELCOME PAGE CONTROL-------

	public void ToggleWelcome(float _to, float _delay = 0){
        //Debug.Log("tOGGLE WELCOME");
        welcome.SetActive(true);
        if(_to == 0){//hide Welcome
            EaseCurve.Instance.TextAlpha(welcomeTxt, welcomeTxt.alpha, _to, 1f, _delay, EaseCurve.Instance.linear, HideAllPages);
            ShowBackground(false, 0);
        }
        else{//show Welcome
            welcomeChild.gameObject.SetActive(false);
            welcomeTxt.alpha = 0;
            welcomeTxt.gameObject.SetActive(true);
            EaseCurve.Instance.TextAlpha(welcomeTxt, welcomeTxt.alpha, _to, 1f, _delay, EaseCurve.Instance.linear, showWelcomeChild);
            ShowBackground(true);
        }
	}


    void showWelcomeChild()
    {
        welcomeChild.SetActive(true);
    }

    //------BACKGROUND CONTROL-------

    void ShowBackground(bool _show = true, float _fadeT = 0.75f, float _delay = 0)
    {
        //bg.SetActive(true);
        Material m = bg.GetComponent<Renderer>().material;
        if(_fadeT == 0){
            Color c = m.color;
            float bgalpha = _show ? 0.86f : 0f;
            bg.GetComponent<Renderer>().material.color = new Color(c.r, c.g, c.b, bgalpha);
        }else{
            if (_show)
                {
                    bg.GetComponent<Renderer>().material.color = new Color(m.color.r, m.color.g, m.color.b, 0);
                    EaseCurve.Instance.MatColor(bg.GetComponent<Renderer>().material,
                        bg.GetComponent<Renderer>().material.color, 
                        new Color(bg.GetComponent<Renderer>().material.color.r, bg.GetComponent<Renderer>().material.color.g, bg.GetComponent<Renderer>().material.color.b, 0.86f),
                         0.75f, _delay,
                        EaseCurve.Instance.linear);
                }
                else
                {
                    bg.GetComponent<Renderer>().material.color = new Color(m.color.r, m.color.g, m.color.b, 0.86f);
                    EaseCurve.Instance.MatColor(m, m.color, new Color(m.color.r, m.color.g, m.color.b, 0f), 0.75f, _delay, EaseCurve.Instance.linear);
                }
        }

    }

    //------OTHER STUFF CONTROL-------


	public void AnswerQuestionOne(string _answer){
        //GA--user answers question one
        PD.globe.timeWaited = 0;
        Debug.Log ("1: " + _answer);
		q1a = _answer;
		Invoke ("ShowQuestionTwo", 0.2f);
	}
	public void AnswerQuestionTwo(string _answer){
        //GA--user answers question two
        PD.globe.timeWaited = 0;
        Debug.Log ("2: " + _answer);
		q2a = _answer;
		Invoke ("ShowQ3", 0.2f);

        thankYou.GetComponent<PinDropThankScreen>().FetchAnswers();

        questionCompleted = true;
	}

    public void ShowQuestionTwo()
    {
        ShowPage(q2.transform);
    }

    public void ShowQ3()
    {
        showQ3withInfo = false;
        q3.transform.Find("proceed").gameObject.SetActive(true);
        ShowPage(q3.transform);
    }

    bool showQ3withInfo = false;
    public void ShowQ3WithInfoBtn()
    {
        //GA--when user taps info button
        showQ3withInfo = true;
        q3.transform.Find("proceed").gameObject.SetActive(false);
        ShowPage(q3.transform);
    }

    public void FinishQuestions()
    {
        PD.globe.timeWaited = 0;
        HideAllPages();
        instruct.text = showQ3withInfo ? "" : "Tap to drop a pin in your home location";

        //save to json
    }

    public void SetFinalPin(){
        Pin p = PD.globe.newUserPin.GetComponent<Pin>();
        undecidedPin = null;
        lastPin = p.gameObject;
        bool destroyedpin = false;
        foreach (Transform pin in PD.globe.pinContainer)
        {
            if (!destroyedpin)
            {
                if (pin != p.transform && !pin.GetComponent<Pin>().active)
                {
                    Destroy(pin.gameObject);
                    destroyedpin = true;
                }
            }


            if (pin != p.transform)
            {
                pin.GetComponent<Pin>().UnsetConfirm();
                pin.GetComponent<Pin>().SetPinColor(new Color32(0,114,206,255));
                //Debug.Log("Set Pin as inactive");
            }
        }

        p.UnsetConfirm();
        p.SetPinText("<b>" + q2a + "</b><br>" + q1a);
        p.SetPinColor(new Color32(0, 191, 111, 255));//green
        p.baseSize *= 0.5f;
        PinData.Instance.SavePin(p.latLon, q1a, q2a);
        PD.globe.newUserPin = null;
        PD.menu.instruct.text = "";

        //remove on random  pin from list

        questionCompleted = false;
        Invoke("ShowThankyouScreen", 0.8f);

    }

    public void ShowThankyouScreen()
    {
        Debug.Log("showing thank you screen");
        ShowBackground(true, 0.1f);
        Invoke("showthankyouscreen", 0.2f);
        //ShowPage(thankYou.transform);
        gpsBtn.SetActive(false);
        infoBtn.SetActive(false);
        closeBtn.SetActive(true);

    }

    void showthankyouscreen()
    {
        foreach (Transform kid in Pages.transform)
        {
            if (kid == thankYou.transform)
            {
                kid.gameObject.SetActive(true);
                CurrentPage = kid.gameObject;
                PD.globe.maxTimeToWait = 30;
            }
            else
            {
                kid.gameObject.SetActive(false);
            }
        }
        thankYou.GetComponent<PinDropThankScreen>().back.SetActive(PD.myKiosk != null);
        thankYou.GetComponent<PinDropThankScreen>().restart();

    }

    public void QuitPinDropScene(){
        //GA--user taps on 'back to wall' button
        GetComponentInParent<PinDropMenu>().PD.myKiosk.StopPinDrop();
        gameObject.SetActive(false);
    }



    private void MakeButtons()
    {
        JSONNode data = PinData.Instance.pinData;
        float row = 0;

        q1.SetActive(true);
        Transform q1btns = q1.transform.Find("Btns");
        for (int i = 0; i < data["roles"].Count; i++)
        {
            GameObject b = Instantiate(btn, q1btns);
            PinButton pb = b.GetComponent<PinButton>();
            pb.SetBtnText(data["roles"][i]["title"]);
            pb.btnColor = Color.white;
            pb.onInteraction.AddListener(() => AnswerQuestionOne(pb.labelText));
            Vector3 pos = Vector3.zero;
            if (Q1B.Count > 0)
            {
                if (PD.myKiosk != null)
                {
                    pos.x = Q1B[i - 1].transform.localPosition.x + (Q1B[i - 1].GetComponent<PinButton>().bc.bounds.size.x + 0.03f);
                }
                else
                {
                    pos.x = Q1B[i - 1].transform.localPosition.x + (Q1B[i - 1].GetComponent<PinButton>().bc.bounds.extents.x * 0.7f);
                }
                if (pos.x > 2f)
                {
                    pos.x = 0f;
                    row++;
                }
            }
            pos.y = -0.25f * row;
            Debug.Log("Q1 " + i + ": " + row + " (" + Q1B.Count + ")");
            b.transform.localPosition = pos;
            Q1B.Add(b);

            string _s = "%";
            char[] c = _s.ToCharArray();
            string[] _rolesp = data["roles"][i]["funfact"]["body"].ToString().Split(c[0]);
            _rolesp[0] = _rolesp[0].Substring(1, _rolesp[0].Length-1);
            rolesPercent.Add(data["roles"][i]["title"], _rolesp[0]);
        }

        //foreach (string key in rolesPercent.Keys){
        //    Debug.Log("roles key " + key);
        //}
        //foreach (string val in rolesPercent.Values)
        //{
        //    Debug.Log("rolse val " + val);
        //}
           

        q1.SetActive(false);
        q2.SetActive(true);
        Transform q2btns = q2.transform.Find("Btns");
        row = 0;
        for (int i = 0; i < data["challenges"].Count; i++)
        {
            GameObject b = Instantiate(btn, q2btns);
            PinButton pb = b.GetComponent<PinButton>();
            pb.SetBtnText(data["challenges"][i]["title"]);
            pb.btnColor = Color.white;
            pb.onInteraction.AddListener(() => AnswerQuestionTwo(pb.labelText));
            Vector3 pos = Vector3.zero;
            if (Q2B.Count > 0)
            {
                if (PD.myKiosk != null)
                {
                    pos.x = Q2B[i - 1].transform.localPosition.x + (Q2B[i - 1].GetComponent<PinButton>().bc.bounds.size.x + 0.03f);
                }
                else
                {
                    pos.x = Q2B[i - 1].transform.localPosition.x + (Q2B[i - 1].GetComponent<PinButton>().bc.bounds.extents.x * 0.7f);
                }
                if (pos.x > 2f)
                {
                    pos.x = 0f;
                    row++;
                }
            }
            pos.y = -0.25f * row;
            b.transform.localPosition = pos;
            Q2B.Add(b);

            string _s = "%";
            char[] c = _s.ToCharArray();
            string[] _challengep = data["challenges"][i]["funfact"]["body"].ToString().Split(c[0]);
            _challengep[0] = _challengep[0].Substring(1, _challengep[0].Length - 1);
            challengesPercent.Add(data["challenges"][i]["title"],_challengep[0]);

        }
        //foreach (string key in challengesPercent.Keys){
        //    Debug.Log("ch " + key);
        //}
        //foreach (string val in challengesPercent.Values)
        //{
        //    Debug.Log("ch val " + val);
        //}
        q2.SetActive(false);
    }


}
