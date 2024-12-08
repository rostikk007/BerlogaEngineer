using UnityEngine;

[System.Serializable]
public class MissionData
{
    [Tooltip("Изображение вопроса")]
    public Sprite questionImage;

    [Tooltip("Варианты ответов в виде изображений")]
    public Sprite[] answerImages;

    [Tooltip("Индекс правильного ответа (начиная с 0)")]
    public int correctAnswerIndex;

    [Tooltip("Описание улучшения, которое получит игрок")]
    public string upgradeDescription;
}