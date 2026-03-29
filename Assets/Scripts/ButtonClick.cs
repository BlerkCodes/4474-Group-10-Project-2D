using UnityEngine;

public class ButtonClick : MonoBehaviour
{
    private AudioManager audioManager;

    private void Awake()
    {
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }

    public void MenuButtonClick()
    {
        audioManager.PlaySFX(audioManager.menuClick);
    }
}
