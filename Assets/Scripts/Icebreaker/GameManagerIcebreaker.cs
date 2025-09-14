
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerIcebreaker : MonoBehaviour
{
    // НОВОЕ: Приватная переменная для хранения монет, заработанных в ЭТОМ раунде.
    // Она будет равна 0 до тех пор, пока другой скрипт не скажет нам, сколько мы заработали.
    private int currentEarnedCoins = 0;

    // НОВЫЙ ПУБЛИЧНЫЙ МЕТОД:
    // Этот метод будут вызывать другие скрипты, чтобы сообщить, сколько монет заработал игрок.
    // Мы сделаем его публичным, чтобы он был виден "снаружи".
    public void SetEarnedCoins(int amount)
    {
        currentEarnedCoins = amount;
        Debug.Log("За эту игру заработано монет: " + currentEarnedCoins); // Для проверки в консоли
    }

    // Метод для кнопки "Вернуться в меню". Теперь он использует сохраненное значение.
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        // Используем значение, которое нам передали через метод SetEarnedCoins.
        // Если ничего не передали, то добавится 0.
        CoinManager.AddCoins(currentEarnedCoins);

        SceneManager.LoadScene("MainMenu");
    }

    // Метод перезапуска остается без изменений.
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}