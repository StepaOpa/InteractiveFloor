using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManagerLabyrinth : MonoBehaviour
{
    [Header("Ссылки на объекты сцены")]
    public GameObject endGamePanel;
    public PlayerControllerLabyrinth player;
    public TimerControllerLabyrinth timer;
    public CameraControllerLabyrinth cameraController;
    public CoinRewardControllerLabyrinth coinRewardController;
    // <<< НОВОЕ: Ссылка на контейнер с игровым UI >>>
    public GameObject inGameUIContainer;

    [Header("Элементы UI на EndGamePanel")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI detailsText;

    private bool isGameOver = false;

    // ... (Start и Update остаются без изменений) ...
    void Start() { /* ... */ }
    void Update() { /* ... */ }

    public void WinGame()
    {
        if (isGameOver) return;
        isGameOver = true;
        titleText.text = "Победа!";
        detailsText.text = "Вы нашли выход из лабиринта!\nЗаработано монет: 10";
        StartCoroutine(EndGameSequence(true));
    }

    public void LoseGame(string reason)
    {
        if (isGameOver) return;
        isGameOver = true;
        titleText.text = "Поражение!";
        detailsText.text = reason;
        StartCoroutine(EndGameSequence(false));
    }

    private IEnumerator EndGameSequence(bool didWin)
    {
        // <<< НОВОЕ: В самом начале концовки выключаем игровой интерфейс >>>
        inGameUIContainer.SetActive(false);

        // --- Остальная логика остается прежней ---
        player.enabled = false;
        timer.enabled = false;
        cameraController.SwitchToFarViewImmediately();
        yield return new WaitForSeconds(1.5f);
        endGamePanel.SetActive(true);

        if (didWin)
        {
            StartCoroutine(coinRewardController.GetRewardSequenceCoroutine(10));
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