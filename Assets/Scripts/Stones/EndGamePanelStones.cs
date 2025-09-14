using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndGamePanelStones : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI detailsText;

    // НОВОЕ: Переменная-память для хранения награды.
    private int coinsEarnedThisRound = 0;

    // ИЗМЕНЕНО: Метод теперь принимает количество заработанных монет.
    public void ShowPanel(bool isVictory, int coinsEarned)
    {
        gameObject.SetActive(true);

        // НОВОЕ: Сохраняем полученную награду в нашу переменную.
        this.coinsEarnedThisRound = coinsEarned;

        if (isVictory)
        {
            titleText.text = "Победа!";
            // ИЗМЕНЕНО: Отображаем количество заработанных монет.
            detailsText.text = $"Все ценные камни собраны\nЗаработано монет: {coinsEarned}";
        }
        else
        {
            titleText.text = "Поражение";
            detailsText.text = "Время вышло";
        }
    }

    public void OnRestartButtonClicked()
    {
        Time.timeScale = 1f; // На всякий случай сбрасываем время
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ИЗМЕНЕНО: Теперь этот метод сохраняет монеты и переходит в меню.
    public void OnMainMenuButtonClicked()
    {
        Time.timeScale = 1f; // Сбрасываем время перед переходом

        // 1. Добавляем сохраненные монеты в общий счет
        CoinManager.AddCoins(coinsEarnedThisRound);

        // 2. Загружаем главное меню
        SceneManager.LoadScene("MainMenu");
    }
}