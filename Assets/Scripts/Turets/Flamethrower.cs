using UnityEngine;
using System.Collections;

public class Flamethrower : MonoBehaviour, IBuildingStats
{
    [Header("Настройки здоровья")]
    [Tooltip("Максимальное количество здоровья турели")]
    public float maxHealth = 100f;
    [Tooltip("Текущее здоровье турели (заполняется автоматически)")]
    public float currentHealth;
    [Tooltip("Длительность красной вспышки при получении урона (в секундах)")]
    public float damageFlashDuration = 0.1f;
    [Tooltip("Длительность анимации смерти (в секундах)")]
    public float deathDuration = 0.5f;

    [Header("Настройки турели")]
    [Tooltip("Радиус обнаружения врагов")]
    public float detectionRange = 3f;
    [Tooltip("Скорость поворота турели")]
    public float rotationSpeed = 2f;
    [Tooltip("Основание турели (если не указано, будет использован текущий объект)")]
    public Transform turretBase;
    [Tooltip("Ствол турели (часть, которая поворачивается)")]
    public Transform turretBarrel;
    [Tooltip("Слой, на котором находятся враги")]
    public LayerMask enemyLayer;

    [Header("Настройки поиска целей")]
    [Tooltip("Скорость поворота при поиске целей")]
    public float searchRotationSpeed = 45f;
    [Tooltip("Минимальное время паузы между поворотами")]
    public float minPauseDuration = 0.5f;
    [Tooltip("Максимальное время паузы между поворотами")]
    public float maxPauseDuration = 1.5f;
    [Tooltip("Минимальный угол поворота при поиске")]
    public float minSearchAngle = 30f;
    [Tooltip("Максимальный угол поворота при поиске")]
    public float maxSearchAngle = 90f;

    [Header("Настройки атаки")]
    [Tooltip("Скорострельность (выстрелов в секунду)")]
    public float fireRate = 0.05f;
    [Tooltip("Префаб частицы огня")]
    public GameObject flamePrefab;
    [Tooltip("Точка, откуда вылетает огонь")]
    public Transform firePoint;
    [Tooltip("Скорость полета частиц огня")]
    public float flameSpeed = 8f;
    [Tooltip("Количество частиц огня за один выстрел")]
    public int flamesPerShot = 3;
    [Tooltip("Угол разброса огня (в градусах)")]
    public float spreadAngle = 25f;
    [Tooltip("Урон от каждой частицы огня")]
    public float flameDamage = 0.5f;
    [Tooltip("Время жизни частицы огня")]
    public float flameLifetime = 0.5f;

    [Header("Настройки отдачи")]
    [Tooltip("Расстояние отдачи ствола")]
    public float recoilDistance = 0.1f;
    [Tooltip("Длительность движения ствола назад")]
    public float recoilDuration = 0.05f;
    [Tooltip("Длительность возврата ствола")]
    public float returnDuration = 0.05f;

    private Transform currentTarget;
    private float nextFireTime;
    private bool isRecoiling = false;
    private Vector3 barrelStartPosition;
    private bool isSearching = false;
    private float baseAngle = 0f;
    private Coroutine searchCoroutine;
    private bool isDead = false;
    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;
    private Vector3 originalScale;
    private bool isFlashing = false;

    void Start()
    {
        if (turretBase == null)
            turretBase = transform;

        barrelStartPosition = turretBarrel.localPosition;
        baseAngle = turretBarrel.eulerAngles.z;
        currentHealth = maxHealth;

        // Получаем все SpriteRenderer'ы турели
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
        }

        originalScale = transform.localScale;
        StartSearching();

