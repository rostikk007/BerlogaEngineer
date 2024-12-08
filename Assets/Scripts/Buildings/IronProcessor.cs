using UnityEngine;
using System.Collections;

public class IronProcessor : MonoBehaviour
{
    [Header("Настройки добычи")]
    [Tooltip("Количество железа, получаемого за одну генерацию")]
    public int ironPerGeneration = 3;

    [Tooltip("Интервал генерации железа в секундах")]
    public float generationInterval = 5f;

    [Header("Настройки эффектов")]
    [Tooltip("Показывать эффект при добыче железа")]
    public bool showProcessingEffect = true;

    [Tooltip("Продолжительность эффекта в секундах")]
    public float effectDuration = 0.7f;

    [Tooltip("Масштаб увеличения при эффекте")]
    public float scaleMultiplier = 1.15f;

    [Header("Настройки здоровья")]
    [Tooltip("Максимальное здоровье переработчика")]
    public float maxHealth = 150f;

    [Tooltip("Текущее здоровье переработчика")]
    private float currentHealth;

    [Tooltip("Длительность красной вспышки при уроне")]
    public float damageFlashDuration = 0.1f;

    [Header("Настройки частиц")]
    [Tooltip("Система частиц дыма")]
    public ParticleSystem smokeEffect;

    [Header("Настройки масштаба")]
    [SerializeField] private Vector3 targetScale = Vector3.one;

    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isProcessing = false;
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

        // Воспроизводим звук появления переработчика
        AudioManager.Instance.PlayBuildingSpawn(transform.position);

        // Запускаем систему частиц дыма, если она назначена
        if (smokeEffect != null)
        {
            smokeEffect.Play();
        }
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

    private IEnumerator InitializeAfterAnimations()
    {
        yield return new WaitForSeconds(1f);
        transform.localScale = targetScale;
        originalScale = targetScale;
        isInitialized = true;
        StartProcessing();
    }

    void StartProcessing()
    {
        if (!isProcessing && !isDead)
        {
            StartCoroutine(ProcessIronRoutine());
        }
    }

    IEnumerator ProcessIronRoutine()
    {
        isProcessing = true;

        while (!isDead)
        {
            yield return new WaitForSeconds(generationInterval);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddIron(ironPerGeneration);

                // Воспроизводим звук добычи железа
                AudioManager.Instance.PlayIronProcessing(transform.position);

                if (showProcessingEffect && !isEffectRunning)
                {
                    StartCoroutine(ShowProcessingEffect());
                }
            }
        }

        isProcessing = false;
    }

    IEnumerator ShowProcessingEffect()
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

        // Воспроизводим звук получения урона
        AudioManager.Instance.PlayBuildingDamaged(transform.position);

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

        // Останавливаем эффект дыма
        if (smokeEffect != null)
        {
            smokeEffect.Stop();
        }

        // Воспроизводим звук уничтожения
        AudioManager.Instance.PlayBuildingDestroyed(transform.position);

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