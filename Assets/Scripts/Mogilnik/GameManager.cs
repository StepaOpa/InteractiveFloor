using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private LevelController levelController;
    private EndGamePanelUI endGamePanelUI;
    [SerializeField] private float winScreenDelay = 0.5f;

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
        else
        {
            Debug.LogWarning("[GameManager] Панель концовки не найдена на сцене. Это нормально для главного меню.");
        }
    }

    private void RegisterEndGamePanel(EndGamePanelUI panel)
    {
        endGamePanelUI = panel;

        if (endGamePanelUI != null)
        {
            endGamePanelUI.gameObject.SetActive(false);
            endGamePanelUI.restartButton.onClick.RemoveAllListeners();
            endGamePanelUI.restartButton.onClick.AddListener(RestartGame);
            Debug.Log("[GameManager] Панель концовки успешно найдена и настроена.");
        }
    }

    public void ShowWinScreen(int finalScore, int totalLevels)
    {
        StartCoroutine(ShowWinScreenCoroutine(finalScore, totalLevels));
    }

    private IEnumerator ShowWinScreenCoroutine(int finalScore, int totalLevels)
    {
        if (endGamePanelUI != null)
        {
            yield return new WaitForSeconds(winScreenDelay);

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.StopAllSounds();
            }

            Time.timeScale = 0f;

            endGamePanelUI.gameObject.SetActive(true);
            endGamePanelUI.ShowWin(finalScore);

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayWinSound();
            }
        }
        else
        {
            Debug.LogError("[GameManager] Панель победы не найдена! Не могу показать экран победы.");
        }
    }

    // --- ВОТ ЕДИНСТВЕННЫЙ И ПРАВИЛЬНЫЙ МЕТОД ПОРАЖЕНИЯ ---
    public void ShowLoseScreen()
    {
        if (endGamePanelUI == null || levelController == null || UIController.Instance == null)
        {
            Debug.LogError("[GameManager] Не могу показать экран поражения! Одна из ключевых ссылок отсутствует (EndGamePanel, LevelController или UIController).");
            return;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopAllSounds();
        }

        Time.timeScale = 0f;

        int finalScore = UIController.Instance.GetCurrentScore();
        int levelsCompleted = UIController.Instance.GetCurrentLevel() - 1;
        int totalLevels = levelController.GetTotalLevelCount();

        endGamePanelUI.gameObject.SetActive(true);
        endGamePanelUI.ShowLose(finalScore, levelsCompleted, totalLevels);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayLoseSound();
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}