using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Основные настройки сетки")]
    [Tooltip("Префаб ячейки сетки")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float cellSize = 1f;

    [Header("Визуальные настройки")]
    [Tooltip("Цвет подсветки доступной ячейки")]
    [SerializeField] private Color availableCellColor = Color.green;
    [Tooltip("Цвет подсветки занятой ячейки")]
    [SerializeField] private Color occupiedCellColor = Color.red;
    
    private GameObject[,] grid;
    private GameObject[,] highlightCells; // Массив для хранения подсветки ячеек

    private void Start()
    {
        grid = new GameObject[gridWidth, gridHeight];
        highlightCells = new GameObject[gridWidth, gridHeight];
        CreateGrid();
    }

    // Создаем визуальную сетку при старте
    private void CreateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 position = GetWorldPosition(x, y);
                GameObject cell = Instantiate(prefab, position, Quaternion.identity, transform);
                cell.name = $"Cell_{x}_{y}";
                highlightCells[x, y] = cell;
                
                // Делаем ячейки немного прозрачными по умолчанию
                SetCellColor(cell, new Color(1f, 1f, 1f, 0.2f));
            }
        }
    }

    // Получить позицию в мировых координатах из координат сетки
    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x * cellSize, y * cellSize, 0);
    }

    // Получить координаты сетки из мировой позиции
    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / cellSize);
        int y = Mathf.FloorToInt(worldPosition.y / cellSize);
        return new Vector2Int(x, y);
    }

    // Подсветить ячейку в зависимости от возможности размещения
    public void HighlightCell(int x, int y, bool canPlace)
    {
        if (IsValidPosition(x, y) && highlightCells[x, y] != null)
        {
            Color highlightColor = canPlace ? availableCellColor : occupiedCellColor;
            SetCellColor(highlightCells[x, y], highlightColor);
        }
    }

    // Сбросить подсветку ячейки
    public void ResetHighlight(int x, int y)
    {
        if (IsValidPosition(x, y) && highlightCells[x, y] != null)
        {
            SetCellColor(highlightCells[x, y], new Color(1f, 1f, 1f, 0.2f));
        }
    }

    private void SetCellColor(GameObject cell, Color color)
    {
        SpriteRenderer renderer = cell.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = color;
        }
    }

    // Проверка валидности координат
    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }

    public void PlaceObject(int x, int y)
    {
        if (IsValidPosition(x, y))
        {
            if (grid[x, y] == null)
            {
                Vector3 position = GetWorldPosition(x, y);
                GameObject obj = Instantiate(prefab, position, Quaternion.identity);
                grid[x, y] = obj;
                HighlightCell(x, y, false); // Подсвечиваем ячейку как занятую
            }
            else
            {
                Debug.Log("Ячейка занята!");
            }
        }
        else
        {
            Debug.Log("Неверные координаты!");
        }
    }

    // Optional: Method to remove object from grid
    public void RemoveObject(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            if (grid[x, y] != null)
            {
                Destroy(grid[x, y]);
                grid[x, y] = null;
            }
        }
    }

    // Optional: Method to check if cell is occupied
    public bool IsCellOccupied(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            return grid[x, y] != null;
        }
        return false;
    }

    // Method to clear the entire grid
    public void ClearGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] != null)
                {
                    Destroy(grid[x, y]);
                    grid[x, y] = null;
                }
            }
        }
    }
}