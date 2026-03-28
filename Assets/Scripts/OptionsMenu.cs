using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    public GameObject optionCanvas;

    private bool isPaused;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OpenOptions(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                OpenOptions(false);
            }
            else
            {
                OpenOptions(true);
            }
        }
    }

    public void OpenOptions(bool pause)
    {
        if (pause)
        {
            optionCanvas.SetActive(true);
        }
        else
        {
            optionCanvas.SetActive(false);
        }

        isPaused = pause;
    }
}
