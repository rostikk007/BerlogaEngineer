using UnityEngine;
using TMPro;
using System.IO;
using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Collections.Generic;

public class DailyBonusManager : MonoBehaviour
{
    [SerializeField] private int scene;
    [SerializeField] private TextMeshProUGUI _wallet;
    [Header("��������� �������")]
    [SerializeField] private int[] _dailyBonusAmounts = { 100, 200, 300, 400, 500, 600, 700 };

    [Header("������ �� UI")]
    [SerializeField] private TextMeshProUGUI _nextBonusTimeText;
    [SerializeField] private TextMeshProUGUI _currentDayText;
    [SerializeField] private TextMeshProUGUI _moneyDayText;
    [SerializeField] private Button _collectButton;
    [SerializeField] private TextMeshProUGUI _buttonText;

    [Header("����� ������")]
    [SerializeField] private Color _activeButtonColor = Color.green;    // ���� �������� ������
    [SerializeField] private Color _inactiveButtonColor = Color.gray;   // ���� ���������� ������
    [SerializeField] private Color _activeTextColor = Color.white;      // ���� ������ �������� ������
    [SerializeField] private Color _inactiveTextColor = Color.black;    // ���� ������ ���������� ������

    private Image _buttonImage;
    private bool _canCollect;
    private string _savePath;
    private SaveData _saveData;
    [System.Serializable]
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
    private void Awake()
    {
        // �������� ��������� Image ������
        _buttonImage = _collectButton.GetComponent<Image>();

        // ���� ����� ������ �� �������� � ����������, ������� ����� ���
        if (_buttonText == null)
        {
            _buttonText = _collectButton.GetComponentInChildren<TextMeshProUGUI>();
        }
    }
    private void Start()
    {
        if (DataLocalProvider.FileJson == null)
        {
            _savePath = DataManager.Instance.SavePath;
        }
        else
        {
            _savePath = DataLocalProvider.FileJson;
        }
        LoadData();
        CheckDailyBonus();
        StartCoroutine(UpdateTimer());
    }

    private void LoadData()
    {
        try
        {
            if (File.Exists(_savePath))
            {
                string json = File.ReadAllText(_savePath);
                // ����� ���� ������ - ����������� ��������� ����������
                _saveData = JsonConvert.DeserializeObject<SaveData>(json); // ������ SaveData
                print(_saveData.Money);
                print(_saveData.LastCollectDate);
                // ���� ��� ������ ������ � ������ � ������ ���
                if (string.IsNullOrEmpty(_saveData.LastCollectDate))
                {
                    _saveData.LastCollectDate = DateTime.Now.AddDays(-1).ToString();
                    _saveData.CurrentDay = 1;
                    _saveData.CollectedToday = false;
                    SavesData();
                }
                else
                    print(123);
            }
            else
            {
                print("���� �� ������");
                //InitializeNewSaveData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"������ ��� �������� ������: {e.Message}");
            //InitializeNewSaveData();
        }
    }
    private void UpdateButtonState(bool isActive)
    {
        if (_collectButton != null)
        {
            _collectButton.interactable = isActive;

            // ��������� ���� ������
            if (_buttonImage != null)
            {
                _buttonImage.color = isActive ? _activeButtonColor : _inactiveButtonColor;
            }

            // ��������� ����� � ��� ����
            if (_buttonText != null)
            {
                _buttonText.text = isActive ? "COLLECT" : "COLLECTED";
                _buttonText.color = isActive ? _activeTextColor : _inactiveTextColor;
            }
        }
    }
    private void InitializeNewSaveData()
    {
        _saveData = new SaveData
        {
            LastCollectDate = DateTime.Now.AddDays(-1).ToString(),
            CurrentDay = 1,
            CollectedToday = false
            
        };
        SavesData();
    }

    private void SavesData()
    {
        try
        {
            if (_saveData == null)
            {
                Debug.LogError("_saveData is null");
                return;
            }

            string currentJson = File.ReadAllText(_savePath);
            var currentData = JsonConvert.DeserializeObject<SaveData>(currentJson);

            // ��������� ������ ���� ����������� ������
            currentData.LastCollectDate = _saveData.LastCollectDate;
            currentData.CurrentDay = _saveData.CurrentDay;
            currentData.CollectedToday = _saveData.CollectedToday;

            // ���� ���������� ������, ��������� � ��
            currentData.Money = _saveData.Money;
            _wallet.text = _saveData.Money.ToString();
            // ��������� ����������� ������
            string json = JsonConvert.SerializeObject(currentData, Formatting.Indented);
            File.WriteAllText(_savePath, json);
            Debug.Log("������ ������� ���������");
        }
        catch (Exception e)
        {
            Debug.LogError($"������ ��� ���������� ������: {e.Message}");
        }
    }

    private void CheckDailyBonus()
    {
        if (_saveData == null)
        {
            Debug.LogError("_saveData is null � CheckDailyBonus");
            return;
        }
        try
        {
            DateTime lastCollectDate = DateTime.Parse(_saveData.LastCollectDate);
            print(lastCollectDate);
            DateTime now = DateTime.Now;
            TimeSpan timeDifference = now - lastCollectDate;
            print(timeDifference);
            if (timeDifference.TotalHours >= 24 && !_saveData.CollectedToday)
            {
                _canCollect = true;
                if (_collectButton != null)
                    _collectButton.interactable = true;
            }
            else
            {
                _canCollect = false;
                if (_collectButton != null)
                    _collectButton.interactable = false;
            }

            if (timeDifference.TotalHours >= 48)
            {
                _saveData.CurrentDay = 1;
                SavesData();
            }
            print(10);
            UpdateUI();
        }
        catch (Exception e)
        {
            Debug.LogError($"������ � CheckDailyBonus: {e.Message}");
            print(000);
        }
    }

    public void CollectDailyBonus()
    {
        if (!_canCollect) return;

        int bonusAmount = _dailyBonusAmounts[_saveData.CurrentDay - 1];

        // ��������� ���������� �����
        _saveData.Money += bonusAmount;

        // ��������� ������ ������
        _saveData.LastCollectDate = DateTime.Now.ToString();
        _saveData.CollectedToday = true;
        _saveData.CurrentDay = (_saveData.CurrentDay % 7) + 1;
        UpdateButtonState(false);
        _canCollect = false;
        if (_collectButton != null)
            _collectButton.interactable = false;

        SavesData();
        UpdateUI();
        ShowBonusCollectedNotification(bonusAmount);
    }

    private IEnumerator UpdateTimer()
    {
        while (true)
        {
            if (!_canCollect)
            {
                DateTime lastCollectDate = DateTime.Parse(_saveData.LastCollectDate);
                DateTime nextCollectTime = lastCollectDate.AddHours(24);
                TimeSpan timeLeft = nextCollectTime - DateTime.Now;

                if (_nextBonusTimeText != null)
                {
                    _nextBonusTimeText.text = $"��������� ����� �����: {timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
                }
            }
            else
            {
                if (_nextBonusTimeText != null)
                {
                    _nextBonusTimeText.text = "����� ��������!";
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private void UpdateUI()
    {
        if (_currentDayText != null)
        {
            _currentDayText.text = $"���� {_saveData.CurrentDay}/7";
            _moneyDayText.text = $"{ _dailyBonusAmounts[_saveData.CurrentDay - 1]}";
        }
    }

    private void ShowBonusCollectedNotification(int amount)
    {
        Debug.Log($"������� ���������� �����: {amount} �����!");
    }
    public void TransInMainMap()
    {
        TransportFromShop.PlayerPosss = 4;
        SceneTransition.SwitchToScene(scene);
    }
}
