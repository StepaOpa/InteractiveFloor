using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections; // <<< НОВОЕ: Добавьте эту строку для работы с корутинами

public class GameManagerLabyrinth : MonoBehaviour
{
    [Header("Ссылки на объекты сцены")]
    public GameObject endGamePanel;
    public PlayerControllerLabyrinth player;
    public TimerControllerLabyrinth timer;
    public CameraControllerLabyrinth cameraController; // <<< НОВОЕ: Ссылка на скрипт камеры

    [Header("Элементы UI на EndGamePanel")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI detailsText;

    private bool isGameOver = false;

    // ... (методы Start и Update остаются без изменений) ...
    void Start()
    {
        endGamePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (isGameOver) return;
        if (timer.timeIsUp)
        {
            LoseGame("Время вышло!");
        }
    }

    // --- Методы Win/Lose теперь запускают корутину ---
    public void WinGame()
    {
        if (isGameOver) return;
        isGameOver = true;
        titleText.text = "Победа!";
        detailsText.text = "Вы нашли выход из лабиринта!";
        StartCoroutine(EndGameSequence()); // <<< ИЗМЕНЕНО
    }

    public void LoseGame(string reason)
    {
        if (isGameOver) return;
        isGameOver = true;
        titleText.text = "Поражение!";
        detailsText.text = reason;
        StartCoroutine(EndGameSequence()); // <<< ИЗМЕНЕНО
    }

    // --- Старый метод EndGameSequence заменен на корутину ---
    private IEnumerator EndGameSequence()
    {
        // 1. Отключаем управление рыбой
        player.enabled = false;

        // 2. Говорим камере переключиться на общий план
        cameraController.SwitchToFarViewImmediately();

        // 3. Ждем 1.5 секунды, пока камера "отъедет"
        yield return new WaitForSeconds(1.5f);

        // 4. Только теперь "замораживаем" игру и показываем панель
        Time.timeScale = 0f;
        endGamePanel.SetActive(true);
    }

    // ... (методы RestartGame и GoToMenu остаются без изменений) ...
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        Debug.Log("Переход в главное меню...");
    }
}