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
        Time.timeScale = 1f;
        CollectableItem.ResetInspectionFlag();

        endGamePanelUI = FindObjectOfType<EndGamePanelUI>(true);

        if (endGamePanelUI != null)
        {
            RegisterEndGamePanel(endGamePanelUI);
        }
        else
        {
            Debug.LogWarning("[GameManager] Панель концовки не найдена на сцене при загрузке. Это нормально для главного меню.");
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
        if (endGamePanelUI != null)
        {
            // --- НОВАЯ НАДЕЖНАЯ ЛОГИКА ---
            // 1. СНАЧАЛА: Принудительно останавливаем ВСЕ звуки.
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.StopAllSounds();
            }

            // 2. ПОТОМ: Ставим игру на паузу.
            Time.timeScale = 0f;
            
            // 3. Показываем панель.
            endGamePanelUI.gameObject.SetActive(true); 
            endGamePanelUI.ShowWin(finalScore);

            // 4. В САМОМ КОНЦЕ: Проигрываем звук победы.
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

    public void ShowLoseScreen(int finalScore, int levelsCompleted)
    {
        if (endGamePanelUI != null)
        {
            // --- НОВАЯ НАДЕЖНАЯ ЛОГИКА ---
            // 1. СНАЧАЛА: Принудительно останавливаем ВСЕ звуки.
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.StopAllSounds();
            }

            // 2. ПОТОМ: Ставим игру на паузу.
            Time.timeScale = 0f;

            // 3. Показываем панель.
            endGamePanelUI.gameObject.SetActive(true);
            endGamePanelUI.ShowLose(finalScore, levelsCompleted);

            // 4. В САМОМ КОНЦЕ: Проигрываем звук поражения.
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayLoseSound();
            }
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