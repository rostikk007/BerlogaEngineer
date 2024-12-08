using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private int scene;
    public void PlayGame()
    {
        SceneManager.LoadScene(11);
    }
    public void ExitGame()
    {
        Debug.Log("Exit");
        Application.Quit();
    }
    public void Sergo()
    {
        SceneTransition.SwitchToScene(scene);
    }
}
