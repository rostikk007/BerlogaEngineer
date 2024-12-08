using UnityEngine;
using System.Collections;

public class Apiary : MonoBehaviour
{
    [Header("��������� ������")]
    [Tooltip("���������� �����, ���������� �� ���� ���������")]
    public int coinsPerGeneration = 5;

    [Tooltip("�������� ��������� ����� � ��������")]
    public float generationInterval = 3f;

    [Header("��������� ��������")]
    [Tooltip("���������� ������ ��� ��������� �����")]
    public bool showGenerationEffect = true;

    [Tooltip("����������������� ������� � ��������")]
    public float effectDuration = 0.5f;

    [Tooltip("������� ���������� ��� �������")]
    public float scaleMultiplier = 1.2f;

    [Header("��������� ��������")]
    [Tooltip("������������ �������� ������")]
    public float maxHealth = 100f;

    [Tooltip("������� �������� ������")]
    private float currentHealth;

    [Tooltip("������������ ������� ������� ��� �����")]
    public float damageFlashDuration = 0.1f;

    [Header("��������� ��������")]
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

        // ������������� ���� ��������� ������
        AudioManager.Instance.PlayApiarySpawn(transform.position);
    }

    private IEnumerator InitializeAfterAnimations()
    {
        // ���� ���������� ���� ��������� ��������
        yield return new WaitForSeconds(1f);

        // ������������� ������� �������
        transform.localScale = targetScale;
        originalScale = targetScale;
        isInitialized = true;

        StartGeneration();
    }

    private void Update()
    {
        if (isInitialized && !isDead && !isEffectRunning)
        {
            // ���� ������� ���������� �� �������� - ���������������
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

                // ������������� ���� ������ ����
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
            // ���������� ��������
            while (elapsed < effectDuration / 2)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / (effectDuration / 2);
                transform.localScale = Vector3.Lerp(targetScale, effectTargetScale, progress);
                yield return null;
            }

            // ������� � �������� ��������
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

        // ������������� ���� ��������� ����� �������
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

        // ������������� ���� ������ ������
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

            // ��������� ������ � ������������
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