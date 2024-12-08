using UnityEngine;
using static WaveManager;

[System.Serializable]
public class GroupSpawnSettings
{
    [Tooltip("Включить групповой спавн врагов")]
    public bool enableGroupSpawn = false;

    [Tooltip("Количество групп врагов")]
    public int groupCount = 3;

    [Tooltip("Количество ближних врагов в одной группе")]
    public int meleeEnemiesPerGroup = 5;

    [Tooltip("Количество дальних врагов в одной группе")]
    public int rangedEnemiesPerGroup = 3;

    [Tooltip("Задержка между появлением групп (в секундах)")]
    public float delayBetweenGroups = 10f;
}

[System.Serializable]
public class WaveSettings
{
    [Header("Настройки фаз")]
    [Tooltip("С какой фазы начинать эту волну")]
    public PhaseType startingPhase = PhaseType.Defense;

    [Tooltip("Включить фазу защиты")]
    public bool includeDefensePhase = true;
    [Tooltip("Включить фазу улучшений")]
    public bool includeUpgradePhase = true;
    [Tooltip("Включить фазу строительства")]
    public bool includeBuildingPhase = true;

    [Tooltip("Длительность фазы защиты (в секундах)")]
    public float defensePhaseTime = 30f;

    [Tooltip("Длительность фазы улучшений (в секундах)")]
    public float upgradePhaseTime = 20f;

    [Tooltip("Длительность фазы строительства (в секундах)")]
    public float buildingPhaseTime = 15f;

    [Header("Настройки врагов")]
    [Tooltip("Количество ближних врагов в волне")]
    public int meleeEnemyCount = 10;

    [Tooltip("Количество дальних врагов в волне")]
    public int rangedEnemyCount = 5;

    [Header("Множители характеристик ближнего врага")]
    [Tooltip("Множитель здоровья ближнего врага")]
    public float meleeHealthMultiplier = 1f;
    [Tooltip("Множитель скорости ближнего врага")]
    public float meleeSpeedMultiplier = 1f;
    [Tooltip("Множитель скорости атаки ближнего врага")]
    public float meleeAttackSpeedMultiplier = 1f;

    [Header("Множители характеристик дальнего врага")]
    [Tooltip("Множитель здоровья дальего врага")]
    public float rangedHealthMultiplier = 1f;
    [Tooltip("Множитель скорости дальнего врага")]
    public float rangedSpeedMultiplier = 1f;
    [Tooltip("Множитель скорости атаки дальнего врага")]
    public float rangedAttackSpeedMultiplier = 1f;

    [Header("Настройки группового спавна")]
    [Tooltip("Настройки группового появления врагов")]
    public GroupSpawnSettings groupSpawn = new GroupSpawnSettings();
}