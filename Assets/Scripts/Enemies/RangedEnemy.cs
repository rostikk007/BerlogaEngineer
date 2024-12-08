using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RangedEnemy : MonoBehaviour
{
    [Header("Награда за убийство")]
    [Tooltip("Количество монет, получаемых за убийство врага")]
    public int coinsReward = 1;
    [Tooltip("Количество очков улучшения, получаемых за убийство врага (0.1 = одно очко за 10 убийств)")]
    public float upgradePointsReward = 0.1f;
    private static float upgradePointsAccumulated = 0f;

    [Header("Настройки здоровья")]
    [Tooltip("Максимальное количество здоровья врага")]
    public float maxHealth = 35f;
    [Tooltip("Текущее здоровье врага (заполняется автоматически)")]
    public float currentHealth;
    [Tooltip("Длительность красной вспышки при получении урона (в секундах)")]
    public float damageFlashDuration = 0.1f;

    [Header("Настройки движения")]
    [Tooltip("Скорость передвижения врага")]
    public float moveSpeed = 2.5f;
    [Tooltip("Оптимальная дистанция для атаки")]
    public float optimalAttackDistance = 5f;
    [Tooltip("Минимальная дистанция, ближе которой враг будет отходить")]
    public float minAttackDistance = 3f;
    [Tooltip("Время сглаживания движения")]
    public float smoothTime = 0.1f;
    [Tooltip("Скорость поворота врага")]
    public float rotationSpeed = 5f;

    [Header("Настройки атаки")]
    [Tooltip("Префаб снаряда")]
    public GameObject projectilePrefab;
    [Tooltip("Максимальная дальность стрельбы")]
    public float maxAttackRange = 7f;
    [Tooltip("Время между выстрелами (в секундах)")]
    public float attackCooldown = 2f;
    [Tooltip("Урон от снаряда")]
    public int projectileDamage = 8;
    [Tooltip("Скорость снаряда")]
    public float projectileSpeed = 8f;
    [Tooltip("Точка появления снаряда")]
    public Transform firePoint;

    [Header("Настройки избегания")]
    [Tooltip("Радиус обнаружения других врагов для избегания столкновений")]
    public float avoidanceRadius = 1.5f;
    [Tooltip("Сила отталкивания от других врагов")]
    public float avoidanceForce = 1f;
    [Tooltip("Слой, на котором находятся враги")]
    public LayerMask enemyLayer;

    [Header("Настройки целей")]
    [Tooltip("Список приоритетов целей")]
    public List<TargetPriority> targetPriorities = new List<TargetPriority>();
    [Tooltip("Радиус обнаружения целей")]
    public float targetDetectionRange = 12f;

    [Header("Настройки анимации")]
    [Tooltip("Скорость анимации сжатия/растяжения")]
    public float squashStretchSpeed = 8f;
    [Tooltip("Сила сжатия")]
    public float squashAmount = 0.2f;
    [Tooltip("Сила растяжения")]
    public float stretchAmount = 0.1f;
    private float squashTime;

    // Приватные переменные
    private Vector2 targetPosition;
    private Vector3 smoothDampVelocity;
    private SpriteRenderer spriteRenderer;
    private float nextAttackTime;
    private bool isAttacking;
    private Vector3 originalScale;
    private bool isFlashing = false;
    private Transform currentTarget;
    private TargetInfo currentTargetInfo;
    private float speedModifier = 1f;
    private float baseSpeed;
    private bool isDead = false;
    private Color originalColor;
    private bool isMoving = false;
    private float stationaryTimer = 0f;
    private const float STATIONARY_THRESHOLD = 0.1f; // Порог скорости для определения остановки
    private const float MIN_STATIONARY_TIME = 0.5f; // Минимальное время остановки перед выстрелом

    // Добавим перечисление состояний
    private enum EnemyState
    {
        Moving,
        Stopping,
        Attacking,
        Repositioning
    }

    // Добавим новые переменные
    private EnemyState currentState = EnemyState.Moving;
    private float stopTimer = 0f;
    private const float STOP_DURATION = 0.5f; // Время остановки перед выстрелом

    [System.Serializable]
    public class TargetPriority
    {
        public string targetName;
        public LayerMask targetLayer;
        public int priority;
        public float priorityWeight = 1f;
    }

    public class TargetInfo
    {
        public Transform transform;
        public int priority;
        public float distance;
        public float weightedPriority;
    }

    void Start()
    {
        baseSpeed = moveSpeed;
        spriteRenderer = GetComponent<SpriteRenderer>();
        targetPosition = transform.position;
        originalScale = transform.localScale;
        originalColor = spriteRenderer.color;
        currentHealth = maxHealth;

        if (firePoint == null)
        {
            firePoint = new GameObject("FirePoint").transform;
            firePoint.SetParent(transform);
            firePoint.localPosition = new Vector3(0.5f, 0, 0);
        }

        targetPriorities.Sort((a, b) => a.priority.CompareTo(b.priority));

        // Воспроизводим звук появления жука
        AudioManager.Instance.PlayBugSpawn(transform.position);

        // Добавляем небольшую анимацию появления
        StartCoroutine(SpawnAnimation());
    }

    void Update()
    {
        if (isDead) return;

        FindNearestTarget();
        if (currentTarget == null) return;

        Vector2 enemyPosition = transform.position;
        Vector2 targetPos = currentTarget.position;
        float distance = Vector2.Distance(enemyPosition, targetPos);
        Vector2 directionToTarget = (targetPos - enemyPosition).normalized;

        // Поворот к цели
        float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.Euler(0, 0, angle),
            rotationSpeed * Time.deltaTime
        );

        switch (currentState)
        {
            case EnemyState.Moving:
                // Обычное движение
                if (distance < minAttackDistance)
                {
                    // Отходим от цели
                    targetPosition = Vector2.Lerp(
                        targetPosition,
                        (Vector2)transform.position - directionToTarget * moveSpeed,
                        Time.deltaTime * moveSpeed
                    );
                }
                else if (distance > optimalAttackDistance)
                {
                    // Приближаемся к цели
                    Vector2 avoidanceVector = CalculateAvoidanceVector();
                    Vector2 finalDirection = (directionToTarget + avoidanceVector).normalized;

                    targetPosition = Vector2.Lerp(
                        targetPosition,
                        (Vector2)transform.position + finalDirection * moveSpeed,
                        Time.deltaTime * moveSpeed
                    );
                }
                else if (Time.time >= nextAttackTime && !isAttacking)
                {
                    // Если в оптимальной дистанции и можем атаковать - останавливаемся
                    currentState = EnemyState.Stopping;
                    stopTimer = 0f;
                    targetPosition = transform.position; // Фиксируем позицию
                }

                // Анимация движения
                AnimateMovement();
                break;

            case EnemyState.Stopping:
                stopTimer += Time.deltaTime;
                if (stopTimer >= STOP_DURATION)
                {
                    currentState = EnemyState.Attacking;
                    StartCoroutine(Attack());
                }
                break;

            case EnemyState.Attacking:
                // Ничего не делаем, ждем окончания атаки
                break;

            case EnemyState.Repositioning:
                // Возвращаемся к движению после атаки
                currentState = EnemyState.Moving;
                break;
        }

        // Применяем движение
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref smoothDampVelocity,
            smoothTime
        );
    }

    void FindNearestTarget()
    {
        List<TargetInfo> potentialTargets = new List<TargetInfo>();

        foreach (var priorityGroup in targetPriorities)
        {
            Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, targetDetectionRange, priorityGroup.targetLayer);
            foreach (var target in targets)
            {
                float distance = Vector2.Distance(transform.position, target.transform.position);
                float weightedPriority = priorityGroup.priority * priorityGroup.priorityWeight + distance / targetDetectionRange;

                potentialTargets.Add(new TargetInfo
                {
                    transform = target.transform,
                    priority = priorityGroup.priority,
                    distance = distance,
                    weightedPriority = weightedPriority
                });
            }
        }

        if (potentialTargets.Count > 0)
        {
            potentialTargets.Sort((a, b) => a.weightedPriority.CompareTo(b.weightedPriority));
            currentTargetInfo = potentialTargets[0];
            currentTarget = currentTargetInfo.transform;
        }
        else
        {
            currentTargetInfo = null;
            currentTarget = null;
        }
    }

    IEnumerator Attack()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        Vector3 startScale = transform.localScale;
        Color startColor = spriteRenderer.color;
        Vector3 startPosition = transform.position;

        // Фаза 1: Подготовка к выстрелу
        float chargeTime = 0.4f;
        float elapsed = 0f;

        // Пульсация и изменение цвета
        while (elapsed < chargeTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / chargeTime;

            // Пульсирующая анимация
            float pulse = 1f + Mathf.Sin(progress * Mathf.PI * 4) * 0.1f;
            transform.localScale = new Vector3(
                startScale.x * pulse,
                startScale.y * pulse,
                startScale.z
            );

            // Плавное изменение цвета
            float colorPulse = Mathf.Sin(progress * Mathf.PI * 4) * 0.5f + 0.5f;
            spriteRenderer.color = Color.Lerp(startColor, Color.yellow, colorPulse);

            yield return null;
        }

        // Фаза 2: Выстрел
        if (currentTarget != null)
        {
            // Быстрое сжатие перед выстрелом
            float compressTime = 0.1f;
            elapsed = 0f;

            Vector3 compressedScale = new Vector3(
                startScale.x * 0.7f,
                startScale.y * 1.3f,
                startScale.z
            );

            while (elapsed < compressTime)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / compressTime;
                transform.localScale = Vector3.Lerp(startScale, compressedScale, progress);
                yield return null;
            }

            // Создаём и запускаем снаряд
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(projectileDamage, projectileSpeed, transform);
            }

            // Звук выстрела
            AudioManager.Instance.PlayRangedBugAttack(transform.position);

            // Отдача
            float recoilTime = 0.15f;
            elapsed = 0f;
            Vector3 recoilOffset = -transform.right * 0.3f; // Отскок назад

            while (elapsed < recoilTime)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / recoilTime;

                // Анимация отдачи с эффектом пружины
                float recoilProgress = 1f - (1f - progress) * (1f - progress); // Квадратичная интерполяция
                transform.position = Vector3.Lerp(
                    startPosition + recoilOffset,
                    startPosition,
                    recoilProgress
                );

                // Растяжение при отдаче
                float stretchFactor = 1f - Mathf.Sin(progress * Mathf.PI) * 0.2f;
                transform.localScale = new Vector3(
                    startScale.x * (2f - stretchFactor),
                    startScale.y * stretchFactor,
                    startScale.z
                );

                yield return null;
            }
        }

        // Фаза 3: Возвращение в исходное состояние
        float returnTime = 0.2f;
        elapsed = 0f;

        while (elapsed < returnTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / returnTime;

            // Плавное возвращение с небольшим отскоком
            float bounce = 1f + Mathf.Sin(progress * Mathf.PI) * 0.05f;
            transform.localScale = Vector3.Lerp(transform.localScale, startScale * bounce, progress);
            transform.position = Vector3.Lerp(transform.position, startPosition, progress);
            spriteRenderer.color = Color.Lerp(Color.yellow, startColor, progress);

            yield return null;
        }

        // Финальная корректировка
        transform.localScale = startScale;
        transform.position = startPosition;
        spriteRenderer.color = startColor;
        isAttacking = false;

        // В конце атаки
        currentState = EnemyState.Repositioning;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (!isFlashing)
        {
            StartCoroutine(DamageFlash());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator DamageFlash()
    {
        isFlashing = true;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(damageFlashDuration);
        spriteRenderer.color = originalColor;
        isFlashing = false;
    }

    private void Die()
    {
        isDead = true;

        // Воспроизводим звук смерти обычного жука вместо дальнего
        AudioManager.Instance.PlayBugDeath(transform.position);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoins(coinsReward);
            upgradePointsAccumulated += upgradePointsReward;

            if (upgradePointsAccumulated >= 1f)
            {
                int pointsToAdd = Mathf.FloorToInt(upgradePointsAccumulated);
                GameManager.Instance.AddUpgradePoints(pointsToAdd);
                upgradePointsAccumulated -= pointsToAdd;
            }
        }

        StartCoroutine(DeathAnimation());
    }

    private IEnumerator DeathAnimation()
    {
        if (GetComponent<Collider2D>() != null)
        {
            GetComponent<Collider2D>().enabled = false;
        }

        Color startColor = spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        float fadeTime = 1f;
        float elapsed = 0f;

        Vector3 startScale = transform.localScale;
        Vector3 endScale = startScale * 1.5f;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = transform.rotation * Quaternion.Euler(0, 0, 180f);

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            float smoothT = Mathf.SmoothStep(0, 1, t);

            spriteRenderer.color = Color.Lerp(startColor, endColor, smoothT);
            transform.localScale = Vector3.Lerp(startScale, endScale, smoothT);
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, smoothT);

            yield return null;
        }

        Destroy(gameObject);
    }

    private Vector2 CalculateAvoidanceVector()
    {
        Vector2 avoidanceVector = Vector2.zero;
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, avoidanceRadius, enemyLayer);

        foreach (Collider2D enemyCollider in nearbyEnemies)
        {
            if (enemyCollider.gameObject == gameObject)
                continue;

            Vector2 directionToEnemy = transform.position - enemyCollider.transform.position;
            float distance = directionToEnemy.magnitude;

            if (distance < avoidanceRadius)
            {
                float strength = (avoidanceRadius - distance) / avoidanceRadius;
                avoidanceVector += directionToEnemy.normalized * strength * avoidanceForce;
            }
        }

        return avoidanceVector;
    }

    public void ApplySpeedModifier(float modifier)
    {
        speedModifier = modifier;
        moveSpeed = baseSpeed * speedModifier;
    }

    public void RemoveSpeedModifier()
    {
        speedModifier = 1f;
        moveSpeed = baseSpeed;
    }

    void OnDrawGizmos()
    {
        // Радиус обнаружения целей
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, targetDetectionRange);

        // Линия к текущей цели
        if (currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }

        // Радиус атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxAttackRange);

        // Оптимальная дистанция
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, optimalAttackDistance);

        // Минимальная дистанция
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, minAttackDistance);

        // Радиус избегания
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, avoidanceRadius);

        // Точка стрельбы
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(firePoint.position, 0.1f);
        }
    }

    // Добавляем новый метод для анимации появления
    private IEnumerator SpawnAnimation()
    {
        // Начинаем с нулевого масштаба
        transform.localScale = Vector3.zero;

        float spawnDuration = 0.3f;
        float elapsed = 0f;

        while (elapsed < spawnDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / spawnDuration;

            // Используем эффект пружины для более интересной анимации
            float scale = Mathf.Sin(progress * Mathf.PI * 0.5f);
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, scale);

            yield return null;
        }

        transform.localScale = originalScale;
    }

    // Вынесем анимацию движения в отдельный метод
    private void AnimateMovement()
    {
        squashTime += Time.deltaTime * squashStretchSpeed;
        float squashStretchFactor = Mathf.Sin(squashTime);

        transform.localScale = new Vector3(
            originalScale.x * (1f - squashAmount * Mathf.Abs(squashStretchFactor)),
            originalScale.y * (1f + stretchAmount * Mathf.Abs(squashStretchFactor)),
            originalScale.z
        );
    }
}