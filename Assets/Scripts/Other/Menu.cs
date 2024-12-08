using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public void SetPlayer(int index)
    {
        PlayerPrefs.SetInt("Player", index);
    }
    private void Update()
    {
        
    }
}
