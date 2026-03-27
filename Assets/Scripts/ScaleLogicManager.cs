using System;
using System.Collections.Generic;
using UnityEngine;

/* Math and Game-State Logic for Balancing Game
 * 20 March, 2026
 * 
 * In this version, I used "animals" for + vals., "balloons" for -
 * 
 * What I ~tried~ to implement:
 *
 * - Accept difficulty level (from settings?; levels 1, 2 and 3)
 * - Difficulty level impacts:
 *      - # of objects in "bank"
 *      - Magnitude of number to balance
 * - 5 rounds using increasing #s to balance (within difficulty range),
 *   but only advance to greater numbers if player solved round
 *   efficiently (<moveThreshold)
 * - Game tracks # of moves player makes
 * - Game listens to move events
 * - Game updates left/right totals
 * - Game reports the balance state, round solved state, game finished
 *   state, as well as what GUI elements to display
 */

public class ScaleLogicManager : MonoBehaviour
{
    // =========================================================
    // NESTED DATA TYPES
    // =========================================================
    
    // MOVABLE OBJECT FOR GRAPHIC
    // Can be "Animal" for +, "Balloon" for -
    [Serializable]
    public class MovableObjectData
    {
        public string objectId;     // Unique ID (e.g. "obj_0", "obj_1")
        public string objectType;   // "Animal" or "Balloon"
        public int value;           // Signed val: Animal positive, Balloon negative
    }
    
    // DATA TO SET-UP ROUND
    // Provides list of elements to be displayed, and their values
    [Serializable]
    public class RoundSetupData
    {
        public int roundNumber;
        public int leftStartValue;
        public int rightStartValue;
        public List<MovableObjectData> MovableObjects = new List<MovableObjectData>();
    }
    
    // DATA TO SHOW STATE OF ROUND
    // Can be used to display state of balance, etc.
    [Serializable]
    public class BalanceStateData
    {
        public int leftCurrentValue;
        public int rightCurrentValue;
        public int difference;   // left - right
        public int moveCount;
        public bool isBalanced;
    }

    // =========================================================
    // GAME EVENTS FOR UI UPDATING
    // =========================================================
    
    // Round is set-up
    public event Action<RoundSetupData> OnRoundSetup;
    
    // Balance state has changed - update scale, etc.
    public event Action<BalanceStateData> OnBalanceChanged;
    
    // Round has been successfully solved
    public event Action<int> OnRoundSolved;
    
    // All 5 rounds finished
    public event Action OnGameFinished;

    // =========================================================
    // GAME STATE HOLDERS
    // =========================================================

    private int selectedDifficulty = 1;
    private int totalRounds = 5;
    private int currentRound = 0;
    private int movableObjectCount = 3;        // i.e. # of animals/balloons
    private int maxValueForDifficulty = 20;     // i.e. highest val to solve
    private int moveThreshold = 4;              // max threshold for moves

    // Benchmark values a < b < c < d < e for difficulty progression
    private List<int> benchmarkValues = new List<int>();
    private int currentRangeIndex = 0;

    // Current round math state
    private int leftStartValue = 0;
    private int rightStartValue = 0;
    private int leftCurrentValue = 0;
    private int rightCurrentValue = 0;
    private int moveCountThisRound = 0;
    private bool roundSolved = false;
    private bool gameStarted = false;
    private bool gameFinished = false;

    // List objects for this round
    private List<MovableObjectData> availableObjects = new List<MovableObjectData>();

    // Keep track of object location
    // e.g. "Bank", "Left", or "Right"
    private Dictionary<string, string> objectLocations = new Dictionary<string, string>();

    // =========================================================
    // PUBLIC METHODS
    // =========================================================
    
    // START NEW GAME WITH "DIFFICULTY" FROM SETTINGS(?)
    // Difficulty can be 1, 2 or 3
    public void StartGame(int difficulty)
    {
        selectedDifficulty = difficulty;
        gameStarted = true;
        gameFinished = false;
        currentRound = 0;
        currentRangeIndex = 0;

        ConfigureDifficulty();
        GenerateBenchmarkValues();
        StartNextRound();
    }
    
