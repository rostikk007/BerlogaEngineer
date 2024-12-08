using UnityEngine;

public class MapBorders : MonoBehaviour
{
    public GameObject wallPrefab; // Префаб стены
    public float mapWidth = 50f; // Ширина карты
    public float mapHeight = 50f; // Высота карты

    void Start()
    {
        CreateBorders();
    }

    void CreateBorders()
    {
        // Создаем нижнюю стену
        for (float x = -mapWidth / 2; x <= mapWidth / 2; x += wallPrefab.transform.localScale.x)
        {
            CreateWall(new Vector3(x, -mapHeight / 2, 0)); // Устанавливаем y на -mapHeight/2 для нижней стены
        }

        // Создаем верхнюю стену
        for (float x = -mapWidth / 2; x <= mapWidth / 2; x += wallPrefab.transform.localScale.x)
        {
            CreateWall(new Vector3(x, mapHeight / 2, 0)); // Устанавливаем y на mapHeight/2 для верхней стены
        }

        // Создаем левую стену
        for (float y = -mapHeight / 2; y <= mapHeight / 2; y += wallPrefab.transform.localScale.y)
        {
            CreateWall(new Vector3(-mapWidth / 2, y, 0)); // Устанавливаем x на -mapWidth/2 для левой стены
        }

        // Создаем правую стену
        for (float y = -mapHeight / 2; y <= mapHeight / 2; y += wallPrefab.transform.localScale.y)
        {
            CreateWall(new Vector3(mapWidth / 2, y, 0)); // Устанавливаем x на mapWidth/2 для правой стены
        }
    }

    void CreateWall(Vector3 position)
    {
        GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity);
        wall.transform.parent = this.transform; // Устанавливаем родителя для удобства
    }
}


