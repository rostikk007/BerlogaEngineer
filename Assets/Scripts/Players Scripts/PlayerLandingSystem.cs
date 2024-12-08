using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
public class PlayerLandingSystem : MonoBehaviour
{
    [Header("Настройки посадки")]
    [Tooltip("Префаб капсулы игрока")]
    [SerializeField] private GameObject playerCapsulePrefab;

    [Tooltip("Ссылка на игрока на сцене")]
    [SerializeField] private GameObject player;

    [Tooltip("Высота появления капсулы")]
    [SerializeField] private float capsuleSpawnHeight = 15f;

    [Tooltip("Скорость падения капсулы")]
    [SerializeField] private float capsuleFallSpeed = 8f;

    [Header("Настройки анимации")]
    [Tooltip("Сила покачивания при спуске")]
    [SerializeField] private float wobbleStrength = 0.2f;

    [Tooltip("Частота покачивания при спуске")]
    [SerializeField] private float wobbleFrequency = 10f;

    [Tooltip("Время исчезновения капсулы")]
    [SerializeField] private float capsuleDisappearDuration = 0.5f;

    [Header("Настройки возрождения")]
    [Tooltip("Задержка перед возрождением")]
    public float respawnDelay = 2f;

    [Tooltip("Расстояние возрождения от ИВИ-21")]
    public float respawnDistance = 2f;

    [Header("События")]
    public UnityEvent onLandingComplete;
    public UnityEvent onEvacuationStarted;
    public UnityEvent onEvacuationComplete;
    public UnityEvent onEmergencyEvacuation;
    public UnityEvent onRespawnStarted;
    public UnityEvent onRespawnComplete;

    [Header("Настройки сцен")]
    [Tooltip("Название сцены для перехода после уничтожения ИВИ-21")]
    public int gameOverSceneName = 3;

    [Tooltip("Название сцены для перехода после успешного сбора данных")]
    public int missionCompleteSceneName = 8;

    [Tooltip("Задержка перед переходом на новую сцену")]
    public float sceneTransitionDelay = 2f;

