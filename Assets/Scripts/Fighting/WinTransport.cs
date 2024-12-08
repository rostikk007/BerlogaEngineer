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
        // Существующие поля
        public int Money;
        public int SelectedCharacterSkin;
        public int SelectedMazeSkin;
        public List<int> OpenCharacterSkins;
        public List<int> OpenMazeSkins;

        // Новые поля для ежедневного бонуса
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

        // Читаем все текущие данные
        string currentJson = File.ReadAllText(savePath);
        SaveData currentData = JsonConvert.DeserializeObject<SaveData>(currentJson);

        if (currentData == null)
        {
            Debug.LogError("Ошибка при чтении данных сохранения!");
            return;
        }

        // Сохраняем старое значение для логирования
        int oldMoney = currentData.Money;

        // Обновляем значение денег
        currentData.Money = oldMoney + 500;

        // Сохраняем все данные обратно
        string json = JsonConvert.SerializeObject(currentData, Formatting.Indented);
        File.WriteAllText(savePath, json);
    }
}