using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndGamePanelStones : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI detailsText;

    // "Память" для монеток, которые "зависли в воздухе"
    private int coinsEarnedThisRound = 0;

    // Этот метод вызывается из GameManagerStones, чтобы показать панель
    public void ShowPanel(bool isVictory, int coinsEarned)
    {
        gameObject.SetActive(true);
        // Запоминаем награду за этот раунд
        this.coinsEarnedThisRound = coinsEarned;

        if (isVictory)
        {
            titleText.text = "Победа!";
            detailsText.text = $"Все ценные камни собраны\nЗаработано монет: {coinsEarned}";
        }
        else
        {
            titleText.text = "Поражение";
            detailsText.text = "Время вышло";
        }
    }

    // --- МЕТОДЫ ДЛЯ ТРЕХ КНОПОК ---

    // 1. Для кнопки "Следующий уровень"
    public void OnNextLevelButtonClicked()
    {
        Time.timeScale = 1f;
        // Сначала добавляем монеты в общую копилку
        CoinManager.AddCoins(coinsEarnedThisRound);
        // Потом переходим на следующий уровень
        MainMenuLevelManager.LoadNextLevel();
    }

    // 2. Для кнопки "Завершить игру"
    public void OnEndGameButtonClicked()
    {
        Time.timeScale = 1f;
        // Сначала добавляем монеты в общую копилку
        CoinManager.AddCoins(coinsEarnedThisRound);
        // Потом переходим на экран общего счета
        SceneManager.LoadScene("TotalScoreScene");
    }

    // 3. Для кнопки "Начать заново"
    public void OnRestartButtonClicked()
    {
        Time.timeScale = 1f;
        // Ничего НЕ добавляем в копилку!
        // Просто перезапускаем текущий уровень
        MainMenuLevelManager.RestartCurrentLevel();
    }
}