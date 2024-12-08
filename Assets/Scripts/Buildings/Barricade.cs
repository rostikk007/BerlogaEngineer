using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Barricade : MonoBehaviour, IBuildingStats
{
    [Header("Настройки здоровья")]
    [Tooltip("Максимальное количество здоровья баррикады")]
    public float maxHealth = 150f;
    [Tooltip("Текущее здоровье баррикады (заполняется автоматически)")]
    public float currentHealth;
    [Tooltip("Длительность красной вспышки при получении урона (в секундах)")]
    public float damageFlashDuration = 0.1f;
    [Tooltip("Длительность анимации смерти (в секундах)")]
    public float deathDuration = 0.5f;

    [Header("Настройки замедления")]
    [Tooltip("Сила замедления (0.3 = снижение скорости на 70%)")]
    public float slowAmount = 0.3f;
    [Tooltip("Слои, на которых находятся враги")]
    public LayerMask enemyLayers;

    [Header("Настройки обнаружения")]
    [Tooltip("Дополнительный радиус обнаружения врагов")]
    public float detectionRadius = 1.5f;

    // Приватные переменные
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Vector3 originalScale;
    private bool isDead = false;
    private bool isFlashing = false;
    private HashSet<MeleeEnemy> touchingEnemies = new HashSet<MeleeEnemy>();
    private AudioManager audioManager;

    void Start()
    {
        // Debug.Log("Баррикада инициализирована");
        audioManager = AudioManager.Instance;

        var collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            if (!collider.isTrigger)
            {
                Debug.LogError("BoxCollider2D должен быть установлен как триггер!");
                collider.isTrigger = true;
            }
        }
        else
        {
            Debug.LogError("BoxCollider2D не найден на объекте баррикады!");
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        originalScale = transform.localScale;
        currentHealth = maxHealth;

        // Проигрываем звук появления стены
        if (audioManager != null)
        {
            audioManager.PlayWallSpawn(transform.position);
        }
    }

    void Update()
    {
        if (isDead) return;

        // Дополнительная проверка врагов в радиусе
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayers);

        foreach (Collider2D collider in nearbyColliders)
        {
            MeleeEnemy enemy = collider.GetComponent<MeleeEnemy>();
            if (enemy != null && !touchingEnemies.Contains(enemy))
            {
                BoxCollider2D barricadeCollider = GetComponent<BoxCollider2D>();
                if (barricadeCollider != null)
                {
                    Vector2 closestPoint = barricadeCollider.bounds.ClosestPoint(enemy.transform.position);
                    float distance = Vector2.Distance(closestPoint, enemy.transform.position);

                    if (distance < 0.1f)
                    {
                        enemy.ApplySpeedModifier(slowAmount);
                        touchingEnemies.Add(enemy);
                        Debug.Log($"Враг {enemy.name} замедлен при входе в зону");
                    }
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        Debug.Log($"Триггер сработал с объектом: {other.gameObject.name}");

        MeleeEnemy enemy = other.GetComponent<MeleeEnemy>();
        if (enemy != null && !touchingEnemies.Contains(enemy))
        {
            //Debug.Log($"Замедляем врага: {enemy.gameObject.name}");
            enemy.ApplySpeedModifier(slowAmount);
            touchingEnemies.Add(enemy);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        MeleeEnemy enemy = other.GetComponent<MeleeEnemy>();
        if (enemy != null && touchingEnemies.Contains(enemy))
        {
            // Снимаем замедление при выходе из зоны
            enemy.RemoveSpeedModifier();
            touchingEnemies.Remove(enemy);
        }
    }
    public void UpdateStats(LevelStats newStats)
    {
        if (newStats == null) return;

        // Обновляем характеристики стены
        maxHealth = newStats.health;
        slowAmount = 1 - (newStats.attackSpeed / 100f); // Конвертируем скорость в замедление
        detectionRadius = newStats.range;

        Debug.Log($"Стена обновлена: Здоровье={maxHealth}, Замедление={slowAmount * 100}%, Радиус={detectionRadius}");

        // Обновляем текущее здоровье пропорционально увеличению максимального
        float healthPercentage = currentHealth / maxHealth;
        currentHealth = maxHealth * healthPercentage;
    }
    void Die()
    {
        isDead = true;

        // Снимаем замедление со всех врагов при уничтожении баррикады
        foreach (var enemy in touchingEnemies)
        {
            if (enemy != null)
            {
                enemy.RemoveSpeedModifier();
                //Debug.Log($"Снято замедление с врага {enemy.name} при уничтожении баррикады");
            }
        }
        touchingEnemies.Clear();

        // Проигрываем звук уничтожения стены
        if (audioManager != null)
        {
            audioManager.PlayWallDeath(transform.position);
        }

        StartCoroutine(DeathAnimation());
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Проигрываем звук получения урона
        if (audioManager != null)
        {
            audioManager.PlayWallDamaged(transform.position);
        }

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

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(damageFlashDuration);
            spriteRenderer.color = originalColor;
        }

        isFlashing = false;
    }

    IEnumerator DeathAnimation()
    {
        if (GetComponent<Collider2D>() != null)
        {
            GetComponent<Collider2D>().enabled = false;
        }

        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < deathDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / deathDuration;

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);

            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 1 - progress;
                spriteRenderer.color = color;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        // Отображаем зону действия баррикады
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            // Рисуем коллайдер
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Vector3 size = new Vector3(
                boxCollider.size.x * transform.localScale.x,
                boxCollider.size.y * transform.localScale.y,
                0.1f
            );
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(boxCollider.offset, size);
        }

        // Рисуем радиус обнаружения
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}