    // START NEXT ROUND
    // If player did well, increase # range
    // If player needed too many moves, stay at curr. level
    // If game finished, send event signal
    public void StartNextRound()
    {
        if (!gameStarted || gameFinished)
            return;

        // Adjust difficulty before starting next round
        if (currentRound > 0) // skip for first round
        {
            if (moveCountThisRound <= moveThreshold)
            {
                // Player did well -> increase range (if possible)
                if (currentRangeIndex < benchmarkValues.Count - 2)
                {
                    currentRangeIndex++;
                }
            }
            else
            {
                // Player struggled -> stay in same range
                // (do nothing)
            }
        }
        
        currentRound++;

        if (currentRound > totalRounds)
        {
            gameFinished = true;
            OnGameFinished?.Invoke();
            return;
        }

        GenerateSolvableRound();
        EmitRoundSetup();
        EmitBalanceState();
    }
    
    // RESET ROUND TO INITIAL STATE
    public void ResetRound()
    {
        leftCurrentValue = leftStartValue;
        rightCurrentValue = rightStartValue;
        moveCountThisRound = 0;
        roundSolved = false;

        objectLocations.Clear();
        for (int i = 0; i < availableObjects.Count; i++)
        {
            objectLocations[availableObjects[i].objectId] = "Bank";
        }

        EmitBalanceState();
    }
    
    // REFRESH STATE WHEN OBJECT IS MOVED
    // e.g. ApplyMove("obj_2", "Right")
    public void ApplyMove(string objectId, string newSide)
    {
        if (!gameStarted || gameFinished || roundSolved)
            return;

        // Make sure object exists
        MovableObjectData obj = FindObjectById(objectId);
        if (obj == null)
        {
            Debug.LogWarning("ApplyMove: object not found: " + objectId);
            return;
        }

        // Normalise side names
        newSide = NormaliseSideName(newSide);
        if (newSide != "Left" && newSide != "Right" && newSide != "Bank")
        {
            Debug.LogWarning("ApplyMove: invalid side: " + newSide);
            return;
        }

        // Find previous location
        string oldSide = "Bank";
        if (objectLocations.ContainsKey(objectId))
        {
            oldSide = objectLocations[objectId];
        }

        // If the object is already there, do nothing
        if (oldSide == newSide)
            return;

        // First remove its old effect
        RemoveObjectEffectFromSide(obj, oldSide);

        // Then apply its new effect
        AddObjectEffectToSide(obj, newSide);

        // Update stored location
        objectLocations[objectId] = newSide;

        // Count player move
        moveCountThisRound++;

        // Notify w/ balance
        EmitBalanceState();

        // Check if solved
        if (leftCurrentValue == rightCurrentValue)
        {
            roundSolved = true;
            OnRoundSolved?.Invoke(currentRound);
        }
    }

    // RETURN CURRENT BALANCE STATE
    // Remove if event notifications are good enough for graphic updates
    public BalanceStateData GetCurrentBalanceState()
    {
        return new BalanceStateData
        {
            leftCurrentValue = leftCurrentValue,
            rightCurrentValue = rightCurrentValue,
            difference = leftCurrentValue - rightCurrentValue,
            moveCount = moveCountThisRound,
            isBalanced = (leftCurrentValue == rightCurrentValue)
        };
    }

    // =========================================================
    // INTERNAL SETUP LOGIC
    // =========================================================
    
    // DEFINE DIFFICULTY LEVELS
    private void ConfigureDifficulty()
    {
        if (selectedDifficulty == 1)
        {
            movableObjectCount = 3;        // i.e. max 3 animals+balloons in bank
            maxValueForDifficulty = 20;     // the maximum value to solve
        }
        else if (selectedDifficulty == 2)
        {
            movableObjectCount = 5;
            maxValueForDifficulty = 40;
        }
        else
        {
            movableObjectCount = 6;
            maxValueForDifficulty = 40;
        }
    }
    
