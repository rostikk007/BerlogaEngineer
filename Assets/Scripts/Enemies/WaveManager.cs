using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class WaveManager : MonoBehaviour
{
    [Header("Настройки волн")]
    [Tooltip("Настройки для каждой волны")]
    public WaveSettings[] waveSettings;

    [Header("Настройки начальной фазы")]
    [Tooltip("С какой фазы начинать каждую волну")]
    public PhaseType startingPhase = PhaseType.Defense;

    [Header("Статистика текущей волны")]
    [SerializeField, ReadOnly]
    private int currentWaveNumber;
    [SerializeField, ReadOnly]
    private string currentPhaseText;
    [SerializeField, ReadOnly]
    private float timeUntilNextPhase;
    [SerializeField, ReadOnly]
    private int meleeEnemiesSpawned;
    [SerializeField, ReadOnly]
    private int rangedEnemiesSpawned;

    [Header("Характеристики врагов в текущей волне")]
    [SerializeField, ReadOnly]
    private float meleeEnemyHealth;
    [SerializeField, ReadOnly]
    private float meleeEnemySpeed;
    [SerializeField, ReadOnly]
    private float meleeEnemyAttackSpeed;
    [SerializeField, ReadOnly]
    private float rangedEnemyHealth;
    [SerializeField, ReadOnly]
    private float rangedEnemySpeed;
    [SerializeField, ReadOnly]
    private float rangedEnemyAttackSpeed;

    [Header("Настройки спавна")]
    [Tooltip("Префаб ближнего врага")]
    public GameObject meleeEnemyPrefab;
    [Tooltip("Префаб дальнего врага")]
    public GameObject rangedEnemyPrefab;
    [Tooltip("Расстояние спавна от границ карты")]
    public float spawnDistance = 5f;
    [Tooltip("Минимальный интервал между спавном врагов")]
    public float minSpawnInterval = 1f;
    [Tooltip("Максимальный интервал между спавном врагов")]
    public float maxSpawnInterval = 3f;

    [Header("Настройки усиления врагов")]
    [Tooltip("Множитель увеличния здоровья с каждой волной")]
    public float healthMultiplier = 1.2f;
    [Tooltip("Множитель увеличения скорости с каждой волной")]
    public float speedMultiplier = 1.1f;
    [Tooltip("Множитель увеличения скорости атаки с каж волной")]
    public float attackSpeedMultiplier = 1.15f;

    [Header("UI элементы")]
    [Tooltip("Текст для отображения информации о текущей фазе")]
    public TextMeshProUGUI phaseInfoText;

    public enum PhaseType { Defense, Upgrade, Building }
    private PhaseType currentPhase;
    private int currentWave = 0;
    private float phaseTimer;
    private bool isWaveActive = false;
    private Camera mainCamera;

    // Добавляем делегат для события
    public delegate void PhaseChangedHandler(PhaseType newPhase);
    public event PhaseChangedHandler OnPhaseChanged;

    // Добавляем приватное поле для хранения общего количества волн
    private int totalWaves;

    private HashSet<PhaseType> completedPhases = new HashSet<PhaseType>();

    void Start()
    {
        mainCamera = Camera.main;

        // Проверяем настройки волн
        if (waveSettings == null || waveSettings.Length == 0)
        {
            Debug.LogError("Не настроены параметры волн!");
            return;
        }

        totalWaves = waveSettings.Length;
        StartNextWave();
    }

    void Update()
    {
        if (isWaveActive)
        {
            phaseTimer -= Time.deltaTime;
            UpdateInspectorInfo();
            UpdatePhaseInfoText();

            if (phaseTimer <= 0)
            {
                SwitchToNextPhase();
            }
        }
    }

    private void StartNextWave()
    {
        currentWave++;
        if (currentWave <= totalWaves)
        {
            completedPhases.Clear();

            WaveSettings currentSettings = waveSettings[currentWave - 1];
            currentPhase = currentSettings.startingPhase;

            switch (currentSettings.startingPhase)
            {
                case PhaseType.Defense:
                    phaseTimer = currentSettings.defensePhaseTime;
                    StartCoroutine(SpawnEnemies());
                    break;
                case PhaseType.Upgrade:
                    phaseTimer = currentSettings.upgradePhaseTime;
                    break;
                case PhaseType.Building:
                    phaseTimer = currentSettings.buildingPhaseTime;
                    break;
            }

            isWaveActive = true;
            OnPhaseChanged?.Invoke(currentPhase);
        }
    }

    private void SwitchToNextPhase()
    {
        WaveSettings currentSettings = waveSettings[currentWave - 1];
        PhaseType nextPhase = currentPhase;
        bool shouldSpawnEnemies = false;

        completedPhases.Add(currentPhase);

        bool allPhasesCompleted =
            (!currentSettings.includeDefensePhase || completedPhases.Contains(PhaseType.Defense)) &&
            (!currentSettings.includeUpgradePhase || completedPhases.Contains(PhaseType.Upgrade)) &&
            (!currentSettings.includeBuildingPhase || completedPhases.Contains(PhaseType.Building));

        if (allPhasesCompleted)
        {
            if (currentWave < totalWaves)
            {
                StartNextWave();
                return;
            }
            else
            {
                isWaveActive = false;
                return;
            }
        }

        switch (currentPhase)
        {
            case PhaseType.Defense:
                if (currentSettings.includeUpgradePhase)
                {
                    nextPhase = PhaseType.Upgrade;
                    phaseTimer = currentSettings.upgradePhaseTime;
                }
                else if (currentSettings.includeBuildingPhase)
                {
                    nextPhase = PhaseType.Building;
                    phaseTimer = currentSettings.buildingPhaseTime;
                }
                break;

            case PhaseType.Upgrade:
                if (currentSettings.includeBuildingPhase)
                {
                    nextPhase = PhaseType.Building;
                    phaseTimer = currentSettings.buildingPhaseTime;
                }
                else if (currentSettings.includeDefensePhase)
                {
                    nextPhase = PhaseType.Defense;
                    phaseTimer = currentSettings.defensePhaseTime;
                    shouldSpawnEnemies = true;
                }
                break;

            case PhaseType.Building:
                if (currentSettings.includeDefensePhase)
                {
                    nextPhase = PhaseType.Defense;
                    phaseTimer = currentSettings.defensePhaseTime;
                    shouldSpawnEnemies = true;
                }
                else if (currentSettings.includeUpgradePhase)
                {
                    nextPhase = PhaseType.Upgrade;
                    phaseTimer = currentSettings.upgradePhaseTime;
                }
                break;
        }

        if (nextPhase != currentPhase)
        {
            currentPhase = nextPhase;
            OnPhaseChanged?.Invoke(currentPhase);
            UpdatePhaseInfoText();

            if (shouldSpawnEnemies)
            {
                StartCoroutine(SpawnEnemies());
            }
        }
    }

    private IEnumerator SpawnEnemies()
    {
        meleeEnemiesSpawned = 0;
        rangedEnemiesSpawned = 0;

        WaveSettings currentWaveSettings = waveSettings[currentWave - 1];

        if (currentWaveSettings.groupSpawn.enableGroupSpawn)
        {
            // Групповой спавн
            for (int group = 0; group < currentWaveSettings.groupSpawn.groupCount; group++)
            {
                Vector2 groupPosition = GetRandomSpawnPosition();

                // Спавним ближних врагов группы
                for (int i = 0; i < currentWaveSettings.groupSpawn.meleeEnemiesPerGroup; i++)
                {
                    Vector2 offset = Random.insideUnitCircle * 2f; // Разброс в группе
                    GameObject enemy = Instantiate(meleeEnemyPrefab, groupPosition + offset, Quaternion.identity);
                    ApplyWaveModifiers(enemy, currentWaveSettings);
                    meleeEnemiesSpawned++;
                }

                // Спавним дальних врагов группы
                for (int i = 0; i < currentWaveSettings.groupSpawn.rangedEnemiesPerGroup; i++)
                {
                    Vector2 offset = Random.insideUnitCircle * 3f; // Больший разброс для дальних
                    GameObject enemy = Instantiate(rangedEnemyPrefab, groupPosition + offset, Quaternion.identity);
                    ApplyWaveModifiers(enemy, currentWaveSettings);
                    rangedEnemiesSpawned++;
                }

                if (group < currentWaveSettings.groupSpawn.groupCount - 1)
                {
                    yield return new WaitForSeconds(currentWaveSettings.groupSpawn.delayBetweenGroups);
                }
            }
        }
        else
        {
            // Обычный спавн по одному
            int totalMeleeToSpawn = currentWaveSettings.meleeEnemyCount;
            int totalRangedToSpawn = currentWaveSettings.rangedEnemyCount;

            while (currentPhase == PhaseType.Defense &&
                  (meleeEnemiesSpawned < totalMeleeToSpawn ||
                   rangedEnemiesSpawned < totalRangedToSpawn))
            {
                // Определяем, какого вага спавнить
                bool canSpawnMelee = meleeEnemiesSpawned < totalMeleeToSpawn;
                bool canSpawnRanged = rangedEnemiesSpawned < totalRangedToSpawn;

                bool spawnRanged = false;
                if (canSpawnMelee && canSpawnRanged)
                {
                    spawnRanged = Random.value > 0.7f;
                }
                else
                {
                    spawnRanged = !canSpawnMelee;
                }

                Vector2 spawnPosition = GetRandomSpawnPosition();
                GameObject enemyPrefab = spawnRanged ? rangedEnemyPrefab : meleeEnemyPrefab;

                GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

                // Увеличиваем счетчики
                if (spawnRanged)
                    rangedEnemiesSpawned++;
                else
                    meleeEnemiesSpawned++;

                ApplyWaveModifiers(enemy, currentWaveSettings);

                float spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        // Ждем, пока все враги будут уничтожены
        while (GameObject.FindGameObjectsWithTag("Enemy").Length > 0)
        {
            yield return new WaitForSeconds(1f);
        }

        SwitchToNextPhase();
    }

    private Vector2 GetRandomSpawnPosition()
    {
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        // Добавляем дополнительное расстояние для гарантированного спавна за пределами видимости
        float extraDistance = spawnDistance + Mathf.Max(cameraWidth, cameraHeight);

        // Выбираем случайную сторону для спавна (0-верх, 1-право, 2-низ, 3-лево)
        int side = Random.Range(0, 4);
        Vector2 position = Vector2.zero;

        switch (side)
        {
            case 0: // Верх
                position = new Vector2(
                    Random.Range(-cameraWidth, cameraWidth),
                    extraDistance
                );
                break;
            case 1: // Право
                position = new Vector2(
                    extraDistance,
                    Random.Range(-cameraHeight, cameraHeight)
                );
                break;
            case 2: // Низ
                position = new Vector2(
                    Random.Range(-cameraWidth, cameraWidth),
                    -extraDistance
                );
                break;
            case 3: // Лево
                position = new Vector2(
                    -extraDistance,
                    Random.Range(-cameraHeight, cameraHeight)
                );
                break;
        }

        return position;
    }

    private void ApplyWaveModifiers(GameObject enemy, WaveSettings settings)
    {
        if (enemy.TryGetComponent<MeleeEnemy>(out var meleeEnemy))
        {
            meleeEnemy.maxHealth *= settings.meleeHealthMultiplier;
            meleeEnemy.moveSpeed *= settings.meleeSpeedMultiplier;
            meleeEnemy.attackCooldown /= settings.meleeAttackSpeedMultiplier;
        }
        else if (enemy.TryGetComponent<RangedEnemy>(out var rangedEnemy))
        {
            rangedEnemy.maxHealth *= settings.rangedHealthMultiplier;
            rangedEnemy.moveSpeed *= settings.rangedSpeedMultiplier;
            rangedEnemy.attackCooldown /= settings.rangedAttackSpeedMultiplier;
        }
    }

    private void UpdateInspectorInfo()
    {
        currentWaveNumber = currentWave;
        currentPhaseText = currentPhase.ToString();
        timeUntilNextPhase = phaseTimer;

        // Добавляем нформацию о максимальном времени текущей фазы
        float maxPhaseTime = GetCurrentPhaseMaxTime();
        currentPhaseText += $" ({Mathf.CeilToInt(phaseTimer)}/{Mathf.CeilToInt(maxPhaseTime)} сек)";

        if (currentWave > 0 && currentWave <= waveSettings.Length)
        {
            WaveSettings settings = waveSettings[currentWave - 1];

            // Характеристики ближнего боя
            if (meleeEnemyPrefab != null)
            {
                var meleeEnemy = meleeEnemyPrefab.GetComponent<MeleeEnemy>();
                if (meleeEnemy != null)
                {
                    meleeEnemyHealth = meleeEnemy.maxHealth * settings.meleeHealthMultiplier;
                    meleeEnemySpeed = meleeEnemy.moveSpeed * settings.meleeSpeedMultiplier;
                    meleeEnemyAttackSpeed = meleeEnemy.attackCooldown / settings.meleeAttackSpeedMultiplier;
                }
            }

            // Характеристики дальнего боя
            if (rangedEnemyPrefab != null)
            {
                var rangedEnemy = rangedEnemyPrefab.GetComponent<RangedEnemy>();
                if (rangedEnemy != null)
                {
                    rangedEnemyHealth = rangedEnemy.maxHealth * settings.rangedHealthMultiplier;
                    rangedEnemySpeed = rangedEnemy.moveSpeed * settings.rangedSpeedMultiplier;
                    rangedEnemyAttackSpeed = rangedEnemy.attackCooldown / settings.rangedAttackSpeedMultiplier;
                }
            }
        }
    }

    private void UpdatePhaseInfoText()
    {
        if (phaseInfoText != null)
        {
            string phaseName = "";
            switch (currentPhase)
            {
                case PhaseType.Defense:
                    phaseName = "Защита";
                    break;
                case PhaseType.Upgrade:
                    phaseName = "Улучшения";
                    break;
                case PhaseType.Building:
                    phaseName = "Строительство";
                    break;
            }

            float timeLeft = Mathf.CeilToInt(phaseTimer);
            phaseInfoText.text = $"Фаза: {phaseName}\nОсталось времени: {timeLeft} сек.";
        }
    }

    // Публичные методы для получения информации о текущем состоянии волны
    public int GetCurrentWave() => currentWave;
    public PhaseType GetCurrentPhase() => currentPhase;
    public float GetPhaseTimeRemaining() => phaseTimer;

    // Добавим метод для получения информации о длительности текущей фазы
    public float GetCurrentPhaseMaxTime()
    {
        if (currentWave <= 0 || currentWave > waveSettings.Length) return 0;

        WaveSettings currentSettings = waveSettings[currentWave - 1];
        switch (currentPhase)
        {
            case PhaseType.Defense:
                return currentSettings.defensePhaseTime;
            case PhaseType.Upgrade:
                return currentSettings.upgradePhaseTime;
            case PhaseType.Building:
                return currentSettings.buildingPhaseTime;
            default:
                return 0;
        }
    }
}
