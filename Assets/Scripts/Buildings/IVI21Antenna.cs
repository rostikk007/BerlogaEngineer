using UnityEngine;
using System.Collections;

public class IVI21Antenna : MonoBehaviour
{
    [Header("Основные настройки")]
    [Tooltip("Максимальное здоровье антенны")]
    public float maxHealth = 100f;

    [Tooltip("Текущее здоровье антенны")]
    private float currentHealth;

    [Header("Настройки сбора данных")]
    [Tooltip("Количество собираемых данных в секунду")]
    public float dataCollectionRate = 1f;

    [Tooltip("Прогресс сбора данных (0-100%)")]
    public float dataCollectionProgress = 0f;

    [Tooltip("Целевое количество данных для победы")]
    public float targetDataAmount = 100f;

    [Header("Настройки визуальных эффектов")]
    [Tooltip("Скорость вращения антенны")]
    public float rotationSpeed = 15f;

    [Tooltip("Амплитуда пульсации при сборе данных")]
    public float pulseAmplitude = 0.1f;

    [Tooltip("Частота пульсации")]
    public float pulseFrequency = 2f;

    [Tooltip("Длительность эффекта получения урона")]
    public float damageFlashDuration = 0.1f;

    [Header("Настройки частиц")]
    [Tooltip("Система частиц для сбора данных")]
    public ParticleSystem dataCollectionParticles;

    [Tooltip("Система частиц для получения урона")]
    public ParticleSystem damageParticles;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Vector3 originalScale;
    private bool isDead = false;
    private bool isFlashing = false;
    private float pulseTime = 0f;

    void Start()
    {
        // Инициализация компонентов
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        originalScale = transform.localScale;

        // Запуск основных процессов
        StartCoroutine(DataCollectionRoutine());
        StartCoroutine(RotationRoutine());

        // Воспроизведение звука появления
        AudioManager.Instance.PlayTurretSpawn(transform.position);
    }

    void Update()
    {
        if (isDead) return;

        // Эффект пульсации
        pulseTime += Time.deltaTime;
        float pulseScale = 1f + pulseAmplitude * Mathf.Sin(pulseTime * pulseFrequency);
        transform.localScale = originalScale * pulseScale;
    }

    private IEnumerator DataCollectionRoutine()
    {
        while (!isDead && dataCollectionProgress < targetDataAmount)
        {
            dataCollectionProgress += dataCollectionRate * Time.deltaTime;

            // Активация системы частиц при сборе данных
            if (dataCollectionParticles != null && !dataCollectionParticles.isPlaying)
            {
                dataCollectionParticles.Play();
            }

            yield return null;
        }

        if (dataCollectionProgress >= targetDataAmount)
        {
            // Здесь можно вызвать событие победы
            //GameManager.Instance.WinGame();
        }
    }

    private IEnumerator RotationRoutine()
    {
        while (!isDead)
        {
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Воспроизведение звука получения урона
        AudioManager.Instance.PlayTurretDamaged(transform.position);

        // Активация частиц урона
        if (damageParticles != null)
        {
            damageParticles.Play();
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

    private IEnumerator DamageFlash()
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

    private void Die()
    {
        isDead = true;

        // Воспроизведение звука уничтожения
        AudioManager.Instance.PlayTurretDeath(transform.position);

        StartCoroutine(DeathAnimation());
    }

    private IEnumerator DeathAnimation()
    {
        float duration = 1.5f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        // Остановка всех частиц
        if (dataCollectionParticles != null) dataCollectionParticles.Stop();
        if (damageParticles != null) damageParticles.Stop();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Уменьшение размера и прозрачности
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 1 - progress;
                spriteRenderer.color = color;
            }

            // Быстрое вращение при уничтожении
            transform.Rotate(Vector3.forward * (rotationSpeed * 3) * Time.deltaTime);

            yield return null;
        }

        // Здесь можно вызвать событие проигрыша
        //GameManager.Instance.LoseGame();

        Destroy(gameObject);
    }
}