        // Воспроизводим звук появления турели
        AudioManager.Instance.PlayTurretSpawn(transform.position);
    }
    public void UpdateStats(LevelStats newStats)
    {
        if (newStats == null) return;

        // Обновляем характеристики огнемёта
        maxHealth = (int)newStats.health;
        detectionRange = newStats.range;
        fireRate = newStats.attackSpeed;
        flameDamage = (int)newStats.damage;

        Debug.Log($"Огнемёт обновлен: Здоровье={maxHealth}, Урон={flameDamage}, Скорострельность={fireRate}, Дальность={detectionRange}");
    
        
    }
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (!isFlashing)
        {
            StartCoroutine(DamageFlash());
        }
    }

    IEnumerator DamageFlash()
    {
        isFlashing = true;

        // Меняем цвет на красный
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            renderer.color = Color.red;
        }

        yield return new WaitForSeconds(damageFlashDuration);

        // Возвращаем оригинальный цвет
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].color = originalColors[i];
        }

        isFlashing = false;
    }

    void Die()
    {
        isDead = true;
        StopAllCoroutines();
        
        // Воспроизводим звук смерти турели
        AudioManager.Instance.PlayTurretDeath(transform.position);
        
        StartCoroutine(DeathAnimation());
    }

    IEnumerator DeathAnimation()
    {
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < deathDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / deathDuration;

            // Уменьшаем размер
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);

            // Постепенно делаем прозрачным
            foreach (SpriteRenderer renderer in spriteRenderers)
            {
                Color color = renderer.color;
                color.a = 1 - progress;
                renderer.color = color;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    void StartSearching()
    {
        if (isDead) return;

        if (searchCoroutine != null)
        {
            StopCoroutine(searchCoroutine);
        }
        isSearching = true;
        searchCoroutine = StartCoroutine(SearchRoutine());
    }

    void Update()
    {
        if (isDead) return;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(turretBase.position, detectionRange, enemyLayer);
        float closestDistance = float.MaxValue;
        Transform closestEnemy = null;

        foreach (Collider2D enemy in enemies)
        {
            float distance = Vector2.Distance(turretBase.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        if (closestEnemy != null)
        {
            currentTarget = closestEnemy;
            isSearching = false;
            if (searchCoroutine != null)
            {
                StopCoroutine(searchCoroutine);
            }
            TrackAndShoot();
        }
        else if (!isSearching)
        {
            currentTarget = null;
            StartSearching();
        }
    }

    void TrackAndShoot()
    {
        if (currentTarget == null || isDead) return;

        Vector3 targetDirection = currentTarget.position - turretBarrel.position;
        float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        turretBarrel.rotation = Quaternion.RotateTowards(turretBarrel.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (Time.time >= nextFireTime && !isRecoiling)
        {
            float angleDifference = Mathf.Abs(Mathf.DeltaAngle(turretBarrel.eulerAngles.z, targetAngle));
            if (angleDifference < 15f)
            {
                StartCoroutine(ShootWithRecoil());
                nextFireTime = Time.time + 1f / fireRate;
            }
        }
    }

    IEnumerator SearchRoutine()
    {
        while (isSearching && !isDead)
        {
            float randomAngle = Random.Range(minSearchAngle, maxSearchAngle);
            bool turnRight = Random.value > 0.5f;
            float targetAngle = (baseAngle - 90f) + (turnRight ? randomAngle : -randomAngle);

            float startAngle = turretBarrel.eulerAngles.z;
            float rotationTime = Mathf.Abs(Mathf.DeltaAngle(startAngle, targetAngle)) / searchRotationSpeed;
            float elapsedTime = 0f;

            while (elapsedTime < rotationTime && isSearching && !isDead)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / rotationTime;
                float newAngle = Mathf.LerpAngle(startAngle, targetAngle, t);
                turretBarrel.rotation = Quaternion.Euler(0, 0, newAngle);
                yield return null;
            }

            if (!isSearching || isDead) yield break;

            float pauseDuration = Random.Range(minPauseDuration, maxPauseDuration);
            yield return new WaitForSeconds(pauseDuration);
        }
    }

    IEnumerator ShootWithRecoil()
    {
        if (isDead) yield break;

        isRecoiling = true;

        Vector3 recoilDirection = -turretBarrel.right;
        ShootFlames();

        float elapsedTime = 0;
        Vector3 startPos = turretBarrel.localPosition;
        Vector3 recoilPos = startPos + recoilDirection * recoilDistance;

        while (elapsedTime < recoilDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / recoilDuration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            turretBarrel.localPosition = Vector3.Lerp(startPos, recoilPos, t);
            yield return null;
        }

        elapsedTime = 0;
        while (elapsedTime < returnDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / returnDuration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            turretBarrel.localPosition = Vector3.Lerp(recoilPos, barrelStartPosition, t);
            yield return null;
        }

        turretBarrel.localPosition = barrelStartPosition;
        isRecoiling = false;
    }

    void ShootFlames()
    {
        if (isDead) return;

        // Воспроизводим звук выстрела огнемета
        AudioManager.Instance.PlayFlamethrowerShot(firePoint.position);

        for (int i = 0; i < flamesPerShot; i++)
        {
            Vector2 baseDirection;
            if (currentTarget != null)
            {
                baseDirection = (currentTarget.position - firePoint.position).normalized;
            }
            else
            {
                float angle = turretBarrel.eulerAngles.z + 90f;
                baseDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            }

            float randomSpread = Random.Range(-spreadAngle, spreadAngle);
            Vector2 spreadDirection = Quaternion.Euler(0, 0, randomSpread) * baseDirection;

            GameObject flame = Instantiate(flamePrefab, firePoint.position, Quaternion.identity);
            FlameParticle flameScript = flame.GetComponent<FlameParticle>();
            if (flameScript == null)
            {
                flameScript = flame.AddComponent<FlameParticle>();
            }

            flameScript.enemyLayer = enemyLayer;
            float randomSpeed = flameSpeed * Random.Range(0.8f, 1.2f);
            flameScript.Initialize(randomSpeed, spreadDirection, flameDamage, flameLifetime);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 position = Application.isPlaying ? turretBase.position : transform.position;
        Gizmos.DrawWireSphere(position, detectionRange);

        if (turretBarrel != null && firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(firePoint.position, firePoint.position + (turretBarrel.right * 2f));
        }
    }
}