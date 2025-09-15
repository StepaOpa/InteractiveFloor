using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndGamePanelStones : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI detailsText;

    private int coinsEarnedThisRound = 0;

    public void ShowPanel(bool isVictory, int coinsEarned)
    {
        gameObject.SetActive(true);
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

    public void OnNextLevelButtonClicked()
    {
        Time.timeScale = 1f;
        CoinManager.AddCoins(coinsEarnedThisRound);
        MainMenuLevelManager.LoadNextLevel();
    }

    // ИЗМЕНЕННЫЙ МЕТОД
    public void OnEndGameButtonClicked()
    {
        Time.timeScale = 1f;
        CoinManager.AddCoins(coinsEarnedThisRound);

        // НОВАЯ СТРОКА: Перед переходом поднимаем флаг!
        MainMenuLevelManager.shouldResetScoreOnLoad = true;

        SceneManager.LoadScene("TotalScoreScene");
    }

    public void OnRestartButtonClicked()
    {
        Time.timeScale = 1f;
        MainMenuLevelManager.RestartCurrentLevel();
    }
}