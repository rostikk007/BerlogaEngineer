using UnityEngine;
using System.Collections.Generic;
public class RandomLevelGenerator : MonoBehaviour
{
    [Header("Настройки генерации")]
    [Tooltip("Ширина карты")]
    public float mapWidth = 48f;
    [Tooltip("Высота карты")]
    public float mapHeight = 50f;
    [Tooltip("Расстояние между объектами")]
    public float spacing = 0.5f;
    [Tooltip("Тег для сгенерированных объектов")]
    public string generatedObjectTag = "GeneratedObject";
    [Tooltip("Родительский объект для сгенерированных объектов")]
    public GameObject generatedObjectsParent;

    [Header("Настройки камней")]
    [Tooltip("Включить генерацию камней")]
    public bool generateStones = true;
    [Tooltip("Префабы камней")]
    public GameObject[] stonePrefabs_LocationForest;
    [Tooltip("Количество камней")]
    public int numberOfStones_LocationForest = 15;

    [Header("Настройки травы")]
    [Tooltip("Включить генерацию травы")]
    public bool generateGrass = true;
    [Tooltip("Префабы травы")]
    public GameObject[] grassPrefabs_LocationForest;
    [Tooltip("Количество травы")]
    public int numberOfGrass_LocationForest = 20;

    [Header("Настройки веток")]
    [Tooltip("Включить генерацию веток")]
    public bool generateVetki = true;
    [Tooltip("Префабы веток")]
    public GameObject[] VetkiPrefabs_LocationForest;
    [Tooltip("Количество веток")]
    public int numberOfVetki_LocationForest = 500;

    [Header("Настройки кочек")]
    [Tooltip("Включить генерацию кочек")]
    public bool generateKocki = true;
    [Tooltip("Префабы кочек")]
    public GameObject[] KockiPrefabs_LocationForest;
    [Tooltip("Количество кочек")]
    public int numberOfKocki = 2000;

    private List<Vector2> occupiedPositions = new List<Vector2>();

    void Start()
    {
        GenerateLevel();
        MoveObjectsToParent();
    }

    void GenerateLevel()
    {
        if (generateStones)
            PlaceRandomObjects(stonePrefabs_LocationForest, numberOfStones_LocationForest);
        
        if (generateGrass)
            PlaceRandomObjectsBezPovorota(grassPrefabs_LocationForest, numberOfGrass_LocationForest);
        
        if (generateVetki)
            PlaceRandomObjectsVetki(VetkiPrefabs_LocationForest, numberOfVetki_LocationForest);
        
        if (generateKocki)
            PlaceRandomObjectsKocki(KockiPrefabs_LocationForest, numberOfKocki);
    }

    void PlaceRandomObjectsVetki(GameObject[] prefabs, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 position;
            GameObject prefab;

            do
            {
                position = GetRandomPosition();
                prefab = prefabs[Random.Range(0, prefabs.Length)];
            } while (IsPositionOccupied(position, prefab));

            float randomScale = Random.Range(0.3f, 0.6f); // Измените диапазон по вашему усмотрению
            Vector3 scale = new Vector3(randomScale, randomScale, randomScale);
            float randomRotationZ = Random.Range(0f, 360f);
            Quaternion rotation = Quaternion.Euler(0f, 0f, randomRotationZ);
            GameObject obj = Instantiate(prefab, position, rotation);
            obj.transform.localScale = scale; // Применение случайного масштаба
            obj.tag = generatedObjectTag;
        }
    }
    void PlaceRandomObjectsKocki(GameObject[] prefabs, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 position;
            GameObject prefab;

            do
            {
                position = GetRandomPosition();
                prefab = prefabs[Random.Range(0, prefabs.Length)];
            } while (IsPositionOccupied(position, prefab));

           
       
            float randomRotationZ = Random.Range(0f, 360f);
            Quaternion rotation = Quaternion.Euler(0f, 0f, randomRotationZ);
            GameObject obj = Instantiate(prefab, position, rotation);
          
        }
    }
    void PlaceObjects(GameObject prefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 position;
            do
            {
                position = GetRandomPosition();
            } while (IsPositionOccupied(position, prefab));
            Instantiate(prefab, position, Quaternion.identity);
        }
    }

    void PlaceRandomObjectsBezPovorota(GameObject[] prefabs, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 position;
            GameObject prefab;

            do
            {
                position = GetRandomPosition();
                prefab = prefabs[Random.Range(0, prefabs.Length)];
            } while (IsPositionOccupied(position, prefab));

            float randomScale = Random.Range(0.8f, 1.2f); // Измените диапазон по вашему усмотрению
            Vector3 scale = new Vector3(randomScale, randomScale, randomScale);
            GameObject obj = Instantiate(prefab, position, Quaternion.identity);
            obj.transform.localScale = scale; // Применение случайного масштаба
            obj.tag = generatedObjectTag;
        }
    }
    void PlaceRandomObjects(GameObject[] prefabs, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 position;
            GameObject prefab;

            do
            {
                position = GetRandomPosition();
                prefab = prefabs[Random.Range(0, prefabs.Length)];
            } while (IsPositionOccupied(position, prefab));

            float randomScale = Random.Range(0.5f, 1.5f); // Измените диапазон по вашему усмотрению
            Vector3 scale = new Vector3(randomScale, randomScale, randomScale);
            float randomRotationZ = Random.Range(0f, 360f);
            Quaternion rotation = Quaternion.Euler(0f, 0f, randomRotationZ);
            GameObject obj = Instantiate(prefab, position, rotation);
            obj.transform.localScale = scale; // Применение случайного масштаба
            obj.tag = generatedObjectTag;
        }
    }

    Vector2 GetRandomPosition()
    {
        float x = Random.Range(-mapWidth / 2, mapWidth / 2);
        float y = Random.Range(-mapHeight / 2, mapHeight / 2);
        return new Vector2(x, y);
    }

    bool IsPositionOccupied(Vector2 position, GameObject prefab)
    {
        // Получаем коллайдер префаба
        Collider2D collider = prefab.GetComponent<Collider2D>();
        if (collider == null)
        {
            //Debug.LogWarning("Prefab does not have a Collider2D component.");
            return false; // Если коллайдер отсутствует, считаем, что позиция свободна
        }

        // Получаем все коллайдеры в области
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, collider.bounds.extents.x + spacing);

        // Проверяем каждый коллайдер
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("GeneratedObject") == true || col.CompareTag("Player") == true) // Замените "YourTag" на нужный тег
            {
                return true; // Ес��и найден объект с нужным тегом, позиция занята
            }
        }

        return false; // Если не найдено объектов с нужным тегом, позиция свободна
    }

    void MoveObjectsToParent()
    {
        // Находим все объекты со специальным тегом
        GameObject[] generatedObjects = GameObject.FindGameObjectsWithTag(generatedObjectTag);

        // Создаем новую папку, если она не существует
        if (generatedObjectsParent == null)
        {
            generatedObjectsParent = new GameObject("GeneratedObjects");
        }

        // Перемещаем объекты в папку
        foreach (GameObject obj in generatedObjects)
        {
            obj.transform.parent = generatedObjectsParent.transform;
        }
    }
}



