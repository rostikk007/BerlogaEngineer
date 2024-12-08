using UnityEngine;

public class BuildingUpgradeStats : MonoBehaviour
{
    [Header("Настройки улучшений турели")]
    public LevelStats[] turretLevelStats = new LevelStats[3] {
        new LevelStats { damage = 10, health = 100, attackSpeed = 1.0f, range = 5 },   // Уровень 1
        new LevelStats { damage = 20, health = 150, attackSpeed = 1.2f, range = 6 },   // Уровень 2
        new LevelStats { damage = 35, health = 200, attackSpeed = 1.5f, range = 7 }    // Уровень 3
    };

    [Header("Настройки улучшений огнемёта")]
    public LevelStats[] flamethrowerLevelStats = new LevelStats[3] {
        new LevelStats { damage = 0.5f, health = 100, attackSpeed = 1.0f, range = 3 }, // Уровень 1
        new LevelStats { damage = 1.0f, health = 150, attackSpeed = 1.2f, range = 4 }, // Уровень 2
        new LevelStats { damage = 1.5f, health = 200, attackSpeed = 1.5f, range = 5 }  // Уровень 3
    };

    [Tooltip("Характеристики стены для каждого уровня")]
    public LevelStats[] barricadeLevelStats = new LevelStats[3];

    private static BuildingUpgradeStats instance;
    public static BuildingUpgradeStats Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<BuildingUpgradeStats>();
            }
            return instance;
        }
    }

    public LevelStats GetBuildingStats(UpgradeBranch.BuildingType type, int level)
    {
        if (level < 0 || level >= 3)
        {
            Debug.LogError($"Некорректный уровень: {level}");
            return null;
        }

        switch (type)
        {
            case UpgradeBranch.BuildingType.BasicTurret:
                return turretLevelStats[level];
            case UpgradeBranch.BuildingType.Wall:
                return barricadeLevelStats[level];
            case UpgradeBranch.BuildingType.FlameTurret:
                return flamethrowerLevelStats[level];
            default:
                Debug.LogError($"Неизвестный тип постройки: {type}");
                return null;
        }
    }
}