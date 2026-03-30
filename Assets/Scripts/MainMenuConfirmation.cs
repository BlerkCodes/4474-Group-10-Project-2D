using UnityEngine;

public class MainMenuConfirmation : MonoBehaviour
{
    public GameObject window;

    private void Awake()
    {
        CloseConfirmation();
    }

    public void OpenConfirmation()
    {
        window.SetActive(true);
    }

    public void CloseConfirmation()
    {
        window.SetActive(false);
    }
}
