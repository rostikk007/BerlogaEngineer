using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UsernameInputController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField _usernameInput;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private TextMeshProUGUI _errorText;

    [Header("Settings")]
    //[SerializeField] private string _nextSceneName = "GameScene";
    [SerializeField] private int _minUsernameLength = 3;
    [SerializeField] private int _maxUsernameLength = 20;
    [SerializeField] private int scene;
    public static string _userInputText;
    public void Sergo()
    {
        SaveInputHoney();
        ValidateInput(_userInputText);
        
    }
    private void Start()
    {
        

        // Настраиваем UI
        //_confirmButton.onClick.AddListener(OnConfirmButtonClick);
        //_usernameInput.onValueChanged.AddListener(OnUsernameChanged);
        //_errorText.gameObject.SetActive(false);

        // Проверяем начальное значение
        //ValidateInput(_usernameInput.text);
    }
    private void SaveInputHoney()
    {
        _userInputText = _usernameInput.text.Trim();
    }
    private void ValidateInput(string username)
    {
        bool isValid = !string.IsNullOrEmpty(username) &&
                      username.Length >= _minUsernameLength &&
                      username.Length <= _maxUsernameLength;

        _errorText.gameObject.SetActive(!isValid);

        if (!isValid)
        {
            if (string.IsNullOrEmpty(username))
            {
                _errorText.text = "Enter your nickname";
            }
            else if (username.Length < _minUsernameLength)
            {
                _errorText.text = $"Minimum nickname length: {_minUsernameLength} symbols";
            }
            else if (username.Length > _maxUsernameLength)
            {
                _errorText.text = $"Maximum nickname length: {_maxUsernameLength} symbols";
            }
        }
        else
            SceneManager.LoadScene(scene);
    }

}