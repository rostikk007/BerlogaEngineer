using UnityEngine;

public class MapBorders : MonoBehaviour
{
    public GameObject wallPrefab; // ������ �����
    public float mapWidth = 50f; // ������ �����
    public float mapHeight = 50f; // ������ �����

    void Start()
    {
        CreateBorders();
    }

    void CreateBorders()
    {
        // ������� ������ �����
        for (float x = -mapWidth / 2; x <= mapWidth / 2; x += wallPrefab.transform.localScale.x)
        {
            CreateWall(new Vector3(x, -mapHeight / 2, 0)); // ������������� y �� -mapHeight/2 ��� ������ �����
        }

        // ������� ������� �����
        for (float x = -mapWidth / 2; x <= mapWidth / 2; x += wallPrefab.transform.localScale.x)
        {
            CreateWall(new Vector3(x, mapHeight / 2, 0)); // ������������� y �� mapHeight/2 ��� ������� �����
        }

        // ������� ����� �����
        for (float y = -mapHeight / 2; y <= mapHeight / 2; y += wallPrefab.transform.localScale.y)
        {
            CreateWall(new Vector3(-mapWidth / 2, y, 0)); // ������������� x �� -mapWidth/2 ��� ����� �����
        }

        // ������� ������ �����
        for (float y = -mapHeight / 2; y <= mapHeight / 2; y += wallPrefab.transform.localScale.y)
        {
            CreateWall(new Vector3(mapWidth / 2, y, 0)); // ������������� x �� mapWidth/2 ��� ������ �����
        }
    }

    void CreateWall(Vector3 position)
    {
        GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity);
        wall.transform.parent = this.transform; // ������������� �������� ��� ��������
    }
}


