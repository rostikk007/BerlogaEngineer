using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.IO;

public class DataManager : MonoBehaviour
{
    [Header("API Settings")]
    [SerializeField] private string _baseUrl = "https://2025.nti-gamedev.ru/api/games/";
    [SerializeField] private string _gameUuid = "���_UUID";
    public static DataManager Instance { get; private set; }
    private string _savePath;
    private PlayerData _currentData;
    public string SavePath => _savePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        print(UsernameInputController._userInputText);
    }

    private void Initialize()
    {
        _savePath = Path.Combine(Application.persistentDataPath, "PlayerSave.json");
        LoadData();
    }

    public PlayerData GetData()
    {
        if (_currentData == null)
        {
            LoadData();
        }
        return _currentData;
    }

    public void LoadData()
    {
        try
        {
            if (File.Exists(_savePath))
            {
                string json = File.ReadAllText(_savePath);
                _currentData = JsonConvert.DeserializeObject<PlayerData>(json);
                Debug.Log("������ ������� ���������");
            }
            else
            {
                _currentData = new PlayerData();
                Debug.Log("������ ����� ������ ������");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"������ ��� �������� ������: {e.Message}");
            _currentData = new PlayerData();
        }
    }

    public void SaveData()
    {
        try
        {
            string json = JsonConvert.SerializeObject(_currentData, Formatting.Indented);
            File.WriteAllText(_savePath, json);
            Debug.Log("������ ������� ���������");
        }
        catch (Exception e)
        {
            Debug.LogError($"������ ��� ���������� ������: {e.Message}");
        }
    }
    public IEnumerator GetPlayersList(Action<List<PlayerData>> callback)
    {
        string url = $"{_baseUrl}{_gameUuid}/players/";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var players = JsonConvert.DeserializeObject<List<PlayerData>>(www.downloadHandler.text);
                callback?.Invoke(players);
            }
            else
            {
                Debug.LogError($"������ ��������� ������ �������: {www.error}");
                callback?.Invoke(null);
            }
        }
    }
}