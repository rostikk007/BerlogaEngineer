using UnityEngine;
using System.Collections;

public class Apiary : MonoBehaviour
{
    [Header("Настройки дохода")]
    [Tooltip("Количество монет, получаемых за одну генерацию")]
    public int coinsPerGeneration = 5;

    [Tooltip("Интервал генерации монет в секундах")]
    public float generationInterval = 3f;

    [Header("Настройки эффектов")]
    [Tooltip("Показывать эффект при генерации монет")]
    public bool showGenerationEffect = true;

    [Tooltip("Продолжительность эффекта в секундах")]
    public float effectDuration = 0.5f;

    [Tooltip("Масштаб увеличения при эффекте")]
    public float scaleMultiplier = 1.2f;

    [Header("Настройки здоровья")]
    [Tooltip("Максимальное здоровье пасеки")]
    public float maxHealth = 100f;

    [Tooltip("Текущее здоровье пасеки")]
    private float currentHealth;

    [Tooltip("Длительность красной вспышки при уроне")]
    public float damageFlashDuration = 0.1f;

    [Header("Настройки масштаба")]
    [SerializeField] private Vector3 targetScale = Vector3.one;

    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isGenerating = false;
    private bool isDead = false;
    private bool isEffectRunning = false;
    private bool isInitialized = false;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        StartCoroutine(InitializeAfterAnimations());

        // Воспроизводим звук появления пасеки
        AudioManager.Instance.PlayApiarySpawn(transform.position);
    }

    private IEnumerator InitializeAfterAnimations()
    {
        // Ждем завершения всех начальных анимаций
        yield return new WaitForSeconds(1f);

        // Устанавливаем целевой масштаб
        transform.localScale = targetScale;
        originalScale = targetScale;
        isInitialized = true;

        StartGeneration();
    }

    private void Update()
    {
        if (isInitialized && !isDead && !isEffectRunning)
        {
            // Если масштаб отличается от целевого - восстанавливаем
            if (Vector3.Distance(transform.localScale, targetScale) > 0.01f)
            {
                transform.localScale = targetScale;
            }
        }
    }

    void StartGeneration()
    {
        if (!isGenerating && !isDead)
        {
            StartCoroutine(GenerateCoinsRoutine());
        }
    }

    IEnumerator GenerateCoinsRoutine()
    {
        isGenerating = true;

        while (!isDead)
        {
            yield return new WaitForSeconds(generationInterval);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoins(coinsPerGeneration);

                // Воспроизводим звук добычи меда
                AudioManager.Instance.PlayHoneyHarvest(transform.position);

                if (showGenerationEffect && !isEffectRunning)
                {
                    StartCoroutine(ShowGenerationEffect());
                }
            }
        }

        isGenerating = false;
    }

    IEnumerator ShowGenerationEffect()
    {
        if (isEffectRunning || !isInitialized) yield break;

        isEffectRunning = true;
        Vector3 effectTargetScale = targetScale * scaleMultiplier;
        float elapsed = 0f;

        try
        {
            // Увеличение масштаба
            while (elapsed < effectDuration / 2)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / (effectDuration / 2);
                transform.localScale = Vector3.Lerp(targetScale, effectTargetScale, progress);
                yield return null;
            }

            // Возврат к целевому масштабу
            elapsed = 0f;
            while (elapsed < effectDuration / 2)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / (effectDuration / 2);
                transform.localScale = Vector3.Lerp(effectTargetScale, targetScale, progress);
                yield return null;
            }
        }
        finally
        {
            transform.localScale = targetScale;
            isEffectRunning = false;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        StartCoroutine(DamageFlash());

        // Воспроизводим звук получения урона пасекой
        AudioManager.Instance.PlayApiaryDamaged(transform.position);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(damageFlashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    void Die()
    {
        isDead = true;
        StopAllCoroutines();

        // Воспроизводим звук смерти пасеки
        AudioManager.Instance.PlayApiaryDeath(transform.position);

        StartCoroutine(DeathAnimation());
    }

    IEnumerator DeathAnimation()
    {
        float duration = 1f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Уменьшаем размер и прозрачность
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
}