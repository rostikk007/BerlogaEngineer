using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class ClickManager : MonoBehaviour
{
    [System.Serializable]
    public class BuildingData
    {
        [Tooltip("Префаб здания для строительства")]
        public GameObject prefab;

        [Tooltip("Название здания")]
        public string name;

        [Header("Стоимость")]
        [Tooltip("Стоимость в монетах")]
        public int coinCost;

        [Tooltip("Стоимость в железе")]
        public int ironCost;
    }

    [Header("Настройки построек")]
    [Tooltip("Массив доступных для строительства зданий")]
    public BuildingData[] buildings;

    [Header("Настройки камеры")]
    [Tooltip("Ссылка на основную камеру")]
    public Camera mainCamera;

    [Header("Настройки анимации строительства")]
    [Tooltip("Префаб капсулы приземления")]
    public GameObject capsulePrefab;

    [Tooltip("Высота появления капсулы")]
    public float capsuleSpawnHeight = 10f;

    [Tooltip("Скорость падения капсулы")]
    public float capsuleFallSpeed = 10f;

    [Tooltip("Время ожидания после приземления")]
    public float landingWaitTime = 0.5f;

    [Tooltip("Длительность исчезновения капсулы")]
    public float capsuleDisappearDuration = 0.15f;

    [Tooltip("Скорость вращения при исчезновении")]
    public float disappearRotationSpeed = 720f;

    [Tooltip("Сила покачивания капсулы")]
    public float wobbleStrength = 0.2f;

    [Tooltip("Частота покачивания капсулы")]
    public float wobbleFrequency = 10f;

    [Header("Настройки отладки")]
    [Tooltip("Показывать отладочные сообщения")]
    public bool showDebugMessages = true;

    [Header("Настройки тегов")]
    [Tooltip("Тег для клеток строительства")]
    public string squareTag = "Square";

    [Tooltip("Тег для сгенерированных объектов")]
    public string generatedObjectTag = "GeneratedObject";

    [Tooltip("Тег для построенных зданий")]
    public string buildingsTag = "Buildings";

    [Tooltip("Имя объекта дерева")]
    public string treeObjectName = "(Clone)";

    [Header("UI элементы")]
    [Tooltip("Кнопки строительства (в том же порядке, что и buildings)")]
    public BuildingButton[] buildingButtons;

    private BuildingData selectedBuilding;
    private BuildingData buildingInProgress;
    private bool isBuildingModeActive = false;
    private bool isConstructing = false;
    private HashSet<Vector3> activeConstructionPoints = new HashSet<Vector3>();

    private struct BuildingConstruction
    {
        public BuildingData buildingType;
        public Vector3 position;
        public GameObject capsule;

        public BuildingConstruction(BuildingData building, Vector3 pos, GameObject cap)
        {
            buildingType = building;
            position = pos;
            capsule = cap;
        }
    }

    private Dictionary<Vector3, BuildingConstruction> activeBuildings = new Dictionary<Vector3, BuildingConstruction>();
    private Dictionary<Vector3, Coroutine> activeConstructions = new Dictionary<Vector3, Coroutine>();

    void Start()
    {
        if (buildings.Length > 0)
        {
            selectedBuilding = buildings[0];
        }

        // Проверяем соответствие массивов
        if (buildingButtons != null && buildingButtons.Length != buildings.Length)
        {
            Debug.LogError("Количество кнопок не соответствует количеству построек!");
        }

        UpdateBuildingPrices();
    }

    void Update()
    {
        if (isBuildingModeActive && selectedBuilding != null)
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                TryPlaceBuilding();
            }
        }
    }

    public void SelectPaseka()
    {
        if (isConstructing) return;
        selectedBuilding = buildings[1];
        if (!isBuildingModeActive)
        {
            ToggleBuildingMode();
        }
    }

    public void SelectWall()
    {
        if (isConstructing) return;
        selectedBuilding = buildings[2];
        if (!isBuildingModeActive)
        {
            ToggleBuildingMode();
        }
    }

    public void SelectTurret()
    {
        if (isConstructing) return;
        selectedBuilding = buildings[3];
        if (!isBuildingModeActive)
        {
            ToggleBuildingMode();
        }
    }

    public void SelectFlamethrower()
    {
        if (isConstructing) return;
        selectedBuilding = buildings[0];
        if (!isBuildingModeActive)
        {
            ToggleBuildingMode();
        }
    }

    public void SelectIronProcessor()
    {
        if (isConstructing) return;
        selectedBuilding = buildings[4]; // Переработчик железа
        if (!isBuildingModeActive)
        {
            ToggleBuildingMode();
        }
    }

    void TryPlaceBuilding()
    {
        if (selectedBuilding == null) return;

        if (GameManager.Instance == null ||
            GameManager.Instance.GetCoins() < selectedBuilding.coinCost ||
            GameManager.Instance.GetIron() < selectedBuilding.ironCost)
        {
            return;
        }

        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePosition, Vector2.zero);

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag(squareTag))
            {
                Vector3 constructionPoint = hit.collider.transform.position;

                if (activeConstructionPoints.Contains(constructionPoint))
                {
                    continue;
                }

                BuildingData buildingToBuild = selectedBuilding;
                bool canPlaceObject = true;
                List<GameObject> objectsToDestroy = new List<GameObject>();
                GameObject existingBuilding = null;

                Collider2D[] colliders = Physics2D.OverlapBoxAll(hit.collider.bounds.center, hit.collider.bounds.size, 0);

                foreach (Collider2D collider in colliders)
                {
                    // Проверяем наличие ИВИ-21 или дерева
                    if (collider.gameObject.name == treeObjectName ||
                        collider.GetComponent<IVI21Antenna>() != null)
                    {
                        canPlaceObject = false;
                        break;
                    }
                    else if (collider.CompareTag(generatedObjectTag))
                    {
                        objectsToDestroy.Add(collider.gameObject);
                    }
                    else if (collider.CompareTag(buildingsTag))
                    {
                        // Проверяем, не является ли существующее здание ИВИ-21
                        if (collider.GetComponent<IVI21Antenna>() != null)
                        {
                            canPlaceObject = false;
                            break;
                        }
                        existingBuilding = collider.gameObject;
                    }
                }

                if (!canPlaceObject)
                {
                    if (showDebugMessages)
                    {
                        Debug.Log("Невозможно построить здание в этом месте: обнаружена антенна ИВИ-21 или дерево");
                    }
                    continue;
                }

                if (GameManager.Instance.GetCoins() >= buildingToBuild.coinCost)
                {
                    activeConstructionPoints.Add(constructionPoint);

                    foreach (var obj in objectsToDestroy)
                    {
                        StartCoroutine(DestroyWithAnimation(obj));
                    }

                    if (existingBuilding != null)
                    {
                        activeBuildings[constructionPoint] = new BuildingConstruction(buildingToBuild, constructionPoint, null);
                        Coroutine constructionCoroutine = StartCoroutine(DestroyAndBuildNew(existingBuilding, constructionPoint, buildingToBuild));
                        activeConstructions[constructionPoint] = constructionCoroutine;
                    }
                    else
                    {
                        activeBuildings[constructionPoint] = new BuildingConstruction(buildingToBuild, constructionPoint, null);
                        Coroutine constructionCoroutine = StartCoroutine(SpawnBuildingWithCapsule(constructionPoint, buildingToBuild));
                        activeConstructions[constructionPoint] = constructionCoroutine;
                    }

                    GameManager.Instance.SpendCoins(buildingToBuild.coinCost);
                    GameManager.Instance.SpendIron(buildingToBuild.ironCost);
                }
            }
        }
    }

    private IEnumerator SpawnBuildingWithCapsule(Vector3 position, BuildingData buildingToBuild)
    {
        GameObject capsule = null;
        AudioSource buildingAudioSource = null;
        GameObject tempAudio = null;

        bool setupSuccess = SetupBuildingComponents(position, ref capsule, ref buildingAudioSource, ref tempAudio);
        if (!setupSuccess)
        {
            CleanupOnFailure(capsule, tempAudio);
            activeConstructions.Remove(position);
            activeBuildings.Remove(position);
            yield break;
        }

        if (buildingAudioSource != null)
            buildingAudioSource.Play();

        if (capsule != null)
        {
            yield return StartCoroutine(CapsuleFallAnimation(capsule, position));
            yield return StartCoroutine(CapsuleDisappearAnimation(capsule));
        }

        GameObject building = Instantiate(buildingToBuild.prefab, position, Quaternion.identity);
        building.tag = buildingsTag;

        ApplyCurrentUpgrades(building);

        BuildingManager.Instance.SetupNewBuilding(building, GetBuildingType(buildingToBuild));

        yield return StartCoroutine(BuildingAppearAnimation(building));

        if (buildingAudioSource != null)
        {
            yield return StartCoroutine(FadeOutAndDestroy(buildingAudioSource));
        }

        activeConstructionPoints.Remove(position);
        activeConstructions.Remove(position);
        activeBuildings.Remove(position);
    }

    private bool SetupBuildingComponents(Vector3 position, ref GameObject capsule, ref AudioSource audioSource, ref GameObject audioObj)
    {
        try
        {
            audioObj = new GameObject("TempBuildingAudio");
            audioObj.transform.position = position;
            audioSource = audioObj.AddComponent<AudioSource>();
            audioSource.clip = AudioManager.Instance.buildingConstructionSound;
            audioSource.loop = true;
            audioSource.volume = AudioManager.Instance.constructionSoundsVolume;
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 15f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;

            Vector3 capsuleStartPosition = position + Vector3.up * capsuleSpawnHeight;
            capsule = Instantiate(capsulePrefab, capsuleStartPosition, Quaternion.identity);

            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    private void CleanupOnFailure(GameObject capsule, GameObject audioObject)
    {
        if (capsule != null) Destroy(capsule);
        if (audioObject != null) Destroy(audioObject);
    }

    private IEnumerator CapsuleFallAnimation(GameObject capsule, Vector3 targetPosition)
    {
        if (capsule == null) yield break;

        Vector3 startPosition = capsule.transform.position;
        float elapsedTime = 0;
        float fallDuration = capsuleSpawnHeight / capsuleFallSpeed;

        while (elapsedTime < fallDuration && capsule != null)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fallDuration;

            float wobble = Mathf.Sin(progress * wobbleFrequency) * wobbleStrength;
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, progress);
            currentPos.x += wobble;

            capsule.transform.position = currentPos;
            capsule.transform.Rotate(Vector3.forward * Time.deltaTime * 360);

            yield return null;
        }

        if (capsule != null)
        {
            capsule.transform.position = targetPosition;
            yield return new WaitForSeconds(landingWaitTime);
        }
    }

    private IEnumerator CapsuleDisappearAnimation(GameObject capsule)
    {
        if (capsule == null) yield break;

        SpriteRenderer capsuleRenderer = capsule.GetComponent<SpriteRenderer>();
        Vector3 startScale = capsule.transform.localScale;
        float elapsedTime = 0;

        while (elapsedTime < capsuleDisappearDuration && capsule != null)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / capsuleDisappearDuration;
            float easedProgress = 1 - Mathf.Pow(1 - progress, 2);

            capsule.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, easedProgress);
            capsule.transform.Rotate(Vector3.forward * Time.deltaTime * disappearRotationSpeed);

            if (capsuleRenderer != null)
            {
                Color color = capsuleRenderer.color;
                color.a = 1 - easedProgress;
                capsuleRenderer.color = color;
            }

            yield return null;
        }

        if (capsule != null)
        {
            Destroy(capsule);
        }
    }

    private IEnumerator BuildingAppearAnimation(GameObject building)
    {
        if (building == null) yield break;

        building.transform.localScale = Vector3.zero;
        float elapsedTime = 0;
        float duration = 0.5f;

        while (elapsedTime < duration && building != null)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            float smoothProgress = 1f - Mathf.Cos(progress * Mathf.PI * 0.5f);

            building.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, smoothProgress);

            yield return null;
        }

        if (building != null)
        {
            building.transform.localScale = Vector3.one;
        }
    }

    private IEnumerator FadeOutAndDestroy(AudioSource audioSource)
    {
        if (audioSource == null) yield break;

        float startVolume = audioSource.volume;
        float fadeDuration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration && audioSource != null)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / fadeDuration;
            float volumeMultiplier = 1f - Mathf.SmoothStep(0f, 1f, normalizedTime);
            audioSource.volume = startVolume * volumeMultiplier;
            yield return null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            Destroy(audioSource.gameObject);
        }
    }

    private IEnumerator DestroyWithAnimation(GameObject objectToDestroy)
    {
        if (objectToDestroy == null) yield break;

        Vector3 originalScale = objectToDestroy.transform.localScale;
        float duration = 1f;
        float elapsed = 0f;
        bool isDestroyed = false;

        try
        {
            while (elapsed < duration && !isDestroyed)
            {
                if (objectToDestroy == null)
                {
                    isDestroyed = true;
                    break;
                }

                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                objectToDestroy.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, progress);
                objectToDestroy.transform.Rotate(0, 0, 360 * Time.deltaTime);

                yield return null;
            }
        }
        finally
        {
            if (!isDestroyed && objectToDestroy != null)
            {
                Destroy(objectToDestroy);
            }
        }
    }

    private IEnumerator DestroyAndBuildNew(GameObject existingBuilding, Vector3 position, BuildingData buildingToBuild)
    {
        if (existingBuilding == null) yield break;

        float destroyDuration = 0.5f;
        float elapsedTime = 0;
        Vector3 originalScale = existingBuilding.transform.localScale;

        while (elapsedTime < destroyDuration && existingBuilding != null)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / destroyDuration;

            existingBuilding.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, progress);
            existingBuilding.transform.Rotate(0, 0, 360 * Time.deltaTime);

            yield return null;
        }

        if (existingBuilding != null)
        {
            Destroy(existingBuilding);
        }

        yield return new WaitForSeconds(0.2f);

        if (GameManager.Instance.GetCoins() >= buildingToBuild.coinCost)
        {
            GameManager.Instance.SpendCoins(buildingToBuild.coinCost);
            StartCoroutine(SpawnBuildingWithCapsule(position, buildingToBuild));
        }

        activeConstructionPoints.Remove(position);
        activeBuildings.Remove(position);
    }

    public void ToggleBuildingMode()
    {
        isBuildingModeActive = !isBuildingModeActive;
        if (showDebugMessages)
        {
            Debug.Log($"Режим строительства {(isBuildingModeActive ? "включен" : "выключен")}");
        }
    }

    public bool IsBuildingModeActive()
    {
        return isBuildingModeActive;
    }

    public string CurrentBuildingName =>
        isConstructing ? buildingInProgress?.name ?? "" : selectedBuilding?.name ?? "";

    public int GetCurrentBuildingCoinCost()
    {
        return isConstructing ? buildingInProgress?.coinCost ?? 0 : selectedBuilding?.coinCost ?? 0;
    }

    public int GetCurrentBuildingIronCost()
    {
        return isConstructing ? buildingInProgress?.ironCost ?? 0 : selectedBuilding?.ironCost ?? 0;
    }

    private UpgradeBranch.BuildingType GetBuildingType(BuildingData data)
    {
        // Определяем тип постройки по префабу
        if (data.prefab.GetComponent<Turret>() != null)
            return UpgradeBranch.BuildingType.BasicTurret;
        if (data.prefab.GetComponent<Barricade>() != null)
            return UpgradeBranch.BuildingType.Wall;
        if (data.prefab.GetComponent<Flamethrower>() != null)
            return UpgradeBranch.BuildingType.FlameTurret;

        return UpgradeBranch.BuildingType.BasicTurret; // По умолчанию
    }

    private void ApplyCurrentUpgrades(GameObject building)
    {
        // Определяем тип постройки
        IBuildingStats buildingStats = building.GetComponent<IBuildingStats>();
        if (buildingStats == null) return;

        UpgradeBranch.BuildingType buildingType;

        if (building.GetComponent<Turret>() != null)
            buildingType = UpgradeBranch.BuildingType.BasicTurret;
        else if (building.GetComponent<Barricade>() != null)
            buildingType = UpgradeBranch.BuildingType.Wall;
        else if (building.GetComponent<Flamethrower>() != null)
            buildingType = UpgradeBranch.BuildingType.FlameTurret;
        else
            return;

        // Получаем текущий уровень улучшений для данного типа постройки
        int currentLevel = BuildingManager.Instance.GetBuildingLevel(buildingType);

        // Если есть улучшения, применяем их
        if (currentLevel > 0)
        {
            LevelStats stats = BuildingUpgradeStats.Instance.GetBuildingStats(buildingType, currentLevel);
            buildingStats.UpdateStats(stats);
        }
    }

    private void UpdateBuildingPrices()
    {
        if (buildingButtons == null) return;

        for (int i = 0; i < buildingButtons.Length && i < buildings.Length; i++)
        {
            if (buildingButtons[i] != null)
            {
                buildingButtons[i].UpdatePriceDisplay(
                    buildings[i].coinCost,
                    buildings[i].ironCost
                );
            }
        }
    }
}