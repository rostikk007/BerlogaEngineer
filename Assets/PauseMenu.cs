using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private int scene;
    public bool _pauseGame;
    public GameObject _pauseGameMenu;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_pauseGame)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }
    public void Resume()
    {
        _pauseGameMenu.SetActive(false);
        Time.timeScale = 1f;
        _pauseGame = false;
    }
    public void Pause()
    {
        _pauseGameMenu.SetActive(true);
        Time.timeScale = 0f;
        _pauseGame = true;
    }
    public void LoadMenu()
    {
        Time.timeScale = 1f;
        TransportFromShop.PlayerPosss = 2;
        SceneTransition.SwitchToScene(scene);
    }

    public void changeScene()
    {
        Time.timeScale = 1f;
        SceneTransition.SwitchToScene(scene);
    }
}

