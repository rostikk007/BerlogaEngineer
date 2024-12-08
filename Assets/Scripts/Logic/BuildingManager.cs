using UnityEngine;
using System.Collections.Generic;

public class BuildingManager : MonoBehaviour
{
    // Синглтон
    public static BuildingManager Instance { get; private set; }

    [System.Serializable]
    public class BuildingSprites
    {
        public Sprite level2Sprite; // Спрайт для 2 уровня
        public Sprite level3Sprite; // Спрайт для 3 уровня
    }

    [Header("Спрайты оснований")]
    [Tooltip("Спрайты для турели")]
    public BuildingSprites turretSprites;

    [Tooltip("Спрайты для стены")]
    public BuildingSprites wallSprites;

    [Tooltip("Спрайты для огнемёта")]
    public BuildingSprites flamethrowerSprites;

    // Словари для хранения уровней прокачки
    private Dictionary<UpgradeBranch.BuildingType, int> buildingLevels = new Dictionary<UpgradeBranch.BuildingType, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeLevels();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeLevels()
    {
        buildingLevels[UpgradeBranch.BuildingType.BasicTurret] = 0;
        buildingLevels[UpgradeBranch.BuildingType.Wall] = 0;
        buildingLevels[UpgradeBranch.BuildingType.FlameTurret] = 0;
    }

    // Обновляет уровень постройки определенного типа
    public void UpdateBuildingLevel(UpgradeBranch.BuildingType type, int level)
    {
        buildingLevels[type] = level;
        UpdateAllBuildingsOfType(type);
    }

    // Получает текущий уровень постройки
    public int GetBuildingLevel(UpgradeBranch.BuildingType type)
    {
        return buildingLevels[type];
    }

    // Обновляет спрайт для новой постройки
    public void SetupNewBuilding(GameObject building, UpgradeBranch.BuildingType type)
    {
        Transform foundation = building.transform.Find("Основание турели (1)");
        if (foundation != null)
        {
            SpriteRenderer spriteRenderer = foundation.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                UpdateBuildingSprite(spriteRenderer, type, buildingLevels[type]);
            }
        }
    }

    // Обновляет все существующие постройки определенного типа
    private void UpdateAllBuildingsOfType(UpgradeBranch.BuildingType type)
    {
        var allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject building in allObjects)
        {
            // Проверяем тип постройки по компоненту
            bool isCorrectType = false;
            switch (type)
            {
                case UpgradeBranch.BuildingType.BasicTurret:
                    isCorrectType = building.GetComponent<Turret>() != null;
                    break;
                case UpgradeBranch.BuildingType.Wall:
                    isCorrectType = building.GetComponent<Barricade>() != null;
                    break;
                case UpgradeBranch.BuildingType.FlameTurret:
                    isCorrectType = building.GetComponent<Flamethrower>() != null;
                    break;
            }

            if (isCorrectType)
            {
                Transform foundation = building.transform.Find("Основание турели (1)");
                if (foundation != null)
                {
                    SpriteRenderer spriteRenderer = foundation.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        UpdateBuildingSprite(spriteRenderer, type, buildingLevels[type]);
                    }
                }
            }
        }
    }

    private void UpdateBuildingSprite(SpriteRenderer renderer, UpgradeBranch.BuildingType type, int level)
    {
        // Меняем спрайт только для 2 и 3 уровня
        BuildingSprites sprites = null;
        switch (type)
        {
            case UpgradeBranch.BuildingType.BasicTurret:
                sprites = turretSprites;
                break;
            case UpgradeBranch.BuildingType.Wall:
                sprites = wallSprites;
                break;
            case UpgradeBranch.BuildingType.FlameTurret:
                sprites = flamethrowerSprites;
                break;
        }

        if (sprites != null)
        {
            if (level == 2)
            {
                renderer.sprite = sprites.level2Sprite;
            }
            else if (level == 3)
            {
                renderer.sprite = sprites.level3Sprite;
            }
        }
    }
}