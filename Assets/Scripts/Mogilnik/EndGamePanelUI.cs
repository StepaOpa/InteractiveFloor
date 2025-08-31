using UnityEngine;
using UnityEngine.UI;
using TMPro; // Если ты используешь TextMeshPro для текста

public class EndGamePanelUI : MonoBehaviour
{
    // --- У тебя здесь уже есть поля для текста и кнопок ---
    [Header("Ссылки на UI элементы")]
    [SerializeField] private TextMeshProUGUI titleText;   // Поле для "Победа!"/"Поражение"
    [SerializeField] private TextMeshProUGUI detailsText; // Поле для "Набрано очков..."
    public Button restartButton;
    [SerializeField] private Button menuButton;

    // <-- НОВОЕ 1/3: Добавляем ссылку на наш контроллер монет -->
    [Header("Система наград")]
    [SerializeField] private CoinRewardController coinRewardController;

    // <-- НОВОЕ 2/3: Добавляем этот метод, чтобы очищать монетки при перезапуске -->
    void OnDisable()
    {
        // Этот код сработает, когда панель выключается
        if (coinRewardController != null)
        {
            coinRewardController.ClearCoins();
        }
    }

    // Этот метод у тебя уже есть. Мы просто добавим в него одну строчку.
    public void ShowWin(int finalScore)
    {
        titleText.text = "Победа!";
        detailsText.text = $"Все уровни пройдены!\nНабрано очков: {finalScore}";
        gameObject.SetActive(true);

        // <-- НОВОЕ 3/3: ЗАПУСКАЕМ АНИМАЦИЮ МОНЕТОК! -->
        if (coinRewardController != null)
        {
            // ИЗМЕНЕНО: Передаем сюда finalScore
            coinRewardController.StartRewardSequence(finalScore);
        }
    }

    // Этот метод у тебя тоже должен быть. Мы его не трогаем, но убедимся, что он на месте.
    public void ShowLose(int finalScore, int levelsCompleted)
    {
        titleText.text = "Поражение";
        detailsText.text = $"Пройдено уровней: {levelsCompleted}\nНабрано очков: {finalScore}";
        gameObject.SetActive(true);
    }
}