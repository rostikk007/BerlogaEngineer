using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IVI21DataProgress : MonoBehaviour
{
    [Header("Настройки прогресса")]
    [Tooltip("Максимальное количество информации для сбора")]
    public float maxDataAmount = 100f;

    [Tooltip("Текущее количество собранной информации")]
    public float currentDataAmount;

    [Header("Настройки UI")]
    [Tooltip("Полоска прогресса")]
    public Image progressBar;

    [Tooltip("Canvas полоски прогресса")]
    [SerializeField] private GameObject progressBarCanvasObject;

    [Tooltip("Смещение полоски прогресса относительно ИВИ-21")]
    public Vector3 progressBarOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Настройки сбора данных")]
    [Tooltip("Количество данных, собираемых за одну итерацию")]
    public float dataCollectionRate = 5f;

    [Tooltip("Интервал между сбором данных (в секундах)")]
    public float collectionInterval = 1f;

    private CanvasGroup progressBarCanvasGroup;
    private Transform progressBarCanvasTransform;
    private PlayerLandingSystem landingSystem;
    private Coroutine autoCollectionCoroutine;
    private bool isDataCollectionComplete = false;
    private bool isDestroyed = false;

    private void Awake()
    {
        if (progressBarCanvasObject != null)
        {
            progressBarCanvasObject.SetActive(false);
        }

        // Находим ссылку на систему высадки
        landingSystem = FindObjectOfType<PlayerLandingSystem>();
    }

    private void Start()
    {
        currentDataAmount = 0f;
        isDataCollectionComplete = false;

        if (progressBarCanvasObject != null)
        {
            progressBarCanvasTransform = progressBarCanvasObject.transform;
            progressBarCanvasGroup = progressBarCanvasObject.GetComponent<CanvasGroup>();
            if (progressBarCanvasGroup == null)
            {
                progressBarCanvasGroup = progressBarCanvasObject.AddComponent<CanvasGroup>();
            }
            progressBarCanvasGroup.alpha = 1f; // Устанавливаем полную видимость
            progressBarCanvasObject.SetActive(true); // Активируем объект сразу
        }
        else
        {
            Debug.LogError("Progress Bar Canvas Object не назначен в инспекторе!", this);
        }

        UpdateProgressBar();
        UpdateProgressBarPosition();
        StartAutoCollection();
    }

    private void LateUpdate()
    {
        if (gameObject.activeSelf)
        {
            UpdateProgressBarPosition();
        }
    }

    public void CollectData(float amount)
    {
        if (isDataCollectionComplete || !gameObject.activeSelf) return;

        currentDataAmount += amount;
        currentDataAmount = Mathf.Clamp(currentDataAmount, 0, maxDataAmount);

        UpdateProgressBar();

        if (currentDataAmount >= maxDataAmount && !isDataCollectionComplete)
        {
            CompleteDataCollection();
        }
    }

    private void CompleteDataCollection()
    {
        if (isDestroyed) return; // Не вызываем эвакуацию, если уже уничтожены

        isDataCollectionComplete = true;
        Debug.Log("ИВИ-21: Сбор данных завершен!");

        // Вызываем эвакуацию
        if (landingSystem != null)
        {
            landingSystem.InitiateEvacuation();
        }
        else
        {
            Debug.LogWarning("PlayerLandingSystem не найдена!");
        }
    }

    private void UpdateProgressBar()
    {
        if (progressBar != null)
        {
            progressBar.fillAmount = currentDataAmount / maxDataAmount;
        }
    }

    private void UpdateProgressBarPosition()
    {
        if (progressBarCanvasTransform != null && gameObject.activeSelf)
        {
            progressBarCanvasTransform.position = transform.position + progressBarOffset;
        }
    }

    private void OnEnable()
    {
        if (progressBarCanvasObject != null)
        {
            progressBarCanvasObject.SetActive(true);
            if (progressBarCanvasGroup != null)
            {
                progressBarCanvasGroup.alpha = 1f;
            }
        }
        StartAutoCollection();
    }

    private void OnDisable()
    {
        if (progressBarCanvasObject != null)
        {
            progressBarCanvasObject.SetActive(false);
        }
        StopAutoCollection();
    }

    public float GetCurrentProgress()
    {
        return currentDataAmount / maxDataAmount;
    }

    public bool IsDataCollectionComplete()
    {
        return isDataCollectionComplete;
    }

    public void ResetProgress()
    {
        currentDataAmount = 0f;
        isDataCollectionComplete = false;
        UpdateProgressBar();
    }

    private void StartAutoCollection()
    {
        if (autoCollectionCoroutine == null)
        {
            autoCollectionCoroutine = StartCoroutine(AutoCollectData());
        }
    }

    private void StopAutoCollection()
    {
        if (autoCollectionCoroutine != null)
        {
            StopCoroutine(autoCollectionCoroutine);
            autoCollectionCoroutine = null;
        }
    }

    private IEnumerator AutoCollectData()
    {
        while (!isDataCollectionComplete)
        {
            CollectData(dataCollectionRate);
            yield return new WaitForSeconds(collectionInterval);
        }
    }

    private void OnDestroy()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            Debug.Log("ИВИ-21 уничтожен! Вызываем экстренную эвакуацию игрока.");

            if (progressBarCanvasObject != null)
            {
                Destroy(progressBarCanvasObject);
            }

            StopAutoCollection();

            // Вызываем экстренную эвакуацию
            if (landingSystem != null)
            {
                landingSystem.InitiateEmergencyEvacuation();
            }
            else
            {
                Debug.LogWarning("PlayerLandingSystem не найдена при уничтожении ИВИ-21!");
            }
        }
    }
}
