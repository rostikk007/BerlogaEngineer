using UnityEngine;
using System.Collections;


public class SquareHighlighter : MonoBehaviour
{
    private Color originalColor;
    private Color highlightColor = Color.yellow; // Цвет выделения
    private SpriteRenderer spriteRenderer;

    private Coroutine colorChangeCoroutine; // Хранит ссылку на запущенную корутину

    [SerializeField] // Позволяет редактировать это поле в инспекторе Unity
    private float animationDuration = 0.2f; // Длительность анимации

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color; // Сохраняем оригинальный цвет
    }

    void OnMouseEnter()
    {
        if (!FindObjectOfType<ClickManager>().IsBuildingModeActive()) // Проверяем, отключен ли режим строительства
        {
            if (colorChangeCoroutine != null)
            {
                StopCoroutine(colorChangeCoroutine); // Останавливаем текущую корутину, если она запущена
                colorChangeCoroutine = null; // Обнуляем ссылку на корутину
            }
            // Здесь можно добавить логику для возврата к оригинальному цвету, если необходимо
            spriteRenderer.color = originalColor; // Возвращаем цвет к оригинальному
        }
        else
        {
            if (colorChangeCoroutine != null)
            {
                StopCoroutine(colorChangeCoroutine); // Останавливаем текущую корутину, если она запущена
            }
            colorChangeCoroutine = StartCoroutine(ChangeColor(originalColor, highlightColor, animationDuration)); // Плавное изменение на желтый
        }
    }

    void OnMouseExit()
    {
        if (!FindObjectOfType<ClickManager>().IsBuildingModeActive()) // Проверяем, отключен ли режим строительства
        {
            if (colorChangeCoroutine != null)
            {
                StopCoroutine(colorChangeCoroutine); // Останавливаем текущую корутину, если она запущена
                colorChangeCoroutine = null; // Обнуляем ссылку на корутину
            }
            // Здесь можно добавить логику для возврата к оригинальному цвету, если необходимо
            spriteRenderer.color = originalColor; // Возвращаем цвет к оригинальному
        }
        else
        {
            if (colorChangeCoroutine != null)
            {
                StopCoroutine(colorChangeCoroutine); // Останавливаем текущую корутину, если она запущена
            }
            colorChangeCoroutine = StartCoroutine(ChangeColor(highlightColor, originalColor, animationDuration)); // Плавное изменение обратно
        }
    }

    private IEnumerator ChangeColor(Color fromColor, Color toColor, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            spriteRenderer.color = Color.Lerp(fromColor, toColor, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null; // Ждем следующий кадр
        }

        spriteRenderer.color = toColor; // Устанавливаем конечный цвет
    }
}



