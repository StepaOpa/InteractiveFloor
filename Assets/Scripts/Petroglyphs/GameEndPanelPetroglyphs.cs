using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameEndPanelPetroglyphs : MonoBehaviour
{
    [Header("UI Компоненты")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI detailsText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Система наград")]
    [SerializeField] private CoinRewardController coinRewardController;

    // НОВОЕ: Переменная для хранения награды за раунд.
    private int coinsEarnedThisRound = 0;

    void Start()
    {
        // Убедимся, что Time.timeScale сброшен на всякий случай
        Time.timeScale = 1f;
        restartButton.onClick.AddListener(OnRestartButtonClick);
        mainMenuButton.onClick.AddListener(OnMainMenuButtonClick);
    }

    // Метод ИЗМЕНЕН: теперь он сохраняет полученную награду.
    public void ShowPanel(bool isWin, int foundCount, int totalCount, int coinsAwarded)
    {
        gameObject.SetActive(true);

        // НОВОЕ: Сохраняем полученное количество монет в нашу "память".
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

    private void OnRestartButtonClick()
    {
        // Сбрасываем Time.timeScale перед перезагрузкой
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Метод ИЗМЕНЕН: теперь он выполняет всю нужную логику.
    private void OnMainMenuButtonClick()
    {
        // Сбрасываем Time.timeScale перед переходом
        Time.timeScale = 1f;

        // 1. Используем сохраненное значение для начисления монет
        CoinManager.AddCoins(coinsEarnedThisRound);

        // 2. Переходим в главное меню
        SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        restartButton.onClick.RemoveListener(OnRestartButtonClick);
        mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClick);
    }
}