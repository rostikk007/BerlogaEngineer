using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//ну ё маё
public class MeleeEnemy : MonoBehaviour
{
    [Header("Награда за убийство")]
    [Tooltip("Количество монет, получаемых за убийство врага")]
    public int coinsReward = 1;
    [Tooltip("Количество очков улучшения, получаемых за убийство врага (0.1 = одно очко за 10 убийств)")]
    public float upgradePointsReward = 0.1f;

    // Статическая переменная для накопления дробных очков
    private static float upgradePointsAccumulated = 0f;

    [Header("Настройки здоровья")]
    [Tooltip("Максимальное количество здоровья врага")]
    public float maxHealth = 50f;
    [Tooltip("Текущее здоровье врага (заполняется автоматически)")]
    public float currentHealth;
    [Tooltip("Длительность красной вспышки при получении урона (в секундах)")]
    public float damageFlashDuration = 0.1f;
    private Color originalColor;
    private bool isDead = false;

    [Header("Настройки движения")]
    [Tooltip("Скорость передвижения врага")]
    public float moveSpeed = 2f;
    [Tooltip("Минимальное расстояние, на котором враг останавливается перед целью")]
    public float minDistanceToTarget = 1f;
    [Tooltip("Время сглаживания движения (чем меньше, тем резче движения)")]
    public float smoothTime = 0.1f;
    [Tooltip("Скорость поворота врага к цели")]
    public float rotationSpeed = 5f;

    [Header("Настройки избегания")]
    [Tooltip("Радиус обнаружения других врагов для избегания столкновений")]
    public float avoidanceRadius = 1f;
    [Tooltip("Сила отталкивания от других врагов")]
    public float avoidanceForce = 1f;
    [Tooltip("Слой, на котором находятся враги (для избегания столкновений)")]
    public LayerMask enemyLayer;

    [Header("Настройки целей")]
    [Tooltip("Список приоритетов целей. Добавьте все типы целей, которые должен атаковать враг")]
    public List<TargetPriority> targetPriorities = new List<TargetPriority>();
    [Tooltip("Радиус обнаружения целей")]
    public float targetDetectionRange = 10f;

    [Header("Настройки анимации сжатия")]
    [Tooltip("Скорость анимации сжатия/растяжения")]
    public float squashStretchSpeed = 8f;
    [Tooltip("Сила сжатия (0.2 = сжатие на 20%)")]
    public float squashAmount = 0.2f;
    [Tooltip("Сила растяжения (0.1 = растяжение на 10%)")]
    public float stretchAmount = 0.1f;
    private float squashTime;

    [Header("Настройки атаки")]
    [Tooltip("Дистанция, с которой враг может атаковать цель")]
    public float attackRange = 1.2f;
    [Tooltip("Время между атаками (в секундах)")]
    public float attackCooldown = 1f;
    [Tooltip("Количество урона, наносимого при атаке")]
    public int attackDamage = 10;

    [Header("Настройки звуков")]
    [Tooltip("Минимальный интервал между звуками получения урона")]
    public float damageSoundInterval = 0.2f;
    private float lastDamageSoundTime;
    // Приватные переменные для внутренней работы
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


    /// <summary>
    /// Класс для настройки приоритетов целей в инспекторе
    /// </summary>
    [System.Serializable]
    public class TargetPriority
    {
        [Tooltip("Название цели (для удобства)")]
        public string targetName;

        [Tooltip("Слой, на котором находится цель")]
        public LayerMask targetLayer;

        [Tooltip("Приоритет цели (0 - самый важный)")]
        public int priority;

        [Tooltip("Вес приоритета (множитель важности, по умолчанию 1)")]
        public float priorityWeight = 1f;
    }

    /// <summary>
    /// Класс для хранения информации о потенциальной цели
    /// </summary>
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

        // Сортируем приоритеты по возрастанию
        targetPriorities.Sort((a, b) => a.priority.CompareTo(b.priority));

        // Воспроизводим звук появления жука
        AudioManager.Instance.PlayBugSpawn(transform.position);

        // Добавляем анимацию появления
        StartCoroutine(SpawnAnimation());
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

