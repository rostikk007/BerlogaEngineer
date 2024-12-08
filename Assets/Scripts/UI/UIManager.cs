using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Ссылки на UI элементы")]
    [Tooltip("Текст для отображения монет")]
    [SerializeField] private TextMeshProUGUI coinsText;

    [Tooltip("Текст для отображения железа")]
    [SerializeField] private TextMeshProUGUI ironText;

    [Tooltip("Текст для отображения очков улучшений")]
    [SerializeField] private TextMeshProUGUI upgradePointsText;

    [Header("Настройки анимации")]
    [Tooltip("Продолжительность анимации")]
    [SerializeField] private float animationDuration = 0.5f;

    [Tooltip("Максимальный масштаб при анимации")]
    [SerializeField] private float maxScale = 1.3f;

    [Tooltip("Скорость пульсации")]
    [SerializeField] private float pulseSpeed = 5f;

    [Tooltip("Интенсивность пульсации")]
    [SerializeField] private float pulseIntensity = 0.1f;

    [Header("Настройки внешнего вида текста")]
    [Tooltip("Градиент для текста монет")]
    [SerializeField] private VertexGradient coinsGradient;

    [Tooltip("Градиент для текста железа")]
    [SerializeField] private VertexGradient ironGradient;

    [Tooltip("Интенсивность свечения текста")]
    [SerializeField] private float glowIntensity = 1f;

    private Vector3 originalCoinsScale;
    private Vector3 originalIronScale;
    private Vector3 originalUpgradePointsScale;
    private float animationTimer;
    private bool isAnimating;
    private TextMeshProUGUI currentAnimatedText;

    private void Start()
    {
        // Сохраняем изначальные размеры текста
        if (coinsText != null) originalCoinsScale = coinsText.transform.localScale;
        if (ironText != null) originalIronScale = ironText.transform.localScale;
        if (upgradePointsText != null) originalUpgradePointsScale = upgradePointsText.transform.localScale;

        // Подписываемся на события изменения значений
        GameManager.Instance.OnCoinsChanged += UpdateCoinsUI;
        GameManager.Instance.OnIronChanged += UpdateIronUI;
        GameManager.Instance.OnUpgradePointsChanged += UpdateUpgradePointsUI;

        // Инициализируем начальные значения
        UpdateCoinsUI(GameManager.Instance.GetCoins());
        UpdateIronUI(GameManager.Instance.GetIron());
        UpdateUpgradePointsUI(GameManager.Instance.GetUpgradePoints());
    }

    private void Update()
    {
        if (isAnimating && currentAnimatedText != null)
        {
            animationTimer += Time.deltaTime;
            float progress = animationTimer / animationDuration;

            // Добавляем эффект пружины для более динамичной анимации
            float springEffect = Mathf.Sin(progress * Mathf.PI * pulseSpeed) * pulseIntensity;

            if (progress <= 0.5f)
            {
                // Увеличиваем масштаб с эффектом пружины
                float scale = Mathf.Lerp(1f, maxScale, progress * 2f) + springEffect;
                currentAnimatedText.transform.localScale = GetOriginalScale(currentAnimatedText) * scale;
            }
            else
            {
                // Уменьшаем масштаб обратно с эффектом пружины
                float scale = Mathf.Lerp(maxScale, 1f, (progress - 0.5f) * 2f) + springEffect;
                currentAnimatedText.transform.localScale = GetOriginalScale(currentAnimatedText) * scale;
            }

            if (progress >= 1f)
            {
                isAnimating = false;
                currentAnimatedText.transform.localScale = GetOriginalScale(currentAnimatedText);
            }
        }
    }

    private Vector3 GetOriginalScale(TextMeshProUGUI text)
    {
        if (text == coinsText) return originalCoinsScale;
        if (text == ironText) return originalIronScale;
        if (text == upgradePointsText) return originalUpgradePointsScale;
        return Vector3.one;
    }

    private void UpdateCoinsUI(int newValue)
    {
        if (coinsText != null)
        {
            coinsText.text = newValue.ToString("N0"); // Форматируем число с разделителями тысяч
            StartTextAnimation(coinsText);
        }
    }

    private void UpdateIronUI(int newValue)
    {
        if (ironText != null)
        {
            ironText.text = newValue.ToString("N0"); // Форматируем число с разделителями тысяч
            StartTextAnimation(ironText);
        }
    }

    private void UpdateUpgradePointsUI(int newValue)
    {
        if (upgradePointsText != null)
        {
            upgradePointsText.text = $"Очки улучшений: {newValue}";
            StartTextAnimation(upgradePointsText);
        }
    }

    private void StartTextAnimation(TextMeshProUGUI textElement)
    {
        currentAnimatedText = textElement;
        animationTimer = 0f;
        isAnimating = true;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCoinsChanged -= UpdateCoinsUI;
            GameManager.Instance.OnIronChanged -= UpdateIronUI;
            GameManager.Instance.OnUpgradePointsChanged -= UpdateUpgradePointsUI;
        }
    }
}