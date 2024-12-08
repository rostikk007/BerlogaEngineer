using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PanelController : MonoBehaviour
{
    [Header("Основные настройки")]
    [Tooltip("Панель, которой управляет контроллер")]
    public GameObject panel;

    [Tooltip("Менеджер строительства")]
    public ClickManager clickManager;

    [Tooltip("Менеджер настроек")]
    public SettingsManager settingsManager;

    [Header("Настройки анимации")]
    [Tooltip("Длительность анимации скольжения")]
    public float slideDuration = 0.5f;

    [Tooltip("Позиция скрытой панели")]
    public Vector2 hiddenPosition;

    [Tooltip("Позиция показанной панели")]
    public Vector2 shownPosition;

    private bool isAnimating = false;
    private RectTransform rectTransform;
    private ScrollRect scrollRect;
   // private RectTransform contentRect;

    private void Start()
    {
        rectTransform = panel.GetComponent<RectTransform>();
        scrollRect = panel.GetComponentInChildren<ScrollRect>();
        //contentRect = scrollRect.content;

        rectTransform.anchoredPosition = hiddenPosition;
        panel.SetActive(false);

        // Автоматический поиск SettingsManager, если он не назначен
        if (settingsManager == null)
        {
            settingsManager = FindObjectOfType<SettingsManager>();
            if (settingsManager == null)
            {
                Debug.LogWarning("SettingsManager не найден на сцене!", this);
            }
        }
    }

    private void Update()
    {
        // Проверяем, не открыто ли меню настроек
        if (Input.GetKeyDown(KeyCode.B) && !isAnimating &&
            (settingsManager == null || !settingsManager.IsPaused))
        {
            TogglePanel();
        }
    }

    private void TogglePanel()
    {
        if (panel.activeSelf)
        {
            StartCoroutine(SlideOut(panel));
        }
        else
        {
            panel.SetActive(true);
            StartCoroutine(SlideIn(panel));
        }
    }

    private IEnumerator SlideIn(GameObject panel)
    {
        isAnimating = true;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / slideDuration;

            // Используем плавную интерполяцию
            float smoothProgress = Mathf.SmoothStep(0, 1, progress);
            rectTransform.anchoredPosition = Vector2.Lerp(hiddenPosition, shownPosition, smoothProgress);

            yield return null;
        }

        rectTransform.anchoredPosition = shownPosition;
        isAnimating = false;
    }

    private IEnumerator SlideOut(GameObject panel)
    {
        isAnimating = true;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / slideDuration;

            // Используем плавную интерполяцию
            float smoothProgress = Mathf.SmoothStep(0, 1, progress);
            rectTransform.anchoredPosition = Vector2.Lerp(shownPosition, hiddenPosition, smoothProgress);

            yield return null;
        }

        rectTransform.anchoredPosition = hiddenPosition;
        panel.SetActive(false);
        clickManager.ToggleBuildingMode();
        isAnimating = false;
    }

    // Публичный метод для проверки состояния анимации
    public bool IsAnimating => isAnimating;

    // Публичный метод для проверки активности панели
    public bool IsPanelActive => panel.activeSelf;
}