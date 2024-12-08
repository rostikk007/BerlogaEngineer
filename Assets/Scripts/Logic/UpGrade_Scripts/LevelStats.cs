using UnityEngine;

[System.Serializable]
public class LevelStats
{
    [Header("Характеристики уровня")]
    [Tooltip("Урон постройки")]
    public float damage;

    [Tooltip("Здоровье постройки")]
    public float health;

    [Tooltip("Скорость атаки")]
    public float attackSpeed;

    [Tooltip("Дальность действия")]
    public float range;
}