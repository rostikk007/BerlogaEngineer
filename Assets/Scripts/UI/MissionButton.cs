using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionButton : MonoBehaviour
{
    [Header("Настройки кнопки")]
    [Tooltip("Название миссии")]
    public string missionName;

    [Tooltip("Описание миссии")]
    [TextArea(3, 5)]
    public string missionDescription;

    [Tooltip("ID миссии")]
    public int missionID;

    [Header("Ссылки на компоненты")]
    [Tooltip("Текст названия миссии")]
    public TextMeshProUGUI titleText;

    private Button button;
}