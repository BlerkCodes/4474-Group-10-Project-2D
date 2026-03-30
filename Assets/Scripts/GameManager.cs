using System;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const int TOTALROUNDS = 5;
    private const int ADAPTIVE_MINIMUM = 2;

    [Serializable]
    public struct Difficulty
    {
        public Difficulty(int minqr, int maxqr, int minba, int maxba)
        {
            minQuestionRange = minqr;
            maxQuestionRange = maxqr;
            minBoxesAmount = minba;
            maxBoxesAmount = maxba;
        }

        public int minQuestionRange;
        public int maxQuestionRange;
        public int minBoxesAmount;
        public int maxBoxesAmount;
    }

    public Difficulty currentDifficulty;
    public ScaleAdjuster adjuster;
    private AudioManager audioManager;

    private int lengthOfAnswer;
    public int currentMinimumLength;

    public List<int> questionValues = new List<int>();

    public int selectedDifficulty;
    public int currentRound;

    [Header("Prefabs")]
    public GameObject baseBoxHolderPrefab;
    public GameObject boxPrefab;

    [Header("GameObject Organizers")]
    public GameObject leftBoxInputParent;
    public GameObject rightBoxInputParent;
    public GameObject baseBoxInputParent;

    [Header("Question Variables")]
    public int leftAnswerVariable;
    public int leftBaseBox;
    public TextMeshProUGUI leftBoxText;
    public TextMeshProUGUI leftTotalText;
    public int rightAnswerVariable;
    public int rightBaseBox;
    public TextMeshProUGUI rightBoxText;
    public TextMeshProUGUI rightTotalText;
    public List<GameObject> leftAnswerBoxes = new List<GameObject>();
    public List<GameObject> rightAnswerBoxes = new List<GameObject>();

    [Header("SkipButton")]
    public GameObject skipButton;
    public int timesDropped;
    public int dropCheck;
    public int lastDropCheck;
    public bool skipOn = false;

    [Header("Levels")]
    public List<ColorChange> levelNodes;
    public bool levelEnd = false;
    public float endTimer;
    public float endWaitTime;
    public int roundsBeaten;
    public WinScreen win;

    [Header("Difficulty 1 Attributes")]
    public int minQuestionRange1;
    public int maxQuestionRange1;
    public int minBoxesAmount1;
    public int maxBoxesAmount1;

    [Header("Difficulty 2 Attributes")]
    public int minQuestionRange2;
    public int maxQuestionRange2;
    public int minBoxesAmount2;
    public int maxBoxesAmount2;

    [Header("Difficulty 3 Attributes")]
    public int minQuestionRange3;
    public int maxQuestionRange3;
    public int minBoxesAmount3;
    public int maxBoxesAmount3;

    private void Awake()
    {
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        skipButton.SetActive(false);
        currentRound = 0;
        adjuster.ChangeCenter();
        win = gameObject.GetComponent<WinScreen>();

        foreach (Transform child in leftBoxInputParent.transform)
        {
            leftAnswerBoxes.Add(child.gameObject);
        }

        foreach (Transform child in rightBoxInputParent.transform)
        {
            rightAnswerBoxes.Add(child.gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        selectedDifficulty = PlayerPrefs.GetInt("GameDifficulty", 1);
        SetDifficulty();
        MakeRound();
        levelNodes[currentRound].ChangeCurrent();
    }

    // Update is called once per frame
    void Update()
    {
        leftAnswerVariable = leftBaseBox;
        rightAnswerVariable = rightBaseBox;
        timesDropped = 0;

        foreach (GameObject leftBox in leftAnswerBoxes)
        {
            if (leftBox.transform.childCount > 0)
            {
                GameObject child = leftBox.transform.GetChild(0).gameObject;
                BoxValue left = child.GetComponent<BoxValue>();
                leftAnswerVariable += left.GetValue();

                if (!skipOn)
                {
                    timesDropped += left.GetDropped();
                }
                else
                {
                    left.ResetDrops();
                }
            }
        }

        foreach (GameObject rightBox in rightAnswerBoxes)
        {
            if (rightBox.transform.childCount > 0)
            {
                GameObject child = rightBox.transform.GetChild(0).gameObject;
                BoxValue right = child.GetComponent<BoxValue>();
                rightAnswerVariable += right.GetValue();

                if (!skipOn)
                {
                    timesDropped += right.GetDropped();
                }
                else
                {
                    right.ResetDrops();
                }

            }
        }

        foreach (Transform baseBoxHolder in baseBoxInputParent.transform)
        {
            if (baseBoxHolder.transform.childCount > 0)
            {
                GameObject child = baseBoxHolder.transform.GetChild(0).gameObject;

                if (!skipOn)
                {
                    timesDropped += child.GetComponent<BoxValue>().GetDropped();
                }
                else
                {
                    child.GetComponent<BoxValue>().ResetDrops();
                }
            }
        }

        leftTotalText.SetText(leftAnswerVariable.ToString());
        rightTotalText.SetText(rightAnswerVariable.ToString());

        if (leftAnswerVariable == rightAnswerVariable)
        {
            adjuster.ChangeCenter();
            RoundComplete();
        }
        else if (leftAnswerVariable > rightAnswerVariable)
        {
            adjuster.ChangeLeft();
            endTimer = 0;
        }
        else
        {
            adjuster.ChangeRight();
            endTimer = 0;
        }

        if (skipOn)
        {
            skipButton.SetActive(true);
        }
        else
        {
            skipButton.SetActive(false);
            dropCheck = timesDropped;
            if (timesDropped >= 15)
            {
                skipOn = true;
            }
        }
    }

    private void SetDifficulty()
    {
        switch (selectedDifficulty)
        {
            case 1:
                currentDifficulty = new Difficulty(minQuestionRange1, maxQuestionRange1, minBoxesAmount1, maxBoxesAmount1);
                break;
            case 2:
                currentDifficulty = new Difficulty(minQuestionRange2, maxQuestionRange2, minBoxesAmount2, maxBoxesAmount2);
                break;
            case 3:
                currentDifficulty = new Difficulty(minQuestionRange3, maxQuestionRange3, minBoxesAmount3, maxBoxesAmount3);
                break;
            default:
                currentDifficulty = new Difficulty(minQuestionRange1, maxQuestionRange1, minBoxesAmount1, maxBoxesAmount1);
                break;
        }
    }

    private void MakeRound()
    {
        if (currentRound > 0)
        {
            foreach (Transform child in leftBoxInputParent.transform)
            {
                DestroyAllChildren(child.gameObject);
            }

            foreach (Transform child in rightBoxInputParent.transform)
            {
                DestroyAllChildren(child.gameObject);
            }

            DestroyAllChildren(baseBoxInputParent);
            leftBaseBox = rightBaseBox;
            leftBoxText.SetText(leftBaseBox.ToString());
        }
        else
        {
            currentMinimumLength = ADAPTIVE_MINIMUM;
            leftBaseBox = UnityEngine.Random.Range(currentDifficulty.minQuestionRange + 1, currentDifficulty.maxQuestionRange + 1);
            leftBoxText.SetText(leftBaseBox.ToString());
        }

        questionValues.Clear();

        do
        {
            rightBaseBox = UnityEngine.Random.Range(currentDifficulty.minQuestionRange + 1, currentDifficulty.maxQuestionRange + 1); // makes the new result
            rightBoxText.SetText(rightBaseBox.ToString());
        } while (rightBaseBox == leftBaseBox);


        // Inserts sequence of numbers into the list that makes sure there is always an answer to the problem
        int numOfBoxes = UnityEngine.Random.Range(currentDifficulty.minBoxesAmount, currentDifficulty.maxBoxesAmount + 1);
        lengthOfAnswer = UnityEngine.Random.Range(currentMinimumLength, numOfBoxes);
        MakeNewQuestion(lengthOfAnswer , leftBaseBox);

        for (int i = lengthOfAnswer; numOfBoxes > i; i++)
        {
            questionValues.Add(UnityEngine.Random.Range(1, (currentDifficulty.maxQuestionRange - currentDifficulty.minQuestionRange)));
        }

        questionValues.Sort();

        foreach (int value in questionValues)
        {
            GameObject holder = Instantiate(baseBoxHolderPrefab, baseBoxInputParent.transform);
            GameObject box = Instantiate(boxPrefab, holder.transform);

            BoxValue boxValue = box.GetComponent<BoxValue>();
            boxValue.SetValue(value);
        }
    }

    private void MakeNewQuestion(int numbersRemaining, int currentResult)
    {
        int randomOperator = UnityEngine.Random.Range(0, 2); // Gets either 0 or 1 / 0 = + / 1 = -

        if (numbersRemaining == 1)
        {
            int num = Mathf.Abs(currentResult - rightBaseBox);
            if (num != 0)
            {
                questionValues.Add(num);
            }
            else
            {
                lengthOfAnswer--;
            }
            return;
        }
        else
        {
            if (randomOperator == 0 || currentResult == currentDifficulty.minQuestionRange && currentResult != currentDifficulty.maxQuestionRange)
            {
                int num = UnityEngine.Random.Range(1, (currentDifficulty.maxQuestionRange - currentResult) + 1);
                questionValues.Add(num);
                int tempResult = currentResult + num;
                MakeNewQuestion(numbersRemaining - 1, tempResult);
            }
            else
            {
                int num = UnityEngine.Random.Range(1, (currentResult - currentDifficulty.minQuestionRange) + 1);
                questionValues.Add(num);
                int tempResult = currentResult - num;
                MakeNewQuestion(numbersRemaining - 1, tempResult);
            }
        }
        return;
    }

    private void RoundComplete()
    {
        if (levelEnd == false)
        {
            audioManager.PlaySFX(audioManager.win);
            levelNodes[currentRound].ChangeComplete();
            currentRound++;
            roundsBeaten++;
            IncreaseMinimumAnswerLength(dropCheck+1);
            dropCheck = 0;
            endTimer = 0;
            skipOn = false;
            levelEnd = true;
        }
        else
        {
            endTimer += Time.deltaTime;
            timesDropped = 0;

            if (endTimer >= endWaitTime)
            {
                if (currentRound < 5)
                {
                    MakeRound();
                    levelNodes[currentRound].ChangeCurrent();
                    endTimer = 0;
                    levelEnd = false;
                }
                else
                {
                    win.WinGame(selectedDifficulty, roundsBeaten);
                }
            }
        }
    }

    public void SkipQuestion()
    {
        if (currentRound < 4)
        {
            levelNodes[currentRound].ChangeSkipped();
            currentRound++;
            MakeRound();
            DecreaseMinimumOnSkip();
            dropCheck = 0;
            levelNodes[currentRound].ChangeCurrent();
            skipOn = false;
        }
        else
        {
            win.WinGame(selectedDifficulty, roundsBeaten);
        }
    }

    public void IncreaseMinimumAnswerLength(int check)
    {
        if (lastDropCheck == 0)
        {
            currentMinimumLength++;
        }
        else if (check <= lastDropCheck)
        {
            if (currentMinimumLength < currentDifficulty.maxBoxesAmount)
            {
                currentMinimumLength++;
            }

        }
        else
        {
            if (currentMinimumLength > currentDifficulty.minBoxesAmount)
            {
                currentMinimumLength--;
            }
        }

        lastDropCheck = check;
    }

    public void DecreaseMinimumOnSkip()
    {
        if (currentMinimumLength > currentDifficulty.minBoxesAmount)
        {
            currentMinimumLength--;
        }

        lastDropCheck = 0;
    }

    private void DestroyAllChildren(GameObject parent)
    {
        List<GameObject> children = new List<GameObject>();

        foreach (Transform child in parent.transform)
        {
            children.Add(child.gameObject);
        }

        foreach (GameObject child in children)
        {
            Destroy(child);
        }
    }
}
