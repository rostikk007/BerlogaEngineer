using UnityEngine;
using TMPro;

public class BattleManager : MonoBehaviour
{
    [Header("Настройки битвы")]
    [SerializeField] private int _totalSkeletons = 15; // Общее количество скелетов
    [SerializeField] private int _nextSceneIndex; // Индекс следующей сцены
    [Tooltip("Задержка перед переходом на следующую сцену")]
    [SerializeField] private float _delayBeforeTransition = 2f; // Задержка перед переходом

    [Header("UI элементы")]
    [SerializeField] private TextMeshProUGUI _enemiesLeftText; // Текст с количеством оставшихся врагов

    private int _killedSkeletons = 0; // Счётчик убитых скелетов

    // Метод, который нужно вызывать при смерти скелета
    public void OnSkeletonDeath()
    {
        _killedSkeletons++;
        UpdateUI();

        // Проверяем, все ли скелеты убиты
        if (_killedSkeletons >= _totalSkeletons)
        {
            StartCoroutine(TransitionToNextScene());
        }
    }

    // Обновление UI
    private void UpdateUI()
    {
        if (_enemiesLeftText != null)
        {
            int remainingSkeletons = _totalSkeletons - _killedSkeletons;
            _enemiesLeftText.text = $"× {remainingSkeletons}";
        }
    }

    // Корутина для перехода на следующую сцену
    private System.Collections.IEnumerator TransitionToNextScene()
    {
        Debug.Log("Все скелеты побеждены! Переход на следующую сцену...");

        // Ждём указанное время
        yield return new WaitForSeconds(_delayBeforeTransition);
        ResetBattle();
        Destroy(_enemiesLeftText);
        SceneTransition.SwitchToScene(_nextSceneIndex);
    }

    // Можно добавить метод для сброса счётчика, если потребуется
    public void ResetBattle()
    {
        _killedSkeletons = 0;
        UpdateUI();
    }
}