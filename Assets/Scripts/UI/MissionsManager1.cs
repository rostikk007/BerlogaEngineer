using UnityEngine;
using UnityEngine.UI;

public class MissionsManager : MonoBehaviour
{
    [Header("Панели")]
    [Tooltip("Панель с вопросом")]
    public GameObject questionPanel;

    [Tooltip("Панель с вариантами ответов")]
    public GameObject answersPanel;

    [Header("UI элементы")]
    [Tooltip("Image для отображения вопроса")]
    public Image questionImage;

    [Tooltip("Кнопки вариантов ответов (3 кнопки)")]
    public Button[] answerButtons = new Button[3];

    [Tooltip("Image для каждой кнопки ответа")]
    public Image[] answerImages = new Image[3];

    private UpgradeBranch currentBranch;
    private MissionData currentMission;

    private void Start()
    {
        // Проверяем наличие всех необходимых компонентов
        if (questionImage == null)
        {
            Debug.LogError("Не назначен Image для вопроса!");
        }

        for (int i = 0; i < 3; i++)
        {
            if (answerImages[i] == null)
            {
                Debug.LogError($"Не назначен Image для ответа {i + 1}!");
            }
        }
    }

    public void ShowMission(UpgradeBranch branch)
    {
        if (!branch.CanShowNextMission())
        {
            Debug.Log("Все улучшения этой ветки уже получены!");
            return;
        }

        currentBranch = branch;
        currentMission = branch.GetCurrentMission();
        DisplayMission();
    }

    private void DisplayMission()
    {
        // Устанавливаем изображение вопроса
        questionImage.sprite = currentMission.questionImage;

        // Показываем варианты ответов
        for (int i = 0; i < 3; i++)
        {
            if (i < currentMission.answerImages.Length)
            {
                answerButtons[i].gameObject.SetActive(true);
                answerImages[i].sprite = currentMission.answerImages[i];

                // Настраиваем размер изображения, чтобы оно корректно отображалось
                answerImages[i].preserveAspect = true;

                int answerId = i;
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => CheckAnswer(answerId));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }

        questionPanel.SetActive(true);
        answersPanel.SetActive(true);
    }

    private void CheckAnswer(int selectedAnswer)
    {
        bool isCorrect = selectedAnswer == currentMission.correctAnswerIndex;

        if (isCorrect)
        {
            Debug.Log("Правильный ответ!");
            currentBranch.OnMissionCompleted();
        }
        else
        {
            Debug.Log("Неправильный ответ! Попробуйте в следующий раз.");
        }

        // В любом случае закрываем панель
        questionPanel.SetActive(false);
        answersPanel.SetActive(false);

        // Закрываем панель улучшений
        var upgradesPanel = FindObjectOfType<UpgradesPanel>();
        if (upgradesPanel != null)
        {
            upgradesPanel.UpgradesHidePanel();
        }
    }
}