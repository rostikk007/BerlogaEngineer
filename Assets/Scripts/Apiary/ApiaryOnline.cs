using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

public class ApiaryOnline : MonoBehaviour
{
    private class SaveData
    {
        // Существующие поляzz
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
    [SerializeField] private SaveLoadManager _saveLoadManager;
    [SerializeField] private InputField _inputHoney;
    [SerializeField] private TMP_Text _honeys;
    [SerializeField] private TMP_Text _levelHoneys;
    [SerializeField] private TMP_Text _onlMon;
    public static int startApiary = 1;
    public static int _onlinee = 0;
    private int _honey;
    private int _levelHoney;
    private int _inputHoneySave = 0;
    
    void Start()
    {
        if (_saveLoadManager == null)
        {
            Debug.LogError("SaveLoadManager не назначен в инспекторе!");
            return;
        }

        if (_inputHoney == null || _honeys == null || _levelHoneys == null || _onlMon == null)
        {
            Debug.LogError("Не все UI компоненты назначены в инспекторе!");
            return;
        }
        _onlMon.text = _onlinee.ToString();
        if (startApiary == 0)
        {
            PlayerPrefs.SetInt("honey", 0);
            PlayerPrefs.SetInt("levelhoney", 0);
        }
        startApiary = 1;
        _honey = PlayerPrefs.GetInt(key: "honey", defaultValue: 0);
        _levelHoney = PlayerPrefs.GetInt(key: "levelhoney", defaultValue: 0);
        DateTime lastSaveTime = UtilsApiary.GetDateTime("lastSaveTime", DateTime.UtcNow);
        TimeSpan timePassed = DateTime.UtcNow - lastSaveTime;
        int secondsPassed = (int)timePassed.TotalSeconds;
        _honey += secondsPassed * _levelHoney;
        UpdateText();
        InvokeRepeating(nameof(PayIncome), 1f, 1f);
    }
    public void HoneyClicked()
    {
        _honey++;
        SavesData();
        UpdateText();
    }
    public void BuyLevelHoney()
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
        if (oldMoney < 1000) return;
        oldMoney -= 1000;
        _levelHoney += 1;
        // Обновляем значение денег
        currentData.Money = oldMoney;
        WithdrawMoney();

        // Сохраняем все данные обратно
        string json = JsonConvert.SerializeObject(currentData, Formatting.Indented);
        File.WriteAllText(savePath, json);
        SavesData();
        UpdateText();

    }
    private void SavesData()
    {
        PlayerPrefs.SetInt("honey",_honey);
        PlayerPrefs.SetInt("levelhoney",_levelHoney);
        UtilsApiary.SetDateTime("lastSaveTime", DateTime.UtcNow);
    }
    private void UpdateText()
    {
        _honeys.text = $"HONEY in the Apiary: {_honey}";
        _levelHoneys.text = $"LEVEL Apiary {_levelHoney}";
    }
    private void PayIncome()
    {
        _honey += _levelHoney;
        SavesData();
        UpdateText();
    }
    private void SaveInputHoney()
    {
        string inputText = _inputHoney.text;
        if (int.TryParse(inputText, out int inputHoneyValue))
        {
            _inputHoneySave = inputHoneyValue;
        }
        //_inputHoneySave = int.Parse(_inputHoney.text);
    }
    public void WithdrawMoney()
    {
        SaveInputHoney();
        if (_honey >= _inputHoneySave)
        {
            _onlinee += _inputHoneySave;
            _honey -= _inputHoneySave;
            _saveLoadManager.OnlineMoneyInput();
            _onlinee = 0;
            _onlMon.text = _onlinee.ToString();
            SavesData();
            UpdateText();
        }
    }
}
