using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManagerLabyrinth : MonoBehaviour
{
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
    private int currentFishCount;

    private bool isGameOver = false;

    void Start()
    {
        currentFishCount = fishObjects.Count;
        UpdateFishCounterUI();
    }

    // <<< ИЗМЕНЕНО: Теперь метод принимает Transform сети, в которую попалась рыбка >>>
    public void CatchOneFish(Transform netTransform)
    {
        if (isGameOver || currentFishCount <= 0) return;

        currentFishCount--;

        // <<< ИЗМЕНЕНО: Вместо отключения рыбки, мы делаем следующее: >>>

        // 1. Находим рыбку, которую нужно "поймать"
        GameObject fishToCatch = fishObjects[currentFishCount];

        // 2. "Отцепляем" её от родительской группы, чтобы она перестала двигаться с игроком
        fishToCatch.transform.parent = null;

        // 3. Перемещаем её в центр сети
        fishToCatch.transform.position = netTransform.position;

        UpdateFishCounterUI();

        if (currentFishCount <= 0)
        {
            LoseGame("Все рыбки попались в сети!");
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
        if (currentFishCount == 3)
        {
            reward = 10;
        }
        else if (currentFishCount == 2)
        {
            reward = 5;
        }
        else if (currentFishCount == 1)
        {
            reward = 1;
        }

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
        if (inGameUIContainer != null)
        {
            inGameUIContainer.SetActive(false);
        }

        player.enabled = false;
        timer.enabled = false;
        cameraController.SwitchToFarViewImmediately();
        yield return new WaitForSeconds(1.5f);
        endGamePanel.SetActive(true);

        if (didWin)
        {
            StartCoroutine(coinRewardController.GetRewardSequenceCoroutine(finalReward));
        }
        else
        {
            Time.timeScale = 0f;
        }
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