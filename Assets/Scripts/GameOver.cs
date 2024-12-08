using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public void LosdMenu()
    {
        Time.timeScale = 1f;
        if (TransportFromShop.PlayerPosss != 5)
            TransportFromShop.PlayerPosss = 2;

        SceneManager.LoadScene("MainMap");
    }
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }
}
