using UnityEngine;
using UnityEngine.SceneManagement; // Эта строка нужна для перезагрузки сцены
using TMPro; // Эта строка нужна для работы с текстом

public class GameManagerLabyrinth : MonoBehaviour
{
    [Header("Ссылки на объекты сцены")]
    public GameObject endGamePanel;          // Сюда перетащим панель конца игры
    public PlayerControllerLabyrinth player; // Сюда перетащим объект рыбы
    public TimerControllerLabyrinth timer;   // Сюда перетащим скрипт таймера

    [Header("Элементы UI на EndGamePanel")]
    public TextMeshProUGUI titleText;        // Текст "Победа" / "Поражение"
    public TextMeshProUGUI detailsText;      // Детальный текст (причина)

    private bool isGameOver = false; // Флаг, который показывает, что игра уже закончилась

    void Start()
    {
        // В начале игры всегда прячем панель конца игры
        endGamePanel.SetActive(false);
        // Включаем время (важно для перезапуска)
        Time.timeScale = 1f;
    }

    void Update()
    {
        // Если игра уже закончилась, то ничего не делаем
        if (isGameOver) return;

        // Постоянно проверяем, не вышло ли время на таймере
        if (timer.timeIsUp)
        {
            // Если время вышло, вызываем функцию поражения
            LoseGame("Время вышло!");
        }
    }

    // Этот метод будет вызываться, когда игрок победил
    public void WinGame()
    {
        if (isGameOver) return; // Дополнительная проверка, чтобы не сработал дважды
        isGameOver = true;

        titleText.text = "Победа!";
        detailsText.text = "Вы нашли выход из лабиринта!";

        EndGameSequence();
    }

    // Этот метод будет вызываться, когда игрок проиграл
    public void LoseGame(string reason)
    {
        if (isGameOver) return;
        isGameOver = true;

        titleText.text = "Поражение!";
        detailsText.text = reason; // Показываем причину: "Время вышло!" или "Попались в сеть!"

        EndGameSequence();
    }

    // Общая последовательность действий в конце игры
    private void EndGameSequence()
    {
        // "Замораживаем" игру
        Time.timeScale = 0f;
        // Отключаем скрипт управления рыбой, чтобы ей нельзя было двигать
        player.enabled = false;
        // Показываем панель
        endGamePanel.SetActive(true);
    }

    // --- Методы для кнопок на EndGamePanel ---

    public void RestartGame()
    {
        // Перезагружаем текущую сцену
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        // В будущем здесь будет код для перехода в главное меню
        // Пока просто выведем сообщение в консоль
        Debug.Log("Переход в главное меню...");
        // SceneManager.LoadScene("MainMenuScene"); // Пример, как это будет выглядеть
    }
}