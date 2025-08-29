using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Теперь это поле будет видно в инспекторе, но мы его будем заполнять кодом
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

        // --- ГЛАВНОЕ ИЗМЕНЕНИЕ ---
        // Ищем объект EndGamePanelUI на сцене, даже если он выключен (true в скобках)
        endGamePanelUI = FindObjectOfType<EndGamePanelUI>(true);

        // Если нашли, сразу регистрируем
        if (endGamePanelUI != null)
        {
            RegisterEndGamePanel(endGamePanelUI);
        }
        else
        {
            Debug.LogWarning("[GameManager] Панель концовки не найдена на сцене при загрузке. Это нормально для главного меню.");
        }
        // -------------------------
    }

    // Этот метод теперь не вызывается извне, а только из OnSceneLoaded
    private void RegisterEndGamePanel(EndGamePanelUI panel)
    {
        endGamePanelUI = panel;
        
        if (endGamePanelUI != null)
        {
            // Убеждаемся, что панель выключена в начале
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
            Time.timeScale = 0f;
            // Теперь мы включаем панель прямо здесь
            endGamePanelUI.gameObject.SetActive(true); 
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
            // И здесь тоже
            endGamePanelUI.gameObject.SetActive(true);
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