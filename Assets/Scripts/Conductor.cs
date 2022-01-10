using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine;

public class Conductor : MonoBehaviour
{
    public GameObject block;
    public GameObject circle;
    private Vector3 blockScale;
    private Vector3 circleScale;

    public Text counterDisplay;
    public Text accuracyDisplay;
    public Text tallyDisplay;

    public AudioClip tickSFX;
    public AudioClip tick1SFX;
    public AudioClip track;
    AudioSource audioSource;

    private int count = 1; // counts beats per measure
    public float bpm = 120;
    public float offset = 0;
    public float inputLag = 0f;
    public int subdivisions = 4;
    private int newSubdivs = 8;
    private float timePerMeasure;
    private double currentTime;
    private double lastTime;
    private double fourthBeat;
    private double startTime; // start time for song, allows buffering to ensure proper sync with video
    private int tally = 0;

    private List<string> noteInputs = new List<string>(); // arrows on screen for current measure
    private List<string> nextNoteInputs = new List<string>(); // preview/next arrows displayed under current
    private bool inputsOk = false; // checks player inputs vs displayed arrows and sets flag
    private bool missedMeasure = false; // if wrong arrow is hit, the measure will be missed
    private int chartPosition = 0;

    private string[] chart;

    public GameObject leftArrow;
    public GameObject rightArrow;
    public GameObject upArrow;
    public GameObject downArrow;
    public GameObject nextLeftArrow;
    public GameObject nextRightArrow;
    public GameObject nextUpArrow;
    public GameObject nextDownArrow;

    private bool horizontalBtnDown = false;
    private bool verticalBtnDown = false;


