using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SettingsManager : MonoBehaviour
{
    [Header("Панели UI")]
    [Tooltip("Панель настроек")]
    public GameObject settingsPanel;

    [Header("Элементы управления звуком")]
    [Tooltip("Ползунок громкости")]
    public Slider volumeSlider;

    [Tooltip("Текст, показывающий текущую громкость")]
    public TMPro.TextMeshProUGUI volumeText;

    [Tooltip("Дополнительный текст")] // Добавляем новый текст
    public TMPro.TextMeshProUGUI additionalText;

    [Header("Настройки анимации")]
    [Tooltip("Скорость затемнения при паузе")]
    public float fadeSpeed = 0.5f;

    [Tooltip("Максимальное затемнение (0-1)")]
    [Range(0, 1)]
    public float maxDarkness = 0.5f;

    [Tooltip("Скорость движения панели настроек")]
    public float slideSpeed = 1000f;

    [Header("Настройки перехода")]
    [Tooltip("Имя сцены главного меню")]
    public int mainMenuSceneName = 4;

    [Header("Визуальные элементы")]
    [Tooltip("Панель затемнения")]
    public Image darkPanel;

    [Header("Управление")]
    [Tooltip("Клавиша для открытия/закрытия настроек")]
    public KeyCode settingsKey = KeyCode.Escape;

    private float originalTimeScale;
    private bool isPaused = false;
    private float currentVolume;
    private RectTransform settingsPanelRect;
    private Vector2 hiddenPosition;
    private Vector2 visiblePosition;

    // Публичное свойство для проверки состояния паузы
    public bool IsPaused => isPaused;

    void Start()
    {
        settingsPanelRect = settingsPanel.GetComponent<RectTransform>();

        // Настраиваем позиции
        hiddenPosition = new Vector2(-Screen.width, 0);
        visiblePosition = Vector2.zero;

        settingsPanelRect.anchoredPosition = hiddenPosition;

        // Настраиваем привязки панели к центру экрана
        settingsPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
        settingsPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
        settingsPanelRect.pivot = new Vector2(0.5f, 0.5f);

        if (darkPanel != null)
            darkPanel.gameObject.SetActive(false);

        // Загружаем сохраненную громкость
        currentVolume = PlayerPrefs.GetFloat("GameVolume", 1f);

        if (volumeSlider != null)
        {
            volumeSlider.value = currentVolume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        UpdateVolumeText();
        UpdateAdditionalText(); // Обновляем новый текст
    }

    void Update()
    {
        if (Input.GetKeyDown(settingsKey))
        {
            ToggleSettings();
        }
    }
    private void UpdateAdditionalText()
    {
        if (additionalText != null)
        {
            // Здесь задаете нужный текст
            additionalText.text = "Ваш текст здесь";
        }
    }
    public void ToggleSettings()
    {
        if (isPaused)
            CloseSettings();
        else
            OpenSettings();
    }

    public void OpenSettings()
    {
        if (isPaused) return;

        isPaused = true;
        originalTimeScale = Time.timeScale;
        StartCoroutine(PauseGameRoutine());
    }

    public void CloseSettings()
    {
        if (!isPaused) return;

        isPaused = false;
        StartCoroutine(ResumeGameRoutine());
    }

    private IEnumerator PauseGameRoutine()
    {
        settingsPanel.SetActive(true);
        darkPanel.gameObject.SetActive(true);

        float elapsed = 0;
        Color startColor = new Color(0, 0, 0, 0);
        Color targetColor = new Color(0, 0, 0, maxDarkness);
        Vector2 startPosition = hiddenPosition;

        while (elapsed < fadeSpeed)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / fadeSpeed;
            float smoothProgress = Mathf.SmoothStep(0, 1, progress);

            darkPanel.color = Color.Lerp(startColor, targetColor, smoothProgress);

            float overshoot = Mathf.Sin(progress * Mathf.PI * 2) * 0.1f * (1 - progress);
            Vector2 currentPosition = Vector2.Lerp(startPosition, visiblePosition, smoothProgress);
            currentPosition.x += overshoot * Screen.width * 0.1f;
            settingsPanelRect.anchoredPosition = currentPosition;

            Time.timeScale = Mathf.Lerp(1f, 0f, smoothProgress);

            yield return null;
        }

        darkPanel.color = targetColor;
        settingsPanelRect.anchoredPosition = visiblePosition;
        Time.timeScale = 0f;
    }

    private IEnumerator ResumeGameRoutine()
    {
        float elapsed = 0;
        Color startColor = darkPanel.color;
        Color targetColor = new Color(0, 0, 0, 0);
        Vector2 startPosition = settingsPanelRect.anchoredPosition;

        while (elapsed < fadeSpeed)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / fadeSpeed;
            float smoothProgress = Mathf.SmoothStep(0, 1, progress);

            darkPanel.color = Color.Lerp(startColor, targetColor, smoothProgress);
            settingsPanelRect.anchoredPosition = Vector2.Lerp(startPosition, hiddenPosition, smoothProgress);
            Time.timeScale = Mathf.Lerp(0f, originalTimeScale, smoothProgress);

            yield return null;
        }

        darkPanel.color = targetColor;
        settingsPanelRect.anchoredPosition = hiddenPosition;
        Time.timeScale = originalTimeScale;

        settingsPanel.SetActive(false);
        darkPanel.gameObject.SetActive(false);
    }

    public void OnVolumeChanged(float value)
    {
        currentVolume = value;
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("GameVolume", value);
        PlayerPrefs.Save();
        UpdateVolumeText();
    }

    private void UpdateVolumeText()
    {
        if (volumeText != null)
        {
            volumeText.text = $"Volume: {Mathf.RoundToInt(currentVolume * 100)}%";
        }
    }

    public void ReturnToMainMenu()
    {
        StartCoroutine(ReturnToMainMenuRoutine());
    }

    private IEnumerator ReturnToMainMenuRoutine()
    {
        float elapsed = 0;
        Color startColor = darkPanel.color;
        Color targetColor = Color.black;

        while (elapsed < fadeSpeed)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / fadeSpeed;
            darkPanel.color = Color.Lerp(startColor, targetColor, progress);
            yield return null;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(4);
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}