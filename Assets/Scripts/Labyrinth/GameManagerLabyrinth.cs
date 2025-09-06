using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic; // Уже есть

public class GameManagerLabyrinth : MonoBehaviour
{
    // <<< НОВОЕ: Структура для хранения информации об отложенной поимке >>>
    private struct PendingCatch
    {
        public GameObject fishToCatch;
        public Transform netTransform;
    }

    [Header("Ссылки на объекты сцены")]
    public GameObject endGamePanel;
    public PlayerControllerLabyrinth player;
    public TimerControllerLabyrinth timer;
    public CameraControllerLabyrinth cameraController;
    public CoinRewardControllerLabyrinth coinRewardController;
    public GameObject inGameUIContainer;

    [Header("Элементы UI на EndGamePanel")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI detailsText;

    [Header("Настройки рыбок")]
    public TextMeshProUGUI fishCounterText;
    public List<GameObject> fishObjects;
    // <<< НОВОЕ: Расстояние, на которое нужно отплыть, чтобы рыбка "попалась" >>>
    [Tooltip("На каком расстоянии от сети рыбка окончательно 'попадется'")]
    public float catchDistanceThreshold = 3.0f;

    private int currentFishCount;
    private bool isGameOver = false;

    // <<< НОВОЕ: Очередь для отложенных поимок >>>
    private Queue<PendingCatch> pendingCatches = new Queue<PendingCatch>();

    void Start()
    {
        currentFishCount = fishObjects.Count;
        UpdateFishCounterUI();
    }

    void Update()
    {
        // В каждом кадре проверяем, не пора ли выполнить отложенную поимку
        ProcessPendingCatches();
    }

    // <<< ИЗМЕНЕНО: Теперь метод не перемещает рыбку, а ставит поимку в очередь >>>
    public void CatchOneFish(Transform netTransform)
    {
        if (isGameOver || currentFishCount <= 0) return;

        currentFishCount--;
        UpdateFishCounterUI(); // Сразу обновляем UI, чтобы игрок видел потерю

        // Находим рыбку, которую нужно будет поймать
        GameObject fishToCatch = fishObjects[currentFishCount];

        // Создаем новую "задачу" на поимку
        PendingCatch newCatch = new PendingCatch
        {
            fishToCatch = fishToCatch,
            netTransform = netTransform
        };

        // Добавляем задачу в очередь
        pendingCatches.Enqueue(newCatch);

        if (currentFishCount <= 0)
        {
            LoseGame("Все рыбки попались в сети!");
        }
    }

    // <<< НОВЫЙ МЕТОД: Обработчик очереди >>>
    void ProcessPendingCatches()
    {
        // Если в очереди есть задачи и игрок еще существует
        if (pendingCatches.Count > 0 && player != null)
        {
            // Смотрим на первую задачу в очереди (не удаляя её)
            PendingCatch nextCatch = pendingCatches.Peek();

            // Проверяем дистанцию от игрока до сети
            float distance = Vector3.Distance(player.transform.position, nextCatch.netTransform.position);

            // Если игрок отплыл достаточно далеко
            if (distance > catchDistanceThreshold)
            {
                // Извлекаем задачу из очереди
                PendingCatch catchToExecute = pendingCatches.Dequeue();

                // И теперь выполняем саму логику "поимки"
                catchToExecute.fishToCatch.transform.parent = null; // Отцепляем от группы
                catchToExecute.fishToCatch.transform.position = catchToExecute.netTransform.position; // Перемещаем в сеть
            }
        }
    }

    void UpdateFishCounterUI()
    {
        if (fishCounterText != null)
        {
            fishCounterText.text = "Рыбок: " + currentFishCount;
        }
    }

    // --- Остальной код скрипта остается без изменений ---

    public void WinGame()
    {
        if (isGameOver) return;
        isGameOver = true;

        int reward = 0;
        if (currentFishCount == 3) { reward = 10; }
        else if (currentFishCount == 2) { reward = 5; }
        else if (currentFishCount == 1) { reward = 1; }

        titleText.text = "Победа!";
        detailsText.text = "Вы нашли выход из лабиринта!\nЗаработано монет: " + reward;
        StartCoroutine(EndGameSequence(true, reward));
    }

    public void LoseGame(string reason)
    {
        if (isGameOver) return;
        isGameOver = true;
        titleText.text = "Поражение!";
        detailsText.text = reason;
        StartCoroutine(EndGameSequence(false, 0));
    }

    private IEnumerator EndGameSequence(bool didWin, int finalReward)
    {
        if (inGameUIContainer != null) { inGameUIContainer.SetActive(false); }
        player.enabled = false;
        timer.enabled = false;
        cameraController.SwitchToFarViewImmediately();
        yield return new WaitForSeconds(1.5f);
        endGamePanel.SetActive(true);

        if (didWin) { StartCoroutine(coinRewardController.GetRewardSequenceCoroutine(finalReward)); }
        else { Time.timeScale = 0f; }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        Debug.Log("Переход в главное меню...");
    }
}