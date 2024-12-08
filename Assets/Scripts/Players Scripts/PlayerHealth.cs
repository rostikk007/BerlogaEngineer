using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Настройки здоровья")]
    [Tooltip("Максимальное здоровье игрока")]
    public float maxHealth = 100f;

    [Tooltip("Текущее здоровье игрока")]
    public float currentHealth;

    [Header("Настройки UI")]
    [Tooltip("Полоска здоровья")]
    public Image healthBar;

    [Tooltip("Canvas полоски здоровья")]
    [SerializeField] private GameObject healthBarCanvasObject;

    [Tooltip("Смещение полоски здоровья относительно игрока")]
    public Vector3 healthBarOffset = new Vector3(0f, 1f, 0f);

    [Header("Настройки анимации")]
    [Tooltip("Время отображения полоски здоровья")]
    public float showDuration = 2f;

    [Tooltip("Время затухания полоски здоровья")]
    public float fadeDuration = 0.5f;

    [Header("Настройки тряски")]
    [Tooltip("Сила тряски при получении урона")]
    public float shakeAmount = 0.1f;

    [Tooltip("Длительность тряски")]
    public float shakeDuration = 0.2f;

    [Header("Настройки звука")]
    [Tooltip("Минимальный интервал между звуками урона")]
    public float damageAudioInterval = 0.2f;

    [Header("Настройки анимации смерти")]
    [Tooltip("Длительность анимации смерти")]
    public float deathAnimationDuration = 0.5f;

    [Tooltip("Скорость вращения при смерти")]
    public float deathRotationSpeed = 720f;

    [Tooltip("Конечный размер при смерти")]
    public float deathFinalScale = 0.1f;

    private CanvasGroup healthBarCanvasGroup;
    private Vector3 healthBarOriginalOffset;
    private Coroutine fadeCoroutine;
    private Coroutine shakeCoroutine;
    private bool isDead = false;
    private float lastDamageSoundTime;
    private Transform healthBarCanvasTransform;
    private PlayerLandingSystem landingSystem;

    private void Awake()
    {
        if (healthBarCanvasObject != null)
        {
            healthBarCanvasObject.SetActive(false);
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        isDead = false;
        healthBarOriginalOffset = healthBarOffset;

        if (healthBarCanvasObject != null)
        {
            healthBarCanvasTransform = healthBarCanvasObject.transform;
            healthBarCanvasGroup = healthBarCanvasObject.GetComponent<CanvasGroup>();
            if (healthBarCanvasGroup == null)
            {
                healthBarCanvasGroup = healthBarCanvasObject.AddComponent<CanvasGroup>();
            }
            healthBarCanvasGroup.alpha = 0f;
        }
        else
        {
            Debug.LogError("Health Bar Canvas Object не назначен в инспекторе!", this);
        }

        UpdateHealthBar();
        UpdateHealthBarPosition();

        landingSystem = FindObjectOfType<PlayerLandingSystem>();
        if (landingSystem == null)
        {
            Debug.LogError("PlayerLandingSystem не найден на сцене!");
        }
    }

    private void OnEnable()
    {
        // Оставляем пустым, чтобы контролировать видимость вручную
    }

    private void OnDisable()
    {
        // Оставляем пустым, чтобы контролировать видимость вручную
    }

    public void EnableHealthBar()
    {
        ShowHealthBarCanvas();
        ShowHealthBar();
    }

    public void DisableHealthBar()
    {
        HideHealthBar();
    }

    void LateUpdate()
    {
        if (gameObject.activeSelf)
        {
            UpdateHealthBarPosition();
        }
        else
        {
            HideHealthBar();
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead || !gameObject.activeSelf) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (Time.time - lastDamageSoundTime >= damageAudioInterval)
        {
            AudioManager.Instance.PlayPlayerDamage(transform.position);
            lastDamageSoundTime = Time.time;
        }

        UpdateHealthBar();
        ShowHealthBar();

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(ShakeHealthBar());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead || !gameObject.activeSelf) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();
        ShowHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth / maxHealth;
        }
    }

    private void UpdateHealthBarPosition()
    {
        if (healthBarCanvasTransform != null && gameObject.activeSelf)
        {
            healthBarCanvasTransform.position = transform.position + healthBarOffset;
        }
    }

    private void HideHealthBar()
    {
        if (healthBarCanvasObject != null)
        {
            healthBarCanvasObject.SetActive(false);
        }
    }

    private void ShowHealthBarCanvas()
    {
        if (healthBarCanvasObject != null)
        {
            healthBarCanvasObject.SetActive(true);
        }
    }

    private void ShowHealthBar()
    {
        if (!gameObject.activeSelf) return;

        if (healthBarCanvasGroup != null)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            healthBarCanvasGroup.alpha = 1f;
            fadeCoroutine = StartCoroutine(FadeHealthBar());
        }
    }

    private IEnumerator ShakeHealthBar()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration && gameObject.activeSelf)
        {
            elapsed += Time.deltaTime;

            Vector3 randomOffset = new Vector3(
                Random.Range(-shakeAmount, shakeAmount),
                Random.Range(-shakeAmount, shakeAmount),
                0f
            );

            healthBarOffset = healthBarOriginalOffset + randomOffset;

            yield return null;
        }

        healthBarOffset = healthBarOriginalOffset;
    }

    private IEnumerator FadeHealthBar()
    {
        yield return new WaitForSeconds(showDuration);

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration && gameObject.activeSelf)
        {
            elapsedTime += Time.deltaTime;
            healthBarCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        healthBarCanvasGroup.alpha = 0f;
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        AudioManager.Instance.PlayPlayerDeath(transform.position);

        // Отключаем управление игроком
        var playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        StartCoroutine(PlayDeathAnimation());
    }

    private IEnumerator PlayDeathAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 originalPosition = transform.position;
        Quaternion originalRotation = transform.rotation;
        float elapsed = 0f;

        // Получаем все спрайты игрока
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        Color[] originalColors = new Color[sprites.Length];

        // Сохраняем оригинальные цвета
        for (int i = 0; i < sprites.Length; i++)
        {
            originalColors[i] = sprites[i].color;
        }

        // Анимация смерти
        while (elapsed < deathAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / deathAnimationDuration;

            // Плавное уменьшение размера с эффектом отскока
            float scaleProgress = 1f - Mathf.Sin(progress * Mathf.PI * 0.5f);
            transform.localScale = Vector3.Lerp(originalScale, originalScale * deathFinalScale, scaleProgress);

            // Вращение с замедлением
            float rotationSpeed = Mathf.Lerp(deathRotationSpeed, deathRotationSpeed * 0.2f, progress);
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

            // Небольшой подпрыгивающий эффект
            float jumpHeight = 0.5f * (1f - progress);
            float verticalOffset = jumpHeight * Mathf.Sin(progress * Mathf.PI);
            transform.position = originalPosition + Vector3.up * verticalOffset;

            // Изменение прозрачности
            float alpha = 1f - progress;
            foreach (SpriteRenderer sprite in sprites)
            {
                Color color = sprite.color;
                color.a = alpha;
                sprite.color = color;
            }

            yield return null;
        }

        // После завершения анимации
        gameObject.SetActive(false);

        // Восстанавливаем оригинальные значения для следующего возрождения
        transform.localScale = originalScale;
        transform.rotation = originalRotation;
        transform.position = originalPosition;

        // Восстанавливаем оригинальные цвета
        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i].color = originalColors[i];
        }

        // Запускаем процесс возрождения
        if (landingSystem != null)
        {
            landingSystem.InitiateRespawn();
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        UpdateHealthBar();
    }
}