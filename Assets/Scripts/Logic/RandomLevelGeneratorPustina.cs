using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomLevelGeneratorPustina : MonoBehaviour
{
    public GameObject[] SmallstonePrefabs_LocationDesert;
    public GameObject[] stonePrefabs_LocationDesert; // Массив префабов камней // Массив префабов травы
    public GameObject[] KockiPrefabs_LocationDesert;  // Массив префабов больших камней
    public GameObject[] grassPrefabs_LocationDesert;

    public int numberSmallstonePrefabs_Location = 15; // Количество камней
    public int numberstonePrefabs_Location = 15; // Количество камней
    public int numberKockiPrefabs_Location = 15; // Количество камней
    public int numberOfGrass_Location = 15; // Количество камней
    public float mapWidth = 48f; // Ширина карты
    public float mapHeight = 50f; // Высота карты
    private List<Vector2> occupiedPositions = new List<Vector2>();
    public string generatedObjectTag = "GeneratedObject";
    public GameObject generatedObjectsParent;
    public float spacing = 0.5f;

    void Start()
    {
        GenerateLevel();
        MoveObjectsToParent();
    }

    void GenerateLevel()
    {
       // PlaceObjects(treePrefab_LocationForest, numberOfTrees_LocationForest); //Расположить обекты в рандомно без поворота и разного размера
         //Расположить обекты в рандомно с поворотом и разного размера
        PlaceRandomObjects(SmallstonePrefabs_LocationDesert, numberstonePrefabs_Location);
        PlaceRandomObjects(stonePrefabs_LocationDesert, numberSmallstonePrefabs_Location);
        PlaceRandomObjectsBezPovorota(grassPrefabs_LocationDesert, numberOfGrass_Location);
       //Расположить обекты в рандомно без поворота и с разным размером
        //PlaceRandomObjectsVetki(VetkiPrefabs_LocationForest, numberOfVetki_LocationForest); //Расположить обекты в рандомно с поворотом и разного размера, но более мелко
        PlaceRandomObjectsKocki(KockiPrefabs_LocationDesert, numberKockiPrefabs_Location); //Расположить обекты в рандомно с поворотом и одинакового размера
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
                return true; // Если найден объект с нужным тегом, позиция занята
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