    // GENERATE BENCHMARK VALUES FOR ROUND PROGRESSION
    // Increasing order: a < b < c < d < e
    private void GenerateBenchmarkValues()
    {
        benchmarkValues.Clear();

        if (selectedDifficulty == 1)
        {
            int a = UnityEngine.Random.Range(3, 6);
            int b = UnityEngine.Random.Range(a + 2, 9);
            int c = UnityEngine.Random.Range(b + 2, 12);
            int d = UnityEngine.Random.Range(c + 2, 16);
            int e = UnityEngine.Random.Range(d + 2, 21);

            benchmarkValues.Add(a);
            benchmarkValues.Add(b);
            benchmarkValues.Add(c);
            benchmarkValues.Add(d);
            benchmarkValues.Add(e);
        }
        else if (selectedDifficulty == 2)
        {
            int a = UnityEngine.Random.Range(8, 16);
            int b = UnityEngine.Random.Range(a + 4, 26);
            int c = UnityEngine.Random.Range(b + 4, 36);
            int d = UnityEngine.Random.Range(c + 4, 48);
            int e = UnityEngine.Random.Range(d + 4, 61);

            benchmarkValues.Add(a);
            benchmarkValues.Add(b);
            benchmarkValues.Add(c);
            benchmarkValues.Add(d);
            benchmarkValues.Add(e);
        }
        else
        {
            int a = UnityEngine.Random.Range(12, 25);
            int b = UnityEngine.Random.Range(a + 6, 40);
            int c = UnityEngine.Random.Range(b + 6, 60);
            int d = UnityEngine.Random.Range(c + 6, 80);
            int e = UnityEngine.Random.Range(d + 6, 100);

            benchmarkValues.Add(a);
            benchmarkValues.Add(b);
            benchmarkValues.Add(c);
            benchmarkValues.Add(d);
            benchmarkValues.Add(e);
        }
    }
    
    // GENERATE A SOLVABLE ROUND
    // THis method ensures that if there are n movable objects, the
    // solution will use either n-1 or n-2 of them (so that player
    // cannot be certain which objects are needed.
    private void GenerateSolvableRound()
    {
        roundSolved = false;
        moveCountThisRound = 0;
        availableObjects.Clear();
        objectLocations.Clear();

        // Pick a range using currentRangeIndex
        int rangeStart = benchmarkValues[Mathf.Clamp(currentRangeIndex, 0, benchmarkValues.Count - 2)];
        int rangeEnd = benchmarkValues[Mathf.Clamp(currentRangeIndex + 1, 1, benchmarkValues.Count - 1)];

        bool success = false;

        while (!success)
        {
            availableObjects.Clear();
            objectLocations.Clear();

            // Pick intended solution size: n-1 or n-2
            int solutionObjectCount = (UnityEngine.Random.value < 0.5f)
                ? movableObjectCount - 1
                : movableObjectCount - 2;

            if (solutionObjectCount < 1)
                solutionObjectCount = 1;

            List<MovableObjectData> hiddenSolution = new List<MovableObjectData>();
            int hiddenSolutionSum = 0;

            // Generate hidden solution objects first
            for (int i = 0; i < solutionObjectCount; i++)
            {
                MovableObjectData obj = new MovableObjectData();
                obj.objectId = "obj_" + i;

                bool makeAnimal = UnityEngine.Random.value < 0.5f;

                if (makeAnimal)
                {
                    obj.objectType = "Animal";
                    obj.value = GetRandomMagnitude();
                }
                else
                {
                    obj.objectType = "Balloon";
                    obj.value = -GetRandomMagnitude();
                }

                hiddenSolution.Add(obj);
                hiddenSolutionSum += obj.value;
            }

            // Avoid zero-sum
            if (hiddenSolutionSum == 0)
                continue;

            // Choose starting left/right so that the hidden solution can balance the round
            // In this case, it is assumed the intended solution will be
            // placed on the RIGHT, but this can be modified to use both sides
            // - Current Solve condition: left = right + hiddenSolutionSum
            bool foundStartValues = false;
            int candidateLeft = 0;
            int candidateRight = 0;

            for (int attempt = 0; attempt < 100; attempt++)
            {
                int leftCandidate = UnityEngine.Random.Range(rangeStart, rangeEnd + 1);
                int rightCandidate = leftCandidate - hiddenSolutionSum;

                if (leftCandidate >= 0 && leftCandidate <= maxValueForDifficulty &&
                    rightCandidate >= 0 && rightCandidate <= maxValueForDifficulty)
                {
                    candidateLeft = leftCandidate;
                    candidateRight = rightCandidate;
                    foundStartValues = true;
                    break;
                }
            }

            if (!foundStartValues)
                continue;

            // Add distractors until total object count = movableObjectCount
            List<MovableObjectData> allObjects = new List<MovableObjectData>();
            allObjects.AddRange(hiddenSolution);

            while (allObjects.Count < movableObjectCount)
            {
                MovableObjectData distractor = new MovableObjectData();
                distractor.objectId = "obj_" + allObjects.Count;

                bool makeAnimal = UnityEngine.Random.value < 0.5f;
                if (makeAnimal)
                {
                    distractor.objectType = "Animal";
                    distractor.value = GetRandomMagnitude();
                }
                else
                {
                    distractor.objectType = "Balloon";
                    distractor.value = -GetRandomMagnitude();
                }

                allObjects.Add(distractor);
            }

            Shuffle(allObjects);

            // Save round state
            leftStartValue = candidateLeft;
            rightStartValue = candidateRight;
            leftCurrentValue = leftStartValue;
            rightCurrentValue = rightStartValue;
            availableObjects = allObjects;

            // Initially, all objects are in the Bank
            for (int i = 0; i < availableObjects.Count; i++)
            {
                objectLocations[availableObjects[i].objectId] = "Bank";
            }

            success = true;
        }
    }
    
