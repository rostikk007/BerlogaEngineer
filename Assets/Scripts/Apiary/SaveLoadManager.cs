using UnityEngine;
using TMPro;
using System.IO;
using System;
using Newtonsoft.Json;


public class SaveLoadManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _moneyText;
    private string _money;
    private string monsper;

    private void Start()
    {
        LoadMoneyFromJson();
    }

    public void LoadMoneyFromJson()
    {
        if (DataLocalProvider.FileJson == null)
        {
            monsper = DataManager.Instance.SavePath;
        }
        else
        {
            monsper = DataLocalProvider.FileJson;
        }
        string jsonContent = File.ReadAllText(monsper);
        if (jsonContent != null)
        {
            // Создаем временный класс для десериализации JSON
            SaveData saveData = JsonUtility.FromJson<SaveData>(jsonContent);
            
            _money = saveData.Money;

            // Проверяем, что компонент TextMeshProUGUI присутствует
            if (_moneyText != null)
            {
                _moneyText.text = _money; // Устанавливаем текст в компонент UI
            }
        }
        else
        {
            _money = "0";
            if (_moneyText != null)
            {
                _moneyText.text = _money;
            }
        }
    }
    public void OnlineMoneyInput()
    {
        if (DataLocalProvider.FileJson == null)
        {
            monsper = DataManager.Instance.SavePath;
        }
        else
        {
            monsper = DataLocalProvider.FileJson;
        }
        string jsonContent = File.ReadAllText(monsper);
        PlayerData currentData = JsonConvert.DeserializeObject<PlayerData>(jsonContent);
        if (currentData != null)
        {
            // Обновляем только значение денег
            int currentMoney = currentData.Money;
            int newMoney = currentMoney + ApiaryOnline._onlinee;
            currentData.Money = newMoney;
            

            // Сериализуем обратно все данные
            string updatedJson = JsonConvert.SerializeObject(currentData, Formatting.Indented);
            File.WriteAllText(monsper, updatedJson);

            // Обновляем отображение
            if (_moneyText != null)
            {
                _moneyText.text = currentData.Money.ToString();
            }
        }
    }
}

[System.Serializable]
public class SaveData
{
    public string Money;
}
