using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApiaryTransport : MonoBehaviour
{
    [SerializeField] private int scene;
    public void PlayGame()
    {
        SceneTransition.SwitchToScene(scene);
    }
}
