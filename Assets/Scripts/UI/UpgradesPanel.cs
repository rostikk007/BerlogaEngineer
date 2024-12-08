using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class UpgradesPanel : MonoBehaviour
{
    [Header("Настройки панели")]
    [Tooltip("Кнопка открытия/закрытия панели улучшений")]
    public Button toggleButton;

    [Tooltip("Текст на кнопке")]
    public Text buttonText;

    [Header("Настройки анимации")]
    [Tooltip("Длительность анимации открытия/закрытия")]
    public float animationDuration = 0.3f;

    [Header("Ссылки на объекты")]
    [Tooltip("Панель улучшений")]
    public GameObject upgradesPanel;

    [Tooltip("Затемнение фона")]
    public Image darkBackground;

    [Tooltip("Максимальное затемнение фона (0-1)")]
    [Range(0, 1)]
    public float maxDarkness = 0.5f;

    private RectTransform panelRect;
    private CanvasGroup panelCanvasGroup;
    private bool isPanelOpen = false;
    private bool isAnimating = false;

    private WaveManager waveManager;
    private bool canUpgrade = false;

    private void Awake()
    {
        // Проверяем, нет и конфликтов с другими скриптами
        var panelController = GetComponent<PanelController>();
        if (panelController != null)
        {
            Debug.LogError("UpgradesPanel: Обнаружен конфликт с PanelController на том же объекте!", this);
        }

        var settingsManager = GetComponent<SettingsManager>();
        if (settingsManager != null)
        {
            Debug.LogError("UpgradesPanel: Обнаружен конфликт с SettingsManager на том же объекте!", this);
        }

        // Сразу скрываем панель при создании объекта
        if (upgradesPanel != null)
        {
            upgradesPanel.SetActive(false);
        }
        if (darkBackground != null)
        {
            darkBackground.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        waveManager = FindObjectOfType<WaveManager>();
        if (waveManager == null)
        {
            Debug.LogError("WaveManager не найден на сцене!");
        }

        // Изначально кнопка неактивна
        SetButtonActive(false);

        // Подписываемся на события смены фазы
        if (waveManager != null)
        {
            waveManager.OnPhaseChanged += HandlePhaseChange;
        }

        // Проверяем и инициализируем компоненты
        if (upgradesPanel == null)
        {
            Debug.LogError("UpgradesPanel: Не назначена панель улучшений!", this);
            return;
        }

        if (toggleButton == null)
        {
            Debug.LogError("UpgradesPanel: Не назначена кнопка переключения!", this);
            return;
        }

        if (darkBackground == null)
        {
            Debug.LogError("UpgradesPanel: Не назначен фон затемнения!", this);
            return;
        }

        // Отключаем навигацию UI для кнопки
        Navigation navigation = toggleButton.navigation;
        navigation.mode = Navigation.Mode.None;
        toggleButton.navigation = navigation;

        // Проверяем кнопку
        if (!toggleButton.interactable)
        {
           // Debug.LogError("UpgradesPanel: Кнопка не интерактивна!", this);
            toggleButton.interactable = true;
        }

        // Получаем компоненты
        panelRect = upgradesPanel.GetComponent<RectTransform>();
        if (panelRect == null)
        {
            Debug.LogError("UpgradesPanel: На панели отсутствует компонент RectTransform!", this);
            return;
        }

        panelCanvasGroup = upgradesPanel.GetComponent<CanvasGroup>();
        if (panelCanvasGroup == null)
        {
            panelCanvasGroup = upgradesPanel.AddComponent<CanvasGroup>();
        }

        // Настраиваем начальное состояние
        upgradesPanel.SetActive(false);
        darkBackground.gameObject.SetActive(false);
        isPanelOpen = false;
        isAnimating = false;

        // Настраиваем кнопку
        toggleButton.onClick.RemoveAllListeners();
        toggleButton.onClick.AddListener(UpgradesTogglePanel);
        UpgradesUpdateButtonText();

        // Отключаем выбор кнопки по умолчанию
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void HandlePhaseChange(WaveManager.PhaseType newPhase)
    {
        // Активируем кнопку только в фазе улучшений
        if (newPhase == WaveManager.PhaseType.Upgrade)
        {
            SetButtonActive(true);
            canUpgrade = true;
        }
        else
        {
            SetButtonActive(false);
            canUpgrade = false;

            // Если панель открыта, закрываем её
            if (isPanelOpen)
            {
                UpgradesHidePanel();
            }
        }
    }

    public void SetButtonActive(bool active)
    {
        if (toggleButton != null)
        {
            toggleButton.interactable = active;
        }
    }

    // Переключение состояния панели
    private void UpgradesTogglePanel()
    {
        if (isPanelOpen)
        {
            UpgradesHidePanel();
        }
        else
        {
            UpgradesShowPanel();
        }
    }

    // Показ панели
    private void UpgradesShowPanel()
    {
        if (upgradesPanel != null && !isPanelOpen && !isAnimating)
        {
            isPanelOpen = true;
            UpgradesUpdateButtonText();
            StartCoroutine(UpgradesShowPanelAnimation());
        }
    }

    // Скрытие панели
    public void UpgradesHidePanel()
    {
        if (upgradesPanel != null && isPanelOpen && !isAnimating)
        {
            isPanelOpen = false;
            UpgradesUpdateButtonText();
            StartCoroutine(UpgradesHidePanelAnimation());

            // После закрытия панели деактивируем кнопку
            SetButtonActive(false);
        }
    }

    private IEnumerator UpgradesShowPanelAnimation()
    {
        isAnimating = true;

        upgradesPanel.SetActive(true);
        darkBackground.gameObject.SetActive(true);

        panelRect.localScale = Vector3.zero;
        panelCanvasGroup.alpha = 0;

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            float smoothProgress = Mathf.SmoothStep(0, 1, progress);

            panelRect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, smoothProgress);
            panelCanvasGroup.alpha = smoothProgress;
            darkBackground.color = new Color(0, 0, 0, maxDarkness * smoothProgress);

            yield return null;
        }

        panelRect.localScale = Vector3.one;
        panelCanvasGroup.alpha = 1;
        darkBackground.color = new Color(0, 0, 0, maxDarkness);

        isAnimating = false;
    }

    private IEnumerator UpgradesHidePanelAnimation()
    {
        isAnimating = true;

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            float smoothProgress = Mathf.SmoothStep(0, 1, progress);

            panelRect.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, smoothProgress);
            panelCanvasGroup.alpha = 1 - smoothProgress;
            darkBackground.color = new Color(0, 0, 0, maxDarkness * (1 - smoothProgress));

            yield return null;
        }

        upgradesPanel.SetActive(false);
        darkBackground.gameObject.SetActive(false);

        isAnimating = false;
    }

    // Обновление текста кнопки
    private void UpgradesUpdateButtonText()
    {
        if (buttonText != null)
        {
            buttonText.text = isPanelOpen ? "Закрыть улучшения" : "Открыть улучшения";
        }
    }

    private void OnDestroy()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(UpgradesTogglePanel);
        }

        if (waveManager != null)
        {
            waveManager.OnPhaseChanged -= HandlePhaseChange;
        }
    }

    private void Update()
    {
        // Сбрасываем выбранный объект UI каждый кадр
        if (EventSystem.current.currentSelectedGameObject == toggleButton.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}