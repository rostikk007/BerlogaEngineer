using UnityEngine;
using TMPro;
using System.IO;
using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class WinTransport : MonoBehaviour
{
    private class SaveData
    {
        // ������������ ����
        public int Money;
        public int SelectedCharacterSkin;
        public int SelectedMazeSkin;
        public List<int> OpenCharacterSkins;
        public List<int> OpenMazeSkins;

        // ����� ���� ��� ����������� ������
        public string LastCollectDate;
        public int CurrentDay;
        public bool CollectedToday;
    }
    [SerializeField] private int scene1;
    [SerializeField] private int scene2;
    private SaveData _saveData;
    private string _savePath;
    public void LosdMenu()
    {
        Time.timeScale = 1f;
        TransportFromShop.PlayerPosss = 2;
        SceneManager.LoadScene(scene1);
        //SceneTransition.SwitchToScene(scene1);
    }
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(scene1);
        //SceneTransition.SwitchToScene(scene2);
    }
    private void Start()
    {
        string savePath = DataLocalProvider.FileJson ?? DataManager.Instance.SavePath;

        // ������ ��� ������� ������
        string currentJson = File.ReadAllText(savePath);
        SaveData currentData = JsonConvert.DeserializeObject<SaveData>(currentJson);

        if (currentData == null)
        {
            Debug.LogError("������ ��� ������ ������ ����������!");
            return;
        }

        // ��������� ������ �������� ��� �����������
        int oldMoney = currentData.Money;

        // ��������� �������� �����
        currentData.Money = oldMoney + 500;

        // ��������� ��� ������ �������
        string json = JsonConvert.SerializeObject(currentData, Formatting.Indented);
        File.WriteAllText(savePath, json);
    }
}