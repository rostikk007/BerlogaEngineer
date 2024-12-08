using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSpawn : MonoBehaviour
{
    private Player playerscene;
    public Transform playerPoss;
    public Transform playerPossShop;
    public Transform playerPossFight;
    public Transform playerPossApiary;
    public Transform playerPossEDM;
    public Transform playerPossSergo;
    public GameObject[] players;

    private void Awake()
    {
        TransportInFight.isLoadingSceneFight = false;
        if (TransportFromShop.PlayerPosss == 1)
        {
            playerPoss = playerPossShop;
            TransportFromShop.PlayerPosss = 100;
        }
        if (TransportFromShop.PlayerPosss == 2)
        {
            playerPoss = playerPossFight;
            TransportFromShop.PlayerPosss = 100;
        }
        if (TransportFromShop.PlayerPosss == 3)
        {
            playerPoss = playerPossApiary;
            TransportFromShop.PlayerPosss = 100;
        }
        if (TransportFromShop.PlayerPosss == 4)
        {
            playerPoss = playerPossEDM;
            TransportFromShop.PlayerPosss = 100;
        }
        if (TransportFromShop.PlayerPosss == 5)
        {
            playerPoss = playerPossSergo;
            TransportFromShop.PlayerPosss = 100;
        }
        playerscene = Instantiate(players[PlayerPrefs.GetInt("Player")], playerPoss.position, Quaternion.identity).GetComponent<Player>();
    }
    void Start()
    {
        
    }

    
}
