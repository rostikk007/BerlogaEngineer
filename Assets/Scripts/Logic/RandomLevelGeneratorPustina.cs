using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomLevelGeneratorPustina : MonoBehaviour
{
    public GameObject[] SmallstonePrefabs_LocationDesert;
    public GameObject[] stonePrefabs_LocationDesert; // ������ �������� ������ // ������ �������� �����
    public GameObject[] KockiPrefabs_LocationDesert;  // ������ �������� ������� ������
    public GameObject[] grassPrefabs_LocationDesert;

    public int numberSmallstonePrefabs_Location = 15; // ���������� ������
    public int numberstonePrefabs_Location = 15; // ���������� ������
    public int numberKockiPrefabs_Location = 15; // ���������� ������
    public int numberOfGrass_Location = 15; // ���������� ������
    public float mapWidth = 48f; // ������ �����
    public float mapHeight = 50f; // ������ �����
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
       // PlaceObjects(treePrefab_LocationForest, numberOfTrees_LocationForest); //����������� ������ � �������� ��� �������� � ������� �������
         //����������� ������ � �������� � ��������� � ������� �������
        PlaceRandomObjects(SmallstonePrefabs_LocationDesert, numberstonePrefabs_Location);
        PlaceRandomObjects(stonePrefabs_LocationDesert, numberSmallstonePrefabs_Location);
        PlaceRandomObjectsBezPovorota(grassPrefabs_LocationDesert, numberOfGrass_Location);
       //����������� ������ � �������� ��� �������� � � ������ ��������
        //PlaceRandomObjectsVetki(VetkiPrefabs_LocationForest, numberOfVetki_LocationForest); //����������� ������ � �������� � ��������� � ������� �������, �� ����� �����
        PlaceRandomObjectsKocki(KockiPrefabs_LocationDesert, numberKockiPrefabs_Location); //����������� ������ � �������� � ��������� � ����������� �������
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

            float randomScale = Random.Range(0.3f, 0.6f); // �������� �������� �� ������ ����������
            Vector3 scale = new Vector3(randomScale, randomScale, randomScale);
            float randomRotationZ = Random.Range(0f, 360f);
            Quaternion rotation = Quaternion.Euler(0f, 0f, randomRotationZ);
            GameObject obj = Instantiate(prefab, position, rotation);
            obj.transform.localScale = scale; // ���������� ���������� ��������
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

            float randomScale = Random.Range(0.8f, 1.2f); // �������� �������� �� ������ ����������
            Vector3 scale = new Vector3(randomScale, randomScale, randomScale);
            GameObject obj = Instantiate(prefab, position, Quaternion.identity);
            obj.transform.localScale = scale; // ���������� ���������� ��������
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

            float randomScale = Random.Range(0.5f, 1.5f); // �������� �������� �� ������ ����������
            Vector3 scale = new Vector3(randomScale, randomScale, randomScale);
            float randomRotationZ = Random.Range(0f, 360f);
            Quaternion rotation = Quaternion.Euler(0f, 0f, randomRotationZ);
            GameObject obj = Instantiate(prefab, position, rotation);
            obj.transform.localScale = scale; // ���������� ���������� ��������
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
        // �������� ��������� �������
        Collider2D collider = prefab.GetComponent<Collider2D>();
        if (collider == null)
        {
            //Debug.LogWarning("Prefab does not have a Collider2D component.");
            return false; // ���� ��������� �����������, �������, ��� ������� ��������
        }

        // �������� ��� ���������� � �������
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, collider.bounds.extents.x + spacing);

        // ��������� ������ ���������
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("GeneratedObject") == true || col.CompareTag("Player") == true) // �������� "YourTag" �� ������ ���
            {
                return true; // ���� ������ ������ � ������ �����, ������� ������
            }
        }

        return false; // ���� �� ������� �������� � ������ �����, ������� ��������
    }

    void MoveObjectsToParent()
    {
        // ������� ��� ������� �� ����������� �����
        GameObject[] generatedObjects = GameObject.FindGameObjectsWithTag(generatedObjectTag);

        // ������� ����� �����, ���� ��� �� ����������
        if (generatedObjectsParent == null)
        {
            generatedObjectsParent = new GameObject("GeneratedObjects");
        }

        // ���������� ������� � �����
        foreach (GameObject obj in generatedObjects)
        {
            obj.transform.parent = generatedObjectsParent.transform;
        }
    }
}
