using UnityEngine;

public class Camera_Follow : MonoBehaviour
{
    [Header("Настройки следования")]
    [Tooltip("Ссылка на объект игрока")]
    public Transform player;
    [Tooltip("Смещение камеры относительно игрока")]
    public Vector3 offset;

    [Header("Границы карты")]
    [Tooltip("Ширина карты")]
    public float mapWidth = 35f;
    [Tooltip("Высота карты")]
    public float mapHeight = 40f;

    [Header("Настройки масштабирования")]
    [Tooltip("Минимальное значение orthographicSize (максимальное приближение)")]
    public float minZoom = 5f;
    [Tooltip("Максимальное значение orthographicSize (максимальное отдаление)")]
    public float maxZoom = 15f;
    [Tooltip("Скорость масштабирования")]
    public float zoomSpeed = 1f;
    [Tooltip("Плавность масштабирования")]
    public float zoomSmoothness = 5f;

    private Camera cam;
    private float targetZoom;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("Камера не найдена на объекте!");
            return;
        }
        targetZoom = cam.orthographicSize;
    }

    void Update()
    {
        // Обработка масштабирования колесиком мыши
        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta != 0)
        {
            // Инвертируем направление прокрутки для более интуитивного управления
            targetZoom -= scrollDelta * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        // Плавное изменение масштаба
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSmoothness);
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Получаем размеры видимой области камеры
        float verticalSize = cam.orthographicSize;
        float horizontalSize = verticalSize * cam.aspect;

        // Вычисляем максимально допустимые границы для камеры
        float maxX = mapWidth/2 - horizontalSize;
        float maxY = mapHeight/2 - verticalSize;

        // Если видимая область больше размера карты, центрируем камеру
        if (horizontalSize * 2 >= mapWidth)
        {
            // Центрируем по X
            maxX = 0;
        }
        if (verticalSize * 2 >= mapHeight)
        {
            // Центрируем по Y
            maxY = 0;
        }

        // Вычисляем желаемую позицию камеры
        Vector3 desiredPosition = player.position + offset;

        // Ограничиваем позицию камеры
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, -maxX, maxX);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, -maxY, maxY);

        // Устанавливаем позицию камеры
        transform.position = new Vector3(desiredPosition.x, desiredPosition.y, offset.z);
    }

    // Метод для программного изменения масштаба
    public void SetZoom(float newZoom)
    {
        targetZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);
    }

    void OnDrawGizmos()
    {
        // Отрисовка границ карты в редакторе
        Gizmos.color = Color.yellow;
        Vector3 center = Vector3.zero;
        Vector3 size = new Vector3(mapWidth, mapHeight, 0);
        Gizmos.DrawWireCube(center, size);
    }
}

