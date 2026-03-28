using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneLoader : MonoBehaviour
{
    public void LoadScene(int difficulty)
    {
        PlayerPrefs.SetInt("GameDifficulty", difficulty);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Game");
        Time.timeScale = 1f;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
