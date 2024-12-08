using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApiaryTrans : MonoBehaviour
{
    [SerializeField] private int scene;
    public void PlayGame()
    {
        TransportFromShop.PlayerPosss = 3;
        TransportInApiary.isLoadingSceneApiary = false;
        SceneTransition.SwitchToScene(scene);
    }
}
