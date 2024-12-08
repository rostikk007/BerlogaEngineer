using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float minDistanceToPlayer = 3f;
    public float smoothTime = 0.3f;
    public float rotationSpeed = 5f;

    [Header("Attack Settings")]
    public GameObject projectilePrefab;
    public float attackRange = 10f;
    public float attackRate = 2f;
    public float projectileSpeed = 10f;
    public float maxShootAngle = 10f;         // Максимальный угол отклонения для выстрела

    [Header("References")]
    public Transform playerTransform;

    private Vector2 currentVelocity;
    private Vector2 targetPosition;
    private float nextAttackTime;
    private Vector3 smoothDampVelocity;
    private bool isRotatingToShoot;           // Поворачивается ли сейчас враг для выстрела
    private Quaternion targetRotation;        // Целевой угол поворота

    void Start()
    {
        nextAttackTime = 0f;
        targetPosition = transform.position;
        isRotatingToShoot = false;
    }

    void Update()
    {
        if (playerTransform == null) return;

        Vector2 enemyPosition = transform.position;
        Vector2 playerPosition = playerTransform.position;
        float distance = Vector2.Distance(enemyPosition, playerPosition);
        Vector2 directionToPlayer = (playerPosition - enemyPosition).normalized;

        // Вычисляем угол к игроку
        float angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        targetRotation = Quaternion.Euler(0, 0, angleToPlayer);

        // Движение
        if (distance > minDistanceToPlayer && !isRotatingToShoot)
        {
            // Обновляем целевую позицию
            targetPosition = Vector2.Lerp(
                targetPosition,
                playerPosition,
                Time.deltaTime * moveSpeed
            );

            // Плавно двигаемся к целевой позиции
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref smoothDampVelocity,
                smoothTime
            );

            // Плавный поворот в сторону движения
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Проверка возможности атаки
        if (distance <= attackRange && Time.time >= nextAttackTime)
        {
            // Начинаем поворот для стрельбы
            isRotatingToShoot = true;

            // Плавно поворачиваемся к цели
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            // Проверяем, достаточно ли точно мы повернулись к цели
            float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);

            if (angleDifference <= maxShootAngle)
            {
                Shoot();
                nextAttackTime = Time.time + 1f / attackRate;
                isRotatingToShoot = false;
            }
        }
        else
        {
            isRotatingToShoot = false;
        }
    }

    void Shoot()
    {
        if (projectilePrefab != null)
        {
            // Создаём снаряд на конце "ствола" врага
            Vector2 shootDirection = transform.right; // Используем правый вектор как направление "ствола"

            GameObject projectile = Instantiate(
                projectilePrefab,
                transform.position,
                transform.rotation // Используем текущий поворот врага
            );

            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = shootDirection * projectileSpeed;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (playerTransform != null)
        {
            // Рисуем линию к игроку
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, playerTransform.position);

            // Рисуем круг минимальной дистанции
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, minDistanceToPlayer);

            // Рисуем круг дистанции атаки
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Рисуем направление "ствола"
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.right * 2f);

            // Рисуем конус допустимого угла стрельбы
            if (isRotatingToShoot)
            {
                Gizmos.color = Color.green;
                Vector3 direction = transform.right;
                float angle1 = Mathf.Deg2Rad * maxShootAngle;
                Vector3 direction1 = new Vector3(
                    direction.x * Mathf.Cos(angle1) - direction.y * Mathf.Sin(angle1),
                    direction.x * Mathf.Sin(angle1) + direction.y * Mathf.Cos(angle1),
                    0
                );
                Vector3 direction2 = new Vector3(
                    direction.x * Mathf.Cos(-angle1) - direction.y * Mathf.Sin(-angle1),
                    direction.x * Mathf.Sin(-angle1) + direction.y * Mathf.Cos(-angle1),
                    0
                );
                Gizmos.DrawRay(transform.position, direction1 * 2f);
                Gizmos.DrawRay(transform.position, direction2 * 2f);
            }
        }
    }
}