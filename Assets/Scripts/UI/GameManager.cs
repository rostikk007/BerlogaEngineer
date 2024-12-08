using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Начальные ресурсы")]
    [Tooltip("Количество монет в начале игры")]
    [SerializeField] private int startingCoins = 100;

    [Tooltip("Количество железа в начале игры")]
    [SerializeField] private int startingIron = 50;

    [Tooltip("Количество очков улучшений в начале игры")]
    [SerializeField] private int startingUpgradePoints = 0;

    [Header("Игровые ресурсы")]
    [Tooltip("Текущее количество монет")]
    private int coins = 0;

    [Tooltip("Текущее количество железа")]
    private int iron = 0;

    [Tooltip("Текущее количество очков улучшений")]
    private int upgradePoints = 0;

    [Header("Игровой процесс")]
    [SerializeField] private float roundDuration = 60f;
    private bool isRoundActive = false;

    public System.Action onRoundStart;
    public System.Action onRoundEnd;

    public void StartRound()
    {
        isRoundActive = true;
        onRoundStart?.Invoke();
    }

    public void EndRound()
    {
        isRoundActive = false;
        onRoundEnd?.Invoke();
        SaveGameState();
    }

    public void RestartLevel()
    {
        ResetToStartingResources();
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    public float GetRoundDuration()
    {
        return roundDuration;
    }

    public bool IsRoundActive()
    {
        return isRoundActive;
    }

    public System.Action<int> OnCoinsChanged;
    public System.Action<int> OnIronChanged;
    public System.Action<int> OnUpgradePointsChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeResources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeResources()
    {
        coins = startingCoins;
        iron = startingIron;
        upgradePoints = startingUpgradePoints;

        OnCoinsChanged?.Invoke(coins);
        OnIronChanged?.Invoke(iron);
        OnUpgradePointsChanged?.Invoke(upgradePoints);
    }

    public void ResetToStartingResources()
    {
        coins = startingCoins;
        iron = startingIron;
        upgradePoints = startingUpgradePoints;

        OnCoinsChanged?.Invoke(coins);
        OnIronChanged?.Invoke(iron);
        OnUpgradePointsChanged?.Invoke(upgradePoints);
    }

    #region Методы для работы с монетами
    public void AddCoins(int amount)
    {
        if (amount < 0) return;

        coins += amount;
        OnCoinsChanged?.Invoke(coins);
    }

    public bool SpendCoins(int amount)
    {
        if (amount < 0) return false;

        if (coins >= amount)
        {
            coins -= amount;
            OnCoinsChanged?.Invoke(coins);
            return true;
        }
        return false;
    }

    public int GetCoins() => coins;
    #endregion

    #region Методы для работы с железом
    public void AddIron(int amount)
    {
        if (amount < 0) return;

        iron += amount;
        OnIronChanged?.Invoke(iron);
    }

    public bool SpendIron(int amount)
    {
        if (amount < 0) return false;

        if (iron >= amount)
        {
            iron -= amount;
            OnIronChanged?.Invoke(iron);
            return true;
        }
        return false;
    }

    public int GetIron() => iron;
    #endregion

    #region Методы для работы с очками улучшений
    public void AddUpgradePoints(int amount)
    {
        if (amount < 0) return;

        upgradePoints += amount;
        OnUpgradePointsChanged?.Invoke(upgradePoints);
    }

    public bool SpendUpgradePoints(int amount)
    {
        if (amount < 0) return false;

        if (upgradePoints >= amount)
        {
            upgradePoints -= amount;
            OnUpgradePointsChanged?.Invoke(upgradePoints);
            return true;
        }
        return false;
    }

    public int GetUpgradePoints() => upgradePoints;
    #endregion

    #region Методы сохранения/загрузки (подготовка для будущего)
    public void SaveGameState()
    {
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetInt("Iron", iron);
        PlayerPrefs.SetInt("UpgradePoints", upgradePoints);
        PlayerPrefs.Save();
    }

    public void LoadGameState()
    {
        coins = PlayerPrefs.GetInt("Coins", 0);
        iron = PlayerPrefs.GetInt("Iron", 0);
        upgradePoints = PlayerPrefs.GetInt("UpgradePoints", 0);

        OnCoinsChanged?.Invoke(coins);
        OnIronChanged?.Invoke(iron);
        OnUpgradePointsChanged?.Invoke(upgradePoints);
    }
    #endregion

    #region Вспомогательные методы
    public bool HasEnoughResources(int coinsNeeded, int ironNeeded)
    {
        return coins >= coinsNeeded && iron >= ironNeeded;
    }

    public bool SpendResources(int coinsAmount, int ironAmount)
    {
        if (HasEnoughResources(coinsAmount, ironAmount))
        {
            SpendCoins(coinsAmount);
            SpendIron(ironAmount);
            return true;
        }
        return false;
    }
    #endregion

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        ResetToStartingResources();
    }
}