    // Start is called before the first frame update
    void Start()
    {
        timePerMeasure = (60 / bpm) * 4;
        audioSource = GetComponent<AudioSource>();
        lastTime = AudioSettings.dspTime + offset;
        blockScale = block.transform.localScale;
        circleScale = circle.transform.localScale;
        chart = System.IO.File.ReadAllLines(@"Assets\Charts\chart.txt");
        LoadNoteChart(chartPosition);

        startTime = AudioSettings.dspTime + 2f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayScheduled(startTime);
        }
        CheckForInput();     
        TimingUpdate();
    }

    string DPadInput()
    {
        if (Input.GetAxisRaw("Horizontal") > 0 && horizontalBtnDown == false)
        {
            horizontalBtnDown = true;
            return "right";
        }
        if (Input.GetAxisRaw("Horizontal") < 0 && horizontalBtnDown == false)
        {
            horizontalBtnDown = true;
            return "left";
        }
        if (Input.GetAxisRaw("Vertical") < 0 && verticalBtnDown == false)
        {
            verticalBtnDown = true;
            return "down";
        }
        if (Input.GetAxisRaw("Vertical") > 0 && verticalBtnDown == false)
        {
            verticalBtnDown = true;
            return "up";
        }
        return "none";
    }

    void CheckForInput()
    {
        if (Input.GetAxisRaw("Horizontal") == 0 && horizontalBtnDown == true)
        {
            horizontalBtnDown = false;
        }
        if (Input.GetAxisRaw("Vertical") == 0 && verticalBtnDown == true)
        {
            verticalBtnDown = false;
        }
        if (Input.GetButtonDown("X"))
        {
            Debug.Log("X");
            CheckInput("X");
        }
        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            CheckInput(DPadInput());
        }
       if (Input.GetAxisRaw("Vertical") != 0)
        {
            CheckInput(DPadInput());
        }
    }

    void TimingUpdate()
    {
        currentTime = AudioSettings.dspTime;
        if (currentTime - lastTime >= timePerMeasure / subdivisions)
        {
            if (count < 4)
            {
                audioSource.PlayOneShot(tickSFX, .2f);
                if (count == 1)
                {

                    Pulse("block");
                }
                lastTime += timePerMeasure / subdivisions;
                if (count == 2)
                {
                    fourthBeat = lastTime + (timePerMeasure / subdivisions) * 2;
                }
                count++;
            }
            else
            {
                audioSource.PlayOneShot(tick1SFX, 1f);
                Pulse("circle");
                lastTime += timePerMeasure / subdivisions;
                subdivisions = newSubdivs;
                count = 1;
            }

            UIUpdate();
        }
    }

    void UIUpdate()
    {
        counterDisplay.text = count.ToString();
        tallyDisplay.text = tally.ToString();
    }

    void Pulse(string type)
    {
        if (type == "block")
        {

            block.transform.localScale = block.transform.localScale + new Vector3(.1f, .1f, .1f);
            circle.transform.localScale = circle.transform.localScale - new Vector3(.1f, .1f, .1f);

        }
        else if (type == "circle")
        {
            block.transform.localScale = block.transform.localScale - new Vector3( .1f, .1f, .1f);
            circle.transform.localScale = circle.transform.localScale + new Vector3(.1f, .1f, .1f); ;

            chartPosition++;
            LoadNoteChart(chartPosition);
        }
    }

    void CheckInput(string buttonPressed)
    {
        float timingWindow = .08f;
        double timePressed = AudioSettings.dspTime - inputLag;

        if (buttonPressed == "X" || buttonPressed == "O")
        {
            double target = fourthBeat;
            Debug.Log("Pressed: " + timePressed);
            Debug.Log("Target:" + target);
            Debug.Log("Subdivs: " + subdivisions);
            Debug.Log("");

            if (timePressed >= target - timingWindow && timePressed <= target + timingWindow && inputsOk && !missedMeasure)
            {
                accuracyDisplay.text = "Good input";
                tally++;
            }
            else if (timePressed >= target - timingWindow && timePressed <= target + timingWindow && !inputsOk)
            {
                accuracyDisplay.text = "Bad input";
                tally = 0;
            }
            else
            {
                accuracyDisplay.text = "Miss";
                missedMeasure = true;
                tally = 0;
            }
        }
        else if (noteInputs.Count > 0 && !missedMeasure)
        {
            switch (buttonPressed)
            {
                case "left":
                    {
                        if (noteInputs[0] == "left")
                        {
                            noteInputs.RemoveAt(0);
                            if (noteInputs.Count == 0)
                            {
                                inputsOk = true;
                            }
                        }
                        else
                        {
                            inputsOk = false;
                            missedMeasure = true;
                        }
                    }
                    break;
                case "right":
                    if (noteInputs[0] == "right")
                        {
                            noteInputs.RemoveAt(0);
                            if (noteInputs.Count == 0)
                            {
                                inputsOk = true;
                            }
                        }
                        else
                        {
                            inputsOk = false;
                            missedMeasure = true;
                        }
                    break;
                case "up":
                    if (noteInputs[0] == "up")
                        {
                            noteInputs.RemoveAt(0);
                            if (noteInputs.Count == 0)
                            {
                                inputsOk = true;
                            }
                        }
                        else
                        {
                            inputsOk = false;
                            missedMeasure = true;
                        }
                    break;
                case "down":
                    if (noteInputs[0] == "down")
                    {
                        noteInputs.RemoveAt(0);
                        if (noteInputs.Count == 0)
                        {
                            inputsOk = true;
                        }
                    }
                    else
                    {
                        inputsOk = false;
                        missedMeasure = true;
                    }
                    break;
            }
        }
    }

    void LoadNoteChart(int chartPos)
    {
        noteInputs.Clear();
        missedMeasure = false;

        switch (chart[chartPos][0])
        {
            case '4':
                newSubdivs = 4;
                break;
            case '8':
                newSubdivs = 8;
                break;
            case '1':
                newSubdivs = 16;
                break;
            case '3':
                newSubdivs = 32;
                break;
        }

        for (int i = 0; i < chart[chartPos].Length; i++)
        {
            if (chart[chartPos][i] == 'l')
            {
                noteInputs.Add("left");
            }
            else if (chart[chartPos][i] == 'r')
            {
                noteInputs.Add("right");
            }
            else if (chart[chartPos][i] == 'u')
            {
                noteInputs.Add("up");
            }
            else if (chart[chartPos][i] == 'd')
            {
                noteInputs.Add("down");
            }
        }

        int nextPos = chartPos + 1;
        for (int i = 0; i < chart[nextPos].Length; i++)
        {
            if (chart[nextPos][i] == 'l')
            {
                nextNoteInputs.Add("left");
            }
            else if (chart[nextPos][i] == 'r')
            {
                nextNoteInputs.Add("right");
            }
            else if (chart[nextPos][i] == 'u')
            {
                nextNoteInputs.Add("up");
            }
            else if (chart[nextPos][i] == 'd')
            {
                nextNoteInputs.Add("down");
            }
        }
        UpdateNoteUI();
    }

    void UpdateNoteUI()
    {
        var prevNotesDisplay = GameObject.FindGameObjectsWithTag("arrows");
        foreach (var prevNote in prevNotesDisplay)
        {
            Destroy(prevNote);
        }
        for (int i = 0; i < noteInputs.Count; i++)
        {
            string note = noteInputs[i];
            float xLoc = -3.35f + (.85f * (float)i);
            float yLoc = -4.86f;
            if (note == "left")
            {
                GameObject left = Instantiate(leftArrow, new Vector3(xLoc, yLoc, -1), Quaternion.identity) as GameObject;
                left.transform.parent = GameObject.Find("Square").transform;
            }
            else if (note == "right")
            {
                GameObject right = Instantiate(rightArrow, new Vector3(xLoc, yLoc, -1), Quaternion.identity) as GameObject;
                right.transform.parent = GameObject.Find("Square").transform;
            }
            else if (note == "up")
            {
                GameObject up = Instantiate(upArrow, new Vector3(xLoc, yLoc, -1), Quaternion.identity) as GameObject;
                up.transform.parent = GameObject.Find("Square").transform;
            }
            else if (note == "down")
            {
                GameObject down = Instantiate(downArrow, new Vector3(xLoc, yLoc, -1), Quaternion.identity) as GameObject;
                down.transform.parent = GameObject.Find("Square").transform;
            }
        }
        UpdateNextNoteUI();
    }

    void UpdateNextNoteUI()
    {
        var prevNotesDisplay = GameObject.FindGameObjectsWithTag("nextArrows");
        foreach (var prevNote in prevNotesDisplay)
        {
            Destroy(prevNote);
        }
        for (int i = 0; i < nextNoteInputs.Count; i++)
        {
            string note = nextNoteInputs[i];
            float xLoc = -3.35f + (.85f * (float)i);
            float yLoc = -6.12f;
            if (note == "left")
            {
                GameObject left = Instantiate(nextLeftArrow, new Vector3(xLoc, yLoc, -1), Quaternion.identity) as GameObject;
                left.transform.parent = GameObject.Find("NextPattern").transform;
            }
            else if (note == "right")
            {
                GameObject right = Instantiate(nextRightArrow, new Vector3(xLoc, yLoc, -1), Quaternion.identity) as GameObject;
                right.transform.parent = GameObject.Find("NextPattern").transform;
            }
            else if (note == "up")
            {
                GameObject up = Instantiate(nextUpArrow, new Vector3(xLoc, yLoc, -1), Quaternion.identity) as GameObject;
                up.transform.parent = GameObject.Find("NextPattern").transform;
            }
            else if (note == "down")
            {
                GameObject down = Instantiate(nextDownArrow, new Vector3(xLoc, yLoc, -1), Quaternion.identity) as GameObject;
                down.transform.parent = GameObject.Find("NextPattern").transform;
            }
        }
    }

    /*    #region input
        void OnLeft()
        {
            CheckInput("left");
        }

        void OnRight()
        {
            CheckInput("right");
        }

        void OnUp()
        {
            CheckInput("up");
        }

        void OnDown()
        {
            CheckInput("down");
        }

        void OnX()
        {
            CheckInput("X");
        }

        void OnO()
        {
            CheckInput("O");
        }
        #endregion*/
}