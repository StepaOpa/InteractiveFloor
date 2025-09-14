using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerIcebreaker : MonoBehaviour
{
    // "Память" для монеток, которые "зависли в воздухе"
    private int currentEarnedCoins = 0;

    // Этот метод вызывается из IcebreakerController, чтобы сообщить результат
    public void SetEarnedCoins(int amount)
    {
        currentEarnedCoins = amount;
    }

    // --- МЕТОДЫ ДЛЯ ТРЕХ КНОПОК ---

    public void OnNextLevelButtonClicked()
    {
        Time.timeScale = 1f;
        // Сначала добавляем монеты в общую копилку
        CoinManager.AddCoins(currentEarnedCoins);
        // Потом переходим на следующий уровень
        MainMenuLevelManager.LoadNextLevel();
    }



    public void OnEndGameButtonClicked()
    {
        Time.timeScale = 1f;
        // Сначала добавляем монеты в общую копилку
        CoinManager.AddCoins(currentEarnedCoins);
        // Поднимаем флаг для сброса счета в главном меню
        MainMenuLevelManager.shouldResetScoreOnLoad = true;
        // Потом переходим на экран общего счета
        SceneManager.LoadScene("TotalScoreScene");
    }

    public void OnRestartButtonClicked()
    {
        Time.timeScale = 1f;
        // Ничего не добавляем в копилку
        // Просто перезапускаем текущий уровень
        MainMenuLevelManager.RestartCurrentLevel();
    }
}