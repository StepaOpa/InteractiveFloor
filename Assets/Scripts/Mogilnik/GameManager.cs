using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private EndGamePanelUI endGamePanelUI;

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
        // Сбрасываем время
        Time.timeScale = 1f;

        // <-- ИЗМЕНЕНИЕ 3: Вызываем сброс флага при каждой загрузке сцены
        CollectableItem.ResetInspectionFlag();
    }

    public void RegisterEndGamePanel(EndGamePanelUI panel)
    {
        endGamePanelUI = panel;
        
        if (endGamePanelUI != null)
        {
            endGamePanelUI.gameObject.SetActive(false);
            endGamePanelUI.restartButton.onClick.RemoveAllListeners();
            endGamePanelUI.restartButton.onClick.AddListener(RestartGame);
            Debug.Log("[GameManager] Панель концовки успешно зарегистрирована и спрятана.");
        }
    }

    public void ShowWinScreen(int finalScore, int totalLevels)
    {
        if (endGamePanelUI != null)
        {
            Time.timeScale = 0f;
            endGamePanelUI.ShowWin(finalScore);
            if (SoundManager.Instance != null) SoundManager.Instance.PlayWinSound();
        }
        else
        {
             Debug.LogError("[GameManager] Панель победы не найдена! Не могу показать экран победы.");
        }
    }

    public void ShowLoseScreen(int finalScore, int levelsCompleted)
    {
        if (endGamePanelUI != null)
        {
            Time.timeScale = 0f;
            endGamePanelUI.ShowLose(finalScore, levelsCompleted);
            if (SoundManager.Instance != null) SoundManager.Instance.PlayLoseSound();
        }
        else
        {
            Debug.LogError("[GameManager] Панель поражения не найдена! Не могу показать экран поражения.");
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}