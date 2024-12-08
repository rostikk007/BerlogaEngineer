using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Настройки движения")]
    [Tooltip("Скорость передвижения игрока")]
    public float moveSpeed = 5f;

    [Tooltip("Слой для препятствий")]
    public LayerMask obstacleLayer;

    [Tooltip("Дистанция проверки столкновений")]
    public float checkDistance = 0.1f;

    [Header("Границы карты")]
    [Tooltip("Левая граница карты")]
    public float leftBoundary = -24.35906f;

    [Tooltip("Правая граница карты")]
    public float rightBoundary = 24.35906f;

    [Tooltip("Верхняя граница карты")]
    public float topBoundary = 24.35906f;

    [Tooltip("Нижняя граница карты")]
    public float bottomBoundary = -24.35906f;

    [Header("Настройки поворота")]
    [Tooltip("Скорость поворота к мыши")]
    public float rotationSpeed = 10f;

    [Tooltip("Включить/выключить плавность поворота")]
    public bool smoothRotation = true;

    [Tooltip("Корректировка угла поворота для спрайта")]
    public float rotationOffset = -90f;

    [Header("Настройки коллизий")]
    [Tooltip("Радиус проверки коллизий")]
    public float collisionCheckRadius = 0.2f;

    [Header("Настройки звуков")]
    [Tooltip("Минимальный интервал между звуками шагов")]
    public float footstepInterval = 0.3f;

    [Header("Автоматическое движение")]
    [Tooltip("Скорость автоматического движения")]
    public float autoMoveSpeed = 3f;

    [Tooltip("Дистанция, на которой считается, что игрок достиг цели")]
    public float reachDistance = 0.5f;

    private Camera mainCamera;
    private Vector2 mousePosition;
    private float lastFootstepTime;
    private bool isMoving = false;
    private bool wasMoving = false;

    private Vector2? targetPosition;
    private bool isAutoMoving = false;
    private System.Action onTargetReached;

    private bool hasReachedDestination = false;
    public bool HasReachedDestination => hasReachedDestination;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (isAutoMoving)
        {
            AutoMove();
        }
        else
        {
            MovePlayer();
        }
        RotatePlayer();
    }

    public void SetDestination(Vector2 destination, System.Action callback = null)
    {
        targetPosition = destination;
        isAutoMoving = true;
        hasReachedDestination = false;
        onTargetReached = callback;
    }

    public void StopAutoMovement()
    {
        isAutoMoving = false;
        targetPosition = null;
        onTargetReached = null;
    }

    private void AutoMove()
    {
        if (!targetPosition.HasValue) return;

        Vector2 direction = (targetPosition.Value - (Vector2)transform.position).normalized;
        Vector2 newPosition = (Vector2)transform.position + direction * autoMoveSpeed * Time.deltaTime;

        // Проверяем, достиг ли игрок цели
        if (Vector2.Distance(transform.position, targetPosition.Value) <= reachDistance)
        {
            transform.position = targetPosition.Value;
            isAutoMoving = false;
            hasReachedDestination = true;
            targetPosition = null;
            onTargetReached?.Invoke();
            return;
        }

        // Проверяем только границы карты, игнорируем препятствия
        if (newPosition.x >= leftBoundary && newPosition.x <= rightBoundary &&
            newPosition.y >= bottomBoundary && newPosition.y <= topBoundary)
        {
            // При автоматическом движении игрок проходит через препятствия
            transform.position = newPosition;

            // Проигрываем звук шагов
            if (Time.time - lastFootstepTime >= footstepInterval)
            {
                AudioManager.Instance.PlayFootstep();
                lastFootstepTime = Time.time;
            }
        }
    }

    void MovePlayer()
    {
        float moveHorizontal = 0f;
        float moveVertical = 0f;

        if (Input.GetKey(KeyCode.W)) moveVertical = 1f;
        if (Input.GetKey(KeyCode.S)) moveVertical = -1f;
        if (Input.GetKey(KeyCode.A)) moveHorizontal = -1f;
        if (Input.GetKey(KeyCode.D)) moveHorizontal = 1f;

        Vector2 movement = new Vector2(moveHorizontal, moveVertical).normalized;

        // Проверяем, движется ли игрок
        isMoving = movement.magnitude > 0;

        // Проигрываем звук шагов только когда игрок движется
        if (isMoving)
        {
            if (Time.time - lastFootstepTime >= footstepInterval)
            {
                AudioManager.Instance.PlayFootstep();
                lastFootstepTime = Time.time;
            }
        }

        Vector2 newPosition = (Vector2)transform.position + movement * moveSpeed * Time.deltaTime;

        if (newPosition.x >= leftBoundary && newPosition.x <= rightBoundary &&
            newPosition.y >= bottomBoundary && newPosition.y <= topBoundary)
        {
            if (!IsObstacleInWay(movement))
            {
                transform.position = newPosition;
            }
            else
            {
                // Замедляем игрока на 40% при прохождении через постройку
                transform.position = (Vector2)transform.position + movement * moveSpeed * 0.6f * Time.deltaTime;
            }
        }

        // Сохраняем текущее состояние движения
        wasMoving = isMoving;
    }

    void RotatePlayer()
    {
        Vector2 targetPosition;

        if (isAutoMoving && this.targetPosition.HasValue)
        {
            targetPosition = this.targetPosition.Value;
        }
        else
        {
            targetPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        }

        Vector2 direction = targetPosition - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle += rotationOffset;

        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        if (smoothRotation)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.rotation = targetRotation;
        }
    }

    bool IsObstacleInWay(Vector2 movement)
    {
        // Проверка с помощью Raycast
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            transform.position,
            movement,
            checkDistance
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null) continue;

            // Проверяем деревья и постройки
            if (hit.collider.gameObject.name == "Дерево(Clone)" ||
                hit.collider.CompareTag("Buildings"))
            {
                return true;
            }
        }

        // Дополнительная проверка с помощью OverlapCircle
        Vector2 checkPosition = (Vector2)transform.position + movement * checkDistance;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            checkPosition,
            collisionCheckRadius
        );

        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject == gameObject) continue; // Пропускаем коллайдер самого игрока

            if (collider.gameObject.name == "Дерево(Clone)" ||
                collider.CompareTag("Buildings"))
            {
                return true;
            }
        }

        return false;
    }

    void OnDrawGizmos()
    {
        // Рисуем направление взгляда игрока
        Gizmos.color = Color.red;
        Vector2 direction = transform.right * 2f;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + direction);

        // Рисуем границы карты
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector2(leftBoundary, topBoundary), new Vector2(rightBoundary, topBoundary));
        Gizmos.DrawLine(new Vector2(rightBoundary, topBoundary), new Vector2(rightBoundary, bottomBoundary));
        Gizmos.DrawLine(new Vector2(rightBoundary, bottomBoundary), new Vector2(leftBoundary, bottomBoundary));
        Gizmos.DrawLine(new Vector2(leftBoundary, bottomBoundary), new Vector2(leftBoundary, topBoundary));

        // Визуализация проверки коллизий
        if (Application.isPlaying)
        {
            float moveHorizontal = 0f;
            float moveVertical = 0f;
            if (Input.GetKey(KeyCode.W)) moveVertical = 1f;
            if (Input.GetKey(KeyCode.S)) moveVertical = -1f;
            if (Input.GetKey(KeyCode.A)) moveHorizontal = -1f;
            if (Input.GetKey(KeyCode.D)) moveHorizontal = 1f;
            Vector2 movement = new Vector2(moveHorizontal, moveVertical).normalized;

            // Рисуем луч проверки препятствий
            Gizmos.color = Color.green;
            Vector2 rayEnd = (Vector2)transform.position + movement * checkDistance;
            Gizmos.DrawLine(transform.position, rayEnd);

            // Рисуем область проверки коллизий
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawWireSphere(rayEnd, collisionCheckRadius);

            // Рисуем цель автоматического движения
            if (isAutoMoving && targetPosition.HasValue)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(targetPosition.Value, reachDistance);
                Gizmos.DrawLine(transform.position, targetPosition.Value);
            }
        }
    }

    public bool IsAutoMoving => isAutoMoving;
}