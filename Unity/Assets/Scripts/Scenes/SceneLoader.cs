using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSinglePlayerScene()
    {
        SceneManager.LoadScene("SinglePlayer");
    }

    public void LoadMultiPlayerScene()
    {
        SceneManager.LoadScene("MultiPlayer");
    }

    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
