using TMPro;
using UnityEngine;

public class WinScreen : MonoBehaviour
{
    public GameObject winCanvas;
    private GameManager gm;

    public TextMeshProUGUI difficultyText;
    public TextMeshProUGUI roundsText;

    private void Awake()
    {
        winCanvas.SetActive(false);
        gm = gameObject.GetComponent<GameManager>();
    }

    public void WinGame(int difficulty, int roundsBeaten)
    {
        difficultyText.SetText("Congradulations on Beating Level " + difficulty.ToString());
        roundsText.SetText("You Scored " + roundsBeaten.ToString() + " / 5 Questions");
        winCanvas.SetActive(true);
    }
}