    private GameObject currentCapsule;
    private Vector3 initialPlayerPosition;
    private bool isLanding = false;
    private bool isEvacuating = false;
    private bool isEmergencyEvacuation = false;
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player == null)
        {
            Debug.LogError("Игрок не найден на сцене!");
            return;
        }

        playerMovement = player.GetComponent<PlayerMovement>();
        playerHealth = player.GetComponent<PlayerHealth>();

        if (playerMovement == null)
        {
            Debug.LogError("Компонент PlayerMovement не найден на игроке!");
            return;
        }

        if (playerHealth == null)
        {
            Debug.LogError("Компонент PlayerHealth не найден на игроке!");
            return;
        }

        initialPlayerPosition = player.transform.position;
        player.SetActive(false);
        playerHealth.DisableHealthBar();
    }

    private void Start()
    {
        StartLanding(); // Начальная высадка игрока
    }

    public void InitiateLanding()
    {
        if (!isLanding && !isEvacuating && !player.activeSelf)
        {
            Debug.Log("ИВИ-21: Данные собраны, вызываем капсулу для эвакуации!");
            StartLanding();
        }
        else
        {
            Debug.LogWarning("Невозможно начать посадку: процесс уже запущен или игрок активен");
        }
    }

    public void InitiateEvacuation()
    {
        if (!isEvacuating && player.activeSelf)
        {
            Debug.Log("ИВИ-21: Данные собраны, начинаем плановую эвакуацию игрока!");
            isEmergencyEvacuation = false;
            StartEvacuation();
        }
        else
        {
            Debug.LogWarning("Невозможно начать эвакуацию: процесс уже запущен или игрок неактивен");
        }
    }

    public void InitiateEmergencyEvacuation()
    {
        if (!isEvacuating && player.activeSelf)
        {
            Debug.Log("ИВИ-21 уничтожен! Начинаем экстренную эвакуацию игрока!");
            isEmergencyEvacuation = true;
            onEmergencyEvacuation?.Invoke();
            StartEvacuation();
        }
        else
        {
            Debug.LogWarning("Невозможно начать экстренную эвакуацию: процесс уже запущен или игрок неактивен");
        }
    }

    private void Update()
    {
        // Убираем автоматическую эвакуацию по таймеру
    }

    public void StartLanding()
    {
        if (!isLanding && !isEvacuating)
        {
            StartCoroutine(LandingSequence());
        }
    }

    private IEnumerator LandingSequence()
    {
        isLanding = true;
        Vector3 landingPosition = initialPlayerPosition;
        Vector3 spawnPosition = landingPosition + Vector3.up * capsuleSpawnHeight;

        currentCapsule = Instantiate(playerCapsulePrefab, spawnPosition, Quaternion.identity);

        float elapsedTime = 0;
        float fallDuration = capsuleSpawnHeight / capsuleFallSpeed;

        while (elapsedTime < fallDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fallDuration;

            float wobble = Mathf.Sin(progress * wobbleFrequency) * wobbleStrength;
            Vector3 currentPos = Vector3.Lerp(spawnPosition, landingPosition, progress);
            currentPos.x += wobble;

            currentCapsule.transform.position = currentPos;
            currentCapsule.transform.Rotate(Vector3.forward * Time.deltaTime * 360);

            yield return null;
        }

        currentCapsule.transform.position = landingPosition;
        yield return new WaitForSeconds(1f);

        player.transform.position = landingPosition;
        player.SetActive(true);
        playerHealth.EnableHealthBar();

        Vector2 exitDirection = Vector2.right;
        Vector2 exitPoint = (Vector2)landingPosition + exitDirection * 0.5f;
        float exitDuration = 0.3f;
        float exitTime = 0f;

        while (exitTime < exitDuration)
        {
            exitTime += Time.deltaTime;
            float progress = exitTime / exitDuration;

            player.transform.position = Vector3.Lerp(landingPosition, exitPoint, progress);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        elapsedTime = 0;
        while (elapsedTime < fallDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fallDuration;

            float wobble = Mathf.Sin(progress * wobbleFrequency) * wobbleStrength;
            Vector3 currentPos = Vector3.Lerp(landingPosition, spawnPosition, progress);
            currentPos.x += wobble;

            currentCapsule.transform.position = currentPos;
            currentCapsule.transform.Rotate(Vector3.forward * Time.deltaTime * 360);

            yield return null;
        }

        Destroy(currentCapsule);
        currentCapsule = null;

        isLanding = false;
        onLandingComplete?.Invoke();
        GameManager.Instance.StartRound();
    }

    private void StartEvacuation()
    {
        if (!isEvacuating && player.activeSelf)
        {
            isEvacuating = true;
            onEvacuationStarted?.Invoke();
            StartCoroutine(EvacuationSequence());
        }
    }

    private IEnumerator EvacuationSequence()
    {
        Vector3 evacuationPosition = player.transform.position;

        currentCapsule = Instantiate(playerCapsulePrefab,
            evacuationPosition + Vector3.up * capsuleSpawnHeight,
            Quaternion.identity);

        float elapsedTime = 0;
        float fallDuration = capsuleSpawnHeight / capsuleFallSpeed;

        while (elapsedTime < fallDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fallDuration;

            float wobble = Mathf.Sin(progress * wobbleFrequency) * wobbleStrength;
            Vector3 currentPos = Vector3.Lerp(
                evacuationPosition + Vector3.up * capsuleSpawnHeight,
                evacuationPosition,
                progress);
            currentPos.x += wobble;

            currentCapsule.transform.position = currentPos;
            currentCapsule.transform.Rotate(Vector3.forward * Time.deltaTime * 360);

            yield return null;
        }

        currentCapsule.transform.position = evacuationPosition;
        yield return new WaitForSeconds(0.5f);

        Vector2 directionToPlayer = ((Vector2)player.transform.position - (Vector2)evacuationPosition).normalized;
        Vector2 approachPoint = (Vector2)evacuationPosition + directionToPlayer * 1f;

        playerMovement.SetDestination(approachPoint);

        float waitTime = 0f;
        float maxWaitTime = 10f;

        while (!playerMovement.HasReachedDestination && waitTime < maxWaitTime)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        float jumpDuration = 0.4f;
        float jumpHeight = 0.5f;
        float jumpTime = 0f;
        Vector3 startPos = player.transform.position;

        while (jumpTime < jumpDuration)
        {
            jumpTime += Time.deltaTime;
            float progress = jumpTime / jumpDuration;

            float heightProgress = Mathf.Sin(progress * Mathf.PI) * jumpHeight;
            Vector3 currentPos = Vector3.Lerp(startPos, evacuationPosition, progress);
            currentPos.y += heightProgress;

            player.transform.position = currentPos;
            yield return null;
        }

        player.transform.position = evacuationPosition;
        yield return new WaitForSeconds(0.2f);

        playerHealth.DisableHealthBar();
        player.SetActive(false);

        Vector3 targetPosition = evacuationPosition + Vector3.up * capsuleSpawnHeight;
        elapsedTime = 0;

        while (elapsedTime < fallDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fallDuration;

            float wobble = Mathf.Sin(progress * wobbleFrequency) * wobbleStrength;
            Vector3 currentPos = Vector3.Lerp(evacuationPosition, targetPosition, progress);
            currentPos.x += wobble;

            currentCapsule.transform.position = currentPos;
            currentCapsule.transform.Rotate(Vector3.forward * Time.deltaTime * 360);

            yield return null;
        }

        Destroy(currentCapsule);

        onEvacuationComplete?.Invoke();

        if (isEmergencyEvacuation)
        {
            yield return new WaitForSeconds(sceneTransitionDelay);
            TransportFromShop.PlayerPosss = 5;
            SceneManager.LoadScene(gameOverSceneName);
        }
        else
        {
            yield return new WaitForSeconds(sceneTransitionDelay);
            TransportFromShop.PlayerPosss = 5;
            SceneManager.LoadScene(missionCompleteSceneName);
        }
    }

    private IEnumerator DisappearCapsule(GameObject capsule)
    {
        Vector3 startScale = capsule.transform.localScale;
        float elapsedTime = 0;

        while (elapsedTime < capsuleDisappearDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / capsuleDisappearDuration;

            capsule.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            capsule.transform.Rotate(Vector3.forward * Time.deltaTime * 720);

            yield return null;
        }

        Destroy(capsule);
    }

    private Vector3 GetRespawnPosition()
    {
        // Находим ИВИ-21 по имени объекта
        GameObject ivi21 = GameObject.Find("IVI21Antenna");
        if (ivi21 != null)
        {
            // Выбираем случайную точку вокруг ИВИ-21
            Vector2 randomCircle = Random.insideUnitCircle.normalized * respawnDistance;
            Vector3 offset = new Vector3(randomCircle.x, randomCircle.y, 0);
            return ivi21.transform.position + offset;
        }

        // Если ИВИ-21 не найден, возвращаемся в начальную позицию
        Debug.LogWarning("ИВИ-21 н�� найден на сцене, возрождение в начальной позиции");
        return initialPlayerPosition;
    }

    public void InitiateRespawn()
    {
        if (!isLanding && !isEvacuating && !player.activeSelf)
        {
            StartCoroutine(RespawnSequence());
        }
    }

    private IEnumerator RespawnSequence()
    {
        onRespawnStarted?.Invoke();

        // Ждем перед началом возрождения
        yield return new WaitForSeconds(respawnDelay);

        Vector3 respawnPosition = GetRespawnPosition();
        Vector3 spawnPosition = respawnPosition + Vector3.up * capsuleSpawnHeight;

        // Подготавливаем игрока перед спавном
        player.transform.position = respawnPosition;
        player.transform.rotation = Quaternion.identity;

        var playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.ResetHealth();
        }

        var playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // Создаем капсулу возрождения
        currentCapsule = Instantiate(playerCapsulePrefab, spawnPosition, Quaternion.identity);

        // Анимация спуска капсулы
        float elapsedTime = 0;
        float fallDuration = capsuleSpawnHeight / capsuleFallSpeed;

        while (elapsedTime < fallDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fallDuration;

            float wobble = Mathf.Sin(progress * wobbleFrequency) * wobbleStrength;
            Vector3 currentPos = Vector3.Lerp(spawnPosition, respawnPosition, progress);
            currentPos.x += wobble;

            currentCapsule.transform.position = currentPos;
            currentCapsule.transform.Rotate(Vector3.forward * Time.deltaTime * 360);

            yield return null;
        }

        // Активируем игрока
        player.SetActive(true);

        // Анимация улета капсулы
        StartCoroutine(DisappearCapsule(currentCapsule));

        onRespawnComplete?.Invoke();
    }

    private void LoadScene(int sceneName)
    {
        if (sceneName > 0)
        {
            try
            {
                TransportFromShop.PlayerPosss = 5;
                SceneTransition.SwitchToScene(sceneName);
                //SceneManager.LoadScene(sceneName);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Ошибка при загрузке сцены {sceneName}: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("Имя сцены не указано в инспекторе!");
        }
    }
}