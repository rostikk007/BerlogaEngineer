using System.Collections;
using UnityEngine;

public class WindSway : MonoBehaviour
{
    public float swayAmount = 0.5f; // Максимальное смещение
    public float swaySpeed = 1f; // Скорость колебания
    public float minInterval = 1f; // Минимальный интервал между колебаниями
    public float maxInterval = 3f; // Максимальный интервал между колебаниями

    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.position; // Сохраняем оригинальную позицию
        StartCoroutine(Sway());
    }

    IEnumerator Sway()
    {
        while (true)
        {
            // Генерируем случайное время ожидания
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            // Генерируем случайное смещение
            float swayX = Random.Range(-swayAmount, swayAmount);
            float swayY = Random.Range(-swayAmount, swayAmount);

            // Плавно перемещаем объект
            Vector3 targetPosition = originalPosition + new Vector3(swayX, swayY, 0);
            float elapsedTime = 0f;

            while (elapsedTime < swaySpeed)
            {
                transform.position = Vector3.Lerp(originalPosition, targetPosition, (elapsedTime / swaySpeed));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Возвращаемся в оригинальную позицию
            elapsedTime = 0f;

            while (elapsedTime < swaySpeed)
            {
                transform.position = Vector3.Lerp(targetPosition, originalPosition, (elapsedTime / swaySpeed));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}
