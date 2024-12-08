using System.Collections;
using UnityEngine;

public class ScaleObject : MonoBehaviour
{
    public Vector3 targetScale = new Vector3(2f, 2f, 2f); // Целевой размер объекта
    public float duration = 2f; // Время, за которое объект изменит размер
    private Vector3 originalScale; // Исходный размер объекта

    void Start()
    {
        originalScale = transform.localScale; // Сохраняем исходный размер
        StartCoroutine(ScaleOverTime(originalScale, targetScale, duration)); // Запускаем корутину
    }

    private IEnumerator ScaleOverTime(Vector3 from, Vector3 to, float time)
    {
        float elapsedTime = 0f; // Время, прошедшее с начала анимации

        // Увеличиваем размер
        while (elapsedTime < time)
        {
            transform.localScale = Vector3.Lerp(from, to, (elapsedTime / time)); // Плавно изменяем размер
            elapsedTime += Time.deltaTime; // Увеличиваем прошедшее время
            yield return null; // Ждем следующего кадра
        }

        transform.localScale = to; // Устанавливаем конечный размер

        // Уменьшаем размер обратно
        elapsedTime = 0f; // Сбрасываем время
        while (elapsedTime < time)
        {
            transform.localScale = Vector3.Lerp(to, from, (elapsedTime / time)); // Плавно изменяем размер обратно
            elapsedTime += Time.deltaTime; // Увеличиваем прошедшее время
            yield return null; // Ждем следующего кадра
        }

        transform.localScale = from; // Устанавливаем исходный размер

        // Запускаем корутину снова для бесконечного цикла
        StartCoroutine(ScaleOverTime(originalScale, targetScale, duration));
    }
}
