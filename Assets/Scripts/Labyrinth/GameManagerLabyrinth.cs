using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Добавлено для использования .ToList()

public class GameManagerLabyrinth : MonoBehaviour
{
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
    [Tooltip("На каком расстоянии от сети рыбка окончательно 'попадется'")]
    public float catchDistanceThreshold = 3.0f;

    // =======================================================================
    // <<< НОВЫЙ РАЗДЕЛ ДЛЯ СПАВНА ЛОВУШЕК >>>
    // =======================================================================
    [Header("Настройки спавна ловушек")]
    [Tooltip("Префаб объекта-ловушки (сети), который будет создан")]
    public GameObject netPrefab;

    [Tooltip("Список всех возможных точек, где могут появиться ловушки")]
    public List<Transform> netSpawnPoints;

    [Tooltip("Минимальное количество ловушек для спавна")]
    [Range(1, 20)]
    public int minNetsToSpawn = 5;

    [Tooltip("Максимальное количество ловушек для спавна")]
    [Range(1, 20)]
    public int maxNetsToSpawn = 8;
    // =======================================================================

    private int currentFishCount;
    private bool isGameOver = false;
    private Queue<PendingCatch> pendingCatches = new Queue<PendingCatch>();
    // "Память" для монеток, которые "зависли в воздухе"
    private int coinsEarnedThisRound = 0;

    void Start()
    {
        currentFishCount = fishObjects.Count;
        UpdateFishCounterUI();

        // <<< ВЫЗОВ НОВОГО МЕТОДА >>>
        SpawnNets();
    }

    // =======================================================================
    // <<< НОВЫЙ МЕТОД ДЛЯ СПАВНА ЛОВУШEK >>>
    // =======================================================================
    void SpawnNets()
    {
        // 1. Проверяем, что все необходимые данные заданы в инспекторе
        if (netPrefab == null || netSpawnPoints.Count == 0)
        {
            Debug.LogError("Префаб ловушки или точки спавна не назначены в GameManagerLabyrinth!");
            return;
        }

        // 2. Определяем, сколько ловушек мы хотим создать в этот раз
        int netsToSpawnCount = Random.Range(minNetsToSpawn, maxNetsToSpawn + 1);

        // 3. Создаем временную копию списка точек, чтобы мы могли удалять из нее использованные
        List<Transform> availablePoints = netSpawnPoints.ToList();

        // 4. Запускаем цикл для создания ловушек
        for (int i = 0; i < netsToSpawnCount; i++)
        {
            // Если доступных точек не осталось, выходим из цикла раньше времени
            if (availablePoints.Count == 0)
            {
                break;
            }

            // Выбираем случайный индекс из списка ДОСТУПНЫХ точек
            int randomIndex = Random.Range(0, availablePoints.Count);
            // Получаем саму точку по этому индексу
            Transform spawnPoint = availablePoints[randomIndex];

            // Создаем новый объект-ловушку из префаба
            // Используем позицию (position) и поворот (rotation) из нашей точки спавна
            Instantiate(netPrefab, spawnPoint.position, spawnPoint.rotation);

            // Удаляем использованную точку из списка, чтобы не выбрать ее снова
            availablePoints.RemoveAt(randomIndex);
        }
    }
    // =======================================================================

    void Update()
    {
        if (isGameOver) return;
        ProcessPendingCatches();
        if (timer != null && timer.timeIsUp)
        {
            LoseGame("Время вышло!");
        }
    }

    public void CatchOneFish(Transform netTransform)
    {
        if (isGameOver || currentFishCount <= 0) return;
        currentFishCount--;
        UpdateFishCounterUI();
        GameObject fishToCatch = fishObjects[currentFishCount];
        PendingCatch newCatch = new PendingCatch { fishToCatch = fishToCatch, netTransform = netTransform };
        pendingCatches.Enqueue(newCatch);
        if (currentFishCount <= 0)
        {
            LoseGame("Все рыбки попались в сети!");
        }
    }

    void ProcessPendingCatches()
    {
        if (pendingCatches.Count > 0 && player != null)
        {
            PendingCatch nextCatch = pendingCatches.Peek();
            float distance = Vector3.Distance(player.transform.position, nextCatch.netTransform.position);
            if (distance > catchDistanceThreshold)
            {
                PendingCatch catchToExecute = pendingCatches.Dequeue();
                catchToExecute.fishToCatch.transform.parent = null;
                catchToExecute.fishToCatch.transform.position = catchToExecute.netTransform.position;
                FishAnimator animator = catchToExecute.fishToCatch.GetComponent<FishAnimator>();
                if (animator != null) animator.enabled = false;
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
        // Запоминаем награду в "память"
        this.coinsEarnedThisRound = finalReward;

        if (inGameUIContainer != null) { inGameUIContainer.SetActive(false); }
        player.enabled = false;
        timer.enabled = false;
        cameraController.SwitchToFarViewImmediately();
        yield return new WaitForSeconds(1.5f);
        endGamePanel.SetActive(true);
        if (didWin) { StartCoroutine(coinRewardController.GetRewardSequenceCoroutine(finalReward)); }
        else { Time.timeScale = 0f; }
    }

    // --- МЕТОДЫ ДЛЯ ТРЕХ КНОПОК ---

    public void OnNextLevelButtonClicked()
    {
        Time.timeScale = 1f;
        CoinManager.AddCoins(coinsEarnedThisRound);
        MainMenuLevelManager.LoadNextLevel();
    }

    public void OnEndGameButtonClicked()
    {
        Time.timeScale = 1f;
        CoinManager.AddCoins(coinsEarnedThisRound);
        // Поднимаем флаг для сброса счета в главном меню
        MainMenuLevelManager.shouldResetScoreOnLoad = true;
        SceneManager.LoadScene("TotalScoreScene");
    }

    private void OnRestartButtonClicked()
    {
        Time.timeScale = 1f;
        // Получаем текущую активную сцену и загружаем её снова по имени
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}