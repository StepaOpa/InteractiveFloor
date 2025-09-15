using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

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

    private int currentFishCount;
    private bool isGameOver = false;
    private Queue<PendingCatch> pendingCatches = new Queue<PendingCatch>();
    // "Память" для монеток, которые "зависли в воздухе"
    private int coinsEarnedThisRound = 0;

    void Start()
    {
        currentFishCount = fishObjects.Count;
        UpdateFishCounterUI();
    }

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

    public void OnRestartButtonClicked()
    {
        Time.timeScale = 1f;
        // Ничего не добавляем в копилку
        MainMenuLevelManager.RestartCurrentLevel();
    }
}