    // Select magnitude based on difficulty
    private int GetRandomMagnitude()
    {
        if (selectedDifficulty == 1)
            return UnityEngine.Random.Range(1, 10);
        if (selectedDifficulty == 2)
            return UnityEngine.Random.Range(2, 16);
        return UnityEngine.Random.Range(3, 21);
    }

    // =========================================================
    // INTERNAL MOVE / MATH LOGIC
    // =========================================================
    
    // REMOVE OBJECT EFFECT FROM PREVIOUS LOCATION
    private void RemoveObjectEffectFromSide(MovableObjectData obj, string oldSide)
    {
        if (oldSide == "Left")
        {
            leftCurrentValue -= obj.value;
        }
        else if (oldSide == "Right")
        {
            rightCurrentValue -= obj.value;
        }
        // "Bank" = effect
    }
    
   // APPLY OBJECT EFFECT ON NEW LOCATION
    private void AddObjectEffectToSide(MovableObjectData obj, string newSide)
    {
        if (newSide == "Left")
        {
            leftCurrentValue += obj.value;
        }
        else if (newSide == "Right")
        {
            rightCurrentValue += obj.value;
        }
        // "Bank" = effect
    }
    
    // FIND OBJECT ID
    private MovableObjectData FindObjectById(string objectId)
    {
        for (int i = 0; i < availableObjects.Count; i++)
        {
            if (availableObjects[i].objectId == objectId)
                return availableObjects[i];
        }
        return null;
    }


    // ENSURE "left" = "LEFT", ETC.
    private string NormaliseSideName(string side)
    {
        if (string.IsNullOrEmpty(side))
            return "Bank";

        side = side.Trim().ToLower();

        if (side == "left")
            return "Left";
        if (side == "right")
            return "Right";
        return "Bank";
    }

    // =========================================================
    // INTERNAL EVENT HELPERS
    // =========================================================
    
    // NOTIFY OF CURRENT ROUND SET-UP
    // Can be used to initialise GUI
    private void EmitRoundSetup()
    {
        RoundSetupData data = new RoundSetupData();
        data.roundNumber = currentRound;
        data.leftStartValue = leftStartValue;
        data.rightStartValue = rightStartValue;
        //data.movableObjects = new List<MovableObjectData>(availableObjects);

        OnRoundSetup?.Invoke(data);
    }
    
    // NOTIFY OF CURRENT BALANCE STATE
    private void EmitBalanceState()
    {
        BalanceStateData state = new BalanceStateData();
        state.leftCurrentValue = leftCurrentValue;
        state.rightCurrentValue = rightCurrentValue;
        state.difference = leftCurrentValue - rightCurrentValue;
        state.moveCount = moveCountThisRound;
        state.isBalanced = (leftCurrentValue == rightCurrentValue);

        OnBalanceChanged?.Invoke(state);
    }

    // =========================================================
    // SMALL UTILITY
    // =========================================================
    
    // SHUFFLE MOVABLE OBJECTS
    private void Shuffle(List<MovableObjectData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, list.Count);
            MovableObjectData temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
