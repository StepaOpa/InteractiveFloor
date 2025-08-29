using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Добавляем для использования корутин

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private EndGamePanelUI endGamePanelUI;
    [SerializeField] private float winScreenDelay = 0.5f; // Задержка перед показом экрана победы

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

    // --- ИЗМЕНЕНИЕ 1: Публичный метод теперь запускает корутину ---
    public void ShowWinScreen(int finalScore, int totalLevels)
    {
        // Мы не показываем экран сразу, а запускаем процесс с задержкой
        StartCoroutine(ShowWinScreenCoroutine(finalScore, totalLevels));
    }

    // --- ИЗМЕНЕНИЕ 2: Сама логика теперь находится в корутине ---
    private IEnumerator ShowWinScreenCoroutine(int finalScore, int totalLevels)
    {
        if (endGamePanelUI != null)
        {
            // 1. ЖДЕМ: Даем время UI-анимации последнего предмета завершиться.
            yield return new WaitForSeconds(winScreenDelay);

            // 2. СНАЧАЛА: Принудительно останавливаем ВСЕ звуки.
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.StopAllSounds();
            }

            // 3. ПОТОМ: Ставим игру на паузу.
            Time.timeScale = 0f;
            
            // 4. Показываем панель.
            endGamePanelUI.gameObject.SetActive(true); 
            endGamePanelUI.ShowWin(finalScore);

            // 5. В САМОМ КОНЦЕ: Проигрываем звук победы.
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


    // Экран поражения не требует задержки, так как он срабатывает по таймеру, а не от сбора предмета.
    // Оставляем его без изменений.
    public void ShowLoseScreen(int finalScore, int levelsCompleted)
    {
        if (endGamePanelUI != null)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.StopAllSounds();
            }

            Time.timeScale = 0f;

            endGamePanelUI.gameObject.SetActive(true);
            endGamePanelUI.ShowLose(finalScore, levelsCompleted);

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