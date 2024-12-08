using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeBranch : MonoBehaviour
{
    [Header("Настройки ветки улучшений")]
    [Tooltip("Тип постройки для улучшения")]
    public BuildingType buildingType;

    [Tooltip("Массив заданий для этой ветки")]
    public MissionData[] missions;

    [Tooltip("Текущий уровень прогресса (от 0 до 2)")]
    private int currentProgress = 0;

    [Header("UI элементы")]
    [Tooltip("Иконка постройки")]
    public Image buildingIcon;

    [Tooltip("Текст прогресса")]
    public Text progressText;

    public enum BuildingType
    {
        BasicTurret,
        Wall,
        FlameTurret
    }

    private void Start()
    {
        InitializeComponent();
        UpdateProgressDisplay();
    }

    private void InitializeComponent()
    {
        if (missions == null || missions.Length != 3)
        {
            Debug.LogError($"Ошибка: для {buildingType} должно быть настроено ровно 3 задания!");
        }

        if (buildingIcon == null)
        {
            Debug.LogWarning($"Предупреждение: для {buildingType} не назначена иконка!");
        }

        if (progressText == null)
        {
            Debug.LogWarning($"Предупреждение: для {buildingType} не назначен текст прогресса!");
        }

        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnBranchButtonClick);
        }
    }

    private void OnBranchButtonClick()
    {
        MissionsManager missionsManager = FindObjectOfType<MissionsManager>();
        if (missionsManager != null)
        {
            missionsManager.ShowMission(this);
        }
        else
        {
            Debug.LogError("Не найден MissionsManager на сцене!");
        }
    }

    public bool CanShowNextMission()
    {
        return currentProgress < missions.Length;
    }

    public MissionData GetCurrentMission()
    {
        if (CanShowNextMission())
        {
            return missions[currentProgress];
        }
        return null;
    }

    public void OnMissionCompleted()
    {
        if (currentProgress < missions.Length)
        {
            currentProgress++;
            UpdateProgressDisplay();
            ApplyUpgrade();
        }
    }

    private void UpdateProgressDisplay()
    {
        if (progressText != null)
        {
            progressText.text = $"Прогресс: {currentProgress}/3";
        }

        if (buildingIcon != null)
        {
            Color iconColor = buildingIcon.color;
            iconColor.a = currentProgress == missions.Length ? 1f : 0.5f;
            buildingIcon.color = iconColor;
        }
    }

    private void ApplyUpgrade()
    {
        if (currentProgress <= 0) return;

        // Получаем новые характеристики для текущего уровня
        LevelStats newStats = BuildingUpgradeStats.Instance.GetBuildingStats(buildingType, currentProgress);

        // Применяем новые характеристики ко всем существующим постройкам
        var buildings = FindObjectsOfType<MonoBehaviour>().OfType<IBuildingStats>();
        foreach (var building in buildings)
        {
            // Проверяем тип постройки перед обновлением
            switch (buildingType)
            {
                case BuildingType.BasicTurret when building is Turret:
                case BuildingType.Wall when building is Barricade:
                case BuildingType.FlameTurret when building is Flamethrower:
                    building.UpdateStats(newStats);
                    break;
            }
        }

        BuildingManager.Instance.UpdateBuildingLevel(buildingType, currentProgress);

        Debug.Log($"Применено улучшение {buildingType} уровня {currentProgress}");
    }

    private void OnDestroy()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveListener(OnBranchButtonClick);
        }
    }
}