using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndGamePanelUI : MonoBehaviour
{
    [Header("Ссылки на UI элементы")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI detailsText;
    public Button restartButton; // Кнопка "Начать заново"
    public Button menuButton; // Кнопка "Завершить игру"
    public Button nextLevelButton; // НОВАЯ КНОПКА "Следующий уровень"

    [Header("Система наград")]
    [SerializeField] private CoinRewardController coinRewardController;

    void OnDisable()
    {
        if (coinRewardController != null)
        {
            coinRewardController.ClearCoins();
        }
    }

    public void ShowWin(int finalScore)
    {
        titleText.text = "Победа!";
        detailsText.text = $"Все уровни пройдены!\nЗаработано монеток: {finalScore}";
        gameObject.SetActive(true);

        if (coinRewardController != null)
        {
            coinRewardController.StartRewardSequence(finalScore);
        }
    }

    public void ShowLose(int finalScore, int levelsCompleted, int totalLevels)
    {
        titleText.text = "Время вышло";
        detailsText.text = $"Пройдено уровней: {levelsCompleted} из {totalLevels}\nЗаработано монеток: {finalScore}";
        gameObject.SetActive(true);
    }
}