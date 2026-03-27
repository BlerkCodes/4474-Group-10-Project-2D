using System;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const int TOTALROUNDS = 5;

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

    public List<int> questionValues = new List<int>();

    private int selectedDifficulty = 1;
    private int currentRound;

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

    [Header("Levels")]
    public List<ColorChange> levelNodes;

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
        currentRound = 0;
        adjuster.ChangeCenter();

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
        MakeDifficulty(selectedDifficulty);
        MakeRound();
    }

    // Update is called once per frame
    void Update()
    {
        leftAnswerVariable = leftBaseBox;
        rightAnswerVariable = rightBaseBox;

        foreach (GameObject leftBox in leftAnswerBoxes)
        {
            if (leftBox.transform.childCount > 0)
            {
                GameObject child = leftBox.transform.GetChild(0).gameObject;
                leftAnswerVariable += child.GetComponent<BoxValue>().GetValue();
            }
        }

        foreach (GameObject rightBox in rightAnswerBoxes)
        {
            if (rightBox.transform.childCount > 0)
            {
                GameObject child = rightBox.transform.GetChild(0).gameObject;
                rightAnswerVariable += child.GetComponent<BoxValue>().GetValue();
            }
        }

        leftTotalText.SetText(leftAnswerVariable.ToString());
        rightTotalText.SetText(rightAnswerVariable.ToString());

        if (leftAnswerVariable == rightAnswerVariable)
        {
            adjuster.ChangeCenter();
            levelNodes[currentRound].ChangeComplete();
            currentRound++;
            MakeRound();
        }
        else if (leftAnswerVariable > rightAnswerVariable)
        {
            adjuster.ChangeLeft();
        }
        else
        {
            adjuster.ChangeRight();
        }
    }

    private void MakeDifficulty(int difficulty)
    {
        switch (difficulty)
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
            leftBaseBox = UnityEngine.Random.Range(currentDifficulty.minQuestionRange, currentDifficulty.maxQuestionRange + 1);
            leftBoxText.SetText(leftBaseBox.ToString());
        }

        questionValues.Clear();

        do
        {
            rightBaseBox = UnityEngine.Random.Range(currentDifficulty.minQuestionRange, currentDifficulty.maxQuestionRange + 1); // makes the new result
            rightBoxText.SetText(rightBaseBox.ToString());
        } while (rightBaseBox == leftBaseBox);


        // Inserts sequence of numbers into the list that makes sure there is always an answer to the problem
        int numOfBoxes = UnityEngine.Random.Range(currentDifficulty.minBoxesAmount, currentDifficulty.maxBoxesAmount + 1);
        int lengthOfAnswer = UnityEngine.Random.Range(2, numOfBoxes);
        MakeNewQuestion(lengthOfAnswer , leftBaseBox);

        for (int i = lengthOfAnswer; numOfBoxes > i; i++)
        {
            if (currentDifficulty.minQuestionRange == 0)
            {
                questionValues.Add(UnityEngine.Random.Range(1, currentDifficulty.maxQuestionRange));
            }
            else
            {
                questionValues.Add(UnityEngine.Random.Range(currentDifficulty.minQuestionRange, currentDifficulty.maxQuestionRange));
            }
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
            questionValues.Add(num);
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
                int num = UnityEngine.Random.Range(1, (currentResult + currentDifficulty.minQuestionRange) + 1);
                questionValues.Add(num);
                int tempResult = currentResult - num;
                MakeNewQuestion(numbersRemaining - 1, tempResult);
            }
        }
        return;
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
