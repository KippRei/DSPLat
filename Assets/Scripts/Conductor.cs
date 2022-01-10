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
    public float inputLag = .15f;
    public int subdivisions = 4;
    private int newSubdivs = 4;
    private float timePerMeasure;
    private double startOfMeasure;
    private double currentTime;
    private double lastTime;
    private double fourthBeat;
    private double startTime; // start time for song, allows buffering to ensure proper sync with video
    private int tally = 0;
    private float cushionForFirstBeat = .15f;

    private List<string> noteInputs = new List<string>(); // arrows on screen for measure
    private bool inputsOk = true; // checks player inputs vs displayed arrows and sets flag
    private bool missedMeasure = false; // if wrong arrow is hit, the measure will be missed
    private int chartPosition = 0;

    private string[] chart;

    public GameObject leftArrow;
    public GameObject rightArrow;
    public GameObject upArrow;
    public GameObject downArrow;


    // Start is called before the first frame update
    void Start()
    {
        timePerMeasure = 60 / bpm * 4;
        audioSource = GetComponent<AudioSource>();
        lastTime = AudioSettings.dspTime + offset;
        startOfMeasure = lastTime - cushionForFirstBeat;
        fourthBeat = startOfMeasure + ((timePerMeasure / subdivisions) * 3) + cushionForFirstBeat + .05;
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
               
        TimingUpdate();
    }

    void TimingUpdate()
    {
        currentTime = AudioSettings.dspTime;
        if (currentTime - lastTime >= timePerMeasure / subdivisions)
        {
            /*fourthBeat = startOfMeasure + ((timePerMeasure / subdivisions) * 3) + cushionForFirstBeat + .05;*/
            if (count < 4)
            {
                count++;
                if (count == 4)
                {
                    StartCoroutine(Pulse("circle"));
                }
                else
                {
                    StartCoroutine(Pulse("block"));
                }
            }
            else
            {
                StartCoroutine(Pulse("block"));
                /*startOfMeasure += (timePerMeasure / subdivisions) * 4;*/
                count = 1;
                inputsOk = false;
            }
            if (count == 1)
            {
                fourthBeat = lastTime + ((timePerMeasure / newSubdivs) * 4);
            }
            lastTime += timePerMeasure / subdivisions;
            subdivisions = newSubdivs;
            UIUpdate();
        }
    }

    void UIUpdate()
    {
        counterDisplay.text = count.ToString();
        tallyDisplay.text = tally.ToString();
    }

    IEnumerator Pulse(string type)
    {
        if (type == "block")
        {
            audioSource.PlayOneShot(tickSFX, .7f);
            Vector3 tempScale = block.transform.localScale + new Vector3( .1f, .1f, .1f);
            block.transform.localScale = tempScale;
            yield return new WaitForSeconds(.1f);
            block.transform.localScale = blockScale;
        }
        else if (type == "circle")
        {
            audioSource.PlayOneShot(tick1SFX, .3f);
            Vector3 tempScale = circle.transform.localScale + new Vector3(.1f, .1f, .1f);
            circle.transform.localScale = tempScale;
            yield return new WaitForSeconds(.001f);
            circle.transform.localScale = circleScale;
            chartPosition++;
            LoadNoteChart(chartPosition);
        }
    }

    void CheckInput(string buttonPressed)
    {
        float timingWindow = .04f;
        if (buttonPressed == "X" || buttonPressed == "O")
        {
            double timePressed = AudioSettings.dspTime - inputLag;
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
    }

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
}