using UnityEngine;
using TMPro;

public class BuildingButton : MonoBehaviour
{
    [Header("UI элементы")]
    [Tooltip("Текст для отображения стоимости в монетах")]
    public TextMeshProUGUI coinPriceText;

    [Tooltip("Текст для отображения стоимости в железе")]
    public TextMeshProUGUI ironPriceText;

    public void UpdatePriceDisplay(int coinPrice, int ironPrice)
    {
        if (coinPriceText != null)
        {
            coinPriceText.text = $"{coinPrice}";
        }

        if (ironPriceText != null)
        {
            ironPriceText.text = $"{ironPrice}";
        }
    }
}