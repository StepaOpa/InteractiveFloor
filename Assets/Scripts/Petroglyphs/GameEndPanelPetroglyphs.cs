using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameEndPanelPetroglyphs : MonoBehaviour
{
    [Header("UI Компоненты")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI detailsText;

    [Header("Система наград")]
    [SerializeField] private CoinRewardController coinRewardController;

    // "Память" для монеток, которые "зависли в воздухе"
    private int coinsEarnedThisRound = 0;

    // Этот метод вызывается из GameManager'а
    public void ShowPanel(bool isWin, int foundCount, int totalCount, int coinsAwarded)
    {
        gameObject.SetActive(true);
        // Запоминаем награду за этот раунд
        this.coinsEarnedThisRound = coinsAwarded;

        if (isWin)
        {
            titleText.text = "Победа!";
            detailsText.text = $"Вы нашли все рисунки!\nЗаработано монеток: {coinsAwarded}";

            if (coinRewardController != null)
            {
                coinRewardController.StartRewardSequence(coinsAwarded);
            }
        }
        else
        {
            titleText.text = "Поражение";
            detailsText.text = $"Время вышло.\nНайдено рисунков: {foundCount} из {totalCount}";
        }
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