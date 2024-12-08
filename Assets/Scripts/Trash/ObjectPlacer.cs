using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    public GridManager gridManager; // Ссылка на GridManager
    public Camera mainCamera; // Ссылка на основную камеру

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Проверяем, нажата ли левая кнопка мыши
        {
            Vector3 mousePosition = Input.mousePosition; // Получаем позицию мыши на экране
            mousePosition.z = mainCamera.nearClipPlane; // Устанавливаем z на близкую плоскость камеры

            // Преобразуем позицию мыши в мировые координаты
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

            // Преобразуем мировые координаты в координаты сетки
            //int x = Mathf.FloorToInt(worldPosition.x / gridManager.cellSize); // Получаем координату X сетки
            //int y = Mathf.FloorToInt(worldPosition.y / gridManager.cellSize); // Получаем координату Y сетки

            // Размещаем объект на сетке
            //gridManager.PlaceObject(x, y);
        }
    }
}

