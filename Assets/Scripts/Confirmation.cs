using TMPro;
using UnityEngine;

public class Confirmation : MonoBehaviour
{
    public GameObject HomeWindow;
    public GameObject LevelSelectWindow;

    private void Awake()
    {
        CloseConfirmationHome();
        CloseConfirmationLevel();
    }

    public void OpenConfirmationHome()
    {
        HomeWindow.SetActive(true);
    }

    public void OpenConfirmationLevel()
    {
        LevelSelectWindow.SetActive(true);
    }

    public void CloseConfirmationHome()
    {
        HomeWindow.SetActive(false);
    }

    public void CloseConfirmationLevel()
    {
        LevelSelectWindow.SetActive(false);
    }
}