    void Update()
    {
        if (isDead) return;

        FindNearestTarget();

        if (currentTarget == null) return;

        Vector2 enemyPosition = transform.position;
        Vector2 targetPos = currentTarget.position;
        float distance = Vector2.Distance(enemyPosition, targetPos);
        Vector2 directionToTarget = (targetPos - enemyPosition).normalized;

        Vector2 avoidanceVector = CalculateAvoidanceVector();
        Vector2 finalDirection = (directionToTarget + avoidanceVector).normalized;

        float angle = Mathf.Atan2(finalDirection.y, finalDirection.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        if (distance > minDistanceToTarget && !isAttacking)
        {
            targetPosition = Vector2.Lerp(
                targetPosition,
                (Vector2)transform.position + finalDirection * moveSpeed,
                Time.deltaTime * moveSpeed
            );

            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref smoothDampVelocity,
                smoothTime
            );

            squashTime += Time.deltaTime * squashStretchSpeed;
            float squashStretchFactor = Mathf.Sin(squashTime);

            transform.localScale = new Vector3(
                originalScale.x * (1f - squashAmount * Mathf.Abs(squashStretchFactor)),
                originalScale.y * (1f + stretchAmount * Mathf.Abs(squashStretchFactor)),
                originalScale.z
            );
        }
        else
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                originalScale,
                Time.deltaTime * squashStretchSpeed
            );
        }

        if (distance <= attackRange && Time.time >= nextAttackTime && !isAttacking)
        {
            StartCoroutine(Attack());
        }
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
    void FindNearestTarget()
    {
        List<TargetInfo> potentialTargets = new List<TargetInfo>();

        // Проходим по всем приоритетам и собираем цели
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

        // Если есть цели, выбираем лучшую по взвешенному приоритету
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

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Проигрываем звук получения урона с интервалом
        if (Time.time - lastDamageSoundTime >= damageSoundInterval)
        {
            AudioManager.Instance.PlayBugAttack(transform.position);
            lastDamageSoundTime = Time.time;
        }

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
        Color currentColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(damageFlashDuration);
        spriteRenderer.color = currentColor;
        isFlashing = false;
    }

    private void Die()
    {
        isDead = true;

        // Проигрываем звук смерти
        AudioManager.Instance.PlayBugDeath(transform.position);

        if (GameManager.Instance != null)
        {
            // Начисляем монеты
            GameManager.Instance.AddCoins(coinsReward);

            // Накапливаем дробные очки улучшения
            upgradePointsAccumulated += upgradePointsReward;

            // Проверяем, накопилось ли целое число очков
            if (upgradePointsAccumulated >= 1f)
            {
                int pointsToAdd = Mathf.FloorToInt(upgradePointsAccumulated);
                GameManager.Instance.AddUpgradePoints(pointsToAdd);

                // Вычитаем начисленные очки, оставляя остаток
                upgradePointsAccumulated -= pointsToAdd;

                // Для отладки
                Debug.Log($"Начислено {pointsToAdd} очков улучшения. Остаток: {upgradePointsAccumulated:F2}");
            }
        }
        else
        {
            Debug.LogWarning("GameManager не найден! Награда за убийство врага не начислена.");
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

    IEnumerator Attack()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        Vector3 startPos = transform.position;
        Vector3 attackDirection = (currentTarget.position - transform.position).normalized;
        Vector3 targetPos = startPos + attackDirection * 0.3f;

        spriteRenderer.color = Color.yellow;

        // Проигрываем звук атаки
        AudioManager.Instance.PlayBugAttack(transform.position);

        float elapsed = 0f;
        float attackDuration = 0.06f;

        while (elapsed < attackDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / attackDuration);
            yield return null;
        }

        if (currentTarget != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
            if (distanceToTarget <= attackRange)
            {
                // При успешной атаке проигрываем соответствующий звук повреждения цели
                if (currentTarget.CompareTag("Buildings"))
                {
                    // Определяем тип постройки и проигрываем соответствующий звук
                    if (currentTarget.GetComponent<Turret>() != null)
                    {
                        AudioManager.Instance.PlayTurretDamaged(currentTarget.position);
                    }
                    else if (currentTarget.GetComponent<Apiary>() != null)
                    {
                        AudioManager.Instance.PlayApiaryDamaged(currentTarget.position);
                    }
                    else if (currentTarget.GetComponent<Barricade>() != null)
                    {
                        AudioManager.Instance.PlayWallDamaged(currentTarget.position);
                    }
                    else if (currentTarget.GetComponent<IronProcessor>() != null)
                    {
                        AudioManager.Instance.PlayBuildingDamaged(currentTarget.position);
                    }
                }

                IDamageable damageable = currentTarget.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(attackDamage);
                }
                else
                {
                    // Проверяем конкретные компоненты
                    PlayerHealth playerHealth = currentTarget.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(attackDamage);
                    }

                    Turret turret = currentTarget.GetComponent<Turret>();
                    if (turret != null)
                    {
                        turret.TakeDamage(attackDamage);
                    }

                    Flamethrower flamethrower = currentTarget.GetComponent<Flamethrower>();
                    if (flamethrower != null)
                    {
                        flamethrower.TakeDamage(attackDamage);
                    }

                    Barricade barricade = currentTarget.GetComponent<Barricade>();
                    if (barricade != null)
                    {
                        barricade.TakeDamage(attackDamage);
                    }

                    Apiary apiary = currentTarget.GetComponent<Apiary>();
                    if (apiary != null)
                    {
                        apiary.TakeDamage(attackDamage);
                    }

                    IronProcessor ironProcessor = currentTarget.GetComponent<IronProcessor>();
                    if (ironProcessor != null)
                    {
                        ironProcessor.TakeDamage(attackDamage);
                    }

                    // Добавляем проверку на антенну
                    IVI21Antenna antenna = currentTarget.GetComponent<IVI21Antenna>();
                    if (antenna != null)
                    {
                        antenna.TakeDamage(attackDamage);
                        AudioManager.Instance.PlayBuildingDamaged(currentTarget.position);
                    }
                }
            }
        }

        elapsed = 0f;
        while (elapsed < attackDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(targetPos, startPos, elapsed / attackDuration);
            yield return null;
        }

        transform.position = startPos;
        spriteRenderer.color = originalColor;
        isAttacking = false;
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
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Радиус избегания других врагов
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, avoidanceRadius);

        // Минимальная дистанция до цели
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, minDistanceToTarget);

        // Направление движения
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.up * 0.5f);
    }
}

public interface IDamageable
{
    void TakeDamage(float damage);
}