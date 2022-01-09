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
    public int subdivisions = 4;
    private float timePerMeasure;
    private double startOfMeasure;
    private double currentTime;
    private double lastTime;
    private double fourthBeat;
    private double startTime; // start time for song, allows buffering to ensure proper sync with video
    private int tally = 0;

    private List<string> noteInputs = new List<string>(); // arrows on screen for measure
    private List<string> playerNoteInputs = new List<string>(); // players arrow input for measure
    private bool inputsOk = true; // checks player inputs vs displayed arrows and sets flag
    private bool audioStarted = false; // check PCM sample of audio track to ensure playing then set flag
    private bool missedMeasure = false; // if wrong arrow is hit, the measure will be missed
    private int difficulty = 1;

    public GameObject leftArrow;
    public GameObject rightArrow;
    public GameObject upArrow;
    public GameObject downArrow;


    // Start is called before the first frame update
    void Start()
    {
        LoadNoteChart(difficulty);
        timePerMeasure = 60 / bpm * 4;
        audioSource = GetComponent<AudioSource>();
        lastTime = AudioSettings.dspTime + offset;
        // Subtracted .15 to allow for slightly early input at beginning of measure
        startOfMeasure = lastTime - .15;
        fourthBeat = startOfMeasure + (timePerMeasure / subdivisions * 3) + .2;
        blockScale = block.transform.localScale;
        circleScale = circle.transform.localScale;

        startTime = AudioSettings.dspTime + 2f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayScheduled(startTime);
        }

        tallyDisplay.text = tally.ToString();
        TimingUpdate(subdivisions);
    }

    void TimingUpdate(int subdivs)
    {
        currentTime = AudioSettings.dspTime;
        if (currentTime - lastTime >= timePerMeasure / subdivs)
        {
            fourthBeat = startOfMeasure + (timePerMeasure / subdivs * 3) + .2;
            if (count < 4)
            {
                count++;
                if (count == 4)
                {
                    startOfMeasure += timePerMeasure / subdivisions * 4;
                    StartCoroutine(Pulse("circle"));
                }
                else
                {
                    StartCoroutine(Pulse("block"));
                }
                counterDisplay.text = count.ToString();
            }
            else
            {
                StartCoroutine(Pulse("block"));
                count = 1;
                inputsOk = false;
                counterDisplay.text = count.ToString();
            }
            lastTime += timePerMeasure / subdivs;
        }
    }

    void UIUpdate()
    {

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
            yield return new WaitForSeconds(timePerMeasure / (subdivisions / 2));
            circle.transform.localScale = circleScale;
            yield return new WaitForSeconds(timePerMeasure / (subdivisions / 2));
            LoadNoteChart(difficulty);
        }
    }

    void CheckInput(string buttonPressed)
    {
        if (buttonPressed == "X" || buttonPressed == "O")
        {
            double timePressed = AudioSettings.dspTime;
            double target = fourthBeat;
            Debug.Log(timePressed);
            Debug.Log(target);

            if (timePressed >= target - .2 && timePressed <= target + .2 && inputsOk && !missedMeasure)
            {
                accuracyDisplay.text = "Good input";
                tally++;
                if (difficulty < 7)
                {
                    difficulty++;
                }
            }
            else if (timePressed >= target - .2 && timePressed <= target + .2 && !inputsOk)
            {
                accuracyDisplay.text = "Bad input";
                tally = 0;
                difficulty = 1;
            }
            else
            {
                accuracyDisplay.text = "Miss";
                missedMeasure = true;
                tally = 0;
                difficulty = 1;
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

    void LoadNoteChart(int diff)
    {
        noteInputs.Clear();
        missedMeasure = false;
        for (int i = 0; i < diff; i++)
        {
            int randNote = Random.Range(0, 4);
            if (randNote == 0)
            {
                noteInputs.Add("left");
            }
            else if (randNote == 1)
            {
                noteInputs.Add("right");
            }
            else if (randNote == 2)
            {
                noteInputs.Add("up");
            }
            else if (randNote == 3)
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