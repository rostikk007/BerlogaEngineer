using TMPro;
using UnityEngine;

public class WalletView : MonoBehaviour
{
    [SerializeField] private TMP_Text _value;

    private Wallet _wallet;

    public void Initialize(Wallet wallet)
    {
        _wallet = wallet;
        UpdateValue(_wallet.GetCurrentCoins());
        _wallet.CoinsChanged += UpdateValue;
    }

    private void OnDestroy()
    {
        if (_wallet != null)
        {
            _wallet.CoinsChanged -= UpdateValue;
        }
    }
    private void UpdateValue(int Value) => _value.text = Value.ToString();
}


