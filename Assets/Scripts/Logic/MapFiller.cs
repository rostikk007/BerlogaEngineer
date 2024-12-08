using UnityEngine;

public class MapFiller : MonoBehaviour
{
    public GameObject squarePrefab; // ������ ��������, ������� �� ����� ���������
    public int mapWidth = 25;       // ������ �����
    public int mapHeight = 25;      // ������ �����
    public float cellSize = 1;      // ������ ������ (������ ��������������� ������� ������ �������)

    void Start()
    {
        FillMap();
    }

    void FillMap()
    {
        // ������� ������������ ������ ��� ���������
        GameObject parentObject = new GameObject("MapParent");

        for (int x = -25; x < mapWidth; x++)
        {
            for (int y = -25; y < mapHeight; y++)
            {
                // ��������� ������� ��� ������� ��������
                Vector3 position = new Vector3(x * cellSize, y * cellSize, 0);

                // ������� ������� �� ������������ �������
                GameObject square = Instantiate(squarePrefab, position, Quaternion.identity);
                square.name = $"Square_{x}_{y}";

                // ������������� �������� ��� ��������
                square.transform.parent = parentObject.transform;

                // ��������� ��������� ��� ��������� ���������
                square.AddComponent<SquareHighlighter>();
            }
        }
    }
}



