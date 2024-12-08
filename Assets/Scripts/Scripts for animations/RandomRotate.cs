using System.Collections;
using UnityEngine;

public class RandomRotate : MonoBehaviour
{
    public float rotationAmount = 15f; // Максимальный угол вращения
    public float minInterval = 1f; // Минимальный интервал между вращениями
    public float maxInterval = 3f; // Максимальный интервал между вращениями
    public float rotationSpeed = 2f; // Скорость вращения

    void Start()
    {
        StartCoroutine(Rotate()); // Запускаем корутину Rotate
    }

    IEnumerator Rotate()
    {
        while (true) // Бесконечный цикл
        {
            // Генерируем случайное время ожидания
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime); // Ожидаем случайное время

            // Генерируем случайный угол вращения
            float randomRotation = Random.Range(-rotationAmount, rotationAmount);
            Quaternion targetRotation = Quaternion.Euler(0, 0, randomRotation); // Целевая ротация

            float elapsedTime = 0f;
            Quaternion initialRotation = transform.rotation; // Сохраняем начальную ротацию

            while (elapsedTime < rotationSpeed) // Плавное вращение
            {
                transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, (elapsedTime / rotationSpeed));
                elapsedTime += Time.deltaTime; // Увеличиваем время
                yield return null; // Ждем следующего кадра
            }

            // Устанавливаем окончательную ротацию
            transform.rotation = targetRotation;
        }
    }
}

