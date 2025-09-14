using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI; // Убедимся, что эта строка есть


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private LevelController levelController;
    private EndGamePanelUI endGamePanelUI;
    [SerializeField] private float winScreenDelay = 0.5f;

    // "Память" для монеток, которые "зависли в воздухе"
    private int coinsEarnedThisRound = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        CollectableItem.ResetInspectionFlag();

        if (levelController == null)
        {
            levelController = FindObjectOfType<LevelController>();
        }

        endGamePanelUI = FindObjectOfType<EndGamePanelUI>(true);

        if (endGamePanelUI != null)
        {
            RegisterEndGamePanel(endGamePanelUI);
        }
    }

    // Этот метод теперь находит и настраивает ВСЕ ТРИ кнопки
    private void RegisterEndGamePanel(EndGamePanelUI panel)
    {
        endGamePanelUI = panel;
        if (endGamePanelUI == null) return;

        endGamePanelUI.gameObject.SetActive(false);

        // Настраиваем кнопку "Начать заново"
        if (endGamePanelUI.restartButton)
        {
            endGamePanelUI.restartButton.onClick.RemoveAllListeners();
            endGamePanelUI.restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
        // Настраиваем кнопку "Завершить игру"
        if (endGamePanelUI.menuButton)
        {
            endGamePanelUI.menuButton.onClick.RemoveAllListeners();
            endGamePanelUI.menuButton.onClick.AddListener(OnEndGameButtonClicked);
        }
        // Настраиваем кнопку "Следующий уровень"
        if (endGamePanelUI.nextLevelButton)
        {
            endGamePanelUI.nextLevelButton.onClick.RemoveAllListeners();
            endGamePanelUI.nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
        }

        Debug.Log("[GameManager] Панель концовки успешно найдена и все три кнопки настроены.");
    }

    public void ShowWinScreen(int finalScore, int totalLevels)
    {
        // Запоминаем награду в "память"
        this.coinsEarnedThisRound = finalScore;
        StartCoroutine(ShowWinScreenCoroutine(finalScore, totalLevels));
    }

    private IEnumerator ShowWinScreenCoroutine(int finalScore, int totalLevels)
    {
        if (endGamePanelUI == null) yield break;
        yield return new WaitForSeconds(winScreenDelay);
        if (SoundManager.Instance != null) SoundManager.Instance.StopAllSounds();
        Time.timeScale = 0f;
        endGamePanelUI.gameObject.SetActive(true);
        endGamePanelUI.ShowWin(finalScore);
        if (SoundManager.Instance != null) SoundManager.Instance.PlayWinSound();
    }

    public void ShowLoseScreen()
    {
        if (endGamePanelUI == null) return;

        if (SoundManager.Instance != null) SoundManager.Instance.StopAllSounds();
        Time.timeScale = 0f;

        int finalScore = UIController.Instance.GetCurrentScore();
        // Запоминаем награду (даже если она 0)
        this.coinsEarnedThisRound = finalScore;

        int levelsCompleted = UIController.Instance.GetCurrentLevel() - 1;
        int totalLevels = levelController.GetTotalLevelCount();

        endGamePanelUI.gameObject.SetActive(true);
        endGamePanelUI.ShowLose(finalScore, levelsCompleted, totalLevels);
        if (SoundManager.Instance != null) SoundManager.Instance.PlayLoseSound();
    }

    // --- МЕТОДЫ, ВЫЗЫВАЕМЫЕ КНОПКАМИ ---

    private void OnNextLevelButtonClicked()
    {
        Time.timeScale = 1f;
        CoinManager.AddCoins(coinsEarnedThisRound);
        MainMenuLevelManager.LoadNextLevel();
    }

    private void OnEndGameButtonClicked()
    {
        Time.timeScale = 1f;
        CoinManager.AddCoins(coinsEarnedThisRound);
        MainMenuLevelManager.shouldResetScoreOnLoad = true;
        SceneManager.LoadScene("TotalScoreScene");
    }

    private void OnRestartButtonClicked()
    {
        Time.timeScale = 1f;
        MainMenuLevelManager.RestartCurrentLevel();
    }
}