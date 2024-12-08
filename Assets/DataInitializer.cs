using UnityEngine;
using TMPro;
using System.IO;
using System;
using Newtonsoft.Json;

public class DataInitializer : MonoBehaviour
{
    private void Awake()
    {
        // Создаем начальные данные, если файл не существует
        if (!File.Exists(GetSavePath()))
        {
            PlayerData initialData = new PlayerData();
            SaveInitialData(initialData);
        }
    }

    private string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, "PlayerSave.json");
    }

    private void SaveInitialData(PlayerData data)
    {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(GetSavePath(), json);
        Debug.Log("Создан начальный файл сохранения");
    }
}