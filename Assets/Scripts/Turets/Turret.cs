using UnityEngine;
using System.Collections;

public class Turret : MonoBehaviour, IBuildingStats
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float damageFlashDuration = 0.1f;
    public float deathDuration = 0.5f;

    [Header("Turret Settings")]
    public float detectionRange = 5f;
    public float rotationSpeed = 2f;
    public Transform turretBase;
    public Transform turretBarrel;
    public LayerMask enemyLayer;

    [Header("Search Settings")]
    public float searchRotationSpeed = 45f;
    public float minPauseDuration = 0.5f;
    public float maxPauseDuration = 1.5f;
    public float minSearchAngle = 30f;
    public float maxSearchAngle = 90f;

    [Header("Attack Settings")]
    public float fireRate = 1f;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;

    [Header("Recoil Settings")]
    public float recoilDistance = 0.2f;
    public float recoilDuration = 0.1f;
    public float returnDuration = 0.1f;
    [Header("Shooting Settings")]
    public float minShootingDistance = 1f;
    private Transform currentTarget;
    private float nextFireTime;
    private bool isRecoiling = false;
    private Vector3 barrelStartPosition;
    private bool isSearching = false;
    private float baseAngle = 0f;
    private float targetSearchAngle;
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

        // Обновляем характеристики турели
        maxHealth = (int)newStats.health;
        detectionRange = newStats.range;
        fireRate = newStats.attackSpeed;

        // Обновляем урон пули
        if (bulletPrefab != null)
        {
            var bulletComponent = bulletPrefab.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.damage = (int)newStats.damage;
            }
        }

        Debug.Log($"Турель обновлена: Здоровье={maxHealth}, Урон={newStats.damage}, Скорострельность={fireRate}, Дальность={detectionRange}");
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

        // Проверка расстояния до цели
        float distanceToTarget = Vector2.Distance(turretBarrel.position, currentTarget.position);
        if (Time.time >= nextFireTime && !isRecoiling && distanceToTarget > minShootingDistance)
        {
            float angleDifference = Mathf.Abs(Mathf.DeltaAngle(turretBarrel.eulerAngles.z, targetAngle));
            if (angleDifference < 5f)
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
            float targetAngle = baseAngle + (turnRight ? randomAngle : -randomAngle);

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

        Vector3 recoilDirection = -turretBarrel.up;
        Shoot();

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

    void Shoot()
    {
        if (isDead) return;

        if (bulletPrefab != null && firePoint != null && currentTarget != null)
        {
            // Воспроизводим звук выстрела
            AudioManager.Instance.PlayBasicTurretShot(firePoint.position);

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript == null)
            {
                bulletScript = bullet.AddComponent<Bullet>();
            }
            Vector2 directionToTarget = (currentTarget.position - firePoint.position).normalized;
            bulletScript.Initialize(bulletSpeed, directionToTarget);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 position = Application.isPlaying ? turretBase.position : transform.position;
        Gizmos.DrawWireSphere(position, detectionRange); // Область обнаружения

        // Рисуем круг, где турель не будет стрелять
        Gizmos.color = Color.red; // Цвет для зоны, где не будет стрельбы
        Gizmos.DrawWireSphere(position, minShootingDistance); // Зона, где не будет стрельбы

        if (turretBarrel != null && firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(firePoint.position, firePoint.position + (turretBarrel.up * 2f));
        }
    }
}