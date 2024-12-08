using UnityEngine;

public class MapFiller : MonoBehaviour
{
    public GameObject squarePrefab; // Префаб квадрата, который мы будем размещать
    public int mapWidth = 25;       // Ширина карты
    public int mapHeight = 25;      // Высота карты
    public float cellSize = 1;      // Размер ячейки (должен соответствовать размеру вашего префаба)

    void Start()
    {
        FillMap();
    }

    void FillMap()
    {
        // Создаем родительский объект для квадратов
        GameObject parentObject = new GameObject("MapParent");

        for (int x = -25; x < mapWidth; x++)
        {
            for (int y = -25; y < mapHeight; y++)
            {
                // Вычисляем позицию для каждого квадрата
                Vector3 position = new Vector3(x * cellSize, y * cellSize, 0);

                // Создаем квадрат на рассчитанной позиции
                GameObject square = Instantiate(squarePrefab, position, Quaternion.identity);
                square.name = $"Square_{x}_{y}";

                // Устанавливаем родителя для квадрата
                square.transform.parent = parentObject.transform;

                // Добавляем компонент для обработки выделения
                square.AddComponent<SquareHighlighter>();
            }
        }
    }
}



